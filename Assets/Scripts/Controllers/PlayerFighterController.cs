using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFighterController : MonoBehaviour
{

    float accelerateValue; // 컨트롤러로 얻어오는 값
    float brakeValue;
    float throttle; //쓰로틀 값

    float rollValue;
    float pitchValue;
    float yawQValue;
    float yawEValue;

    float speed;

    [Header("Aircraft Settings")]
    [SerializeField]
    float maxSpeed = 301.7f;
    [SerializeField]
    float minSpeed = 15;
    [SerializeField]
    float defaultSpeed = 60;

    [Header("Move Variables")]
    [SerializeField]
    float throttleAmount;

    [SerializeField]
    float accelerateAmount;
    [SerializeField]
    float brakeAmount;
    [SerializeField]
    float calibrateAmount;  // Speed Calibration

    [SerializeField]
    float rollAmount;
    [SerializeField]
    float pitchAmount;
    [SerializeField]
    float yawAmount;

    [SerializeField]
    float rotateLerpAmount;

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

    [Header("Misc.")]
    [SerializeField]
    float stallSpeed;
    [SerializeField]
    float gravityFactor;

    [SerializeField]
    List<JetEngineController> jetEngineControllers;

    Rigidbody rb;
    float speedReciprocal; // maxSpeed의 역수
    Vector3 rotateReciprocal;

    CameraController cameraController;
    UIController uiController;

    // public gets
    public float Speed
    {
        get { return speed; }
    }

    public bool IsHighGTurning
    {
        get { return isHighGTurning; }
    }
    public Vector3 RotateValue
    {
        get { return rotateValue; }
    }
    bool isAutoPilot;
    public bool IsAutoPilot
    {
        get { return isAutoPilot; }
    }
    bool isStalling;
    public bool IsStalling
    {
        get { return isStalling; }
    }

    public void Accelerate(InputAction.CallbackContext context)
    {
        accelerateValue = context.ReadValue<float>();
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
    void Stall()
    {
        Quaternion targetRotation = Quaternion.Euler(90, transform.eulerAngles.y, transform.eulerAngles.z);
        Quaternion diffQuaternion = Quaternion.Inverse(transform.rotation) * targetRotation;

        Vector3 diffAngle = diffQuaternion.eulerAngles;

        // Adjustment
        if (diffAngle.x > 180) diffAngle.x -= 360;
        if (diffAngle.y > 180) diffAngle.y -= 360;
        if (diffAngle.z > 180) diffAngle.z -= 360;
        diffAngle.x = Mathf.Clamp(diffAngle.x, -pitchAmount, pitchAmount);
        diffAngle.y = Mathf.Clamp(diffAngle.y, -yawAmount, yawAmount);
        diffAngle.z = Mathf.Clamp(diffAngle.z, -rollAmount, rollAmount);

        rotateValue = Vector3.Lerp(rotateValue, diffAngle, rotateLerpAmount * Time.deltaTime);
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
        if (speed < stallSpeed)
        {
            // Ignore all rotation input and head to the ground
            isStalling = true;
            Stall();
        }
        else
        {
            isStalling = false;

            // 오토파일럿 (Press Q + E)
            if (yawQValue == 1 && yawEValue == 1)
            {
                isAutoPilot = true;
                Autopilot(out rotateVector);
            }
            // 비행기 회전
            else
            {
                isAutoPilot = false;
                rotateVector = new Vector3(pitchValue * pitchAmount * highGPitchFactor, (yawEValue - yawQValue) * yawAmount, -rollValue * rollAmount);
            }
            rotateValue = Vector3.Lerp(rotateValue, rotateVector, rotateLerpAmount * Time.fixedDeltaTime);
        }

        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotateValue * Time.fixedDeltaTime));

        //비행기 전진
        throttle = Mathf.Lerp(throttle, accelerateValue - brakeValue, throttleAmount * Time.deltaTime);

        //비행기 가속
        if (throttle > 0)
        {
            float accelEase = (maxSpeed + (transform.position.y * 0.01f) - speed) * speedReciprocal;
            speed += throttle * accelerateAmount * accelEase * Time.fixedDeltaTime;
        }
        else if (throttle < 0)
        {
            //비행기 감속
            float brakeEase = (speed - minSpeed) * speedReciprocal;
            speed += throttle * brakeAmount * brakeEase * Time.fixedDeltaTime;
        }
        //기본 속도로 복구
        float release = 1 - Mathf.Abs(throttle);
        speed += release * (defaultSpeed - speed) * speedReciprocal * calibrateAmount * Time.fixedDeltaTime;

        // Gravity
        float gravityFallByPitch = gravityFactor * Mathf.Sin(transform.eulerAngles.x * Mathf.Deg2Rad);
        speed += gravityFallByPitch * Time.fixedDeltaTime;

        rb.velocity = transform.forward * speed;
    }
    void PassCameraControl()
    {
        float zoomValue = accelerateValue - brakeValue;
        cameraController.AdjustCameraValue(zoomValue, rollValue, pitchValue);
    }
    void JetEngineControl()
    {
        foreach (JetEngineController jet in jetEngineControllers)
        {
            jet.InputValue = throttle;
        }
    }
    void OnDisable()
    {
        foreach (JetEngineController jet in jetEngineControllers)
        {
            jet.enabled = false;
        }
        CapsuleCollider[] colliders = GetComponents<CapsuleCollider>();
        foreach (CapsuleCollider collider in colliders)
        {
            collider.enabled = false;
        }
    }
    void OnEnable()
    {
        foreach (JetEngineController jet in jetEngineControllers)
        {
            jet.enabled = true;
        }
        CapsuleCollider[] colliders = GetComponents<CapsuleCollider>();
        foreach (CapsuleCollider collider in colliders)
        {
            collider.enabled = true;
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

        speedReciprocal = 1 / maxSpeed;
        speed = defaultSpeed;
    }
    void Update()
    {
        SetUI();
        JetEngineControl();
    }
    void FixedUpdate()
    {
        MoveAircraft();
        PassCameraControl();
    }
}

