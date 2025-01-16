using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    [SerializeField]
    PlayerInput playerInput;

    [Header("Object Pools")]
    public ObjectPools smokeTrailDamagePool;
    public ObjectPools bulletObjectPool;
    public ObjectPools missileObjectPool;
    public ObjectPools specialWeaponObjectPool;
    public ObjectPools bulletHitEffectObjectPool;
    public ObjectPools groundHitEffectObjectPool;
    public ObjectPools BigExplosionPool;
    public ObjectPools smokeTrailEffectObjectPool;
    public ObjectPools borderIncicatorObjectPool;
    public ObjectPools mpbmEffectObjectPool;

    [Header("Controllers")]
    [SerializeField]
    UIController uiController;
    [SerializeField]
    PlayerFighterController playerFighterController;
    [SerializeField]
    PlayerAircraft playerAircraft;
    [SerializeField]
    WeaponController weaponController;
    [SerializeField]
    CameraController cameraController;
    [SerializeField]
    TargetController targetController;

    [Header("Mission Managers")]
    [SerializeField]
    ScriptManager scriptManager;
    [SerializeField]
    MissionManager missionManager;

    [Header("Pause Control")]
    bool isPaused = false;

    [SerializeField]
    FadeController fadeController;

    [SerializeField]
    PauseController pauseController;

    [SerializeField]
    List<GameObject> hideOnPause;
    [SerializeField]
    GameObject pauseUICanvas;

    [Header("Game Over Control")]
    [SerializeField]
    GameObject gameOverUICanvas;
    [SerializeField]
    AudioSource gameOverAudioSource;

    [SerializeField]
    PauseController gameOverController;

    [SerializeField]
    List<GameObject> disableOnShowGameOverUI;

    bool isGameOver = false;

    [SerializeField]
    UnityEvent executeOnGameOver;
    [SerializeField]
    List<GameObject> disableOnGameOver;
    [SerializeField]
    List<GameObject> disableOnGameOverObjects;
    [SerializeField]
    List<GameObject> enableOnGameOverObjects;

    [SerializeField]
    DeathCam deathCam;

    [SerializeField]
    AudioController audioController;

    List<TargetObject> objects = new List<TargetObject>();

    [SerializeField]
    Color normalColor;
    [SerializeField]
    Color warningColor;

    public static Color NormalColor
    {
        get { return Instance.normalColor; }
    }

    public static Color WarningColor
    {
        get { return Instance.warningColor; }
    }

    public static PlayerFighterController PlayerFighterController
    {
        get { return Instance?.playerFighterController; }
    }
    public static PlayerAircraft PlayerAircraft
    {
        get { return Instance?.playerAircraft; }
    }
    public static WeaponController WeaponController
    {
        get { return Instance?.weaponController; }
    }
    public static ScriptManager ScriptManager
    {
        get { return Instance?.scriptManager; }
    }
    public static MissionManager MissionManager
    {
        get { return Instance?.missionManager; }
    }
    public static CameraController CameraController
    {
        get { return Instance?.cameraController; }
    }
    public static UIController UIController
    {
        get { return Instance?.uiController; }
    }
    public static TargetController TargetController
    {
        get { return Instance?.targetController; }
    }
    public static PlayerInput PlayerInput
    {
        get { return Instance?.playerInput; }
    }
    public bool IsGameOver
    {
        get { return isGameOver; }
    }
    public bool IsPaused
    {
        get { return isPaused; }
    }
    public static GameManager Instance
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
    public void AddEnemy(TargetObject targetObject)
    {
        objects.Add(targetObject);
    }

    public void RemoveEnemy(TargetObject targetObject)
    {
        objects.Remove(targetObject);
    }
    public float GetDistanceFromPlayer(Transform otherTransform)
    {
        return Vector3.Distance(otherTransform.position, playerAircraft.transform.position);
    }
    public static float GetAngleBetweenTransform(Transform otherTransform)
    {
        Vector3 direction = PlayerAircraft.transform.forward;
        Vector3 diff = otherTransform.position - PlayerAircraft.transform.position;
        return Vector3.Angle(diff, direction);
    }
    public List<TargetObject> GetTargetsWithinDistance(float distance, float searchAngle = 0, bool getNearestTarget = false)
    {
        List<TargetObject> objectsWithinDistance = new List<TargetObject>();
        TargetObject nearestTarget = null;
        float minDistance = distance;

        foreach (TargetObject targetObject in objects)
        {
            // Search within searchAngle
            if (searchAngle != 0)
            {
                if (GetAngleBetweenTransform(targetObject.transform) > searchAngle) continue;
            }

            float targetDistance = Vector3.Distance(targetObject.transform.position, playerAircraft.transform.position);

            if (targetDistance < distance)
            {
                if (getNearestTarget == true && targetDistance < minDistance)
                {
                    nearestTarget = targetObject;
                    minDistance = targetDistance;
                }
                objectsWithinDistance.Add(targetObject);
            }
            else
            {
                targetObject.isNextTarget = false;
            }
        }

        if (getNearestTarget == true)
        {
            objectsWithinDistance.Clear();
            objectsWithinDistance.Add(nearestTarget);
        }
        return objectsWithinDistance;
    }
    public void CreateDamageSmokeEffect(Transform transform)
    {
        GameObject smokeTrailObject = smokeTrailDamagePool.GetPooledObject();
        if (smokeTrailObject != null)
        {
            smokeTrailObject.SetActive(true);
            smokeTrailObject.GetComponent<SmokeTrail>()?.SetFollowTransform(transform);
        }
    }
    public void GameOver(bool isDead, bool isInstantDeath = false)
    {
        float gameOverFadeOutDelay = 5.0f;
        audioController.TargetBGMVolume = AudioController.MIN_VOLUME;

        // Set UI
        UIController.SetLabel(AlertUIController.LabelEnum.MissionFailed);

        foreach (TargetObject obj in objects)
        {
            obj.DeleteMinimapSprite();
        }

        executeOnGameOver.Invoke();

        targetController.RemoveAllTargetUI();
        objects.Clear();
        weaponController.ChangeTarget();

        foreach (GameObject obj in disableOnGameOver)
        {
            obj.SetActive(false);
        }

        if (isDead)
        {
            foreach (GameObject obj in disableOnGameOverObjects)
            {
                obj.SetActive(false);
            }
            foreach (GameObject obj in enableOnGameOverObjects)
            {
                obj.SetActive(true);
            }

            playerInput.enabled = false;
            deathCam.PlayAnimation(isInstantDeath);

            gameOverFadeOutDelay = 7.0f;

            if (isGameOver == false)
            {
                gameOverAudioSource.Play();
            }
        }

        isGameOver = true;
        scriptManager.ClearScriptQueue(true);

        ResultData.elapsedTime += GameManager.UIController.StopCountAndGetElapsedTime();
        missionManager.OnGameOver(isDead);
        Invoke("GameOverFadeOut", gameOverFadeOutDelay);
    }
    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            if (isGameOver == true) return;
            Pause();
        }
    }
    public void Pause()
    {
        Time.timeScale = (isPaused == true) ? 1 : 0;
        isPaused = (Time.timeScale == 0);
        uiController.MinimapCamera.SetPauseMinimapCamera(isPaused);

        foreach (GameObject obj in hideOnPause)
        {
            obj.SetActive(!isPaused);
        }
        pauseUICanvas.SetActive(isPaused);

        AudioListener.pause = isPaused;

        string actionMapName = (isPaused == true) ? "UI" : "FighterAction";
        playerInput.SwitchCurrentActionMap(actionMapName);
        pauseController.enabled = isPaused;
    }
    public void RestartFromCheckpoint()
    {
        playerInput.enabled = false;
        pauseController.enabled = false;
        gameOverController.enabled = false;

        fadeController.OnFadeOutComplete.AddListener(RestartFromCheckpointEvent);
        fadeController.FadeOut();
    }

    void RestartFromCheckpointEvent()
    {
        missionManager.SetupForRestartFromCheckpoint();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RestartMission()
    {
        MissionManager.phase = 1;
        ResultData.elapsedTime = 0;

        playerInput.enabled = false;
        pauseController.enabled = false;
        gameOverController.enabled = false;

        fadeController.OnFadeOutComplete.AddListener(RestartMissionEvent);
        fadeController.FadeOut();
    }

    void RestartMissionEvent()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitMission()
    {
        playerInput.enabled = false;
        pauseController.enabled = false;
        gameOverController.enabled = false;

        fadeController.OnFadeOutComplete.AddListener(QuitMissionEvent);
        fadeController.FadeOut();
    }

    void QuitMissionEvent()
    {
        ResultData.missionName = "";
        SceneManager.LoadScene("Title");
    }

    public void ShowResultScene()
    {
        SceneManager.LoadScene("Title");
    }
    void GameOverFadeOut()
    {
        CancelInvoke();

        fadeController.OnFadeOutComplete.AddListener(ShowGameOverUI);
        fadeController.FadeOut(FadeController.FadeInReserveType.FadeIn);
    }
    void ShowGameOverUI()
    {
        Time.timeScale = 0;
        AudioListener.pause = true;
        EnablePlayerInput(false);
        gameOverUICanvas.SetActive(true);

        foreach (GameObject obj in disableOnShowGameOverUI)
        {
            obj.SetActive(false);
        }
    }
    public void EnablePlayerInput(bool usePlayerActionMap = true)
    {
        playerInput.enabled = true;
        playerInput.actions.Disable();

        string actionMapName = (usePlayerActionMap == true) ? "FighterAction" : "UI";
        playerInput.SwitchCurrentActionMap(actionMapName);
    }
    public void SetGlobalFog(bool value)
    {
        RenderSettings.fog = value;
    }
    void Awake()
    {
        if (instance == null)
            {
                instance = this;
            }
        Time.timeScale = 1;
        AudioListener.pause = false;
    }
}
