using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float sensitivityX = 1.0f;
    public float sensitivityY = 1.0f;
    public float rotationSpeed = 10f;
    public enum CameraIndex
    {
        ThirdView,
        FirstView,
        FirstViewWithCockpit,
    }

    Vector2 lookInputValue;
    Vector2 lookValue;
    Vector3 thirdPivotOriginPosition;

    [Header("Camera Objects")]
    public Camera[] cameras = new Camera[3];
    public Transform cameraPivot;
    public Transform thirdViewCameraPivot;

    Camera currentCamera;

    int cameraViewIndex = 0;

    [Header("Camera Lerp")]
    public float lerpAmount;

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

    [SerializeField]
    JetEngineController jetEngineController;

    [Header("Sounds")]
    [SerializeField]
    AudioClip engineInClip;
    [SerializeField]
    AudioClip engineOutClip;

    [SerializeField]
    AudioSource engineAudioSource;

    UIController uiController;
    public Transform targetArrowTransform;

    public void Look(InputAction.CallbackContext context)
    {
        lookInputValue = context.ReadValue<Vector2>();
    }

    public void ChangeCameraView(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            lookValue = Vector3.zero;
            cameraViewIndex = (++cameraViewIndex) % 3;
            SetCamera();
            SetEngineAudio();
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
        Quaternion rotateQuaternion;

        if (lockOnTargetTransform != null)
        {
            rotateQuaternion = Quaternion.Lerp(thirdViewCameraPivot.localRotation, CalculateLockOnRotation(), lerpAmount * Time.fixedDeltaTime);
        }
        else
        {
            Vector3 rotateValue = new Vector3(lookInputValue.y * -90, lookInputValue.x * 180, rollValue * rollAmount);
            rotateQuaternion = Quaternion.Lerp(thirdViewCameraPivot.localRotation, Quaternion.Euler(rotateValue), lerpAmount * Time.fixedDeltaTime);
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
    void SetEngineAudio()
    {
        switch ((CameraIndex)cameraViewIndex)
        {
            case CameraIndex.FirstView:
                engineAudioSource.clip = engineInClip;
                engineAudioSource.Play();
                jetEngineController.SetAudioEffect(true);
                break;

            case CameraIndex.ThirdView:
                engineAudioSource.clip = engineOutClip;
                engineAudioSource.Play();
                jetEngineController.SetAudioEffect(false);
                break;

            default:
                break;
        }
    }
    void Start()
    {
        thirdPivotOriginPosition = thirdViewCameraPivot.localPosition;
        uiController = GameManager.UIController;
        SetCamera();
    }
    void FixedUpdate()
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
