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
    PlayerAircraft playerAircraft;
    [SerializeField]
    WeaponController weaponController;
    [SerializeField]
    CameraController cameraController;
    [SerializeField]
    TargetController targetController;

    public static PlayerFighterController PlayerFighterController
    {
        get { return Instance?.playerFighterController; }
    }
    public static PlayerAircraft PlayerAircraft
    {
        get { return Instance?.playerAircraft; }
    }
    public static WeaponController WeaponController
    {
        get { return Instance?.weaponController; }
    }
    public static CameraController CameraController
    {
        get { return Instance?.cameraController; }
    }
    public static UIController UIController
    {
        get { return Instance?.uiController; }
    }
    public static TargetController TargetController
    {
        get { return Instance?.targetController; }
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
    public float GetDistanceFromPlayer(Transform otherTransform)
    {
        return Vector3.Distance(otherTransform.position, playerAircraft.transform.position);
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
}
