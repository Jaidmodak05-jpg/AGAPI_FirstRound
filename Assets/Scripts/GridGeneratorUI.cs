using System;
using System.Collections.Generic;
using UnityEngine;

public class GridGeneratorUI : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public RectTransform board;       // The UI area that holds cards
    public GameObject cardPrefab;     // The Card.prefab
    public Vector2 spacing = new Vector2(8f, 8f);

    /// <summary>
    /// Builds a rows x cols grid of CardUI under 'board', auto-sizing each card to fit.
    /// Returns the list of CardUI in row-major order.
    /// </summary>
    public List<CardUI> Generate(int rows, int cols, System.Random rng, Func<int, string> labelForId = null)
    {
        if (board == null || cardPrefab == null)
        {
            Debug.LogError("GridGeneratorUI: board or cardPrefab is not assigned.");
            return new List<CardUI>();
        }

        // 1) Clear existing children
        for (int i = board.childCount - 1; i >= 0; i--)
            Destroy(board.GetChild(i).gameObject);

        int total = rows * cols;
        if (total <= 0) return new List<CardUI>();

        // 2) Prepare deck IDs (pairs: 0,0,1,1,2,2,...)
        int pairs = total / 2; // if odd total, we’ll pad one extra below
        var ids = new List<int>(total);
        for (int i = 0; i < pairs; i++) { ids.Add(i); ids.Add(i); }
        while (ids.Count < total) ids.Add(pairs); // pad if rows*cols is odd

        // Shuffle
        for (int i = 0; i < ids.Count; i++)
        {
            int j = rng.Next(i, ids.Count);
            (ids[i], ids[j]) = (ids[j], ids[i]);
        }

        // 3) Calculate card size to fit Board
        Vector2 area = board.rect.size;
        float cellW = (area.x - spacing.x * (cols - 1)) / cols;
        float cellH = (area.y - spacing.y * (rows - 1)) / rows;
        float size = Mathf.Floor(Mathf.Min(cellW, cellH)); // square cards

        // 4) Center the full grid (requires board.pivot = 0.5,0.5)
        float totalW = cols * size + (cols - 1) * spacing.x;
        float totalH = rows * size + (rows - 1) * spacing.y;
        float originX = -totalW * 0.5f + size * 0.5f; // top-left card center X
        float originY = totalH * 0.5f - size * 0.5f; // top-left card center Y

        var result = new List<CardUI>(total);
        int k = 0;

        // 5) Instantiate and position
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var go = Instantiate(cardPrefab, board);
                var rt = (RectTransform)go.transform;
                rt.sizeDelta = new Vector2(size, size);

                float x = originX + c * (size + spacing.x);
                float y = originY - r * (size + spacing.y);
                rt.anchoredPosition = new Vector2(x, y);

                int id = ids[k++];
                var card = go.GetComponent<CardUI>();
                string label = (labelForId != null) ? labelForId(id) : (id + 1).ToString();
                card.Init(id, label, null);

                result.Add(card);
            }
        }

        return result;
    }
}
