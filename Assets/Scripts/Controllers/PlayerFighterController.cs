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
    public float accelerateAmount; // ���� 
    public float brakeAmount;

    public float lerpAmount;

    public float calibrateAmount; // �⺻ �ӵ����� ���� �ӵ�
    public float maxSpeed = 301.7f; // �ִ� �ӷ�
    public float minSpeed = 20f; //���� �ӷ�
    public float defaultSpeed = 60f; // �⺻ �ӵ�

    float rollValue;
    float pitchValue;
    float yawValue;
    float accelerateValue; // ��Ʈ�ѷ��� ������ ��
    float brakeValue;

    float speedReciprocal; // maxSpeed�� ����

    Vector2 rightStickValue;
    Vector3 rotateValue;
    Rigidbody rb;
    void MoveAircraft()
    {
        //����� ȸ��
        Vector3 lerpVector = new Vector3(pitchValue * pitchAmount, yawValue * yawAmount, -rollValue * rollAmount);
        rotateValue = Vector3.Lerp(rotateValue, lerpVector, lerpAmount * Time.fixedDeltaTime);

        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotateValue * Time.fixedDeltaTime));
        //����� ����
        rb.velocity = transform.forward * speed * Time.fixedDeltaTime;
        //����� ����
        float accelEase = (maxSpeed - speed) * speedReciprocal;
        speed += accelerateValue * accelerateAmount * accelEase * Time.fixedDeltaTime;
        //����� ����
        float brakeEase = (speed - minSpeed) * speedReciprocal;
        speed -= brakeValue * brakeAmount * brakeEase * Time.fixedDeltaTime;
        //�⺻ �ӵ��� ����
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
