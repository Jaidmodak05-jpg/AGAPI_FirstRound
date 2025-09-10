using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerUI : MonoBehaviour
{
    [Header("Refs")]
    public GridGeneratorUI grid;           // your existing spawner
    public RectTransform board;            // board parent
    public GameObject cardPrefab;          // Card1 (the prefab wired to CardUI)
    public List<Sprite> faceSprites;       // fill in Inspector
    public AudioManager audioMgr;          // optional sounds

    [Header("Config")]
    public int rows = 4;
    public int cols = 4;

    readonly List<CardUI> allCards = new();
    readonly List<CardUI> pending = new();
    bool resolving;

    void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        // spawn grid (use your existing grid.Generate if you prefer)
        allCards.Clear();
        pending.Clear();

        System.Random rng = new System.Random();

        // Build the deck ids (pairs) and shuffle
        int total = rows * cols;
        int pairs = total / 2;
        var ids = new List<int>(total);
        for (int i = 0; i < pairs; i++) { ids.Add(i); ids.Add(i); }
        while (ids.Count < total) ids.Add(pairs);
        for (int i = 0; i < ids.Count; i++)
        {
            int j = rng.Next(i, ids.Count);
            (ids[i], ids[j]) = (ids[j], ids[i]);
        }

        // Clear board
        for (int i = board.childCount - 1; i >= 0; i--) Destroy(board.GetChild(i).gameObject);

        // Simple grid placement (use your GridGeneratorUI if you already have it):
        float size = 220f, gap = 8f;
        float totalW = cols * size + (cols - 1) * gap;
        float totalH = rows * size + (rows - 1) * gap;
        float ox = -totalW * 0.5f + size * 0.5f;
        float oy = totalH * 0.5f - size * 0.5f;

        int k = 0;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                int id = ids[k++];
                var go = Instantiate(cardPrefab, board);
                var rt = (RectTransform)go.transform;
                rt.sizeDelta = new Vector2(size, size);
                rt.anchoredPosition = new Vector2(ox + c * (size + gap), oy - r * (size + gap));

                var card = go.GetComponent<CardUI>();
                card.Init(id, OnCardClicked);
                if (id >= 0 && id < faceSprites.Count)
                    card.SetFrontSprite(faceSprites[id]);

                allCards.Add(card);
            }
    }

    void OnCardClicked(CardUI card)
    {
        if (resolving) return;
        if (pending.Count >= 2) return;
        if (card.IsFaceUp) return; // already open
        if (card.IsMatched) return;

        audioMgr?.Flip(); // optional sfx
        card.FlipUp(() =>
        {
            pending.Add(card);
            if (pending.Count == 2) StartCoroutine(ResolvePair());
        });
    }

    IEnumerator ResolvePair()
    {
        resolving = true;

        // tiny delay so the player can see both
        yield return new WaitForSeconds(0.25f);

        var a = pending[0];
        var b = pending[1];

        if (a.CardId == b.CardId)
        {
            // match!
            a.SetMatched();
            b.SetMatched();
            audioMgr?.Match();
            yield return a.StartCoroutine(a.Vanish());
            yield return b.StartCoroutine(b.Vanish());
        }
        else
        {
            // miss → flip both back down
            audioMgr?.Miss();
            a.FlipDown();
            b.FlipDown();
        }

        pending.Clear();
        resolving = false;
    }
}
