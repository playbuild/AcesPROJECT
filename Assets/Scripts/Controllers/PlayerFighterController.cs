using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFighterController : MonoBehaviour
{
    CameraController cameraController;
    UIController uiController;

    public float speed;
    public float Speed
    {
        get
        {
            return speed;
        }
    }

    public float rollAmount;
    public float pitchAmount;
    public float yawAmount;
    public float accelerateAmount; // 가속 
    public float brakeAmount;

    public float lerpAmount;

    public float calibrateAmount; // 기본 속도로의 복구 속도
    public float maxSpeed = 301.7f; // 최대 속력
    public float minSpeed = 20f; //최저 속력
    public float defaultSpeed = 60f; // 기본 속도

    float rollValue;
    float pitchValue;
    float yawValue;
    float accelerateValue; // 컨트롤러로 얻어오는 값
    float brakeValue;

    float speedReciprocal; // maxSpeed의 역수

    Vector2 rightStickValue;
    Vector3 rotateValue;
    Rigidbody rb;
    void MoveAircraft()
    {
        //비행기 회전
        Vector3 lerpVector = new Vector3(pitchValue * pitchAmount, yawValue * yawAmount, -rollValue * rollAmount);
        rotateValue = Vector3.Lerp(rotateValue, lerpVector, lerpAmount * Time.fixedDeltaTime);

        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotateValue * Time.fixedDeltaTime));
        //비행기 전진
        rb.velocity = transform.forward * speed * Time.fixedDeltaTime;
        //비행기 가속
        float accelEase = (maxSpeed - speed) * speedReciprocal;
        speed += accelerateValue * accelerateAmount * accelEase * Time.fixedDeltaTime;
        //비행기 감속
        float brakeEase = (speed - minSpeed) * speedReciprocal;
        speed -= brakeValue * brakeAmount * brakeEase * Time.fixedDeltaTime;
        //기본 속도로 복구
        if (accelerateValue == 0 && brakeValue == 0)
        {
            speed += (defaultSpeed - speed) * speedReciprocal * calibrateAmount * Time.fixedDeltaTime;
        }

        rb.velocity = transform.forward * speed;
    }

    void Start()
    {
        uiController = GameManager.UIController;

        rb = GetComponent<Rigidbody>();

        cameraController = GetComponent<CameraController>();
    }

    void FixedUpdate()
    {
        MoveAircraft();
        PassCameraControl();
    }
    void PassCameraControl()
    {
        float zoomValue = accelerateValue - brakeValue;
        cameraController.AdjustCameraValue(zoomValue, rollValue, pitchValue);
    }

    void OnEnable()
    {
        speedReciprocal = 1 / maxSpeed;
    }

    public void Brake(InputAction.CallbackContext context)
    {
        brakeValue = context.ReadValue<float>();
    }

    public void Roll(InputAction.CallbackContext context)
    {
        rollValue = context.ReadValue<float>();
    }
    public void Pitch(InputAction.CallbackContext context)
    {
        pitchValue = context.ReadValue<float>();
    }

    public void Yaw(InputAction.CallbackContext context)
    {
        yawValue = context.ReadValue<float>();
    }
    public void Accelerate(InputAction.CallbackContext context)
    {
        accelerateValue = context.ReadValue<float>();
    }
    void Update()
    {
        uiController.SetAltitude((int)(transform.position.y * 5));
        uiController.SetSpeed((int)(speed * 10));
    }
}
