using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PixyScript : EnemyAircraft
{
    [Header("PixyScript Properties")]
    bool isInvincible;
    bool isAttackable;

    int phase = 1;

    [SerializeField]
    PixyTLS pixyTLS;

    [SerializeField]
    UnityEvent phase1EndEvents;
    [SerializeField]
    UnityEvent phase2EndEvents;
    [SerializeField]
    UnityEvent phase3EndEvents;

    EnemyWeaponController weaponController;
    PixyMPBMController mpbmController;
    ECMSystem ecmSystem;

    public bool MPBMController
    {
        set { mpbmController.enabled = value; }
    }
    public bool ECMSystem
    {
        set { ecmSystem.enabled = value; }
    }

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
            weaponController.enabled = value;

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

    public override void OnDamage(float damage, int layer, string tag = "")
    {
        if (isAttackable == false) return;
        if (ecmSystem.enabled == true && tag == "Bullet") return;

        float applyDamage = (isInvincible == true) ? 0 : damage;

        base.OnDamage(applyDamage, layer);
    }

    protected override void DestroyObject()
    {
        IsAttackable = false;

        switch (phase)
        {
            case 1:
                phase1EndEvents.Invoke();
                break;

            case 2:
                phase2EndEvents.Invoke();
                break;

            case 3:
                phase3EndEvents.Invoke();
                break;
        }

        phase++;
    }
    public void CallDestroyFunction()
    {
        CommonDestroyFunction();
    }

    public void InvokeActivateEnemy()
    {
        Invoke("ActivateEnemy", 3.0f);
    }
    public void ActivateEnemy()
    {
        Debug.Log("Activated");
        hp = objectInfo.HP;
        IsAttackable = true;
    }
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        isInvincible = false;
        isAttackable = true;

        mpbmController = GetComponent<PixyMPBMController>();
    }
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        weaponController = GetComponent<EnemyWeaponController>();
        Debug.Log(phase);
    }
}
