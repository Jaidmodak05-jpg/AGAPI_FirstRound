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

    // runtime
    int score;
    int combo;
    float timeLeft;
    bool running;

    readonly List<CardUI> pending = new();
    bool resolving = false;

    void Start() => StartNewGame(rows, cols);

    public void StartNewGame(int r, int c)
    {
        rows = r; cols = c;

        grid.Generate(rows, cols, new System.Random());

        score = 0;
        combo = 0;
        timeLeft = startingTime;
        running = true;

        UpdateUI();
    }

    void Update()
    {
        if (!running) return;

        timeLeft -= Time.deltaTime;
        if (timerText) timerText.text = FormatTime(timeLeft);

        if (timeLeft <= 0f)
        {
            running = false;
            if (audioMgr) audioMgr.GameOver();
            // show game over UI here if you want
        }
    }

    string FormatTime(float t)
    {
        t = Mathf.Max(0, t);
        int m = (int)(t / 60f);
        int s = (int)(t % 60f);
        return $"{m:0}:{s:00}";
    }

    void UpdateUI()
    {
        if (scoreText) scoreText.text = $"Score: {score}";
        if (comboText) comboText.text = combo > 0 ? $"Combo: x{combo}" : "";
        if (timerText) timerText.text = FormatTime(timeLeft);
    }

    // Called by CardUI after it finishes FlipUp
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
        yield return new WaitForSeconds(0.25f); // let the player see both

        var a = pending[0];
        var b = pending[1];

        if (a.CardId == b.CardId)
        {
            // MATCH
            combo++;
            score += 100 * combo;
            if (audioMgr) audioMgr.Match(combo);

            yield return StartCoroutine(a.Vanish());
            yield return StartCoroutine(b.Vanish());
        }
        else
        {
            // MISS
            combo = 0;
            if (audioMgr) audioMgr.Miss();

            yield return StartCoroutine(a.FlipDown());
            yield return StartCoroutine(b.FlipDown());
        }

        pending.Clear();
        resolving = false;
        UpdateUI();
    }
}
