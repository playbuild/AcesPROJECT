using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertUIController : MonoBehaviour
{
    [Header("Warning/Alert Label Objects")]
    // Attack
    [SerializeField]
    GameObject caution;
    [SerializeField]
    GameObject warning;
    [SerializeField]
    GameObject missileAlert;

    // Status
    [SerializeField]
    GameObject pullUp;
    [SerializeField]
    GameObject stalling;
    [SerializeField]
    GameObject damaged;

    // Misc
    [SerializeField]
    GameObject autopilot;
    [SerializeField]
    GameObject fire;
    [SerializeField]
    GameObject missileReloading;

    // Category : Attack

    // Category : Status

    // Misc.
    void ShowAutopilotUI()
    {
        if (GameManager.PlayerFighterController.IsAutoPilot != autopilot.activeInHierarchy)
            autopilot.SetActive(GameManager.PlayerFighterController.IsAutoPilot);
    }

    void ShowStallingUI()
    {
        if (GameManager.PlayerFighterController.IsStalling != stalling.activeInHierarchy)
            stalling.SetActive(GameManager.PlayerFighterController.IsStalling);
    }


    // Update is called once per frame
    void Update()
    {
        ShowAutopilotUI();
        ShowStallingUI();
    }
}
