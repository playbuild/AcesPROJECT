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

    public void Fire(float launchSpeed, int layer)
    {
        speed += launchSpeed;
        gameObject.layer = layer;
        rb.velocity = transform.forward * speed;
    }
    void OnCollisionEnter(Collision other)
    {
        CreateHitEffect();
        DisableBullet();
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
        void CreateHitEffect()
        {
            // Instantiate in world space
            ObjectPools effectPool = GameManager.Instance.bulletHitEffectObjectPool;
            GameObject effect = effectPool.GetPooledObject();
            effect.transform.position = transform.position;
            effect.transform.rotation = transform.rotation;
            effect.SetActive(true);
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
