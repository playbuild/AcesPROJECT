using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAircraft : AircraftAI
{
    [Header("EnemyAircraft Properties")]
    [SerializeField]
    float destroyDelay = 3;

    [SerializeField]
    Transform smokeTransformParent;

    [SerializeField]
    [Range(0, 1)]
    float playerTrackingRate = 0.5f;

    [SerializeField]
    float minimumPlayerDistance = 2000; // 플레이어와의 거리가 이 값보다 길면 무조건 플레이어를 추적

    protected override Vector3 CreateWaypoint()
    {
        float rate = Random.Range(0.0f, 1.0f);
        float distance = Vector3.Distance(transform.position, GameManager.PlayerAircraft.transform.position);

        if (rate < playerTrackingRate)
        {
            return GameManager.PlayerAircraft.transform.position;
        }
        else
        {
            return base.CreateWaypoint();
        }
    }

    protected override void DestroyObject()
    {
        CommonDestroyFunction();
        Invoke("DelayedDestroy", destroyDelay);
    }

    void DelayedDestroy()
    {
        GameObject obj = Instantiate(destroyEffect, transform.position, Quaternion.identity);
        obj.transform.localScale *= 3;
        Destroy(gameObject);
    }

    public override void OnDamage(float damage, int layer, string tag = "")
    {
        base.OnDamage(damage, layer);

        for (int i = 0; i < smokeTransformParent.childCount; i++)
        {
            GameManager.Instance.CreateDamageSmokeEffect(smokeTransformParent.GetChild(i));
        }
    }


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
