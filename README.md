Overview

This is a simple Memory Matching Game built in Unity.
Players flip over two cards at a time, aiming to find matching pairs.
The game features:

Flipping cards with smooth animations

Score and combo tracking

Four sound effects (flip, match, mismatch, game over)

Countdown timer and Game Over panel with restart option

How to Play

Click on a card to flip it.

Flip two cards:

If they match → both vanish and you earn points (combo multiplier applies).

If they don’t match → both flip back down after a short delay.

Keep playing until:

All pairs are matched, or

The timer runs out → Game Over panel appears.

Use the Restart button on the Game Over panel to play again.

Changing Rows, Columns, and Layout

You can control the grid size (number of cards) in the Inspector:

Select the GameControllerUI object in the scene.

In the Inspector, set:

Rows → number of card rows (e.g. 2, 3, 4, 5)

Cols → number of card columns (e.g. 2, 3, 4, 6)

Example: Rows = 4, Cols = 4 → creates a 4×4 grid (16 cards, 8 pairs).

Select the GridGeneratorUI object and adjust:

Board → RectTransform that defines the play area (cards auto-resize to fit).

Spacing → gap between cards.

Card Prefab → the reusable card object (already set).

Face Sprites → array of images used for card faces. Add or remove sprites as needed.

Timer and Scoring

Starting Time (seconds): Set in GameControllerUI → Starting Time (default 90).

Scoring System:

+100 points × current combo for a correct match.

Combo increases with consecutive matches and resets on a miss.

Score and combo are displayed in the HUD.

Sounds

The game uses four sound effects handled by AudioManager:

Flip

Match

Miss (mismatch)

Game Over

To change them:

Select the AudioManager object in the scene.

Drag your desired audio clips into the corresponding fields in the Inspector.

Game Over Panel

Shows Final Score and Best Score when the timer ends.

Includes a Restart button → wired to GameControllerUI.OnRestartButton().

Panel is hidden during gameplay and auto-activates on game over.

How to Add New Sprites

Import images into Assets/Sprites.

In GridGeneratorUI → Face Sprites, add new entries.

The system automatically pairs and shuffles them.

Project Structure

CardUI.cs – Handles flip, vanish, and click logic for each card.

GridGeneratorUI.cs – Spawns cards into a grid, auto-resizes them to fit.

GameControllerUI.cs – Main controller (timer, score, combo, game over logic).

AudioManager.cs – Plays SFX (flip, match, miss, game over).
