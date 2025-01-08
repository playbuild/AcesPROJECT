using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Missile : MonoBehaviour
{
    Transform parent;
    Rigidbody rb;

    TargetObject target;
    float speed;
    public string missileName;

    [Header("Properties")]

    [SerializeField]
    float damage;

    public bool isSpecialWeapon;
    public float maxSpeed;
    public float accelAmount;
    public float turningForce;

    [SerializeField]
    [Range(0, 1)]
    protected float smartTrackingRate = 0.3f;

    public float boresightAngle;
    public float lifetime;

    public float targetSearchSpeed;
    public float lockDistance;

    public ParticleSystem explosionPrefab;

    public float cooldown;
    public int payload;

    [Header("UI")]
    public Sprite missileFrameSprite;
    public Sprite missileFillSprite;
    public MinimapSprite minimapSprite;

    GameObject smokeTrailEffect;
    public Transform smokeTrailPosition;

    bool isHit = false;
    bool isDisabled = false;
    bool hasWarned = false;

    Rigidbody targetRigidbody = null;

    public bool HasWarned
    {
        get { return hasWarned; }
        set { hasWarned = value; }
    }

    public bool IsDisabled
    {
        get { return isDisabled; }
    }

    public void Launch(TargetObject target, float launchSpeed, int layer)
    {
        this.target = target;

        targetRigidbody = target?.GetComponent<Rigidbody>();
        minimapSprite.SetMinimapSpriteVisible(target != null);
        isDisabled = (target == null);

        // Send Message to object that it is locked on
        target?.AddLockedMissile(this);

        speed = launchSpeed;
        gameObject.layer = layer;

        smokeTrailEffect = GameManager.Instance.smokeTrailEffectObjectPool.GetPooledObject();
        if (smokeTrailEffect != null)
        {
            smokeTrailEffect.GetComponent<SmokeTrail>()?.SetFollowTransform(smokeTrailPosition);
            smokeTrailEffect.SetActive(true);
        }
    }

    void Start()
    {
        Invoke("DisableMissile", lifetime);
    }
    Vector3 GetPredictedTargetPosition()
    {
        if (targetRigidbody == null) return target.transform.position;

        Vector3 predictedPos = target.transform.position;
        float timeToCatchUp = MathHelper.GetTimeToCatchUp(targetRigidbody, rb);
        float angle = Vector3.Angle(transform.forward, target.transform.forward);

        if (timeToCatchUp > 0)
        {
            // Considering target velocity
            predictedPos += timeToCatchUp * targetRigidbody.velocity;
        }
        else
        {
            // Not considering target velocity
            float distance = Vector3.Distance(target.transform.position, transform.position);
            predictedPos += (distance / speed) * (Mathf.Cos(angle) + 1) * 0.5f * targetRigidbody.velocity;
        }

        return predictedPos;
    }
    void LookAtTarget()
    {
        if (target == null)
            return;

        Vector3 targetPos = Vector3.Lerp(target.transform.position, GetPredictedTargetPosition(), smartTrackingRate);
        Vector3 targetDir = target.transform.position - transform.position;
        float angle = Vector3.Angle(targetDir, transform.forward);

        if (angle > boresightAngle)
        {
            // UI
            ShowMissedLabel();
            minimapSprite.SetMinimapSpriteVisible(false);

            isDisabled = true;
            // Send Message to object that it is no more locked on
            target.GetComponent<TargetObject>()?.RemoveLockedMissile(this);
            target = null;
            return;
        }

        Quaternion lookRotation = Quaternion.LookRotation(targetPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turningForce * Time.deltaTime);
    }
    //미사일 충돌 이후 폭발
    void OnCollisionEnter(Collision other)
    {
        if (target != null && other.gameObject == target.gameObject)
        {
            isHit = true;
        }
        other.gameObject.GetComponent<TargetObject>()?.OnDamage(damage, gameObject.layer);

        Explode();
        DisableMissile();
    }
    void Explode()
    {
        ObjectPools effectPool = GameManager.Instance.BigExplosionPool;
        GameObject effect = effectPool.GetPooledObject();
        effect.transform.position = transform.position;
        effect.transform.rotation = transform.rotation;
        effect.SetActive(true);
    }
    public void RemoveTarget()
    {
        target = null;
        DisableMissile();
    }
    // Called by ECM System
    public void EvadeByECM(Vector3 randomPosition)
    {
        if (target != null)
        {
            target.RemoveLockedMissile(this);

            if (isDisabled == false && isHit == false)
            {
                ShowMissedLabel();
            }
        }

        target = null;

        Vector3 randomDirection = randomPosition - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(randomDirection);
        rb.rotation = lookRotation;
    }

    void DisableMissile()
    {
        hasWarned = false;

        if (target != null)
        {
            target.RemoveLockedMissile(this);

            if (isDisabled == false && isHit == false)
            {
                ShowMissedLabel();
            }
        }

        isDisabled = true;
        transform.parent = parent;
        gameObject.SetActive(false);
    }
    void ShowMissedLabel()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GameManager.UIController.SetLabel(AlertUIController.LabelEnum.Missed);
        }
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void OnDisable()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    // 미사일 속도 조절
    void FixedUpdate()
    {
        if (speed < maxSpeed)
        {
            speed += accelAmount * Time.deltaTime;
        }

        transform.Translate(new Vector3(0, 0, speed * Time.deltaTime));
        LookAtTarget();
        if (speed < maxSpeed)
        {
            speed += accelAmount * Time.fixedDeltaTime;
        }

        rb.velocity = transform.forward * speed;
    }
}
