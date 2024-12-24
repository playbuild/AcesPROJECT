using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Missile : MonoBehaviour
{
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
            target = null;
            return;
        }

        Quaternion lookRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turningForce * Time.deltaTime);
    }
    //�̻��� �浹 ���� ����
    void OnCollisionEnter(Collision other)
    {
        other.gameObject.GetComponent<TargetObject>()?.OnDamage(damage);

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
    // �̻��� �ӵ� ����
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
