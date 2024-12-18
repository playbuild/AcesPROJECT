using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    public GameObject targetUIObject;
    List<TargetUI> targetUIs;
    TargetUI currentTargettedUI;
    TargetObject lockedTarget;

    [SerializeField]
    TargetArrow targetArrow;

    [SerializeField]
    TargetLock targetLock;

    public bool IsLocked
    {
        get { return targetLock.IsLocked; }
    }
    void Start()
    {
        if (lockedTarget != null)
        {
            GameManager.UIController.SetTargetText(lockedTarget.Info);
            targetArrow.SetTarget(lockedTarget);    // Set Arrow UI
        }
    }
    public void CreateTargetUI(TargetObject targetObject)
    {
        GameObject obj = Instantiate(targetUIObject);
        TargetUI targetUI = obj.GetComponent<TargetUI>();
        targetUI.Target = targetObject;
        targetUIs.Add(targetUI);

        obj.transform.SetParent(transform, false);
    }

    public void RemoveTargetUI(TargetObject targetObject)
    {
        TargetUI targetUI = FindTargetUI(targetObject);
        if (targetUI.Target != null)
        {
            targetUIs.Remove(targetUI);
            Destroy(targetUI.gameObject);
        }
    }

    public void ChangeTarget(TargetObject lockedTarget)
    {
        targetArrow.SetTarget(lockedTarget);
        GameManager.UIController.SetTargetText(lockedTarget.Info);

        TargetUI targetUI = FindTargetUI(lockedTarget);
        if (targetUI.Target != null)
        {
            currentTargettedUI.SetTargetted(false);
            currentTargettedUI = targetUI;
            targetUI.SetTargetted(true);
        }
    }
    public void ShowTargetArrow(bool show)
    {
        targetArrow.SetArrowVisible(show);
    }

    public TargetUI FindTargetUI(TargetObject targetObject)
    {
        foreach (TargetUI targetUI in targetUIs)
        {
            if (targetUI.Target == lockedTarget)
            {
                return targetUI;
            }
        }
        return null;
    }
    public void SetTargetUILock(bool isLocked)
    {
        currentTargettedUI?.SetLock(isLocked);
    }
}
