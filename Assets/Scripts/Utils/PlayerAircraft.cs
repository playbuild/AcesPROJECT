using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerAircraft : TargetObject
{
    [SerializeField]
    float rotateSpeedOnDestroy = 600;

    int score = 0;
    UIController uiController;


    public override void OnDamage(float damage, int layer)
    {
        base.OnDamage(damage, layer);
        uiController.SetDamage((int)(Info.HP - hp / Info.HP * 100));
    }
    public void OnScore(int score)
    {
        this.score += score;
        uiController.SetScoreText(this.score);
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
    }
}
