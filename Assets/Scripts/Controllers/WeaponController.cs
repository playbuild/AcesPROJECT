using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(PlayerFighterController))]

public class WeaponController : MonoBehaviour
{
    // Weapon Inputs
    bool useSpecialWeapon;

    [Header("Common Weapon System")]
    TargetObject target;

    ObjectPools missilePool;
    int missileCnt;

    public Transform leftMissileTransform;
    public Transform rightMissileTransform;

    ObjectPools specialWeaponPool;
    string specialWeaponName;
    int specialWeaponCnt;
    float spwCooldownTime;

    float missileCooldownTime;
    float rightMslCooldown;
    float leftMslCooldown;

    ObjectPools bulletPool;

    bool isFocusingTarget;

    public int bulletCnt;
    public Transform gunTransform;
    public float gunRPM;
    float fireInterval;

    [Header("Missile")]
    [SerializeField]
    Missile missile;
    WeaponSlot[] mslSlots = new WeaponSlot[2];

    [Header("Special Weapon")]
    [SerializeField]
    Missile specialWeapon;
    WeaponSlot[] spwSlots = new WeaponSlot[2];

    [Header("UI / Misc.")]
    [SerializeField]
    MinimapCamera minimapCamera;

    [SerializeField]
    GunCrosshair gunCrosshair;

    [Header("Sounds")]
    [SerializeField]
    AudioClip ammunationZeroClip;
    [SerializeField]
    AudioClip cooldownClip;

    [SerializeField]
    AudioSource voiceAudioSource;
    [SerializeField]
    AudioSource weaponAudioSource;
    [SerializeField]
    AudioSource missileAudioSource;

    [SerializeField]
    GunAudio gunAudio;

    PlayerFighterController fighterController;
    UIController uiController;

    // Weapon Callbacks
    public void Fire(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            if (useSpecialWeapon == true)
            {
                LaunchMissile(ref specialWeaponCnt, ref specialWeaponPool, ref spwSlots);
            }
            else
            {
                LaunchMissile(ref missileCnt, ref missilePool, ref mslSlots);
            }
        }
    }

    public void Guns(InputAction.CallbackContext context)
    {
        switch (context.action.phase)
        {
            case InputActionPhase.Performed:
                InvokeRepeating("FireMachineGun", 0, fireInterval);
                gunAudio.IsFiring = true;
                break;

            case InputActionPhase.Canceled:
                CancelInvoke("FireMachineGun");
                gunAudio.IsFiring = false;
                break;
        }
    }
    public void OnChangeTarget(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Started)
        {
            isFocusingTarget = false;
        }

        // Hold Interaction Performed (0.3s)
        else if (context.action.phase == InputActionPhase.Performed)
        {
            if (target == null) return;

            GameManager.CameraController.LockOnTarget(target.transform);
            isFocusingTarget = true;
        }

        else if (context.action.phase == InputActionPhase.Canceled)
        {
            // Hold : Focus
            if (isFocusingTarget == true)
            {
                GameManager.CameraController.LockOnTarget(null);
            }
            // Press : Change
            else
            {
                ChangeTarget();
            }
        }
    }
    public void ChangeTarget()
    {
        TargetObject newTarget = GetNextTarget();
        if (newTarget == null)   // No target
        {
            GameManager.CameraController.LockOnTarget(null);
            GameManager.TargetController.ChangeTarget(null);
            gunCrosshair.SetTarget(null);
            target = null;

            return;
        }

        if (newTarget != null && newTarget == target) return;
        target = GetNextTarget();
        target.isNextTarget = false;
        GameManager.TargetController.ChangeTarget(target);
        gunCrosshair.SetTarget(target.transform);
    }

    TargetObject GetNextTarget()
    {
        List<TargetObject> targets = GameManager.Instance.GetTargetsWithinDistance(3000);
        TargetObject selectedTarget = null;

        if (targets.Count == 0) return null;

        else if (targets.Count == 1) selectedTarget = targets[0];

        else
        {
            if (target == null) return targets[0];   // not selected

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] == target)
                {
                    if (i == targets.Count - 1)  // last index
                    {
                        targets[1].isNextTarget = true;
                        targets[0].isNextTarget = false;
                        selectedTarget = targets[0];
                    }
                    else
                    {
                        if (i + 1 == targets.Count - 1)  // i + 1 == last index
                        {
                            targets[0].isNextTarget = true;
                        }
                        else    // something that is not last and before last index
                        {
                            targets[i + 2].isNextTarget = true;
                        }
                        selectedTarget = targets[i + 1];
                    }
                }
            }
        }
        return selectedTarget;
    }

    void FireMachineGun()
    {
        if (bulletCnt <= 0)
        {
            // Beep sound
            CancelInvoke("FireMachineGun");
            return;
        }

        GameObject bullet = bulletPool.GetPooledObject();
        bullet.transform.position = gunTransform.position;
        bullet.transform.rotation = transform.rotation;
        bullet.SetActive(true);

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.Fire(fighterController.Speed, gameObject.layer);
        bulletCnt--;
    }
    WeaponSlot GetAvailableWeaponSlot(ref WeaponSlot[] weaponSlots)
    {
        WeaponSlot oldestSlot = null;

        foreach (WeaponSlot slot in weaponSlots)
        {
            if (slot.IsAvailable() == true)
            {
                if (oldestSlot == null)
                {
                    oldestSlot = slot;
                }
                else if (oldestSlot.LastStartCooldownTime > slot.LastStartCooldownTime)
                {
                    oldestSlot = slot;
                }
            }
        }

        return oldestSlot;
    }

    void LaunchMissile(ref int weaponCnt, ref ObjectPools objectPool, ref WeaponSlot[] weaponSlots)
    {
        WeaponSlot availableWeaponSlot = GetAvailableWeaponSlot(ref weaponSlots);

        //Ammunition Zero
        if (weaponCnt <= 0)
        {
            if (voiceAudioSource.isPlaying == false)
            {
                voiceAudioSource.PlayOneShot(ammunationZeroClip);
            }
            return;
        }
        // Not available : Beep sound
        if (availableWeaponSlot == null)
        {
            weaponAudioSource.PlayOneShot(cooldownClip);
            return;
        }

        Vector3 missilePosition;

        // Select Launch Position
        if (weaponCnt % 2 == 1)
        {
            missilePosition = rightMissileTransform.position;
        }
        else
        {
            missilePosition = leftMissileTransform.position;
        }

        // Start Cooldown
        availableWeaponSlot.StartCooldown();

        // Get from Object Pool and Launch
        GameObject missile = missilePool.GetPooledObject();
        missile.transform.position = missilePosition;
        missile.transform.rotation = transform.rotation;
        missile.SetActive(true);

        Missile missileScript = missile.GetComponent<Missile>();
        TargetObject targetObject = (target != null && GameManager.TargetController.IsLocked == true) ? target : null;
        missileScript.Launch(targetObject, fighterController.Speed + 15, gameObject.layer);

        weaponCnt--;

        uiController.SetMissileText(missileCnt);
        uiController.SetSpecialWeaponText(specialWeaponName, specialWeaponCnt);

        weaponAudioSource.PlayOneShot(SoundManager.Instance.GetMissileLaunchClip());
    }
    //void MissileCooldown(ref float cooldown)
    //{
    //    if (cooldown > 0)
    //    {
    //        cooldown -= Time.deltaTime;
    //        if (cooldown < 0) cooldown = 0;
    //    }
    //    else return;
    //}
    public void SwitchWeapon(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            useSpecialWeapon = !useSpecialWeapon;
            SetUIAndTarget();
        }
    }
    void SetUIAndTarget()
    {
        Missile switchedMissile = (useSpecialWeapon == true) ? specialWeapon : missile;
        WeaponSlot[] weaponSlots = (useSpecialWeapon == true) ? spwSlots : mslSlots;

        uiController.SetMissileText(missileCnt);
        uiController.SetGunText(bulletCnt);
        uiController.SetSpecialWeaponText(specialWeaponName, specialWeaponCnt);
        uiController.SwitchWeapon(weaponSlots, useSpecialWeapon, switchedMissile);
        GameManager.TargetController.SwitchWeapon(switchedMissile);
    }
    void SetArmament()
    {
        // Guns
        fireInterval = 60.0f / gunRPM;

        // Missiles
        missileCnt = missile.payload;
        missileCooldownTime = missile.cooldown;
        for (int i = 0; i < 2; i++)
        {
            mslSlots[i] = new WeaponSlot(missileCooldownTime);
        }

        // Special Weapons
        specialWeaponCnt = specialWeapon.payload;
        spwCooldownTime = specialWeapon.cooldown;
        specialWeaponName = specialWeapon.missileName;

        for (int i = 0; i < 2; i++)
        {
            spwSlots[i] = new WeaponSlot(spwCooldownTime);
        }
    }
    void SetMinimapCamera()
    {
        // Minimap
        Vector2 distance = new Vector3(transform.position.x - target.transform.position.x,
                                       transform.position.z - target.transform.position.z);
        minimapCamera.SetZoom(distance.magnitude);
    }

    public void Awake()
    {
        fighterController = GetComponent<PlayerFighterController>();
    }
    void Start()
    {
        uiController = GameManager.UIController;

        missilePool = GameManager.Instance.missileObjectPool;
        specialWeaponPool = GameManager.Instance.specialWeaponObjectPool;
        bulletPool = GameManager.Instance.bulletObjectPool;

        missilePool.poolObject = missile.gameObject;
        specialWeaponPool.poolObject = specialWeapon.gameObject;

        useSpecialWeapon = false;

        //uiController.SetMissileText(missileCnt);
        //uiController.SetGunText(bulletCnt);

        SetArmament();
        SetUIAndTarget();
    }
    void Update()
    {
        //MissileCooldown(ref rightMslCooldown);
        //MissileCooldown(ref leftMslCooldown);

        foreach (WeaponSlot slot in mslSlots)
        {
            slot.UpdateCooldown();
        }
        foreach (WeaponSlot slot in spwSlots)
        {
            slot.UpdateCooldown();
        }

        if (target != null)
        {
            SetMinimapCamera();
        }

        //uiController.SetMissileText(missileCnt);
        //uiController.SetGunText(bulletCnt);
        //uiController.SetSpecialWeaponText(specialWeaponName, specialWeaponCnt);
    }
}
