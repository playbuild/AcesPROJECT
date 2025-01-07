using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixyScript : EnemyAircraft
{
    [Header("PixyScript Properties")]
    bool isInvincible;
    bool isAttackable;
    public bool IsInvincible
    {
        set { isInvincible = value; }
    }
    public bool IsAttackable
    {
        set
        {
            isAttackable = value;
            SetMinimapSpriteVisible(value);

            if (isAttackable == false)
            {
                GameManager.Instance.RemoveEnemy(this);
                GameManager.TargetController.RemoveTargetUI(this);
                GameManager.WeaponController?.ChangeTarget();
            }

            else
            {
                GameManager.Instance.AddEnemy(this);
                GameManager.TargetController.CreateTargetUI(this);
            }
        }
    }
    public void ActivateEnemy()
    {
        Debug.Log("Activated");
        hp = objectInfo.HP;
        IsAttackable = true;
    }

    public override void OnDamage(float damage, int layer)
    {
        if (isAttackable == false) return;

        float applyDamage = (isInvincible == true) ? 0 : damage;
        base.OnDamage(applyDamage, layer);
    }

    protected override void DestroyObject()
    {
        IsAttackable = false;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        isInvincible = false;
        isAttackable = true;
    }
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
