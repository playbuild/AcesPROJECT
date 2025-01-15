using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    List<RectTransform> selectableOptions;
    [SerializeField]
    RectTransform selectIndicator;

    [SerializeField]
    UnityEvent onBackEvent;

    [SerializeField]
    TextMeshProUGUI descriptionText;

    [SerializeField]
    AudioSource audioSource;

    int currentIndex;

    UISelect GetCurrentUISelect()
    {
        return selectableOptions[currentIndex].GetComponent<UISelect>();
    }

    void ChangeSelection()
    {
        // Cursor
        selectIndicator.anchoredPosition = new Vector2(selectIndicator.anchoredPosition.x,
                                                       selectableOptions[currentIndex].anchoredPosition.y);
        // Description
        // descriptionText.text = GetCurrentUISelect()?.Description;
    }


    IEnumerator ConfirmCoroutine()
    {
        MainMenuController.PlayerInput.enabled = false;
        yield return new WaitForSeconds(0.3f);
        MainMenuController.PlayerInput.enabled = true;
        GetCurrentUISelect()?.OnSelectEvent.Invoke();
    }

    public void Navigate(InputAction.CallbackContext context)
    {
        float y = context.ReadValue<Vector2>().y;

        if (y == -1 && currentIndex < selectableOptions.Count - 1)
        {
            ++currentIndex;
        }
        else if (y == 1 && currentIndex > 0)
        {
            --currentIndex;
        }
        else return;

        MainMenuController.Instance.PlayScrollAudioClip();
        ChangeSelection();
    }

    public void Confirm(InputAction.CallbackContext context)
    {
        selectIndicator.GetComponent<Animation>().Play();
        MainMenuController.Instance.PlayConfirmAudioClip();

        StartCoroutine(ConfirmCoroutine());
    }

    public void Back(InputAction.CallbackContext context)
    {
        MainMenuController.Instance.PlayBackAudioClip();
        onBackEvent.Invoke();
    }

    void OnEnable()
    {
        if (MainMenuController.PlayerInput == null) return;

        currentIndex = 0;
        ChangeSelection();

        InputAction navigateAction = MainMenuController.PlayerInput.actions.FindAction("Navigate");
        navigateAction.started += Navigate;
        InputAction submitAction = MainMenuController.PlayerInput.actions.FindAction("Submit");
        submitAction.started += Confirm;
        InputAction backAction = MainMenuController.PlayerInput.actions.FindAction("Back");
        backAction.started += Back;
    }

    void OnDisable()
    {
        if (MainMenuController.PlayerInput == null) return;

        InputAction navigateAction = MainMenuController.PlayerInput.actions.FindAction("Navigate");
        navigateAction.started -= Navigate;
        InputAction submitAction = MainMenuController.PlayerInput.actions.FindAction("Submit");
        submitAction.started -= Confirm;
        InputAction backAction = MainMenuController.PlayerInput.actions.FindAction("Back");
        backAction.started -= Back;
    }
}