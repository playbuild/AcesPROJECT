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
        Vector3 rotateValue = new Vector3(lookValue.y * -90, lookValue.x * 180, 0);
        cameraPivot.localEulerAngles = rotateValue;
        uiController.AdjustFirstViewUI(rotateValue);
    }

    void Rotate1stViewWithCockpitCamera()
    {
        Vector3 rotateValue = new Vector3(lookValue.y * -90, lookValue.x * 135, 0);
        if (rotateValue.x > 0)
            rotateValue.x *= 0.3f;

        cameraPivot.localEulerAngles = rotateValue;
        uiController.AdjustFirstViewUI(rotateValue);
    }

    void Rotate3rdViewCamera()
    {
        Transform cameraTransform = currentCamera.transform;

        Vector3 rotateValue = new Vector3(lookValue.y * -90, lookValue.x * 180, rollValue * rollAmount);
        Vector3 adjustPosition = new Vector3(0, pitchValue * pitchAmount - Mathf.Abs(lookValue.y), -zoomValue * zoomAmount);

        thirdViewCameraPivot.localEulerAngles = rotateValue;
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
