using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private GameConfig gameConfig;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject modeSelectPanel;
    [SerializeField] private GameObject aiDifficultyPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Settings UI Elements")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TMP_Text musicVolumeLabel;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Text sfxVolumeLabel;
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private TMP_Dropdown languageDropdown;

    [Header("Menu Text References")]
    [SerializeField] private TMP_Text playButtonText;
    [SerializeField] private TMP_Text settingsButtonText;
    [SerializeField] private TMP_Text quitButtonText;
    [SerializeField] private TMP_Text settingsPanelTitle;
    [SerializeField] private TMP_Text languageLabel;

    private void Start()
    {

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic();
        }

        LoadSettingsToUI();

        ShowMainMenu();
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.buttonClick != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClick);
        }
    }

    public void ShowModeSelectPanel()
    {
        PlayClickSound();
        mainMenuPanel.SetActive(false);
        modeSelectPanel.SetActive(true);
        aiDifficultyPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void ShowAIDifficultyPanel()
    {
        PlayClickSound();
        mainMenuPanel.SetActive(false);
        modeSelectPanel.SetActive(false);
        aiDifficultyPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        PlayClickSound();
        mainMenuPanel.SetActive(true);
        modeSelectPanel.SetActive(false);
        aiDifficultyPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void ShowSettingsPanel()
    {
        PlayClickSound();
        mainMenuPanel.SetActive(false);
        modeSelectPanel.SetActive(false);
        aiDifficultyPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    
    // --- Logika Pemilihan Mode ---

    public void SelectGameMode(int mode)
    {
        if (gameConfig == null)
        {
            Debug.LogError("GameConfig belum di-assign di MainMenuController!");
            return;
        }

        if (mode == 0) 
        {
            PlayClickSound();
            gameConfig.gameMode = GameMode.PlayerVsPlayer;
            StartGame();
        }
        else 
        {
            gameConfig.gameMode = GameMode.PlayerVsAI;
            ShowAIDifficultyPanel();
        }
    }
    
    public void SelectDifficulty(int level)
    {
        PlayClickSound();
        if (gameConfig != null)
        {
            gameConfig.aiDifficulty = (AILevel)level;
            StartGame();
        }
    }

    private void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ExitGame()
    {
        PlayClickSound();
        Debug.Log("Keluar dari permainan...");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void LoadSettingsToUI()
    {
        if (gameConfig == null) return;
        
        musicVolumeSlider.value = gameConfig.musicVolume;
        sfxVolumeSlider.value = gameConfig.sfxVolume;
        soundToggle.isOn = gameConfig.soundEnabled;
        
        OnMusicVolumeChanged(gameConfig.musicVolume);
        OnSFXVolumeChanged(gameConfig.sfxVolume);
        OnSoundToggleChanged(gameConfig.soundEnabled);

        if (languageDropdown != null)
        {
            languageDropdown.value = (int)gameConfig.currentLanguage;
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }
        
        UpdateLanguageUI();
    }

    public void OnSettingsButton()
    {
        PlayClickSound();

        bool newState = !settingsPanel.activeSelf;
        settingsPanel.SetActive(newState);

        if (newState)
        {
        // FORCE REFRESH localizations in settings panel
            var localized = settingsPanel.GetComponentsInChildren<TMP_TextAutoTranslate>(true);
            foreach (var t in localized)
                t.Refresh();
        }
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (gameConfig != null) gameConfig.musicVolume = value;
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value);
        UpdateLanguageUI();
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (gameConfig != null) gameConfig.sfxVolume = value;
        if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value);
        UpdateLanguageUI();
    }

    public void OnSoundToggleChanged(bool isOn)
    {
        if (gameConfig != null) gameConfig.soundEnabled = isOn;
        if (AudioManager.Instance != null) AudioManager.Instance.ToggleSound(isOn);
    }

    /// <summary>
    ///
    /// </summary>
    public void OnLanguageChanged(int value)
    {
        if (gameConfig != null)
        {
            gameConfig.currentLanguage = (Language)value;
        }
        PlayClickSound();
        UpdateLanguageUI();
    }

    /// <summary>
    /// Menerjemahkan semua teks di Main Menu
    /// </summary>
    private void UpdateLanguageUI()
    {
        if (gameConfig == null) return;

        bool isIndonesian = gameConfig.currentLanguage == Language.Indonesian;

        // Terjemahkan semua teks
        if (playButtonText != null) playButtonText.text = isIndonesian ? "MAIN" : "PLAY";
        if (settingsButtonText != null) settingsButtonText.text = isIndonesian ? "PENGATURAN" : "SETTINGS";
        if (quitButtonText != null) quitButtonText.text = isIndonesian ? "KELUAR" : "QUIT";
        if (settingsPanelTitle != null) settingsPanelTitle.text = isIndonesian ? "Pengaturan" : "Settings";
        if (languageLabel != null) languageLabel.text = isIndonesian ? "Bahasa" : "Language";

        // Terjemahkan teks yang memiliki nilai (slider)
        if (musicVolumeLabel != null)
        {
            string label = isIndonesian ? "Musik" : "Music";
            musicVolumeLabel.text = $"{label}: {Mathf.RoundToInt(gameConfig.musicVolume * 100)}%";
        }
        if (sfxVolumeLabel != null)
        {
            string label = isIndonesian ? "SFX" : "SFX";
            sfxVolumeLabel.text = $"{label}: {Mathf.RoundToInt(gameConfig.sfxVolume * 100)}%";
        }
    }
}