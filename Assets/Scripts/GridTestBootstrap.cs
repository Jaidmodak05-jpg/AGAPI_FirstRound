using System;
using System.Collections.Generic;
using UnityEngine;

public class GridTestBootstrap : MonoBehaviour
{
    public GridGeneratorUI grid;
    public int rows = 4;
    public int cols = 4;

    private List<CardUI> cards;

    void Start()
    {
        if (grid == null) { Debug.LogError("Assign GridGeneratorUI in inspector."); return; }

        var rng = new System.Random(Guid.NewGuid().GetHashCode());
        cards = grid.Generate(rows, cols, rng, id => (id + 1).ToString());

        // Give each card a temporary click action: toggle flip
        foreach (var c in cards)
        {
            // Re-init to inject a click callback
            int id = c.CardId;
            c.Init(id, (id + 1).ToString(), OnCardClicked);
        }
    }

    private void OnCardClicked(CardUI card)
    {
        if (card.IsFaceUp) card.FlipDown(); else card.FlipUp();
    }
}
