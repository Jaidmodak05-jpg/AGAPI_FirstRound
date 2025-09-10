using System;
using System.Collections.Generic;
using UnityEngine;

public class GridGeneratorUI : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public RectTransform board;
    public GameObject cardPrefab;
    public Vector2 spacing = new Vector2(8f, 8f);
    public List<Sprite> faceSprites;

    public List<CardUI> Generate(int rows, int cols, System.Random rng)
    {
        if (!board || !cardPrefab)
        {
            Debug.LogError("GridGeneratorUI: board or cardPrefab is not assigned.");
            return new List<CardUI>();
        }

        for (int i = board.childCount - 1; i >= 0; i--)
            Destroy(board.GetChild(i).gameObject);

        int total = rows * cols;
        if (total <= 0) return new List<CardUI>();

        int pairs = total / 2;
        var ids = new List<int>(total);
        for (int i = 0; i < pairs; i++) { ids.Add(i); ids.Add(i); }
        while (ids.Count < total) ids.Add(pairs);

        for (int i = 0; i < ids.Count; i++)
        {
            int j = rng.Next(i, ids.Count);
            (ids[i], ids[j]) = (ids[j], ids[i]);
        }

        Vector2 area = board.rect.size;
        float cellW = (area.x - spacing.x * (cols - 1)) / cols;
        float cellH = (area.y - spacing.y * (rows - 1)) / rows;
        float size = Mathf.Floor(Mathf.Min(cellW, cellH));

        float totalW = cols * size + (cols - 1) * spacing.x;
        float totalH = rows * size + (rows - 1) * spacing.y;
        float originX = -totalW * 0.5f + size * 0.5f;
        float originY = totalH * 0.5f - size * 0.5f;

        var result = new List<CardUI>(total);
        int k = 0;

        for (int r = 0; r < rows; r++)
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
                card.Init(id, null);
                if (faceSprites != null && faceSprites.Count > 0)
                    card.SetFrontSprite(faceSprites[id % faceSprites.Count]);

                result.Add(card);
            }

        return result;
    }
}
