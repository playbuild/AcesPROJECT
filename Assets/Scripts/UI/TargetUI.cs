using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetUI : MonoBehaviour
{
    [SerializeField]
    TargetObject targetObject;

    [Header("Texts")]
    [SerializeField]
    TextMeshProUGUI distanceText;
    [SerializeField]
    TextMeshProUGUI nameText;
    [SerializeField]
    TextMeshProUGUI nicknameText;
    [SerializeField]
    TextMeshProUGUI targetText;

    [Header("Properties")]
    [SerializeField]
    bool isMainTarget;

    [SerializeField]
    float hideDistance;

    [SerializeField]
    GameObject uiObject;
    GameObject blinkUIObject;

    bool isTargetted;
    ObjectInfo objectInfo;
    RectTransform rectTransform;

    public TargetObject Target
    {
        get
        {
            return targetObject;
        }
        set
        {
            targetObject = value;
            objectInfo = targetObject.Info;

            nameText.text = objectInfo.ObjectName;
            nicknameText.text = objectInfo.ObjectNickname;
            targetText.gameObject.SetActive(objectInfo.MainTarget);
        }
    }

    RectTransform canvasRect;
    Camera activeCamera;

    // Recursive search
    Canvas GetCanvas(Transform parentTransform)
    {
        if (parentTransform.GetComponent<Canvas>() != null)
        {
            return parentTransform.GetComponent<Canvas>();
        }
        else
        {
            return GetCanvas(parentTransform.parent);
        }
    }

    public void SetTargetted(bool isTargetted)
    {
        this.isTargetted = isTargetted;

        if (isTargetted == true)
        {
            InvokeRepeating("Blink", 0, 0.5f);
        }
        else
        {
            CancelInvoke("Blink");
        }
    }

    void Blink()
    {
        blinkUIObject.SetActive(!blinkUIObject.activeInHierarchy);
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        Canvas canvas = GetCanvas(transform.parent);
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }

        Target = targetObject;  // execute Setter code
    }

    // Update is called once per frame
    void Update()
    {
        if (targetObject == null)
            return;

        activeCamera = GameManager.CameraController.GetActiveCamera();
        Vector3 screenPosition = activeCamera.WorldToScreenPoint(targetObject.transform.position);
        float distance = GameManager.Instance.GetDistanceFromPlayer(targetObject.transform);

        // if screenPosition.z < 0, the object is behind camera
        if (screenPosition.z > 0)
        {
            // Text
            distanceText.text = string.Format("{0:0}", distance);
            // UI Position
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(activeCamera, targetObject.transform.position);
            rectTransform.anchoredPosition = screenPoint - canvasRect.sizeDelta * 0.5f;
        }

        uiObject.SetActive(distance < hideDistance);
    }
}
