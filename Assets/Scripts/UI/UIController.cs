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
    [SerializeField]
    AlertUIController alertUIController;

    [Header("Upper Right UI")]
    [SerializeField]
    GameObject checkpointReachedUI;

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

    [Header("Minimap Misc.")]
    [SerializeField]
    MinimapCamera minimapCamera;

    [Header("Materials")]
    [SerializeField]
    Material spriteMaterial;
    [SerializeField]
    Material fontMaterial;

    [Header("Colors")]
    [SerializeField]
    Color cautionColor;

    List<MaskColorChange> maskColorChanges = new List<MaskColorChange>();

    [SerializeField]
    Color normalColor;
    [SerializeField]
    Color warningColor;

    [Header("Sounds")]
    [SerializeField]
    AudioClip spwChangeAudioClip;
    [SerializeField]
    AudioClip mslChangeAudioClip;

    AudioSource audioSource;

    float remainTime;
    int score = 0;
    float damage = 0;
    bool isTimeLow = false;
    bool isRedTimerActive = false;
    bool enableCount = true;

    float elapsedTime = 0;

    public bool IsRedTimerActive
    {
        set { isRedTimerActive = value; }
    }
    public MinimapCamera MinimapCamera
    {
        get { return minimapCamera; }
    }
    public float StopCountAndGetElapsedTime()
    {
        enableCount = false;
        return elapsedTime;
    }
    public void StartCountAndResetElapsedTime()
    {
        enableCount = true;
        elapsedTime = 0;
    }
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
    void SetAircraftDamageUI()
    {
        if (damage < 34)
        {
            aircraftImage.color = GameManager.NormalColor;
        }
        else if (damage < 67)
        {
            aircraftImage.color = cautionColor;
        }
        else
        {
            aircraftImage.color = GameManager.WarningColor;
        }
    }
    public void SetDamage(int damage)
    {
        string text = string.Format("<align=left>DMG<line-height=0>\n<align=right>{0}%<line-height=0>", damage);
        dmgText.text = text;

        this.damage = damage;
        SetAircraftDamageUI();

        if (damage > 0)
        {
            StartCoroutine(alertUIController.ShowDamagedUI());
        }
    }
    public void SwitchWeapon(WeaponSlot[] weaponSlots, bool useSpecialWeapon, Missile missile, bool playAudio = true)
    {
        mslIndicator.SetActive(!useSpecialWeapon);
        spwIndicator.SetActive(useSpecialWeapon);

        // Justify that weaponSlots contains 2 slots
        leftMslCooldownImage.SetWeaponData(weaponSlots[0], missile.missileFrameSprite, missile.missileFillSprite);
        rightMslCooldownImage.SetWeaponData(weaponSlots[1], missile.missileFrameSprite, missile.missileFillSprite);

        if (playAudio == true)
        {
            AudioClip audioClip = (useSpecialWeapon == true) ? spwChangeAudioClip : mslChangeAudioClip;
            audioSource.PlayOneShot(audioClip);
        }
    }
    public void SwitchUI(CameraController.CameraIndex index)
    {
        bool isFirstView = (index == CameraController.CameraIndex.FirstView ||
                            index == CameraController.CameraIndex.FirstViewWithCockpit);

        firstCenterViewTransform.gameObject.SetActive(isFirstView);

        RectTransform parentTransform = (isFirstView) ? firstCenterViewTransform : thirdCenterViewTransform;
        commonCenterUI.SetParent(parentTransform);
    }
    public void AddMaskColorChange(MaskColorChange mask)
    {
        maskColorChanges.Add(mask);
    }
    public void SetWarningUIColor(bool isWarning)
    {
        Color color;
        if (isWarning == true)
        {
            color = GameManager.WarningColor;
            aircraftImage.color = GameManager.WarningColor;
        }
        else
        {
            color = GameManager.NormalColor;
            SetAircraftDamageUI();
        }

        spriteMaterial.color = color;
        fontMaterial.SetColor("_FaceColor", color);

        foreach (MaskColorChange maskColorChange in maskColorChanges)
        {
            maskColorChange.ChangeComponentColor(color);
        }
    }

    public void SetThrottle(float throttle)
    {
        throttleGauge.fillAmount = (1 + throttle) * 0.5f;
    }
    public void SetHeading(float heading)
    {
        headingUIController.SetHeading(heading);
    }
    public void AdjustFirstViewUI(Vector3 cameraRotation)
    {
        if (cameraRotation.x > 180) cameraRotation.x -= 360;
        if (cameraRotation.y > 180) cameraRotation.y -= 360;

        Vector2 canvasResolution = new Vector2(firstViewCanvas.pixelRect.width,firstViewCanvas.pixelRect.height);
        Vector2 convertedRotation = new Vector2(cameraRotation.y * firstViewAdjustAngle.x,cameraRotation.x * firstViewAdjustAngle.y);

        firstCenterViewTransform.anchoredPosition = convertedRotation * canvasResolution;
    }
    public void SetRemainTime(int remainTime)
    {
        this.remainTime = (float)remainTime;
    }
    void SetTime()
    {
        remainTime -= Time.deltaTime;
        if (remainTime <= 0)
        {
            CancelInvoke();
            GameManager.Instance.GameOver(false);
            remainTime = 0;
        }

        int seconds = (int)remainTime;

        int min = seconds / 60;
        int sec = seconds % 60;
        int millisec = (int)((remainTime - seconds) * 100);
        string text = string.Format("TIME <mspace=18>{0:00}</mspace>:<mspace=18>{1:00}</mspace>:<mspace=18>{2:00}</mspace>", min, sec, millisec);
        timeText.text = text;
    }
    public void SetScoreText(int score)
    {
        this.score += score;
        string text = string.Format("SCORE <mspace=18>{0}</mspace>", this.score);
        scoreText.text = text;
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
    public void SetLabel(AlertUIController.LabelEnum labelEnum)
    {
        alertUIController.SetLabel(labelEnum);
    }
    public void ShowCheckpointReachedUI()
    {
        StartCoroutine(SetCheckpointReachedUI());
    }

    public IEnumerator SetCheckpointReachedUI()
    {
        checkpointReachedUI.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        checkpointReachedUI.SetActive(false);
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        firstViewAdjustAngle = new Vector2(1 / firstViewAdjustAngle.x, 1 / firstViewAdjustAngle.y);

        //SetWarningUIColor(true); // TEST
    }
    void Update()
    {
        SetTime();
    }
}
