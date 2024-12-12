using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class MinimapCamera : MonoBehaviour
{
    public enum MinimapIndex
    {
        Small,
        Large,
        All
    }

    public Transform target;
    public GameObject playerIcon;
    public float offsetRatio;

    public float smallViewSize;
    public float largeViewSize;
    public float allViewSize;

    Camera cam;
    Vector2 size;
    public Transform indicator;
    public float indicatorSize;
    float sizeReciprocal;
    int minimapIndex;

    public GameObject[] minimaps = new GameObject[3];

    public void ChangeMinimapView(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            minimapIndex = (++minimapIndex) % 3;
            SetCamera();
        }
    }
    public void SetCamera()
    {
        switch ((MinimapIndex)minimapIndex)
        {
            case MinimapIndex.Small:
                cam.orthographicSize = smallViewSize;
                cam.cullingMask &= (1 << LayerMask.NameToLayer("Minimap"));
                break;

            case MinimapIndex.Large:
                cam.orthographicSize = largeViewSize;
                cam.cullingMask |= (1 << LayerMask.NameToLayer("Minimap (Player)"));
                break;

            case MinimapIndex.All:
                cam.orthographicSize = allViewSize;
                break;
        }
        size = new Vector2(cam.orthographicSize, cam.orthographicSize * cam.aspect);

        for (int i = 0; i < minimaps.Length; i++)
        {
            minimaps[i].gameObject.SetActive(i == minimapIndex);
        }
    }
    public void ShowBorderIndicator(Vector3 position)
    {
        Debug.Log("debug");
        float reciprocal;
        float rotation;
        Vector2 distance = new Vector3(transform.position.x - position.x, transform.position.z - position.z);

        // When the x, z positions are same
        if (distance.x == 0 || distance.y == 0)
            return;

        if ((MinimapIndex)minimapIndex == MinimapIndex.Small)
        {
            distance = Quaternion.Euler(0, 0, target.eulerAngles.y) * distance;
        }

        // X axis
        if (Mathf.Abs(distance.x) > Mathf.Abs(distance.y))
        {
            reciprocal = -Mathf.Abs(size.x / distance.x);
            rotation = (distance.x > 0) ? 90 : -90;
        }
        // Y axis
        else
        {
            reciprocal = -Mathf.Abs(size.y / distance.y);
            rotation = (distance.y > 0) ? 180 : 0;
        }
        float scale = sizeReciprocal * GetCameraViewSize();

        indicator.localScale = new Vector3(scale, scale, scale);
        indicator.localPosition = new Vector3(distance.x * reciprocal, distance.y * reciprocal, 1);
        indicator.localEulerAngles = new Vector3(0, 0, rotation);

        if (indicator.gameObject.activeInHierarchy == false)
        {
            indicator.gameObject.SetActive(true);
        }
    }

    public void HideBorderIncitator()
    {
        indicator.gameObject.SetActive(false);
    }
    public float GetCameraViewSize()
    {
        return cam.orthographicSize;
    }
    void Awake()
    {
        minimapIndex = (int)MinimapIndex.Small;
        cam = GetComponent<Camera>();
        SetCamera();
        sizeReciprocal = indicatorSize / GetCameraViewSize();
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 targetForwardVector = target.forward;
        targetForwardVector.y = 0;
        targetForwardVector.Normalize();

        Vector3 position;
        float cameraRotation;

        if (minimapIndex == (int)MinimapIndex.Small)
        {
            position = new Vector3(target.transform.position.x, 1, target.transform.position.z)
                           + targetForwardVector * offsetRatio * cam.orthographicSize;
            cameraRotation = -target.eulerAngles.y;
        }
        else
        {
            if (minimapIndex == (int)MinimapIndex.Large)
            {
                position = new Vector3(target.transform.position.x, 1, target.transform.position.z);
            }
            else
            {
                position = new Vector3(0, 1, 0);
            }
            cameraRotation = 0;
        }
        transform.position = position;
        transform.eulerAngles = new Vector3(90, 0, cameraRotation);
    }
}
