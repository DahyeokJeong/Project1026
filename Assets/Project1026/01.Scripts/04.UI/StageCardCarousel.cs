using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class StageCardCarousel : ScrollRect
{
    [SerializeField] private RectTransform[] cards;
    [SerializeField, Min(1f)] private float cardSpacing = 150f;
    [SerializeField, Range(0.1f, 1f)] private float centerScale = 0.8f;
    [SerializeField, Range(0.1f, 1f)] private float sideScale = 0.55f;

    public event Action<int?> CenteredCardChanged;
    public int? CenteredCardIndex { get; private set; }

    private CanvasGroup[] cardGroups;
    private bool dragging;
    private float snapVelocity;
    private int snapIndex;
    private float appliedSpacing;

    public void SetCards(RectTransform[] stageCards)
    {
        cards = stageCards;
        cardGroups = new CanvasGroup[cards.Length];

        for (int i = 0; i < cards.Length; i++)
        {
            cardGroups[i] = cards[i].GetComponent<CanvasGroup>();
            if (cardGroups[i] == null)
                cardGroups[i] = cards[i].gameObject.AddComponent<CanvasGroup>();
        }

        snapIndex = Mathf.Clamp(snapIndex, 0, Mathf.Max(0, cards.Length - 1));
        snapVelocity = 0f;
        SetCenteredCard(null);
        LayoutCards();
    }

    public void Focus(int index)
    {
        if (cards == null || cards.Length == 0)
            return;

        snapIndex = Mathf.Clamp(index, 0, cards.Length - 1);
        snapVelocity = 0f;
        SetCenteredCard(null);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (cards != null && cardGroups == null)
            SetCards(cards);
    }

    protected override void OnDisable()
    {
        dragging = false;
        snapVelocity = 0f;
        SetCenteredCard(null);
        base.OnDisable();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        if (eventData.button != PointerEventData.InputButton.Left ||
            cards == null || cards.Length < 2)
            return;

        dragging = true;
        snapVelocity = 0f;
        SetCenteredCard(null);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        if (eventData.button != PointerEventData.InputButton.Left || !dragging)
            return;

        dragging = false;
        if (cards != null && cards.Length > 1)
            Focus(Mathf.RoundToInt(horizontalNormalizedPosition * (cards.Length - 1)));
    }

    public override void OnScroll(PointerEventData eventData)
    {
        if (cards == null || cards.Length < 2 || dragging)
            return;

        Vector2 delta = eventData.scrollDelta;
        float direction = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? delta.x : -delta.y;
        if (direction != 0f)
            Focus(snapIndex + (direction > 0f ? 1 : -1));
    }

    protected override void LateUpdate()
    {
        if (viewport != null && content != null &&
            (!Mathf.Approximately(content.rect.height, viewport.rect.height) ||
             !Mathf.Approximately(appliedSpacing, cardSpacing *
                 Mathf.Min(viewport.rect.width / 480f, viewport.rect.height / 200f))))
        {
            LayoutCards();
        }

        base.LateUpdate();

        if (content == null || viewport == null || cards == null || cards.Length == 0)
            return;

        int? settledIndex = null;
        if (!dragging)
        {
            int lastIndex = cards.Length - 1;
            if (lastIndex == 0)
            {
                settledIndex = 0;
            }
            else
            {
                float current = horizontalNormalizedPosition * lastIndex;
                float next = Mathf.SmoothDamp(
                    current, snapIndex, ref snapVelocity,
                    0.15f, Mathf.Infinity, Time.unscaledDeltaTime);

                bool settled = Mathf.Abs(next - snapIndex) < 0.001f;
                horizontalNormalizedPosition = (settled ? snapIndex : next) / lastIndex;

                if (settled)
                {
                    snapVelocity = 0f;
                    settledIndex = snapIndex;
                }
            }
        }

        LayoutCards();
        SetCenteredCard(settledIndex);
    }

    private void SetCenteredCard(int? index)
    {
        if (CenteredCardIndex == index)
            return;

        CenteredCardIndex = index;
        CenteredCardChanged?.Invoke(index);
    }

    private void LayoutCards()
    {
        if (content == null || viewport == null || cards == null || cards.Length == 0)
            return;

        float layoutScale = Mathf.Min(viewport.rect.width / 480f, viewport.rect.height / 200f);
        if (layoutScale <= 0f)
            return;

        float spacing = cardSpacing * layoutScale;
        float previousSpacing = appliedSpacing > 0f ? appliedSpacing : spacing;
        float currentIndex = Mathf.Clamp(
            -content.anchoredPosition.x / previousSpacing, 0f, cards.Length - 1);

        content.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal, viewport.rect.width + (cards.Length - 1) * spacing);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, viewport.rect.height);
        content.anchoredPosition = new Vector2(-currentIndex * spacing, 0f);
        appliedSpacing = spacing;

        float viewportCenter = viewport.rect.width * 0.5f;
        for (int i = 0; i < cards.Length; i++)
        {
            float distance = Mathf.Abs(i - currentIndex);
            float scale = layoutScale * Mathf.Lerp(
                centerScale, sideScale, Mathf.SmoothStep(0f, 1f, distance));

            cards[i].anchoredPosition = new Vector2(viewportCenter + i * spacing, 0f);
            cards[i].localScale = new Vector3(scale, scale, 1f);

            float alpha = 1f - Mathf.SmoothStep(
                0f, 1f, Mathf.InverseLerp(1f, 1.3f, distance));

            cardGroups[i].alpha = alpha;
            cardGroups[i].blocksRaycasts = alpha > 0.01f;
        }
    }
}