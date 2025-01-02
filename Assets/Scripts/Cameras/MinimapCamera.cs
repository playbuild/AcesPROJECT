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

    public Camera cam;

    [Header("Follow Target / Small View Offset Ratio")]
    public Transform target;
    public float offsetRatio;

    [Header("Minimap sizes")]
    public float smallViewSize;
    public float smallZoomViewSize;
    public float largeViewSize;
    public float allViewSize;

    public float zoomDistanceThreshold;
    public float zoomLerpAmount;
    bool isZooming;

    [Header("Icon resize factors (small = 1)")]
    public float largeViewIconResizeFactor;
    public float allViewIconResizeFactor;

    [Header("Minimap UI")]
    public GameObject[] minimaps = new GameObject[3];

    Vector2 cameraSize;
    float sizeReciprocal;
    int minimapIndex;

    public MinimapIndex GetMinimapIndex()
    {
        return (MinimapIndex)minimapIndex;
    }

    public Vector2 CameraSize
    {
        get { return cameraSize; }
    }
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
                cam.cullingMask |= (1 << LayerMask.NameToLayer("Minimap(Player)"));
                break;

            case MinimapIndex.All:
                cam.orthographicSize = allViewSize;
                cam.cullingMask |= (1 << LayerMask.NameToLayer("Minimap(Player)"));
                break;
        }

        for (int i = 0; i < minimaps.Length; i++)
        {
            minimaps[i].gameObject.SetActive(i == minimapIndex);
        }

        cameraSize = new Vector2(cam.orthographicSize, cam.orthographicSize * cam.aspect);
    }
    public float GetInitCameraViewSize()
    {
        return cam.orthographicSize;
    }
    public float GetIconResizeFactor()
    {
        float size = cam.orthographicSize;

        switch ((MinimapIndex)minimapIndex)
        {
            case MinimapIndex.Large:
                return size * largeViewIconResizeFactor;

            case MinimapIndex.All:
                return size * allViewIconResizeFactor;

            default:
                return size;
        }
    }
    void AdjustCameraTransform()
    {
        if (target == null) return;

        Vector3 position;
        float cameraRotation;

        if (minimapIndex == (int)MinimapIndex.Small)
        {
            Vector3 targetForwardVector = target.forward;
            targetForwardVector.y = 0;
            targetForwardVector.Normalize();

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
                position = transform.position;
                position.y = 1;
            }
            cameraRotation = 0;
        }
        cam.transform.position = position;
        cam.transform.eulerAngles = new Vector3(90, 0, cameraRotation);
    }
    public void SetZoom(float distance)
    {
        if (distance < zoomDistanceThreshold)
        {
            isZooming = true;
        }
        else if (distance > zoomDistanceThreshold * 1.5f)
        {
            isZooming = false;
        }
    }
    void ZoomCamera()
    {
        float size = (isZooming == true) ? smallZoomViewSize : smallViewSize;
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, size, zoomLerpAmount * Time.deltaTime);
        cameraSize = new Vector2(cam.orthographicSize, cam.orthographicSize * cam.aspect);
    }
    void Awake()
    {
        minimapIndex = (int)MinimapIndex.Small;
        SetCamera();
        isZooming = false;
    }
    // Update is called once per frame
    void Update()
    {
        AdjustCameraTransform();

        if (minimapIndex == (int)MinimapIndex.Small)
        {
            ZoomCamera();
        }
    }
}
