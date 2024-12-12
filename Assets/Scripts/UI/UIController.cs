using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("1st-3rd View Control")]
    public RectTransform commonCenterUI;
    public RectTransform firstCenterViewTransform;
    public RectTransform thirdCenterViewTransform;

    public Canvas firstViewCanvas;
    public Vector2 firstViewAdjustAngle;

    // Center
    [Header("Common Center UI")]
    [SerializeField]
    TextMeshProUGUI speedText;
    [SerializeField]
    TextMeshProUGUI altitudeText;

    // Upper Left
    [Header("Upper Left UI")]
    [SerializeField]
    TextMeshProUGUI timeText;
    [SerializeField]
    TextMeshProUGUI scoreText;
    [SerializeField]
    TextMeshProUGUI targetText;

    // Lower Right
    [Header("Lower Right UI : Armament")]
    [SerializeField]
    TextMeshProUGUI gunText;
    [SerializeField]
    TextMeshProUGUI mslText;
    [SerializeField]
    TextMeshProUGUI spwText;

    [SerializeField]
    TextMeshProUGUI dmgText;

    [SerializeField]
    GameObject mslIndicator;
    [SerializeField]
    GameObject spwIndicator;

    // Status
    [Header("Lower Right UI : Aircraft/Weapon Status")]
    [SerializeField]
    Image aircraftImage;
    [SerializeField]
    CooldownImage leftMslCooldownImage;
    [SerializeField]
    CooldownImage rightMslCooldownImage;

    [Header("UV Status")]
    public UVController speedUV;
    public UVController altitudeUV;
    public Image throttleGauge;
    [SerializeField]
    HeadingUIController headingUIController;

    public void SetSpeed(int speed)
    {
        string text = string.Format("<mspace=18>{0}</mspace>", speed);
        speedText.text = text;

        speedUV.SetUV(speed);
    }
    public void SetAltitude(int altitude)
    {
        string text = string.Format("<mspace=18>{0}</mspace>", altitude);
        altitudeText.text = text;

        altitudeUV.SetUV(altitude);
    }
    public void SetGunText(int bullets)
    {
        string text = string.Format("<align=left>GUN<line-height=0>\n<align=right><mspace=18>{0}</mspace><line-height=0>", bullets);
        gunText.text = text;
    }
    public void SetMissileText(int missiles)
    {
        string text = string.Format("<align=left>MSL<line-height=0>\n<align=right><mspace=18>{0}</mspace><line-height=0>", missiles);
        mslText.text = text;
    }
    public void SetSpecialWeaponText(string specialWeaponName, int specialWeapons)
    {
        string text = string.Format("<align=left>{0}<line-height=0>\n<align=right><mspace=18>{1}</mspace><line-height=0>", specialWeaponName, specialWeapons);
        spwText.text = text;
    }
    public void SwitchWeapon(WeaponSlot[] weaponSlots, bool useSpecialWeapon, Missile missile)
    {
        mslIndicator.SetActive(!useSpecialWeapon);
        spwIndicator.SetActive(useSpecialWeapon);

        // Justify that weaponSlots contains 2 slots
        leftMslCooldownImage.SetWeaponData(weaponSlots[0], missile.missileFrameSprite, missile.missileFillSprite);
        rightMslCooldownImage.SetWeaponData(weaponSlots[1], missile.missileFrameSprite, missile.missileFillSprite);
    }
    public void SwitchUI(CameraController.CameraIndex index)
    {
        bool isFirstView = (index == CameraController.CameraIndex.FirstView ||
                            index == CameraController.CameraIndex.FirstViewWithCockpit);

        firstCenterViewTransform.gameObject.SetActive(isFirstView);

        RectTransform parentTransform = (isFirstView) ? firstCenterViewTransform : thirdCenterViewTransform;
        commonCenterUI.SetParent(parentTransform);
    }
    public void SetThrottle(float throttle)
    {
        throttleGauge.fillAmount = (1 + throttle) * 0.5f;
    }
    public void SetHeading(float heading)
    {
        headingUIController.SetHeading(heading);
    }
    void Start()
    {
        firstViewAdjustAngle = new Vector2(1 / firstViewAdjustAngle.x, 1 / firstViewAdjustAngle.y);
    }
    public void AdjustFirstViewUI(Vector3 cameraRotation)
    {
        if (cameraRotation.x > 180) cameraRotation.x -= 360;
        if (cameraRotation.y > 180) cameraRotation.y -= 360;

        Vector2 canvasResolution = new Vector2(firstViewCanvas.pixelRect.width,firstViewCanvas.pixelRect.height);
        Vector2 convertedRotation = new Vector2(cameraRotation.y * firstViewAdjustAngle.x,cameraRotation.x * firstViewAdjustAngle.y);

        firstCenterViewTransform.anchoredPosition = convertedRotation * canvasResolution;
    }
    public void SetTargetText(ObjectInfo objectInfo)
    {
        if (objectInfo == null || objectInfo.ObjectName == "")
        {
            targetText.text = "";
        }
        else
        {
            string objectName = objectInfo.ObjectName + " " + objectInfo.ObjectNickname;
            string text = string.Format("TARGET {0} +<mspace=18>{1}</mspace>", objectName, objectInfo.Score);
            targetText.text = text;
        }
    }
    void Update()
    {
        
    }
}
