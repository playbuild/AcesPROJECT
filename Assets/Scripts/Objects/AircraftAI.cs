using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftAI : TargetObject
{
    [Header("Aircraft Settings")]
    [SerializeField]
    float maxSpeed;
    [SerializeField]
    float minSpeed;
    [SerializeField]
    float defaultSpeed;

    float speed;
    float targetSpeed;
    bool isAcceleration;

    [Header("Accel/Rotate Values")]
    [SerializeField]
    float accelerateLerpAmount = 1.0f;
    [SerializeField]
    float accelerateAmount = 50.0f;
    float currentAccelerate;
    float accelerateReciprocal;

    [SerializeField]
    float turningForce;
    float currentTurningForce;

    [Header("Z Rotate Values")]
    [SerializeField]
    float zRotateMaxThreshold = 0.5f;
    [SerializeField]
    float zRotateAmount = 90;
    [SerializeField]
    float zRotateLerpAmount;

    float turningTime;
    float currentTurningTime;

    [Header("Waypoint")]
    [SerializeField]
    List<Transform> initialWaypoints;
    Queue<Transform> waypointQueue;

    [SerializeField]
    float waypointMinHeight;
    [SerializeField]
    float waypointMaxHeight;

    [SerializeField]
    BoxCollider areaCollider;

    Vector3 currentWaypoint;

    float prevWaypointDistance;
    float waypointDistance;
    bool isComingClose;

    float prevRotY;
    float currRotY;
    float rotateAmount;
    float zRotateValue;

    [Header("Misc.")]
    [SerializeField]
    [Range(0, 1)]
    float evasionRate = 0.5f;

    [SerializeField]
    float newWaypointDistance = 500;

    [SerializeField]
    List<JetEngineController> jetEngineControllers;

    [SerializeField]
    GameObject waypointObject;

    public float Speed
    {
        get { return speed; }
    }
    public void ForceChangeWaypoint(Vector3 waypoint)
    {
        currentWaypoint = waypoint;
        Invoke("Phase3ChangeWaypoint", 0.5f);
    }
    void Phase3ChangeWaypoint()
    {
        currentWaypoint = GameManager.PlayerAircraft.transform.position;
    }

    public static Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
    protected virtual Vector3 CreateWaypoint()
    {
        if (areaCollider != null)
        {
            return CreateWaypointWithinArea();
        }

        else
        {
            return CreateWaypointAroundItself();
        }
    }
    void RandomizeSpeedAndTurn()
    {
        // Speed
        targetSpeed = Random.Range(minSpeed, maxSpeed);
        isAcceleration = (speed < targetSpeed);

        // TurningForce
        currentTurningForce = Random.Range(0.5f * turningForce, turningForce);
        turningTime = 1 / currentTurningForce;
    }
    Vector3 CreateWaypointWithinArea()
    {
        if (areaCollider == null) return currentWaypoint;

        float height = Random.Range(waypointMinHeight, waypointMaxHeight);
        Vector3 waypointPosition = RandomPointInBounds(areaCollider.bounds);

        RaycastHit hit;
        Physics.Raycast(waypointPosition, Vector3.down, out hit);

        if (hit.distance != 0)
        {
            waypointPosition.y += height - hit.distance;
        }
        // New waypoint is below ground
        else
        {
            Physics.Raycast(waypointPosition, Vector3.up, out hit);

            if (hit.distance == 0)
            {
                waypointPosition.y = height;
            }
            else
            {
                waypointPosition.y += height + hit.distance;
            }
        }

        return waypointPosition;
    }

    Vector3 CreateWaypointAroundItself()
    {
        float distance = Random.Range(newWaypointDistance * 0.7f, newWaypointDistance);
        float height = Random.Range(waypointMinHeight, waypointMaxHeight);
        float angle = Random.Range(0, 360);
        Vector3 directionVector = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
        Vector3 waypointPosition = transform.position + directionVector * distance;
        Vector3 raycastPosition = waypointPosition;
        raycastPosition.y = 5000;

        RaycastHit hit;
        Physics.Raycast(raycastPosition, Vector3.down, out hit);

        if (hit.distance != 0)
        {
            waypointPosition.y += height - (5000 - hit.distance);
        }
        // New waypoint is outside of the map
        else
        {
            waypointPosition.y = height;
        }

        return waypointPosition;
    }

    protected void ChangeWaypoint()
    {
        if (waypointQueue.Count == 0)
        {
            currentWaypoint = CreateWaypoint();
        }
        else
        {
            currentWaypoint = waypointQueue.Dequeue().position;
        }

        waypointDistance = Vector3.Distance(transform.position, currentWaypoint);
        prevWaypointDistance = waypointDistance;
        isComingClose = false;

        RandomizeSpeedAndTurn();
    }

    void CheckWaypoint()
    {
        if (currentWaypoint == null) return;
        waypointDistance = Vector3.Distance(transform.position, currentWaypoint);

        if (waypointDistance >= prevWaypointDistance) // Aircraft is going farther from the waypoint
        {
            if (isComingClose == true)
            {
                ChangeWaypoint();
            }
        }
        else
        {
            isComingClose = true;
        }

        prevWaypointDistance = waypointDistance;
    }

    void Rotate()
    {
        if (currentWaypoint == null)
            return;

        Vector3 targetDir = currentWaypoint - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(targetDir);

        float delta = Quaternion.Angle(transform.rotation, lookRotation);
        if (delta > 0f)
        {
            float lerpAmount = Mathf.SmoothDampAngle(delta, 0.0f, ref rotateAmount, currentTurningTime);
            lerpAmount = 1.0f - (lerpAmount / delta);

            Vector3 eulerAngle = lookRotation.eulerAngles;
            eulerAngle.z += zRotateValue * zRotateAmount;
            lookRotation = Quaternion.Euler(eulerAngle);

            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, lerpAmount);
        }
    }

    void ZAxisRotate()
    {
        currRotY = transform.eulerAngles.y;
        float diff = prevRotY - currRotY;

        if (diff > 180) diff -= 360;
        if (diff < -180) diff += 360;

        prevRotY = transform.eulerAngles.y;
        zRotateValue = Mathf.Lerp(zRotateValue, Mathf.Clamp(diff / zRotateMaxThreshold, -1, 1), zRotateLerpAmount * Time.fixedDeltaTime);
    }
    void AdjustSpeed()
    {
        currentAccelerate = 0;
        if (isAcceleration == true && speed < targetSpeed)
        {
            currentAccelerate = accelerateAmount;
        }
        else if (isAcceleration == false && speed > targetSpeed)
        {
            currentAccelerate = -accelerateAmount;
        }
        speed += currentAccelerate * Time.deltaTime;

        currentTurningTime = Mathf.Lerp(currentTurningTime, turningTime, 1);
    }
    void Move()
    {
        transform.Translate(new Vector3(0, 0, speed) * Time.deltaTime);
    }
    void JetEngineControl()
    {
        foreach (JetEngineController jet in jetEngineControllers)
        {
            jet.InputValue = currentAccelerate * accelerateReciprocal;
        }
    }
    public override void OnMissileAlert()
    {
        float rate = Random.Range(0.0f, 1.0f);
        if (rate <= evasionRate)
        {
            ChangeWaypoint();
        }
    }


    protected override void Start()
    {
        base.Start();
        {
            speed = targetSpeed = defaultSpeed;

            accelerateReciprocal = 1 / accelerateAmount;

            currentTurningForce = turningForce;
            currentTurningTime = turningTime = 1 / turningForce;

            waypointQueue = new Queue<Transform>();

            if (initialWaypoints.Count > 0)
            {
                foreach (Transform t in initialWaypoints)
                {
                    waypointQueue.Enqueue(t);
                }
            }
            ChangeWaypoint();
        }
    }

    protected virtual void Update()
    {
            CheckWaypoint();
            JetEngineControl();
            ZAxisRotate();
            Rotate();

            AdjustSpeed();
            Move();
            CheckMissileDistance();
            //currentTurningTime = Mathf.Lerp(currentTurningTime, turningTime, 1);
        }
    }
