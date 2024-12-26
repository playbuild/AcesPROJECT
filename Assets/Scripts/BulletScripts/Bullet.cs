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

    [SerializeField]
    float damage;

    public void Fire(float launchSpeed, int layer)
    {
        speed += launchSpeed;
        gameObject.layer = layer;
        rb.velocity = transform.forward * speed;
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
            other.gameObject.GetComponent<TargetObject>()?.OnDamage(damage, gameObject.layer);
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
        // Update is called once per frame
        void Update()
        {
            transform.Translate(new Vector3(0, 0, speed * Time.deltaTime));
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
        }
    }
