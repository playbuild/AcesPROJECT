using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    public Transform target;
    public float offsetRatio;

    Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        Vector3 targetForwardVector = target.forward;
        targetForwardVector.y = 0;
        targetForwardVector.Normalize();

        Vector3 position = new Vector3(target.transform.position.x, 1, target.transform.position.z)
                           + targetForwardVector * offsetRatio * cam.orthographicSize;
        transform.position = position;
        transform.eulerAngles = new Vector3(90, 0, -target.eulerAngles.y);
    }
}
