using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Board References")]
    [SerializeField] private Hole[] playerOneHoles;
    [SerializeField] private Hole[] playerTwoHoles;
    [SerializeField] public Hole playerOneStorehouse;
    [SerializeField] public Hole playerTwoStorehouse;

    [Header("Gameplay Settings")]
    [SerializeField] private int startingSeeds = 7;
    [SerializeField] private float seedMoveSpeed = 5f;
    [SerializeField] private float liftHeight = 0.5f;

    [Header("References")]
    [SerializeField] private UIController uiController;
    [SerializeField] public GameConfig config;
    [SerializeField] public GameObject pausePanel;
    [SerializeField] private bool sowDebugLogs = false;
    [SerializeField] private AIController aiController;

    [Header("Game State")]
    public int currentPlayer = 1;
    public bool isGameActive = false;
    public bool isSowing = false;
    public bool IsPaused { get; private set; } = false;
    private bool isAITurn = false;

    [Header("Stats (for Game End)")]
    private int p1TotalMoves = 0;
    private int p2TotalMoves = 0;
    private int p1ExtraTurns = 0;
    private int p2ExtraTurns = 0;
    private int p1MaxSeedsCaptured = 0;
    private int p2MaxSeedsCaptured = 0;
    private float gameStartTime = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (config == null) { Debug.LogError("[GameManager] GameConfig belum di-assign!"); return; }
        if (uiController == null) { Debug.LogError("[GameManager] UIController belum di-assign!"); return; }

        startingSeeds = config.initialSeedsPerHole;
        InitializeBoard();
        UpdateUI();

        if (pausePanel != null) pausePanel.SetActive(false);
        IsPaused = false;
        Time.timeScale = 1f;
        gameStartTime = Time.time;

        if (config.gameMode == GameMode.PlayerVsAI && !config.hasSeenIntroTutorial)
        {
            StartIntroTutorial();
        }
        else
        {
            SetGameActive(true);
            // inform UI: game start
            uiController.UpdateGameStatus(GameEvent.GameStart, currentPlayer);
        }
    }

    private void InitializeBoard()
    {
        if (aiController != null)
        {
            aiController.Initialize(this, playerTwoHoles, playerOneHoles, playerTwoStorehouse, playerOneStorehouse);
        }

        foreach (Hole hole in playerOneHoles) hole.Initialize(startingSeeds, uiController);
        foreach (Hole hole in playerTwoHoles) hole.Initialize(startingSeeds, uiController);
        playerOneStorehouse.Initialize(0, uiController);
        playerTwoStorehouse.Initialize(0, uiController);

        currentPlayer = 1;
        isGameActive = false;
        isSowing = false;
        isAITurn = false;

        p1TotalMoves = 0; p2TotalMoves = 0; p1ExtraTurns = 0; p2ExtraTurns = 0; p1MaxSeedsCaptured = 0; p2MaxSeedsCaptured = 0;
    }

    public void RestartGame()
    {
        InitializeBoard();
        UpdateUI();
        if (uiController != null) uiController.HideGameOver();

        gameStartTime = Time.time;
        if (config.gameMode == GameMode.PlayerVsAI && !config.hasSeenIntroTutorial)
        {
            StartIntroTutorial();
        }
        else
        {
            SetGameActive(true);
            uiController.UpdateGameStatus(GameEvent.GameStart, currentPlayer);
        }
    }

    private void UpdateUI()
    {
        if (uiController == null) return;
        uiController.UpdateScores(playerOneStorehouse.GetSeedCount(), playerTwoStorehouse.GetSeedCount(), currentPlayer);

        uiController.UpdateGameStatus(GameEvent.TurnChanged, currentPlayer);
    }

    public void SelectHole(Hole hole)
    {
        if (isSowing || IsPaused) return;

        if (!isGameActive && !isAITurn) return;

        if (!IsValidMove(hole))
        {
            if (sowDebugLogs) Debug.Log($"[SelectHole] Gerakan tidak valid: {hole.name}");
            return;
        }

        if (currentPlayer == 1) p1TotalMoves++; else p2TotalMoves++;
        StartSowing(hole);
    }

    private void CheckGameOver()
    {
        int p1Seeds = 0; foreach (var hole in playerOneHoles) p1Seeds += hole.GetSeedCount();
        int p2Seeds = 0; foreach (var hole in playerTwoHoles) p2Seeds += hole.GetSeedCount();

        if (p1Seeds == 0 || p2Seeds == 0)
        {
            Debug.Log("[GameManager] Game Over! Salah satu sisi kosong.");
            isGameActive = false;

            int p1Remaining = p1Seeds;
            foreach (var hole in playerOneHoles) p1Remaining += hole.ClearSeeds();

            int p2Remaining = p2Seeds;
            foreach (var hole in playerTwoHoles) p2Remaining += hole.ClearSeeds();

            if (p1Remaining > 0) playerOneStorehouse.AddSeeds(p1Remaining);
            if (p2Remaining > 0) playerTwoStorehouse.AddSeeds(p2Remaining);

            if (uiController != null) uiController.UpdateScores(playerOneStorehouse.GetSeedCount(), playerTwoStorehouse.GetSeedCount(), 0);

            int p1Final = playerOneStorehouse.GetSeedCount();
            int p2Final = playerTwoStorehouse.GetSeedCount();

            string winner = (p1Final > p2Final) ? "Player 1" : ((p2Final > p1Final) ? "Player 2" : LocalizationManager.Get("DRAW"));

            float duration = Time.time - gameStartTime;
            int minutes = Mathf.FloorToInt(duration / 60);
            int seconds = Mathf.FloorToInt(duration % 60);

            // pass raw stats as numbers; UI will localize using keys
            int totalSeeds = p1Final + p2Final;

            if (uiController != null) uiController.ShowGameOver(winner, p1Final, p2Final, totalSeeds);
        }
    }

    private void SwitchPlayer()
    {
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        UpdateUI();

        if (config.gameMode == GameMode.PlayerVsAI && currentPlayer == 2)
        {
            isAITurn = true;
            isGameActive = false;
            StartCoroutine(HandleAITurn());
        }
    }

    public void UnpauseGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        // Kalau bukan giliran AI → aktifkan player input
        if (!isAITurn)
            isGameActive = true;
    }

    public void ResumeTurn()
    {
        // Jika barusan contextual tutorial muncul saat AI turn
        if (isAITurn)
        {
            StartCoroutine(HandleAITurn());
            return;
        }

        // Kalau sedang sowing → lanjutkan sperti biasa
        if (isSowing)
        {
            // biarkan coroutine sowing berjalan (sudah di-hold oleh yield WaitUntil)
            return;
        }

        // Kalau player harusnya bisa main, aktifkan game
        isGameActive = true;
    }


    private IEnumerator HandleAITurn()
    {
        Debug.Log("[AI] Memulai giliran AI...");
        yield return new WaitForSeconds(1.0f);

        if (aiController != null)
        {
            aiController.MakeMove(config.aiDifficulty);
        }
        else
        {
            isAITurn = false;
            Debug.LogError("[GameManager] AIController tidak ditemukan!");
        }
    }

    #region Sowing Logic (kept same)
    public void StartSowing(Hole startHole)
    {
        if (isSowing) return;
        isSowing = true;
        isGameActive = false;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.seedPickup);
        StartCoroutine(SowSeedsCoroutine(startHole));
    }

    private IEnumerator SowSeedsCoroutine(Hole startHole)
    {
        Hole currentHole = startHole;
        Hole lastHole = startHole;

        while (currentHole != null)
        {
            if (currentHole.GetHoleType() == HoleType.Storehouse) break;

            List<GameObject> seedsToSow = currentHole.TakeAllSeedObjects();
            int seedCount = seedsToSow.Count;
            if (seedCount == 0) break;

            for (int i = 0; i < seedCount; i++)
            {
                Hole previousHole = lastHole;
                lastHole = GetNextHole(previousHole);

                if (lastHole == null) { Debug.LogError("[SOW] Next hole is null!"); break; }

                if ((currentPlayer == 1 && lastHole == playerTwoStorehouse) ||
                    (currentPlayer == 2 && lastHole == playerOneStorehouse))
                {
                    lastHole = GetNextHole(lastHole);
                }

                float currentMoveSpeed = seedMoveSpeed;
                if (config != null && config.isFastForwardActive) currentMoveSpeed *= config.fastForwardSpeedMultiplier;

                yield return StartCoroutine(lastHole.AnimateAddSeedCoroutine(
                    seedsToSow[i], previousHole.GetSeedContainer(), liftHeight, currentMoveSpeed));

                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.seedDrop);
                if (uiController != null) uiController.UpdateScores(playerOneStorehouse.GetSeedCount(), playerTwoStorehouse.GetSeedCount(), currentPlayer);
            }

            if (lastHole == null) break;

            if (lastHole.GetHoleType() == HoleType.Storehouse)
            {
                currentHole = null;
            }
            else if (lastHole.GetSeedCount() == 1)
            {
                currentHole = null;
            }
            else
            {
                if (config.gameMode == GameMode.PlayerVsAI && !config.hasSeenSowContinueTip && lastHole.GetHoleType() != HoleType.Storehouse)
                {
                    config.hasSeenSowContinueTip = true;
                    PauseForTutorial(TutorialType.SowContinue);
                    yield return new WaitUntil(() => !IsPaused);
                }
                currentHole = lastHole;
            }
        }

        isSowing = false;

        if (lastHole != null)
        {
            bool wasCapture = false;
            if (lastHole.GetHoleType() != HoleType.Storehouse && lastHole.GetSeedCount() == 1)
            {
                Hole oppositeHole = GetOppositeHole(lastHole);
                if (oppositeHole != null && oppositeHole.GetSeedCount() > 0)
                {
                    if ((lastHole.GetHoleType() == HoleType.PlayerOne && currentPlayer == 1) ||
                        (lastHole.GetHoleType() == HoleType.PlayerTwo && currentPlayer == 2))
                    {
                        wasCapture = true;
                    }
                }
            }

            bool isExtraTurn = (currentPlayer == 1 && lastHole == playerOneStorehouse) ||
                               (currentPlayer == 2 && lastHole == playerTwoStorehouse);

            if (isExtraTurn)
            {
                if (config.gameMode == GameMode.PlayerVsAI && !config.hasSeenExtraTurnTip)
                {
                    config.hasSeenExtraTurnTip = true;
                    PauseForTutorial(TutorialType.ExtraTurn);
                    yield return new WaitUntil(() => !IsPaused);
                }

                uiController.UpdateGameStatus(GameEvent.ExtraTurn, currentPlayer);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.extraTurn);

                p1ExtraTurns += (currentPlayer == 1) ? 1 : 0;
                p2ExtraTurns += (currentPlayer == 2) ? 1 : 0;

                if (config.gameMode == GameMode.PlayerVsAI && currentPlayer == 2)
                {
                    isAITurn = true;
                    StartCoroutine(HandleAITurn());
                }
                else
                {
                    isGameActive = true;
                }
            }
            else if (wasCapture)
            {
                if (config.gameMode == GameMode.PlayerVsAI && !config.hasSeenCaptureTip)
                {
                    config.hasSeenCaptureTip = true;
                    PauseForTutorial(TutorialType.Capture);
                    yield return new WaitUntil(() => !IsPaused);
                }

                yield return StartCoroutine(HandleCaptureRoutine(lastHole));
                SwitchPlayer();
            }
            else
            {
                SwitchPlayer();
            }
        }

        CheckGameOver();

        if (!isAITurn && !IsPaused && currentPlayer == 1)
        {
            isGameActive = true;
        }
    }

    private IEnumerator HandleCaptureRoutine(Hole lastHole)
    {
        Hole oppositeHole = GetOppositeHole(lastHole);

        if (oppositeHole == null || oppositeHole.GetSeedCount() == 0)
        {
            yield break;
        }

        List<GameObject> capturedSeeds = oppositeHole.TakeAllSeedObjects();
        List<GameObject> capturingSeeds = lastHole.TakeAllSeedObjects();

        int totalCaptured = capturedSeeds.Count + capturingSeeds.Count;

        if (currentPlayer == 1) p1MaxSeedsCaptured = Mathf.Max(p1MaxSeedsCaptured, totalCaptured);
        else p2MaxSeedsCaptured = Mathf.Max(p2MaxSeedsCaptured, totalCaptured);

        if (uiController != null)
        {
            int p1Current = playerOneStorehouse.GetSeedCount();
            int p2Current = playerTwoStorehouse.GetSeedCount();
            if (currentPlayer == 1) p1Current += totalCaptured; else p2Current += totalCaptured;
            uiController.UpdateScores(p1Current, p2Current, currentPlayer);
        }

        Hole targetStorehouse = (currentPlayer == 1) ? playerOneStorehouse : playerTwoStorehouse;

        float currentMoveSpeed = seedMoveSpeed * 2;
        if (config != null && config.isFastForwardActive) currentMoveSpeed *= config.fastForwardSpeedMultiplier;

        foreach (var seed in capturedSeeds)
            StartCoroutine(targetStorehouse.AnimateAddSeedCoroutine(seed, oppositeHole.GetSeedContainer(), liftHeight, currentMoveSpeed));
        foreach (var seed in capturingSeeds)
            StartCoroutine(targetStorehouse.AnimateAddSeedCoroutine(seed, lastHole.GetSeedContainer(), liftHeight, currentMoveSpeed));

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.seedCapture);

        float dist = Vector3.Distance(oppositeHole.transform.position, targetStorehouse.transform.position);
        float waitTime = (dist / currentMoveSpeed) + 0.5f;

        yield return new WaitForSeconds(waitTime);
    }

    #endregion

    #region Helpers

    private Hole GetOppositeHole(Hole hole)
    {
        if (hole.GetHoleType() == HoleType.PlayerOne)
        {
            int index = hole.GetHoleIndex();
            int oppositeIndex = (playerTwoHoles.Length - 1) - index;
            if (oppositeIndex >= 0 && oppositeIndex < playerTwoHoles.Length)
                return playerTwoHoles[oppositeIndex];
        }
        else if (hole.GetHoleType() == HoleType.PlayerTwo)
        {
            int index = hole.GetHoleIndex();
            int oppositeIndex = (playerOneHoles.Length - 1) - index;
            if (oppositeIndex >= 0 && oppositeIndex < playerOneHoles.Length)
                return playerOneHoles[oppositeIndex];
        }
        return null;
    }

    private Hole GetNextHole(Hole current)
    {
        try
        {
            int index = current.GetHoleIndex();
            HoleType type = current.GetHoleType();

            if (type == HoleType.PlayerOne)
            {
                if (index < playerOneHoles.Length - 1) return playerOneHoles[index + 1];
                return playerOneStorehouse;
            }
            if (type == HoleType.Storehouse && current == playerOneStorehouse) return playerTwoHoles[0];

            if (type == HoleType.PlayerTwo)
            {
                if (index < playerTwoHoles.Length - 1) return playerTwoHoles[index + 1];
                return playerTwoStorehouse;
            }
            if (type == HoleType.Storehouse && current == playerTwoStorehouse) return playerOneHoles[0];

            return null;
        }
        catch (System.Exception) { return null; }
    }

    public bool IsValidMove(Hole hole)
    {
        if (hole == null) return false;

        bool canInteract = (isGameActive || isAITurn) && !isSowing && !IsPaused;
        if (!canInteract) return false;

        if (hole.GetSeedCount() == 0) return false;

        bool isP1Turn = currentPlayer == 1;
        HoleType holeType = hole.GetHoleType();

        if ((isP1Turn && holeType == HoleType.PlayerOne) ||
            (!isP1Turn && holeType == HoleType.PlayerTwo && config.gameMode == GameMode.PlayerVsPlayer)) return true;

        if (!isP1Turn && holeType == HoleType.PlayerTwo && config.gameMode == GameMode.PlayerVsAI) return true;

        return false;
    }

    public void EndTurnAfterAI() { isAITurn = false; isGameActive = true; }
    public void SetFastForward(bool isOn) { if (config != null) config.isFastForwardActive = isOn; }
    public void SetGameActive(bool isActive) { isGameActive = isActive; }

    public void TogglePause()
    {
        if (IsPaused && Time.timeScale == 0f && (pausePanel == null || !pausePanel.activeSelf))
        {
            return;
        }

        IsPaused = !IsPaused;
        if (pausePanel != null) pausePanel.SetActive(IsPaused);

        if (IsPaused)
        {
            if (uiController != null) uiController.UpdatePausePanelInfo(playerOneStorehouse.GetSeedCount(), playerTwoStorehouse.GetSeedCount(), currentPlayer);
            Time.timeScale = 0f;
        }
        else Time.timeScale = 1f;
    }

    #endregion

    #region Tutorial
    private void StartIntroTutorial()
    {
        Debug.Log("[GameManager] Memulai Tutorial Pengenalan...");
        SetGameActive(false); IsPaused = true;
        if (uiController != null) uiController.ShowIntroTutorial();
    }
    public void CompleteIntroTutorial()
    {
        if (config != null) config.hasSeenIntroTutorial = true;
        if (uiController != null)
        SetGameActive(true); IsPaused = false;
        uiController.UpdateGameStatus(GameEvent.TurnChanged, currentPlayer);
    }
    public void PauseForTutorial(TutorialType type)
    {
        if (IsPaused) return;
        Debug.Log($"[GameManager] Pause untuk Tutorial: {type}");
        IsPaused = true; Time.timeScale = 0f;
        if (uiController != null) uiController.ShowContextualTutorial(type);
    }
    public void ResumeFromTutorial()
    {
        Debug.Log("[GameManager] Melanjutkan game dari Tutorial.");
        IsPaused = false; Time.timeScale = 1f;
        if (!isAITurn) isGameActive = true;
    }

    public Hole GetNextHolePublic(Hole h)
    {
        return GetNextHole(h);
    }
    
    #endregion
}