using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class CardUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Assign in Prefab (ROOT Card)")]
    [SerializeField] private GameObject front;        // child named "Front"
    [SerializeField] private GameObject back;         // child named "Back"
    [SerializeField] private Image frontImage;        // Image inside Front

    [Header("Optional")]
    [SerializeField] private float flipDuration = 0.22f;

    public int CardId { get; private set; }
    public bool IsFaceUp { get; private set; }
    public bool IsMatched { get; private set; }
    public bool IsLocked { get; private set; }

    bool isFlipping;
    Action<CardUI> onClicked;

    void Reset()
    {
        if (!front) front = transform.Find("Front")?.gameObject;
        if (!back) back = transform.Find("Back")?.gameObject;
        if (!frontImage && front) frontImage = front.GetComponentInChildren<Image>(true);

        // 🔸 Ensure the root has a raycastable Graphic so clicks reach OnPointerClick
        if (!TryGetComponent<Image>(out var hitbox))
        {
            hitbox = gameObject.AddComponent<Image>();
            hitbox.color = new Color(0, 0, 0, 0); // fully transparent
        }
        hitbox.raycastTarget = true;

        // (CanvasGroup is fine but not required here)
        if (!TryGetComponent<CanvasGroup>(out var cg)) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    // Called by your grid/controller right after Instantiate
    public void Init(int id, Action<CardUI> onClickedCallback)
    {
        CardId = id;
        onClicked = onClickedCallback;

        IsMatched = false;
        IsLocked = false;
        isFlipping = false;

        // Start like your manual setup: both ON, Back above Front
        if (front)
        {
            front.SetActive(true);
            if (frontImage)
            {
                frontImage.enabled = true;
                frontImage.color = Color.white;
            }
        }
        if (back)
        {
            back.SetActive(true);
            back.transform.SetAsLastSibling();   // Back on top to cover Front
        }

        IsFaceUp = false; // Back visible → considered face-down

        transform.localScale = Vector3.one;
        gameObject.SetActive(true);
    }

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

    // Root receives clicks now because of the invisible Image hitbox
    public void OnPointerClick(PointerEventData _)
    {
        if (IsLocked || IsMatched || isFlipping) return;

        if (onClicked != null)
        {
            onClicked(this); // let the controller decide (enforce 2 at a time, etc.)
        }
        else
        {
            // local test toggle
            if (IsFaceUp) FlipDown();
            else FlipUp();
        }
    }

    public void FlipUp(Action onDone = null)
    {
        if (IsFaceUp || IsLocked || IsMatched) { onDone?.Invoke(); return; }
        StartCoroutine(FlipRoutine(true, onDone));
    }

    public void FlipDown(Action onDone = null)
    {
        if (!IsFaceUp) { onDone?.Invoke(); return; }
        StartCoroutine(FlipRoutine(false, onDone));
    }

    IEnumerator FlipRoutine(bool toFaceUp, Action onDone)
    {
        isFlipping = true;

        // quick squash animation (optional)
        Vector3 s0 = transform.localScale;
        float half = Mathf.Max(0.01f, flipDuration * 0.5f);

        for (float t = 0; t < half; t += Time.deltaTime)
        {
            float k = 1f - Mathf.Clamp01(t / half);
            transform.localScale = new Vector3(k, s0.y, s0.z);
            yield return null;
        }
        transform.localScale = new Vector3(0f, s0.y, s0.z);

        // 🔸 The actual visibility switch
        if (toFaceUp)
        {
            if (front) front.transform.SetAsFirstSibling(); // not required, neat
            if (back)
            {
                back.transform.SetAsLastSibling();
                back.SetActive(false); // hide Back → reveal Front exactly like you do manually
            }
        }
        else
        {
            if (back)
            {
                back.transform.SetAsLastSibling();
                back.SetActive(true);  // show Back → cover Front again
            }
        }
        IsFaceUp = toFaceUp;

        for (float t = 0; t < half; t += Time.deltaTime)
        {
            float k = Mathf.Clamp01(t / half);
            transform.localScale = new Vector3(k, s0.y, s0.z);
            yield return null;
        }
        transform.localScale = s0;

        isFlipping = false;
        onDone?.Invoke();
    }

    public IEnumerator Vanish(float duration = 0.15f)
    {
        if (!TryGetComponent<CanvasGroup>(out var cg))
            cg = gameObject.AddComponent<CanvasGroup>();

        Vector3 s0 = transform.localScale;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            cg.alpha = 1f - k;
            transform.localScale = Vector3.Lerp(s0, Vector3.zero, k);
            yield return null;
        }
        cg.alpha = 0f;
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
}
