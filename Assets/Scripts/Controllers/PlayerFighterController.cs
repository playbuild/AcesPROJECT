using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFighterController : MonoBehaviour
{
    CameraController cameraController;
    UIController uiController;

    float accelerateValue; // ��Ʈ�ѷ��� ������ ��
    float brakeValue;
    float throttle; //����Ʋ ��

    float rollValue;
    float pitchValue;
    float yawQValue;
    float yawEValue;

    public float speed;

    [Header("Aircraft Settings")]
    public float maxSpeed = 301.7f; // �ִ� �ӷ�
    public float minSpeed = 20f; //���� �ӷ�
    public float defaultSpeed = 60f; // �⺻ �ӵ�

    [Header("Move Variables")]
    public float throttleAmount;
    public float accelerateAmount; // ���� 
    public float brakeAmount;
    public float calibrateAmount; // �⺻ �ӵ����� ���� �ӵ�
    public float rollAmount;
    public float pitchAmount;
    public float yawAmount;
    public float rotateLerpAmount;

    //public float lerpAmount;
    //Vector2 rightStickValue;
    Vector3 rotateValue;

    [Header("High-G Turn")]
    [SerializeField]
    float highGFactor = 1.5f;
    [SerializeField]
    float highGTurnTime = 2.0f;

    float highGCooldown;
    float highGReciprocal;
    bool isHighGPressed;
    bool isHighGEnabled;

    bool isHighGTurning;

    [SerializeField]
    List<JetEngineController> jetEngineControllers;

    Rigidbody rb;
    float speedReciprocal; // maxSpeed�� ����

    // public gets
    public float Speed
    {
        get
        {
            return speed;
        }
    }
    public bool IsHighGTurning
    {
        get { return isHighGTurning; }
    }
    public Vector3 RotateValue
    {
        get { return rotateValue; }
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

    public void YawQ(InputAction.CallbackContext context)
    {
        yawQValue = context.ReadValue<float>();
    }

    public void YawE(InputAction.CallbackContext context)
    {
        yawEValue = context.ReadValue<float>();
    }
    public void Accelerate(InputAction.CallbackContext context)
    {
        accelerateValue = context.ReadValue<float>();
    }
    void SetUI()
    {
        uiController.SetAltitude((int)(transform.position.y * 5));
        uiController.SetSpeed((int)(speed * 10));
        uiController.SetThrottle(throttle);
        uiController.SetHeading(transform.eulerAngles.y);
    }
    void CheckHighGTurn(ref float accel, ref float brake, ref float highGPitchFactor)
    {
        isHighGTurning = false;

        // Factor decreases 2 to 1
        if (accelerateValue == 1 && brakeValue == 1) // Button
        {
            if (pitchValue < -0.7f)
            {
                if (isHighGEnabled == true)
                {
                    isHighGEnabled = false;
                    isHighGPressed = true;
                }

                if (highGCooldown < 0) isHighGPressed = false;

                if (isHighGEnabled == true || isHighGPressed == true)
                {
                    accel = 0;
                    brake *= highGFactor * (1 + highGCooldown * highGReciprocal);
                    highGPitchFactor = highGFactor * (1 + highGCooldown * highGReciprocal);

                    highGCooldown -= Time.deltaTime;
                    isHighGTurning = true;
                }
            }
        }
        else // Button Released
        {
            isHighGPressed = false;
            isHighGEnabled = true;
        }

        if (isHighGPressed == false)
        {
            highGCooldown += Time.deltaTime * 2;
            if (highGCooldown >= highGTurnTime)
            {
                highGCooldown = highGTurnTime;
            }
        }
    }

    void Autopilot(out Vector3 rotateVector)
    {
        rotateVector = -transform.rotation.eulerAngles;
        if (rotateVector.x < -180) rotateVector.x += 360;
        if (rotateVector.z < -180) rotateVector.z += 360;

        rotateVector.x = Mathf.Clamp(rotateVector.x * 2, -pitchAmount, pitchAmount);
        rotateVector.z = Mathf.Clamp(rotateVector.z * 2, -rollAmount, rollAmount);
        rotateVector.y = 0;
    }

    void MoveAircraft()
    {
        float accel = accelerateValue;
        float brake = brakeValue;
        float highGPitchFactor = 1;

        // High-G Turn
        CheckHighGTurn(ref accel, ref brake, ref highGPitchFactor);
        // === Rotation ===
        Vector3 rotateVector;

        // �������Ϸ� (Press Q + E)
        if (yawQValue == 1 && yawEValue == 1)
        {
            Autopilot(out rotateVector);
        }
        // ����� ȸ��
        else
        {
            rotateVector = new Vector3(pitchValue * pitchAmount * highGPitchFactor, (yawEValue - yawQValue) * yawAmount, -rollValue * rollAmount);
        }
        rotateValue = Vector3.Lerp(rotateValue, rotateVector, rotateLerpAmount * Time.deltaTime);
        //transform.Rotate(rotateValue * Time.deltaTime);

        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotateValue * Time.fixedDeltaTime));
        //����� ����
        //rb.velocity = transform.forward * speed * Time.fixedDeltaTime;

        throttle = Mathf.Lerp(throttle, accelerateValue - brakeValue, throttleAmount * Time.deltaTime);

        //����� ����
        if (throttle > 0)
        {
            float accelEase = (maxSpeed + (transform.position.y * 0.01f) - speed) * speedReciprocal;
            speed += throttle * accelerateAmount * accelEase * Time.fixedDeltaTime;
        }
        else if (throttle < 0)
        {
            //����� ����
            float brakeEase = (speed - minSpeed) * speedReciprocal;
            speed += throttle * brakeAmount * brakeEase * Time.fixedDeltaTime;
        }
        //�⺻ �ӵ��� ����
        float release = 1 - Mathf.Abs(throttle);
        speed += release * (defaultSpeed - speed) * speedReciprocal * calibrateAmount * Time.deltaTime;

        rb.velocity = transform.forward * speed;

        transform.Translate(new Vector3(0, 0, speed * Time.deltaTime));
    }

    void JetEngineControl()
    {
        foreach (JetEngineController jet in jetEngineControllers)
        {
            jet.InputValue = throttle;
        }
    }
    void Start()
    {
        uiController = GameManager.UIController;

        accelerateValue = 0;
        brakeValue = 0;
        rollValue = 0;
        pitchValue = 0;
        yawEValue = 0;
        yawQValue = 0;

        highGCooldown = highGTurnTime;
        highGReciprocal = 1 / highGCooldown;
        isHighGPressed = false;
        isHighGEnabled = true;

        rb = GetComponent<Rigidbody>();

        cameraController = GetComponent<CameraController>();
    }
    void Update()
    {
        SetUI();
        JetEngineControl();
    }
}
