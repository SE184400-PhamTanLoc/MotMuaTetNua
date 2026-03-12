using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Controller generic cho Settings Panel trong gameplay scene.
/// Có confirm dialog cho Quit to Menu và Quit Game.
/// </summary>
public class InGameSettingsPanelController : MonoBehaviour
{
    public static bool IsAnySettingsOpen { get; private set; }

    [Header("Settings Panel")]
    public GameObject settingsPanelRoot;
    public GameObject gameplayHudRoot;
    public KeyCode toggleKey = KeyCode.Escape;
    public bool allowKeyboardToggle = true;
    public bool pauseGameWhenOpen = true;
    [Tooltip("Các script gameplay cần tắt khi mở settings (ví dụ MouseLook, PlayerMovement, PlayerInteractor).")]
    public MonoBehaviour[] scriptsToDisableWhileOpen;
    [Tooltip("Nếu danh sách trên trống, tự tìm các script gameplay phổ biến để tắt.")]
    public bool autoFindGameplayScriptsIfListEmpty = true;

    [Header("Buttons (optional if wired via OnClick)")]
    public Button openSettingsButton;
    public Button closeSettingsButton;
    public Button quitToMenuButton;
    public Button quitGameButton;

    [Header("Confirm Dialog")]
    public GameObject confirmDialogRoot;
    public TMP_Text confirmMessageTMP;
    public Text confirmMessageUGUI;
    public Button confirmYesButton;
    public Button confirmNoButton;
    [TextArea] public string quitToMenuConfirmMessage = "Bạn có chắc muốn về menu chính không?";
    [TextArea] public string quitGameConfirmMessage = "Bạn có chắc muốn thoát game không?";

    [Header("Scene")]
    public string mainMenuSceneName = "StartMenu";

    private enum ConfirmAction
    {
        None,
        QuitToMenu,
        QuitGame
    }

    private ConfirmAction pendingConfirmAction = ConfirmAction.None;
    private bool isOpen;

    private void Start()
    {
        AudioSettingsManager.InstanceOrCreate();
        AutoPopulateGameplayScriptsIfNeeded();

        if (settingsPanelRoot != null) settingsPanelRoot.SetActive(false);
        if (confirmDialogRoot != null) confirmDialogRoot.SetActive(false);
        isOpen = false;
        IsAnySettingsOpen = false;
    }

    private void OnEnable()
    {
        if (openSettingsButton != null) openSettingsButton.onClick.AddListener(OpenSettings);
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);
        if (quitToMenuButton != null) quitToMenuButton.onClick.AddListener(RequestQuitToMenu);
        if (quitGameButton != null) quitGameButton.onClick.AddListener(RequestQuitGame);
        if (confirmYesButton != null) confirmYesButton.onClick.AddListener(OnConfirmYes);
        if (confirmNoButton != null) confirmNoButton.onClick.AddListener(OnConfirmNo);
    }

    private void OnDisable()
    {
        if (openSettingsButton != null) openSettingsButton.onClick.RemoveListener(OpenSettings);
        if (closeSettingsButton != null) closeSettingsButton.onClick.RemoveListener(CloseSettings);
        if (quitToMenuButton != null) quitToMenuButton.onClick.RemoveListener(RequestQuitToMenu);
        if (quitGameButton != null) quitGameButton.onClick.RemoveListener(RequestQuitGame);
        if (confirmYesButton != null) confirmYesButton.onClick.RemoveListener(OnConfirmYes);
        if (confirmNoButton != null) confirmNoButton.onClick.RemoveListener(OnConfirmNo);
    }

    private void Update()
    {
        if (!allowKeyboardToggle) return;
        if (!Input.GetKeyDown(toggleKey)) return;

        // Nếu đang hiện confirm thì ESC đóng confirm trước.
        if (confirmDialogRoot != null && confirmDialogRoot.activeSelf)
        {
            OnConfirmNo();
            return;
        }

        if (isOpen) CloseSettings();
        else OpenSettings();
    }

    public void OpenSettings()
    {
        if (settingsPanelRoot != null) settingsPanelRoot.SetActive(true);
        if (gameplayHudRoot != null) gameplayHudRoot.SetActive(false);
        if (confirmDialogRoot != null) confirmDialogRoot.SetActive(false);
        pendingConfirmAction = ConfirmAction.None;

        isOpen = true;
        IsAnySettingsOpen = true;
        ApplyPauseState(true);
        SetGameplayScriptsEnabled(false);
    }

    public void CloseSettings()
    {
        if (settingsPanelRoot != null) settingsPanelRoot.SetActive(false);
        if (gameplayHudRoot != null) gameplayHudRoot.SetActive(true);
        if (confirmDialogRoot != null) confirmDialogRoot.SetActive(false);
        pendingConfirmAction = ConfirmAction.None;

        isOpen = false;
        IsAnySettingsOpen = false;
        ApplyPauseState(false);
        SetGameplayScriptsEnabled(true);
    }

    public void RequestQuitToMenu()
    {
        OpenConfirm(quitToMenuConfirmMessage, ConfirmAction.QuitToMenu);
    }

    public void RequestQuitGame()
    {
        OpenConfirm(quitGameConfirmMessage, ConfirmAction.QuitGame);
    }

    private void OpenConfirm(string message, ConfirmAction action)
    {
        pendingConfirmAction = action;
        SetConfirmMessage(message);
        if (confirmDialogRoot != null) confirmDialogRoot.SetActive(true);
    }

    private void OnConfirmYes()
    {
        switch (pendingConfirmAction)
        {
            case ConfirmAction.QuitToMenu:
                IsAnySettingsOpen = false;
                ApplyPauseState(false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (string.IsNullOrWhiteSpace(mainMenuSceneName))
                {
                    Debug.LogWarning("InGameSettingsPanelController: mainMenuSceneName đang rỗng.");
                }
                else
                {
                    SceneManager.LoadScene(mainMenuSceneName);
                }
                break;

            case ConfirmAction.QuitGame:
                IsAnySettingsOpen = false;
                ApplyPauseState(false);
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }

        pendingConfirmAction = ConfirmAction.None;
        if (confirmDialogRoot != null) confirmDialogRoot.SetActive(false);
    }

    private void OnConfirmNo()
    {
        pendingConfirmAction = ConfirmAction.None;
        if (confirmDialogRoot != null) confirmDialogRoot.SetActive(false);
    }

    private void SetConfirmMessage(string message)
    {
        if (confirmMessageTMP != null) confirmMessageTMP.text = message;
        if (confirmMessageUGUI != null) confirmMessageUGUI.text = message;
    }

    private void ApplyPauseState(bool paused)
    {
        if (!pauseGameWhenOpen) return;
        Time.timeScale = paused ? 0f : 1f;
    }

    private void SetGameplayScriptsEnabled(bool enabled)
    {
        if (scriptsToDisableWhileOpen == null || scriptsToDisableWhileOpen.Length == 0) return;

        for (int i = 0; i < scriptsToDisableWhileOpen.Length; i++)
        {
            MonoBehaviour mb = scriptsToDisableWhileOpen[i];
            if (mb != null)
            {
                mb.enabled = enabled;
            }
        }
    }

    private void AutoPopulateGameplayScriptsIfNeeded()
    {
        if (!autoFindGameplayScriptsIfListEmpty) return;
        if (scriptsToDisableWhileOpen != null && scriptsToDisableWhileOpen.Length > 0) return;

        List<MonoBehaviour> list = new List<MonoBehaviour>();

        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null) list.Add(playerMovement);

        MouseLook mouseLook = FindFirstObjectByType<MouseLook>();
        if (mouseLook != null) list.Add(mouseLook);

        PlayerInteractor playerInteractor = FindFirstObjectByType<PlayerInteractor>();
        if (playerInteractor != null) list.Add(playerInteractor);

        CameraStateController cameraStateController = FindFirstObjectByType<CameraStateController>();
        if (cameraStateController != null) list.Add(cameraStateController);

        scriptsToDisableWhileOpen = list.ToArray();
    }
}
