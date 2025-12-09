using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Congklak/Game Config", order = 1)]
public class GameConfig : ScriptableObject
{
    [Header("Gameplay Settings")]
    [Tooltip("Initial seeds per hole (typical Congklak = 7)")]
    public int initialSeedsPerHole = 7;

    [Header("Animation")]
    [Tooltip("Delay between each seeded drop when animating (seconds)")]
    public float animationSpeed = 0.1f;
    public float fastForwardSpeedMultiplier = 5f;
    public bool isFastForwardActive = false;

    [Header("Audio Settings")]
    public float musicVolume = 0.1f;
    public float sfxVolume = 0.5f;
    public bool soundEnabled = true;

    [Header("AI & Mode")]
    public AILevel aiDifficulty = AILevel.Medium;
    public GameMode gameMode = GameMode.PlayerVsPlayer;

    [Header("Tutorial")]
    [Tooltip("Set ke true jika pop-up pengenalan awal sudah dilihat")]
    public bool hasSeenIntroTutorial = false;
    [Tooltip("Set ke true jika tips 'Extra Turn' sudah dilihat")]
    public bool hasSeenExtraTurnTip = false;
    [Tooltip("Set ke true jika tips 'Capture (Nembak)' sudah dilihat")]
    public bool hasSeenCaptureTip = false;
    [Tooltip("Set ke true jika tips 'Sow Continue' (bukan extra turn) sudah dilihat")]
    public bool hasSeenSowContinueTip = false;

    [Header("Localization")]
    [Tooltip("Bahasa yang sedang digunakan")]
    public Language currentLanguage = Language.Indonesian; // Default: Indonesia
}

public enum AILevel { Easy, Medium, Hard }
public enum GameMode { PlayerVsPlayer, PlayerVsAI }
public enum Language { Indonesian, English }