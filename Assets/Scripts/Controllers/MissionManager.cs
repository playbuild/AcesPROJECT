using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    [Header("Game Properties")]
    [SerializeField]
    MissionInfo missionInfo;

    [SerializeField]
    int timeLimit;

    [Header("Common Scripts")]
    [SerializeField]
    List<string> onMissionStartScripts;
    [SerializeField]
    List<string> onMissionAccomplishedScripts;
    [SerializeField]
    List<string> onMissionFailedScripts;
    [SerializeField]
    List<string> onDeadScripts;


    public MissionInfo MissionInfo
    {
        get { return missionInfo; }
    }

    public void InvokeMethod(string methodName, float delay)
    {
        Invoke(methodName, delay);
    }
    public virtual void OnGameOver(bool isDead)
    {
        GameManager.ScriptManager.ClearScriptQueue();

        if (isDead)
        {
            int index = UnityEngine.Random.Range(0, onDeadScripts.Count);
            GameManager.ScriptManager.AddScript(onDeadScripts[index]);
        }
        else
        {
            GameManager.ScriptManager.AddScript(onMissionFailedScripts);
        }
    }

    void Start()
    {
        GameManager.UIController.SetRemainTime(timeLimit);
        Debug.Log(onMissionStartScripts.Count);
        GameManager.ScriptManager.AddScript(onMissionStartScripts);
    }
}
