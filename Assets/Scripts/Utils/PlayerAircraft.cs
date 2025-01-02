using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerAircraft : TargetObject
{
    public enum WarningStatus
    {
        NONE,
        WARNING,
        MISSILE_ALERT,
        MISSILE_ALERT_EMERGENCY
    }

    [SerializeField]
    float missileEmergencyDistance;

    [SerializeField]
    MissileIndicatorController missileIndicatorController;

    [SerializeField]
    float rotateSpeedOnDestroy = 600;

    int score = 0;
    UIController uiController;

    public float MissileEmergencyDistance
    {
        get { return missileEmergencyDistance; }
    }
    public override void AddLockedMissile(Missile missile)
    {
        base.AddLockedMissile(missile);
        missileIndicatorController.AddMissileIndicator(missile);
    }
    public override void OnDamage(float damage, int layer)
    {
        base.OnDamage(damage, layer);
        uiController.SetDamage((int)(Info.HP - hp / Info.HP * 100));
    }
    public void OnScore(int score)
    {
        this.score += score;
        uiController.SetScoreText(this.score);
    }
    public WarningStatus GetWarningStatus()
    {
        if (lockedMissiles.Count > 0)
        {
            foreach (Missile missile in lockedMissiles)
            {
                float distance = Vector3.Distance(transform.position, missile.transform.position);
                if (distance < missileEmergencyDistance)
                {
                    return WarningStatus.MISSILE_ALERT_EMERGENCY;
                }
            }

            return WarningStatus.MISSILE_ALERT;
        }

        if (IsLocking == true)
        {
            return WarningStatus.WARNING;
        }

        return WarningStatus.NONE;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        uiController = GameManager.UIController;

        uiController.SetDamage(0);
        uiController.SetScoreText(0);

        rotateSpeedOnDestroy *= Random.Range(0.5f, 1.0f);
    }
    void Update()
    {
    }
}
