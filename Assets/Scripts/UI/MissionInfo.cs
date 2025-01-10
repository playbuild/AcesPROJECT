using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionInfo", menuName = "Scriptable Object Asset/MissionInfo")]
public class MissionInfo : ScriptableObject
{
    [SerializeField]
    string missionName;

    [Header("S/A/B/C Rank Score")]
    [SerializeField]
    int[] rankScoreCutoff = new int[4] { 40000, 30000, 20000, 10000 };
    [SerializeField]
    int timeLimit;
    [SerializeField]
    int timeBonusPerSecond;

    [SerializeField]
    TextAsset scriptJson;
    [SerializeField]
    TextAsset script_EN;
    [SerializeField]
    TextAsset script_KR;

    public string MissionName
    {
        get { return missionName; }
    }

    public int[] RankScoreCutoff
    {
        get { return rankScoreCutoff; }
    }

    public int TimeLimit
    {
        get { return timeLimit; }
    }

    public int TimeBonusPerSecond
    {
        get { return timeBonusPerSecond; }
    }

    public TextAsset ScriptJSON
    {
        get { return scriptJson; }
    }
    public TextAsset GetScriptXML()
    {
        return script_EN;
    }
}
