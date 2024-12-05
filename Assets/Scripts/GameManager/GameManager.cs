using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    [Header("Object Pools")]
    public ObjectPools bulletObjectPool;
    public ObjectPools missileObjectPool;
    public ObjectPools specialWeaponObjectPool;
    public ObjectPools bulletHitEffectObjectPool;
    public ObjectPools BigExplosionPool;

    [Header("Controllers")]
    [SerializeField]
    UIController uiController;
    [SerializeField]
    PlayerFighterController playerFighterController;
    [SerializeField]
    WeaponController weaponController;

    public static PlayerFighterController PlayerFighterController
    {
        get { return Instance?.playerFighterController; }
    }
    public static WeaponController WeaponController
    {
        get { return Instance?.weaponController; }
    }
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }
        void Awake()
    {
        if (instance == null)
            {
                instance = this;
            }
        Time.timeScale = 1;
        AudioListener.pause = false;
    }
    public static UIController UIController
    {
        get { return Instance?.uiController; }
    }
}
