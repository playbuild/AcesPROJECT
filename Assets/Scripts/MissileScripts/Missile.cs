using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Missile : MonoBehaviour
{
    Transform parent;
    Rigidbody rb;

    Transform target;
    float speed;
    public string missileName;

    [Header("Properties")]

    [SerializeField]
    float damage;

    public bool isSpecialWeapon;
    public float maxSpeed;
    public float accelAmount;
    public float turningForce;

    public float targetSearchSpeed;
    public float lockDistance;

    public float boresightAngle;
    public float lifetime;

    public ParticleSystem explosionPrefab;

    public float cooldown;
    public int payload;

    public Sprite missileFrameSprite;
    public Sprite missileFillSprite;

    GameObject smokeTrailEffect;
    public Transform smokeTrailPosition;

    bool isHit = false;
    bool isDisabled = false;

    public void Launch(Transform target, float launchSpeed, int layer)
    {
        this.target = target;

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
    void LookAtTarget()
    {
        if (target == null)
            return;

        Vector3 targetDir = target.position - transform.position;
        float angle = Vector3.Angle(targetDir, transform.forward);

        if (angle > boresightAngle)
        {
            GameManager.UIController.SetLabel(AlertUIController.LabelEnum.Missed);
            isDisabled = true;
            target = null;
            return;
        }

        Quaternion lookRotation = Quaternion.LookRotation(target.position - transform.position);
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

    void DisableMissile()
    {
        if (target != null && isDisabled == false && isHit == false)
        {
            GameManager.UIController.SetLabel(AlertUIController.LabelEnum.Missed);
        }

        transform.parent = parent;
        gameObject.SetActive(false);
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
