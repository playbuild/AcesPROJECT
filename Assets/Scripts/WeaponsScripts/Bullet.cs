using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    Rigidbody rb;
    Transform parent;
    TrailRenderer trailRenderer;

    public float speed;
    public float lifetime;

    TargetObject reservedTargetObject;

    [SerializeField]
    float damage;

    public void Fire(float launchSpeed, int layer, TargetObject reservedHitTargetObject = null)
    {
        speed += launchSpeed;
        gameObject.layer = layer;
        rb.velocity = transform.forward * speed;

        if (reservedHitTargetObject != null)
        {
            reservedTargetObject = reservedHitTargetObject;
            GetComponent<Collider>().isTrigger = true;

            float reachTime = Vector3.Distance(transform.position, reservedHitTargetObject.transform.position) / (speed + launchSpeed);
            Invoke("ReserveHit", reachTime);
        }
    }
    public void ReserveHit()
    {
        reservedTargetObject.OnDamage(damage, gameObject.layer, gameObject.tag);
        CreateHitEffect(GameManager.Instance.bulletHitEffectObjectPool);
        DisableBullet();
    }
    void OnCollisionEnter(Collision other)
    {
        ObjectPools effectPool;
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            effectPool = GameManager.Instance.groundHitEffectObjectPool;
        }
        else
        {
            effectPool = GameManager.Instance.bulletHitEffectObjectPool;
            other.gameObject.GetComponent<TargetObject>()?.OnDamage(damage, gameObject.layer, gameObject.tag);
        }
        CreateHitEffect(effectPool);
        DisableBullet();
    }
    void CreateHitEffect(ObjectPools effectPool)
    {
        // Instantiate in world space
        GameObject effect = effectPool.GetPooledObject();
        effect.transform.position = transform.position;
        effect.transform.rotation = transform.rotation;
        effect.SetActive(true);
    }
    void DisableBullet()
    {
        gameObject.SetActive(false);
        transform.parent = parent;
    }
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            trailRenderer = GetComponent<TrailRenderer>();
            parent = transform.parent;
        }
        void OnEnable()
        {
            trailRenderer.Clear();
            GetComponent<Collider>().isTrigger = false;
            Invoke("DisableBullet", lifetime);
        }
        void OnDisable()
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            CancelInvoke();
        }
    }
