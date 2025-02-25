using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private static MainMenuController instance = null;

    [SerializeField]
    PlayerInput playerInput;

    [SerializeField]
    FadeController fadeController;

    [SerializeField]
    GameObject mainMenuScreen;
    [SerializeField]
    GameObject settingsScreen;
    [SerializeField]
    GameObject resultScreen;

    [SerializeField]
    TextMeshProUGUI descriptionText;

    [SerializeField]
    float initDelay;

    [Header("Audios")]
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    AudioClip scrollAudioClip;
    [SerializeField]
    AudioClip confirmAudioClip;
    [SerializeField]
    AudioClip backAudioClip;

    GameObject currentActiveScreen = null;
    MenuController currentMenuController = null;

    public void SetDescriptionText(string text)
    {
        descriptionText.text = text;
    }

    public static MainMenuController Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    public static PlayerInput PlayerInput
    {
        get { return Instance?.playerInput; }
    }

    public void Navigate(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Started)
        {
            PlayScrollAudioClip();
            currentMenuController?.Navigate(context);
        }
    }

    public void Confirm(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Started)
        {
            PlayConfirmAudioClip();
            currentMenuController?.Confirm(context);
        }
    }

    public void Back(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Started)
        {
            PlayBackAudioClip();
            currentMenuController?.Back(context);
        }
    }

    public void PlayConfirmAudioClip()
    {
        audioSource.PlayOneShot(confirmAudioClip);
    }

    public void PlayScrollAudioClip()
    {
        audioSource.PlayOneShot(scrollAudioClip);
    }

    public void PlayBackAudioClip()
    {
        audioSource.PlayOneShot(backAudioClip);
    }

    void SetCurrentActiveScreen(GameObject screenObject)
    {
        currentActiveScreen?.SetActive(false);
        currentActiveScreen = screenObject;
        currentActiveScreen.SetActive(true);
    }

    public void SetLanguage(string language)
    {
        if (language.ToLower() == "en") GameSettings.languageSetting = GameSettings.Language.EN;
        if (language.ToLower() == "kr") GameSettings.languageSetting = GameSettings.Language.KR;
    }

    public void SetDifficulty(int difficulty)
    {
        GameSettings.difficultySetting = (GameSettings.Difficulty)difficulty;
    }

    public void ShowMainMenu()
    {
        SetCurrentActiveScreen(mainMenuScreen);
    }

    public void ShowSettingsMenu()
    {
        SetCurrentActiveScreen(settingsScreen);
    }

    public void ShowResultMenu()
    {
        SetCurrentActiveScreen(resultScreen);
    }

    public void StartMission()
    {
        playerInput.enabled = false;
        LoadingController.sceneName = "ZERO";

        fadeController.OnFadeOutComplete.AddListener(ReserveLoadScene);
        fadeController.FadeOut();

        currentActiveScreen.GetComponent<MenuController>().enabled = false;
    }

    public void ReserveLoadScene()
    {
        SceneManager.LoadScene("Loading");
    }

    public void Quit()
    {
        playerInput.enabled = false;

        fadeController.OnFadeOutComplete.AddListener(QuitEvent);
        fadeController.FadeOut();
    }

    void QuitEvent()
    {
        Application.Quit();
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        mainMenuScreen.SetActive(false);
        resultScreen.SetActive(false);
        settingsScreen.SetActive(false);
    }

    void Start()
    {
        if (ResultData.missionName != "")
        {
            SetCurrentActiveScreen(resultScreen);
        }
        else
        {
            SetCurrentActiveScreen(mainMenuScreen);
        }
    }
}
