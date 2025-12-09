using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    // ---------------------------------------------------
    // FAST FORWARD BUTTON
    // ---------------------------------------------------
    [Header("Fast Forward Button")]
    public UnityEngine.UI.Image fastForwardButtonImage;
    public Sprite fastForwardOnSprite;
    public Sprite fastForwardOffSprite;

    public void SetFastForwardVisual(bool isActive)
    {
        if (fastForwardButtonImage == null) return;
        fastForwardButtonImage.sprite = isActive ? fastForwardOnSprite : fastForwardOffSprite;
    }

    // ---------------------------------------------------
    // SCORE UI
    // ---------------------------------------------------
    [Header("Score UI")]
    public TMP_Text playerOneScoreTxt;
    public TMP_Text playerTwoScoreTxt;

    // ---------------------------------------------------
    // STATUS UI
    // ---------------------------------------------------
    [Header("Status UI")]
    public TMP_Text gameStatusTxt;

    // ---------------------------------------------------
    // GAME END PANEL
    // ---------------------------------------------------
    [Header("Game End Panel")]
    public GameObject gameEndPanel;
    public TMP_Text winnerText;
    public TMP_Text finalScoreText;
    public TMP_Text statsText;

    // ---------------------------------------------------
    // PAUSE PANEL
    // ---------------------------------------------------
    [Header("Pause Panel")]
    public GameObject pausePanel;
    public TMP_Text pausePanel_ScoreText;
    public TMP_Text pausePanel_TurnText;

    // ---------------------------------------------------
    // INTRO TUTORIAL
    // ---------------------------------------------------
    [Header("Intro Tutorial")]
    [SerializeField] private GameObject introTutorialPanelContainer;
    [SerializeField] private GameObject[] introTutorialPages;

    private int introIndex = 0;

    // ---------------------------------------------------
    // CONTEXTUAL TUTORIAL
    // ---------------------------------------------------
    [Header("Contextual Tutorial")]
    public GameObject contextualTutorialPanel;
    public TMP_Text contextualTitleTxt;
    public TMP_Text contextualBodyTxt;

    void Start()
    {
        if (gameEndPanel != null) gameEndPanel.SetActive(false);
        if (contextualTutorialPanel != null) contextualTutorialPanel.SetActive(false);
    }

    // =====================================================
    // SCORE & STATUS
    // =====================================================
    public void UpdateScores(int p1Score, int p2Score, int currentPlayer)
    {
        if (playerOneScoreTxt != null) playerOneScoreTxt.text = p1Score.ToString();
        if (playerTwoScoreTxt != null) playerTwoScoreTxt.text = p2Score.ToString();

        if (gameStatusTxt != null)
        {
            string key = (currentPlayer == 1) ? "TURN_P1" : "TURN_P2";
            gameStatusTxt.text = LocalizationManager.Get(key);
        }
    }

    public void UpdateGameStatus(GameEvent ev, int currentPlayer)
    {
        if (gameStatusTxt == null) return;

        switch (ev)
        {
            case GameEvent.GameStart:
                gameStatusTxt.text = LocalizationManager.Get("GAME_START");
                break;

            case GameEvent.TurnChanged:
                gameStatusTxt.text = LocalizationManager.Get(
                    (currentPlayer == 1) ? "TURN_P1" : "TURN_P2"
                );
                break;

            case GameEvent.ExtraTurn:
                gameStatusTxt.text = LocalizationManager.Get("EXTRA_TURN");
                break;
        }
    }

    // =====================================================
    // INTRO TUTORIAL
    // =====================================================
    public void ShowIntroTutorial()
    {
        if (introTutorialPanelContainer == null || introTutorialPages.Length == 0)
        {
            Debug.LogError("[UIController] Intro tutorial panel/pages not assigned!");
            return;
        }

        introIndex = 0;
        introTutorialPanelContainer.SetActive(true);

        for (int i = 0; i < introTutorialPages.Length; i++)
            introTutorialPages[i].SetActive(i == 0);
    }

    public void OnIntroTutorialNext()
    {
        if (introTutorialPages.Length == 0) return;

        introTutorialPages[introIndex].SetActive(false);
        introIndex++;

        if (introIndex < introTutorialPages.Length)
        {
            introTutorialPages[introIndex].SetActive(true);
            return;
        }

        HideIntroTutorial();
        GameManager.Instance.CompleteIntroTutorial();
    }

    public void HideIntroTutorial()
    {
        if (introTutorialPanelContainer != null)
            introTutorialPanelContainer.SetActive(false);
    }

    // =====================================================
    // CONTEXTUAL TUTORIAL
    // =====================================================
    public void ShowContextualTutorial(TutorialType type)
    {
        if (contextualTutorialPanel == null ||
            contextualTitleTxt == null ||
            contextualBodyTxt == null)
            return;

        contextualTutorialPanel.SetActive(true);

        string titleKey = "";
        string bodyKey = "";

        switch (type)
        {
            case TutorialType.SowContinue:
                titleKey = "CTX_SOW_TITLE";
                bodyKey = "CTX_SOW_BODY";
                break;

            case TutorialType.ExtraTurn:
                titleKey = "CTX_EXTRA_TITLE";
                bodyKey = "CTX_EXTRA_BODY";
                break;

            case TutorialType.Capture:
                titleKey = "CTX_CAPTURE_TITLE";
                bodyKey = "CTX_CAPTURE_BODY";
                break;
        }

        contextualTitleTxt.text = LocalizationManager.Get(titleKey);
        contextualBodyTxt.text = LocalizationManager.Get(bodyKey);
    }

    public void OnContextualNext()
    {
        HideContextualTutorial();
        GameManager.Instance.UnpauseGame();
        GameManager.Instance.ResumeTurn();
    }

    public void HideContextualTutorial()
    {
        if (contextualTutorialPanel != null)
            contextualTutorialPanel.SetActive(false);
    }

    // =====================================================
    // PAUSE PANEL
    // =====================================================
    public void UpdatePausePanelInfo(int p1Score, int p2Score, int currentPlayer)
    {
        if (pausePanel_ScoreText != null)
        {
            pausePanel_ScoreText.text =
                $"{LocalizationManager.Get("P1")}: {p1Score}\n" +
                $"{LocalizationManager.Get("P2")}: {p2Score}";
        }

        if (pausePanel_TurnText != null)
        {
            pausePanel_TurnText.text =
                LocalizationManager.Get(
                    (currentPlayer == 1) ? "TURN_P1" : "TURN_P2"
                );
        }
    }

    // =====================================================
    // GAME OVER PANEL
    // =====================================================
    public void ShowGameOver(string winner, int p1Score, int p2Score, int totalSeeds)
    {
        if (gameEndPanel != null) gameEndPanel.SetActive(true);

        if (winnerText != null) winnerText.text = winner;

        if (finalScoreText != null)
        {
            finalScoreText.text =
                $"{LocalizationManager.Get("P1")}: {p1Score}\n" +
                $"{LocalizationManager.Get("P2")}: {p2Score}";
        }

        if (statsText != null)
            statsText.text = $"{LocalizationManager.Get("TOTAL_SEEDS")}: {totalSeeds}";
    }

    public void HideGameOver()
    {
        if (gameEndPanel != null) gameEndPanel.SetActive(false);
    }

    // =====================================================
    // LANGUAGE HANDLING
    // =====================================================
    public void OnLanguageChanged(int index)
    {
        LocalizationManager.SetLanguage((LocalizationManager.Language)index);
        RefreshAllLocalizedUI();
    }

    public void RefreshAllLocalizedUI()
    {
        UpdatePausePanelInfo(
            GameManager.Instance.playerOneStorehouse.GetSeedCount(),
            GameManager.Instance.playerTwoStorehouse.GetSeedCount(),
            GameManager.Instance.currentPlayer
        );

        UpdateGameStatus(GameEvent.TurnChanged, GameManager.Instance.currentPlayer);

        if (gameEndPanel != null && gameEndPanel.activeSelf)
        {
            int p1 = GameManager.Instance.playerOneStorehouse.GetSeedCount();
            int p2 = GameManager.Instance.playerTwoStorehouse.GetSeedCount();
            int total = p1 + p2;

            ShowGameOver("", p1, p2, total); // winner tetap di-set sebelumnya
        }
    }
}