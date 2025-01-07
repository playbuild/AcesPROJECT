using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TargetObject : MonoBehaviour
{
    [SerializeField]
    protected ObjectInfo objectInfo;

    [SerializeField]
    protected GameObject destroyEffect;

    protected bool isEnemy;
    protected float hp;
    public bool isNextTarget;

    int lastHitLayer;

    protected List<Missile> lockedMissiles = new List<Missile>();
    protected bool isWarning;

    protected MinimapSprite minimapSprite;
    protected bool isDestroyed;

    protected TargetUI targetUI;

    public TargetUI TargetUI
    {
        get { return targetUI; }
        set { targetUI = value; }
    }

    public ObjectInfo Info
    {
        get
        {
            return objectInfo;
        }
    }

    bool isLocking;
    public bool IsLocking
    {
        get { return isLocking; }
        set { isLocking = value; }
    }
    public void DeleteMinimapSprite()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject childObject = transform.GetChild(i).gameObject;
            if (childObject.layer == LayerMask.NameToLayer("Minimap"))
            {
                Destroy(childObject);
            }
        }
    }
    public void SetMinimapSpriteVisible(bool visible)
    {
        if (minimapSprite != null)
        {
            minimapSprite.gameObject.SetActive(visible);
            SetMinimapSpriteBlink(false);
        }
    }
    public void SetMinimapSpriteBlink(bool blink)
    {
        if (minimapSprite != null)
        {
            minimapSprite.SetMinimapSpriteBlink(blink);
        }
    }
    protected void CommonDestroyFunction()
    {
        isDestroyed = true;

        GameObject obj = Instantiate(destroyEffect, transform.position, Quaternion.identity);
        if (isEnemy == true)
        {
            GameManager.Instance?.RemoveEnemy(this);
            GameManager.TargetController?.RemoveTargetUI(this);
            GameManager.WeaponController?.ChangeTarget();

            if (lastHitLayer == LayerMask.NameToLayer("Player"))
            {
                GameManager.PlayerAircraft.OnScore(objectInfo.Score);
                GameManager.UIController.SetLabel(AlertUIController.LabelEnum.Destroyed);
            }
        }
    }
    public virtual void OnDamage(float damage, int layer)
    {
        hp -= damage;
        lastHitLayer = layer;

        if (lastHitLayer == LayerMask.NameToLayer("Player")) // Hit by Player
        {
            GameManager.UIController.SetLabel(AlertUIController.LabelEnum.Hit);
        }

        if (hp <= 0)
        {
            DestroyObject();
            GameManager.UIController.SetLabel(AlertUIController.LabelEnum.Destroyed);
        }
    }
    public virtual void OnMissileAlert()
    {

    }
    public virtual void AddLockedMissile(Missile missile)
    {
        lockedMissiles.Add(missile);
    }

    public void RemoveLockedMissile(Missile missile)
    {
        lockedMissiles.Remove(missile);
    }

    protected void CheckMissileDistance()
    {
        bool existWarningMissile = false;
        bool executeWarning = false;
        foreach (Missile missile in lockedMissiles)
        {
            float distance = Vector3.Distance(missile.transform.position, transform.position);

            if (distance < Info.WarningDistance)
            {
                existWarningMissile = true;

                if (missile.HasWarned == false)
                {
                    executeWarning = true;
                    missile.HasWarned = true;
                    break;
                }
            }
        }

        if (executeWarning)
        {
            OnMissileAlert();
        }

        if (existWarningMissile == true)
        {
            isWarning = true;
        }
        else
        {
            isWarning = false;
        }
    }
    protected virtual void DestroyObject()
    {
        GameObject obj = Instantiate(destroyEffect, transform.position, Quaternion.identity);
        if (isEnemy == true)
        {
            GameManager.Instance?.RemoveEnemy(this);
            GameManager.TargetController?.RemoveTargetUI(this); // Test Only
            GameManager.WeaponController?.ChangeTarget();
        }

        Destroy(gameObject);
        DeleteMinimapSprite();
    }
    protected virtual void Start()
    {
        isEnemy = gameObject.layer != LayerMask.NameToLayer("Player");
        if (isEnemy == true)
        {
            GameManager.TargetController.CreateTargetUI(this);
            GameManager.Instance?.AddEnemy(this);
        }
        hp = objectInfo.HP;
    }
    void OnDestroy()
    {
        if (GameManager.TargetController != null)
        {
            GameManager.TargetController.RemoveTargetUI(this);
        }
    }
}
