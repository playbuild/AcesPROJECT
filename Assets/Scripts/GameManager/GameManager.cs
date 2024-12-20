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
    public ObjectPools smokeTrailEffectObjectPool;

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

    List<TargetObject> objects = new List<TargetObject>();

    [SerializeField]
    Color normalColor;
    [SerializeField]
    Color warningColor;

    public static Color NormalColor
    {
        get { return Instance.normalColor; }
    }

    public static Color WarningColor
    {
        get { return Instance.warningColor; }
    }

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
    public void AddEnemy(TargetObject targetObject)
    {
        objects.Add(targetObject);
    }

    public void RemoveEnemy(TargetObject targetObject)
    {
        objects.Remove(targetObject);
    }
    public float GetDistanceFromPlayer(Transform otherTransform)
    {
        return Vector3.Distance(otherTransform.position, playerAircraft.transform.position);
    }
    public static float GetAngleBetweenTransform(Transform otherTransform)
    {
        Vector3 direction = PlayerAircraft.transform.forward;
        Vector3 diff = otherTransform.position - PlayerAircraft.transform.position;
        return Vector3.Angle(diff, direction);
    }
    public List<TargetObject> GetTargetsWithinDistance(float distance, float searchAngle = 0, bool getNearestTarget = false)
    {
        List<TargetObject> objectsWithinDistance = new List<TargetObject>();
        TargetObject nearestTarget = null;
        float minDistance = distance;

        foreach (TargetObject targetObject in objects)
        {
            // Search within searchAngle
            if (searchAngle != 0)
            {
                if (GetAngleBetweenTransform(targetObject.transform) > searchAngle) continue;
            }

            float targetDistance = Vector3.Distance(targetObject.transform.position, playerAircraft.transform.position);

            if (targetDistance < distance)
            {
                if (getNearestTarget == true && targetDistance < minDistance)
                {
                    nearestTarget = targetObject;
                    minDistance = targetDistance;
                }
                objectsWithinDistance.Add(targetObject);
            }
            else
            {
                targetObject.isNextTarget = false;
            }
        }

        if (getNearestTarget == true)
        {
            objectsWithinDistance.Clear();
            objectsWithinDistance.Add(nearestTarget);
        }
        return objectsWithinDistance;
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
