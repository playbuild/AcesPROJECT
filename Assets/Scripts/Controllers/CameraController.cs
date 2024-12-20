using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public enum CameraIndex
    {
        ThirdView,
        FirstView,
        FirstViewWithCockpit,
    }

    [Header("Camera Lerp")]
    public float lerpAmount;

    [Header("Camera Objects")]
    public Camera[] cameras = new Camera[3];

    Vector2 lookInputValue;
    Vector2 lookValue;

    public Transform cameraPivot;
    public Transform thirdViewCameraPivot;

    float zoomValue;
    public float zoomLerpAmount;
    public float zoomAmount;

    float rollValue;
    public float rollLerpAmount;
    public float rollAmount;

    float pitchValue;
    public float pitchLerpAmount;
    public float pitchAmount;

    Transform lockOnTargetTransform;

    UIController uiController;
    public Transform targetArrowTransform;

    Camera currentCamera;

    int cameraViewIndex = 0;

    Vector3 thirdPivotOriginPosition;

    public void Look(InputAction.CallbackContext context)
    {
        lookInputValue = context.ReadValue<Vector2>();
    }

    public void ChangeCameraView(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            cameraViewIndex = (++cameraViewIndex) % 3;
            SetCamera();
        }
    }

    void SetCamera()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (i == cameraViewIndex)
            {
                currentCamera = cameras[i];
            }
            cameras[i].enabled = (i == cameraViewIndex);
        }
        targetArrowTransform.SetParent(currentCamera.transform, false);
        uiController.SwitchUI((CameraIndex)cameraViewIndex);
    }
    void Start()
    {
        uiController = GameManager.UIController;
        SetCamera();
        thirdPivotOriginPosition = thirdViewCameraPivot.localPosition;
    }

    //카메라 역동적 조절
    public void AdjustCameraValue(float aircraftAccelValue, float aircraftRollValue, float aircraftPitchValue)
    {
        zoomValue = Mathf.Lerp(zoomValue, aircraftAccelValue, zoomLerpAmount * Time.deltaTime);
        rollValue = Mathf.Lerp(rollValue, aircraftRollValue, rollLerpAmount * Time.deltaTime);
        pitchValue = Mathf.Lerp(pitchValue, aircraftPitchValue, pitchLerpAmount * Time.deltaTime);
    }

    //카메라 모드에 따른 시점 변경 조절
    void Rotate1stViewCamera()
    {
        Quaternion rotateQuaternion;

        if (lockOnTargetTransform != null)
        {
            rotateQuaternion = Quaternion.Lerp(cameraPivot.localRotation, CalculateLockOnRotation(), lerpAmount * Time.deltaTime);
        }
        else
        {
            Vector3 rotateValue = new Vector3(lookValue.y * -90, lookValue.x * 180, 0);
            rotateQuaternion = Quaternion.Lerp(cameraPivot.localRotation, Quaternion.Euler(rotateValue), lerpAmount * Time.deltaTime);
        }

        cameraPivot.localRotation = rotateQuaternion;
        uiController.AdjustFirstViewUI(rotateQuaternion.eulerAngles);
    }

    void Rotate1stViewWithCockpitCamera()
    {
        Quaternion rotateQuaternion;

        if (lockOnTargetTransform != null)
        {
            rotateQuaternion = Quaternion.Lerp(cameraPivot.localRotation, CalculateLockOnRotation(), lerpAmount * Time.deltaTime);

            // + Adjust/Clamp value
            Vector3 rotateValue = rotateQuaternion.eulerAngles;
            if (rotateValue.x > 180) rotateValue.x -= 360;
            if (rotateValue.y > 180) rotateValue.y -= 360;
            rotateValue.x = Mathf.Clamp(rotateValue.x, -90, 27);
            rotateValue.y = Mathf.Clamp(rotateValue.y, -135, 135);

            rotateQuaternion = Quaternion.Euler(rotateValue);
        }
        else
        {
            Vector3 rotateValue = new Vector3(lookValue.y * -90, lookValue.x * 135, 0);
            if (rotateValue.x > 0) rotateValue.x *= 0.3f;
            rotateQuaternion = Quaternion.Lerp(cameraPivot.localRotation, Quaternion.Euler(rotateValue), lerpAmount * Time.deltaTime);
        }

        cameraPivot.localRotation = rotateQuaternion;
        uiController.AdjustFirstViewUI(rotateQuaternion.eulerAngles);
    }

    void Rotate3rdViewCamera()
    {
        Transform cameraTransform = currentCamera.transform;
        Quaternion rotateQuaternion;

        if (lockOnTargetTransform != null)
        {
            rotateQuaternion = Quaternion.Lerp(thirdViewCameraPivot.localRotation, CalculateLockOnRotation(), lerpAmount * Time.deltaTime);
            thirdViewCameraPivot.localRotation = rotateQuaternion;
        }
        else
        {
            Vector3 rotateValue = new Vector3(lookInputValue.y * -90, lookInputValue.x * 180, rollValue * rollAmount);
            rotateQuaternion = Quaternion.Lerp(thirdViewCameraPivot.localRotation, Quaternion.Euler(rotateValue), lerpAmount * Time.deltaTime);
        }
        thirdViewCameraPivot.localRotation = rotateQuaternion;
        Vector3 adjustPosition = new Vector3(0, pitchValue * pitchAmount - Mathf.Abs(lookValue.y) * 1.5f, -zoomValue * zoomAmount);
        thirdViewCameraPivot.localPosition = thirdPivotOriginPosition + adjustPosition;
    }
    public Camera GetActiveCamera()
    {
        if (GameManager.PlayerAircraft == null) return null;
        return currentCamera;
    }
    public void LockOnTarget(Transform targetTransform)
    {
        lockOnTargetTransform = targetTransform;
    }
    public Quaternion CalculateLockOnRotation()
    {
        Vector3 targetLocalPosition = transform.InverseTransformPoint(lockOnTargetTransform.position);
        Vector3 rotateVector = Quaternion.LookRotation(targetLocalPosition, transform.up).eulerAngles;
        rotateVector.z = 0; // z value must be 0
        return Quaternion.Euler(rotateVector); // Recalculate
    }
    void Update()
    {
        lookValue = Vector2.Lerp(lookValue, lookInputValue, lerpAmount * Time.deltaTime);

        switch ((CameraIndex)cameraViewIndex)
        {
            case CameraIndex.FirstView:
                Rotate1stViewCamera();
                break;
            case CameraIndex.FirstViewWithCockpit:
                Rotate1stViewWithCockpitCamera();
                break;
            case CameraIndex.ThirdView:
                Rotate3rdViewCamera();
                break;
        }
    }
}
