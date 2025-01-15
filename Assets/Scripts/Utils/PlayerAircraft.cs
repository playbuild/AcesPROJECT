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
    float destroyDelay = 1;

    [SerializeField]
    float rotateSpeedOnDestroy = 600;

    [SerializeField]
    Transform smokeTransformParent;

    [SerializeField]
    List<TrailRenderer> contrails;

    [SerializeField]
    GameObject aircraftModel;

    int score = 0;
    UIController uiController;

    public float MissileEmergencyDistance
    {
        get { return missileEmergencyDistance; }
    }
    public int Score
    {
        get { return score; }
    }
    public override void AddLockedMissile(Missile missile)
    {
        base.AddLockedMissile(missile);
        missileIndicatorController.AddMissileIndicator(missile);
    }
    public void DisableAllMissiles()
    {
        foreach (Missile lockedMissile in lockedMissiles)
        {
            lockedMissile.RemoveTarget();
        }
        lockedMissiles.Clear();
    }

    // Ground/Object Collision
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground") ||
            other.gameObject.GetComponent<TargetObject>() != null)
        {

            DestroyObjectImmediate();
        }
    }
    public override void OnDamage(float damage, int layer, string tag = "")
    {
        base.OnDamage(damage, layer);
        uiController.SetDamage((int)(Info.HP - hp / Info.HP * 100));

        for (int i = 0; i < smokeTransformParent.childCount; i++)
        {
            GameManager.Instance.CreateDamageSmokeEffect(smokeTransformParent.GetChild(i));
        }
    }
    public void OnScore(int score)
    {
        this.score += score;
        uiController.SetScoreText(this.score);
    }
    protected override void DestroyObject()
    {
        CommonDestroyFunction();
        GameManager.Instance.GameOver(true, false);
        Invoke("DelayedDestroy", destroyDelay);

        foreach (TrailRenderer trailRenderer in contrails)
        {
            trailRenderer.emitting = false;
        }
    }
    void DestroyObjectImmediate()
    {
        CancelInvoke();
        CommonDestroyFunction();
        GameManager.Instance.GameOver(true, true);

        DelayedDestroy();
    }

    void DelayedDestroy()
    {
        GameObject obj = Instantiate(destroyEffect, transform.position, Quaternion.identity);
        Destroy(aircraftModel);

        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        GetComponent<PlayerFighterController>().enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
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
        if (isDestroyed == true)
        {
            transform.Rotate(0, 0, rotateSpeedOnDestroy * Time.deltaTime);
        }
    }
}
