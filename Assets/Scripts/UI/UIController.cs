using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
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
    public void SetThrottle(float throttle)
    {
        throttleGauge.fillAmount = (1 + throttle) * 0.5f;
    }
    void Start()
    {
        
    }
    void Update()
    {
        
    }
}
