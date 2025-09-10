using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class CardUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Assign in Prefab (ROOT Card)")]
    [SerializeField] private GameObject front;     // child named "Front"
    [SerializeField] private GameObject back;      // child named "Back"
    [SerializeField] private Image frontImage;     // Image component on Front (or on a child)

    [Header("Optional")]
    [SerializeField] private float flipDuration = 0.22f;

    [HideInInspector] public GameControllerUI controller;   // set by spawner
    [HideInInspector] public AudioManager audioMgr;         // optional (or use controller.audioMgr)

    public int CardId { get; private set; }
    public bool IsFaceUp { get; private set; }
    public bool IsMatched { get; private set; }
    public bool IsLocked { get; private set; }

    // ----- init from spawner -----
    public void Init(int id, Sprite faceSprite)
    {
        CardId = id;
        SetFrontSprite(faceSprite);
        ShowFace(false);        // start face-down
        IsMatched = false;
        IsLocked = false;
    }

    // Kept so GridGeneratorUI can call it directly if preferred
    public void SetFrontSprite(Sprite s)
    {
        if (frontImage) frontImage.sprite = s;
    }

    public void SetMatched()
    {
        IsMatched = true;
        IsLocked = true;
    }

    public void Lock(bool v) => IsLocked = v;

    // EXACTLY what you were doing manually in the Hierarchy
    void ShowFace(bool faceUp)
    {
        IsFaceUp = faceUp;

        if (front) front.SetActive(faceUp);     // FRONT on
        if (back) back.SetActive(!faceUp);     // BACK off

        if (frontImage)
        {
            frontImage.enabled = faceUp;
            if (faceUp) frontImage.color = Color.white;
        }

        if (faceUp && front) front.transform.SetAsLastSibling(); // bring to top
    }

    // ------- Flip coroutines (controller awaits these) -------
    public IEnumerator FlipUp()
    {
        if (IsLocked || IsMatched || IsFaceUp) yield break;

        // (optional tween could go here over flipDuration)
        ShowFace(true);

        if (audioMgr) audioMgr.Flip();
        yield return null;
    }

    public IEnumerator FlipDown()
    {
        if (IsLocked || IsMatched || !IsFaceUp) yield break;

        ShowFace(false);
        yield return null;
    }

    public IEnumerator Vanish()
    {
        IsLocked = true;
        IsMatched = true;

        // quick fade (optional)
        float t = 0f;
        var cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();

        while (t < 1f)
        {
            t += Time.deltaTime / 0.25f;
            cg.alpha = 1f - Mathf.Clamp01(t);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    // When clicked: flip up, then tell controller “I’m up”
    public void OnPointerClick(PointerEventData _)
    {
        if (IsLocked || IsMatched || IsFaceUp) return;
        StartCoroutine(FlipAndNotify());
    }

    IEnumerator FlipAndNotify()
    {
        yield return StartCoroutine(FlipUp());
        if (controller) controller.OnCardFlippedUp(this);
    }
}
