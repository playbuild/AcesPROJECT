using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissionZERO : MissionManager
{
    int phase = 1;

    [Header("Phase System")]
    [Header("Phase 1")]
    [SerializeField]
    List<string> onPhase1StartScripts;
    [SerializeField]
    List<string> onPhase1EndScripts;

    [Header("Phase 2")]
    [SerializeField]
    UnityEvent onPhase2StartEvents;
    [SerializeField]
    List<string> onPhase2StartScripts;
    [SerializeField]
    List<string> onPhase2EndScripts;

    [Header("Phase 3")]
    [SerializeField]
    UnityEvent onPhase3StartEvents;
    [SerializeField]
    List<string> onPhase3StartScripts;
    [SerializeField]
    List<string> onPhase3EndScripts;

    [Space(10)]
    [SerializeField]
    PixyScript pixy;
    // Start is called before the first frame update
    public void OnPhaseEnd()
    {
        switch (phase)
        {
            case 1:
                GameManager.ScriptManager.AddScript(onPhase1EndScripts);
                break;

            case 2:
                GameManager.ScriptManager.AddScript(onPhase2EndScripts);
                break;

            case 3:
                GameManager.ScriptManager.AddScript(onPhase3EndScripts);
                break;
        }
        ++phase;
    }
    public void OnPhaseStart()
    {
        switch (phase)
        {
            case 1:
                GameManager.ScriptManager.AddScript(onPhase1StartScripts);
                break;

            case 2:
                GameManager.ScriptManager.AddScript(onPhase2StartScripts);
                onPhase2StartEvents.Invoke();
                break;

            case 3:
                GameManager.ScriptManager.AddScript(onPhase3StartScripts);
                onPhase3StartEvents.Invoke();
                break;
        }
    }
    public void Phase1Start()
    {
        Debug.Log("Phase 1 Start");
        OnPhaseStart();
    }
}
