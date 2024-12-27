using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftAI : MonoBehaviour
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

    //[SerializeField]
    //float speedLerpAmount;

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

    [SerializeField]
    List<Transform> initialWaypoints;
    Queue<Transform> waypointQueue;

    //Transform currentWaypoint;

    float prevWaypointDistance;
    float waypointDistance;
    bool isComingClose;

    float prevRotY;
    float currRotY;
    float rotateAmount;
    float zRotateValue;

    [SerializeField]
    float newWaypointDistance;
    [SerializeField]
    float waypointMinHeight;
    [SerializeField]
    float waypointMaxHeight;

    [SerializeField]
    BoxCollider areaCollider;

    Vector3 currentWaypoint;

    [SerializeField]
    GameObject waypointObject;

    public static Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
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
    void CreateWaypoint()
    {
        float distance = Random.Range(newWaypointDistance * 0.7f, newWaypointDistance);
        float height = Random.Range(waypointMinHeight, waypointMaxHeight);
        float angle = Random.Range(0, 360);
        Vector3 directionVector = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
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
            waypointPosition.y += height + hit.distance;
        }

        Instantiate(waypointObject, waypointPosition, Quaternion.identity);

        currentWaypoint = waypointPosition;
    }

    void ChangeWaypoint()
    {
        if (waypointQueue.Count == 0)
        {
            CreateWaypoint();
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


    void Start()
    {
        speed = targetSpeed = defaultSpeed;
        accelerateReciprocal = 1 / accelerateAmount;

        currentTurningForce = turningForce;
        turningTime = 1 / turningForce;
        currentTurningTime = turningTime;

        waypointQueue = new Queue<Transform>();
        foreach (Transform t in initialWaypoints)
        {
            waypointQueue.Enqueue(t);
        }
        for (int i = 0; i < 50; i++)
        {
            CreateWaypoint();
        }
        ChangeWaypoint();
    }

    void Update()
    {
        CheckWaypoint();
        ZAxisRotate();
        Rotate();

        AdjustSpeed();
        Move();

        //currentTurningTime = Mathf.Lerp(currentTurningTime, turningTime, 1);
    }
}
