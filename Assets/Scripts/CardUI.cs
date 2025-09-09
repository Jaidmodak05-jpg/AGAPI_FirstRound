using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Assign in Prefab")]
    [SerializeField] private GameObject front;       // Front GO (image)
    [SerializeField] private GameObject back;        // Back GO (image)
    [SerializeField] private TMP_Text frontLabel;    // Optional text on front

    [Header("Flip Settings")]
    [SerializeField] private float flipDuration = 0.22f; // seconds

    public int CardId { get; private set; }
    public bool IsFaceUp { get; private set; }
    public bool IsMatched { get; private set; }
    public bool IsLocked { get; private set; }

    private bool isFlipping;
    private Action<CardUI> onClicked; // set by GameController

    // ---------- Lifecycle ----------
    private void Reset()
    {
        // Auto-wire common child names when adding the script
        if (!front) front = transform.Find("Front")?.gameObject;
        if (!back) back = transform.Find("Back")?.gameObject;
        if (!frontLabel && front) frontLabel = front.GetComponentInChildren<TMP_Text>(true);
    }

    // ---------- Public API ----------
    /// Init called by GameController after instantiation
    public void Init(int id, string displayText, Action<CardUI> onClickedCallback)
    {
        CardId = id;
        onClicked = onClickedCallback;

        IsMatched = false;
        IsLocked = false;
        IsFaceUp = false;
        isFlipping = false;

        if (frontLabel) frontLabel.text = displayText;
        ShowFace(false); // start face-down
        transform.localScale = Vector3.one; // safety
    }

    public void SetMatched()
    {
        IsMatched = true;
        IsLocked = true;
        // Optional: visual tone-down
        var cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0.6f;
    }

    public void Lock(bool locked) => IsLocked = locked;

    // Click handler (desktop + mobile via EventSystem)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsLocked || IsMatched || isFlipping) return;
        onClicked?.Invoke(this);
    }

    public void FlipUp(Action onDone = null)
    {
        if (IsFaceUp || IsLocked || IsMatched || isFlipping) { onDone?.Invoke(); return; }
        StartCoroutine(FlipRoutine(true, onDone));
    }

    public void FlipDown(Action onDone = null)
    {
        if (!IsFaceUp || isFlipping) { onDone?.Invoke(); return; }
        StartCoroutine(FlipRoutine(false, onDone));
    }

    // ---------- Internals ----------
    private IEnumerator FlipRoutine(bool toFaceUp, Action onDone)
    {
        isFlipping = true;
        Vector3 start = transform.localScale;
        float half = Mathf.Max(0.01f, flipDuration * 0.5f);

        // 1) shrink X to 0
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            float k = 1f - Mathf.Clamp01(t / half);
            transform.localScale = new Vector3(k, start.y, start.z);
            yield return null;
        }
        transform.localScale = new Vector3(0f, start.y, start.z);

        // 2) swap face
        ShowFace(toFaceUp);

        // 3) expand X back to 1
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            float k = Mathf.Clamp01(t / half);
            transform.localScale = new Vector3(k, start.y, start.z);
            yield return null;
        }
        transform.localScale = start;

        IsFaceUp = toFaceUp;
        isFlipping = false;
        onDone?.Invoke();
    }

    private void ShowFace(bool faceUp)
    {
        if (front) front.SetActive(faceUp);
        if (back) back.SetActive(!faceUp);
    }
}
