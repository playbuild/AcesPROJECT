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
    bool isGunFiring;

    PlayerFighterController fighterController;
    UIController uiController;

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

    public int bulletCnt;
    public Transform gunTransform;
    public float gunRPM;
    float fireInterval;

    public Transform target;

    [Header("Missile")]
    [SerializeField]
    Missile missile;
    WeaponSlot[] mslSlots = new WeaponSlot[2];

    [Header("Special Weapon")]
    [SerializeField]
    Missile specialWeapon;
    WeaponSlot[] spwSlots = new WeaponSlot[2];

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

        if (missileCnt <= 0)
        {
            //Ammunition Zero
            return;
        }
        if (leftMslCooldown > 0 && rightMslCooldown > 0)
        {
            // Beep sound
            return;
        }

        Vector3 missilePosition = (missileCnt % 2 == 1) ? rightMissileTransform.position : leftMissileTransform.position;

        if (missileCnt % 2 == 1)
        {
            missilePosition = rightMissileTransform.position;
            rightMslCooldown = missileCooldownTime;
        }
        else
        {
            missilePosition = leftMissileTransform.position;
            leftMslCooldown = missileCooldownTime;
        }

        // Start Cooldown
        availableWeaponSlot.StartCooldown();

        // Get from Object Pool and Launch
        GameObject missile = missilePool.GetPooledObject();
        missile.transform.position = missilePosition;
        missile.transform.rotation = transform.rotation;
        missile.SetActive(true);

        Missile missileScript = missile.GetComponent<Missile>();
        missileScript.Launch(target, fighterController.Speed + 15, gameObject.layer);

        missileCnt--;

        uiController.SetMissileText(missileCnt);
        uiController.SetSpecialWeaponText(specialWeaponName, specialWeaponCnt);
    }
    void MissileCooldown(ref float cooldown)
    {
        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
            if (cooldown < 0) cooldown = 0;
        }
        else return;
    }
    public void Guns(InputAction.CallbackContext context)
    {
        switch (context.action.phase)
        {
            case InputActionPhase.Performed:
                isGunFiring = true;
                InvokeRepeating("FireMachineGun", 0, fireInterval);
                break;

            case InputActionPhase.Canceled:
                isGunFiring = false;
                CancelInvoke("FireMachineGun");
                break;
        }
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
    }


    public void Awake()
    {
        missilePool = GameManager.Instance.missileObjectPool;
        specialWeaponPool = GameManager.Instance.specialWeaponObjectPool;
        bulletPool = GameManager.Instance.bulletObjectPool;

        missilePool.poolObject = missile.gameObject;
        specialWeaponPool.poolObject = specialWeapon.gameObject;
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
    void Start()
    {
        uiController = GameManager.UIController;

        fighterController = GetComponent<PlayerFighterController>();

        missilePool = GameManager.Instance.missileObjectPool;
        bulletPool = GameManager.Instance.bulletObjectPool;

        uiController.SetMissileText(missileCnt);
        uiController.SetGunText(bulletCnt);

        SetArmament();
        SetUIAndTarget();
    }
    void Update()
    {
        MissileCooldown(ref rightMslCooldown);
        MissileCooldown(ref leftMslCooldown);

        foreach (WeaponSlot slot in mslSlots)
        {
            slot.UpdateCooldown();
        }
        foreach (WeaponSlot slot in spwSlots)
        {
            slot.UpdateCooldown();
        }

        uiController.SetMissileText(missileCnt);
        uiController.SetGunText(bulletCnt);
        uiController.SetSpecialWeaponText(specialWeaponName, specialWeaponCnt);
    }
}
