using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIController uiController;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    private bool isFastForwardActive = false;

    private void Awake()
    {
        if (gameManager == null) gameManager = GameManager.Instance;
        if (uiController == null) uiController = FindObjectOfType<UIController>();
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClick);
    }

    // ======================================================
    // FAST FORWARD
    // ======================================================
    public void OnFastForwardButtonPressed()
    {
        PlayClickSound();

        isFastForwardActive = !isFastForwardActive;
        gameManager?.SetFastForward(isFastForwardActive);

        uiController?.SetFastForwardVisual(isFastForwardActive);
    }
    
    // ======================================================
    // PAUSE
    // ======================================================
    public void OnPauseButton()
    {
        PlayClickSound();
        gameManager?.TogglePause();
    }

    public void OnResumeButton()
    {
        PlayClickSound();
        gameManager?.TogglePause();
    }

    // ======================================================
    // RESTART
    // ======================================================
    public void OnRestartButton()
    {
        PlayClickSound();
        Time.timeScale = 1f;

        isFastForwardActive = false;
        gameManager?.RestartGame();
    }

    // ======================================================
    // MAIN MENU
    // ======================================================
    public void OnMainMenuButton()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ======================================================
    // EXIT
    // ======================================================
    public void OnExitButton()
    {
        PlayClickSound();
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ======================================================
    // SETTINGS
    // ======================================================
    public void OnSettingsButton()
    {
        PlayClickSound();
        if (settingsPanel != null)
            settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    // ======================================================
    // LANGUAGE DROPDOWN
    // ======================================================
    public void OnLanguageDropdownChanged(int index)
    {
        PlayClickSound();
        uiController?.OnLanguageChanged(index);
    }
}