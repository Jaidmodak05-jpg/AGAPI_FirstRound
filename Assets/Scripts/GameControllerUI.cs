using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameControllerUI : MonoBehaviour
{
    [Header("Refs")]
    public GridGeneratorUI grid;
    public RectTransform board;

    [Header("HUD")]
    public TMP_Text scoreText;
    public TMP_Text comboText;
    public TMP_Text timerText;

    [Header("Audio")]
    public AudioManager audioMgr;

    [Header("Config")]
    public int rows = 4;
    public int cols = 4;
    public float startingTime = 90f;

    [Header("Game Over UI (optional)")]
    public GameObject gameOverPanel;    // drag panel (inactive by default)
    public TMP_Text finalScoreText;   // optional
    public TMP_Text bestScoreText;    // optional

    // runtime
    int score;
    int combo;
    float timeLeft;
    bool running;

    readonly List<CardUI> pending = new();
    bool resolving = false;

    int bestScore;

    void Start()
    {
        // load best score from previous runs
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        StartNewGame(rows, cols);
    }

    public void StartNewGame(int r, int c)
    {
        rows = r; cols = c;

        // clean fresh run
        score = 0;
        combo = 0;
        timeLeft = startingTime;
        running = true;
        resolving = false;
        pending.Clear();

        if (gameOverPanel) gameOverPanel.SetActive(false);

        // spawn grid (your GridGenerator wires sprites + controller + audio)
        grid.Generate(rows, cols, new System.Random());

        UpdateUI();
    }

    void Update()
    {
        if (!running) return;

        timeLeft -= Time.deltaTime;

        // update HUD every frame
        UpdateUI();

        // time-up → end run
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            running = false;
            if (audioMgr) audioMgr.GameOver();
            FinishRun();
        }
    }

    void FinishRun()
    {
        // save best score
        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
        }

        // show overlay
        if (finalScoreText) finalScoreText.text = $"Score: {score}";
        if (bestScoreText) bestScoreText.text = $"Best: {bestScore}";
        if (gameOverPanel) gameOverPanel.SetActive(true);
    }

    void UpdateUI()
    {
        if (scoreText) scoreText.text = $"Score: {score}";
        if (comboText) comboText.text = combo > 0 ? $"Combo: x{combo}" : "Combo: -";
        if (timerText) timerText.text = FormatTime(timeLeft);

        // low-time color shift (white → red under 20s)
        if (timerText)
        {
            var t = Mathf.InverseLerp(20f, 0f, timeLeft);
            timerText.color = Color.Lerp(Color.white, new Color(1f, .3f, .3f), t);
        }
    }

    string FormatTime(float t)
    {
        t = Mathf.Max(0f, t);
        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);
        return $"{m:00}:{s:00}";
    }

    // Called by CardUI *after* it finishes FlipUp (see CardUI.OnPointerClick)
    public void OnCardFlippedUp(CardUI card)
    {
        if (!running || resolving) return;
        if (pending.Contains(card)) return;

        pending.Add(card);
        if (pending.Count == 2)
            StartCoroutine(ResolvePair());
    }

    IEnumerator ResolvePair()
    {
        resolving = true;

        // tiny pause so both faces are visible
        yield return new WaitForSeconds(0.25f);

        var a = pending[0];
        var b = pending[1];

        if (a.CardId == b.CardId)
        {
            // MATCH
            a.SetMatched(); b.SetMatched();

            combo++;
            score += 100 * combo;
            if (audioMgr) audioMgr.Match(combo);

            yield return StartCoroutine(a.Vanish());
            yield return StartCoroutine(b.Vanish());

            // early win if everything is gone
            if (AllCardsGone())
            {
                running = false;
                if (audioMgr) audioMgr.GameOver();
                FinishRun();
            }
        }
        else
        {
            // MISS
            combo = 0;
            score = Mathf.Max(0, score - 20);
            if (audioMgr) audioMgr.Miss();

            yield return StartCoroutine(a.FlipDown());
            yield return StartCoroutine(b.FlipDown());
        }

        pending.Clear();
        resolving = false;
        UpdateUI();
    }

    bool AllCardsGone()
    {
        if (!board) return false;
        for (int i = 0; i < board.childCount; i++)
            if (board.GetChild(i).gameObject.activeSelf)
                return false;
        return true;
    }

    // called by Restart button
    public void OnRestartButton()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
        StartNewGame(rows, cols);
    }
}
