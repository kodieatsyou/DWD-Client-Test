using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardScroller : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IScrollHandler
{
    [Header("References")]
    [SerializeField] private RectTransform cardLayout;
    [SerializeField] private ScrollRect cardScrollRect;
    [SerializeField] private RectTransform cardViewportPanel;
    [SerializeField] private RectTransform cardContentPanel;
    [SerializeField] private GameObject cardItemPrefab;

    [Header("Settings")]
    [SerializeField] private float foldOutCardTime = 0.2f;
    [SerializeField] private float cardBoundsThreshold;
    [SerializeField] private float cardSnapSpeed;
    [SerializeField] private int cardSpacing = 45;

    private float cardLayoutSize;
    private float cardWidth;
    private float cardSnapDistanceMultiple;
    private float closestPosition;
    private bool needsToSnap = false;
    private bool deckIsOut = true;
    private bool isPositiveScroll;
    private Vector2 lastDragPos;

    void Start()
    {
        cardContentPanel.localPosition = new Vector3(0, cardContentPanel.localPosition.y, cardContentPanel.localPosition.z);
        cardWidth = cardItemPrefab.GetComponent<RectTransform>().rect.width;
        cardSnapDistanceMultiple = cardWidth + cardSpacing;
        cardLayoutSize = cardLayout.sizeDelta.x;
        MakeCardObjs();
    }

    void Update()
    {
        closestPosition = Mathf.Round(cardContentPanel.localPosition.x / cardSnapDistanceMultiple) * cardSnapDistanceMultiple;
        if (needsToSnap)
        {
            SnapToCards();
        }
    }

    void SnapToCards()
    {
        if (cardScrollRect.velocity.magnitude < 100f)
        {
            cardScrollRect.velocity = Vector3.zero;
            float distanceToMove = Mathf.Abs(closestPosition - cardContentPanel.localPosition.x);
            float t = Mathf.Clamp01(cardSnapSpeed * Time.deltaTime / distanceToMove);

            cardContentPanel.localPosition = Vector3.Lerp(cardContentPanel.localPosition, new Vector3(closestPosition, cardContentPanel.localPosition.y, cardContentPanel.localPosition.z), t);

            if (Mathf.Abs(cardContentPanel.localPosition.x - closestPosition) < 0.01f)
            {
                needsToSnap = false;
            }
        }
    }

    void MakeCardObjs()
    {
        float originX = -cardContentPanel.rect.width * 0.5f;
        float startPos = originX - (cardSpacing * 0.5f + cardWidth * 2f);
        float posOffset = cardWidth * 0.5f;
        int count = 0;
        foreach (Card c in CardManager.instance.cards)
        {
            GameObject cardObj = Instantiate(cardItemPrefab, cardContentPanel);
            cardObj.GetComponent<Image>().sprite = c.GetCardSprite();
            cardObj.name = c.ToString();
            c.SetTransform(cardObj.GetComponent<RectTransform>());

            Vector3 cardPos = new Vector3(originX, 0, 0);
            cardPos.x = startPos + count * (cardWidth + cardSpacing);
            cardObj.GetComponent<RectTransform>().localPosition = cardPos;
            count++;
        }
    }

    void HandleInfiniteScrolling()
    {
        int currentCardIndex = isPositiveScroll ? CardManager.instance.cards.Length - 1 : 0;
        Transform currentCard = cardContentPanel.GetChild(currentCardIndex);

        if (!CardOutOfView(currentCard))
            return;

        int endCardIndex = isPositiveScroll ? 0 : CardManager.instance.cards.Length - 1;
        Transform endCard = cardContentPanel.GetChild(endCardIndex);

        Vector3 newPos = endCard.position;

        if (isPositiveScroll)
        {
            newPos.x = endCard.position.x - (cardWidth + cardSpacing);
        }
        else
        {
            newPos.x = endCard.position.x + (cardWidth + cardSpacing);
        }

        currentCard.position = newPos;

        currentCard.SetSiblingIndex(endCardIndex);
    }

    bool CardOutOfView(Transform card)
    {
        float positiveXThreshold = cardViewportPanel.position.x + (cardViewportPanel.rect.width * 0.5f) + cardBoundsThreshold;
        float negativeXThreshold = cardViewportPanel.position.x - (cardViewportPanel.rect.width * 0.5f) - cardBoundsThreshold;
        return isPositiveScroll ? card.position.x - (cardWidth * 0.5f) > positiveXThreshold :
            card.position.x + (cardWidth * 0.5f) < negativeXThreshold;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        needsToSnap = false;
        lastDragPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        isPositiveScroll = eventData.position.x > lastDragPos.x;
        lastDragPos = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        needsToSnap = true;
    }

    public void OnScroll(PointerEventData eventData)
    {
        isPositiveScroll = eventData.scrollDelta.y < 0;
        needsToSnap = false;
        cardScrollRect.velocity = Vector2.zero;
        float newXPos = eventData.scrollDelta.y > 0 ? closestPosition - cardSnapDistanceMultiple : closestPosition + cardSnapDistanceMultiple;
        cardContentPanel.localPosition = new Vector3(newXPos, cardContentPanel.localPosition.y, cardContentPanel.localPosition.z);
    }

    public void OnScroll()
    {
        HandleInfiniteScrolling();
    }

    public void OnToggleDeck()
    {
        //StartCoroutine(FoldInOutDeck(!deckIsOut));
    }

    IEnumerator FoldInOutDeck(bool foldOut)
    {
        deckIsOut = foldOut;
        float endSizeX = foldOut ? cardLayoutSize : 0f;
        Vector2 startSizeDelta = cardLayout.sizeDelta;
        float timer = 0f;
        while (timer < foldOutCardTime)
        {
            timer += Time.deltaTime;
            cardLayout.sizeDelta = Vector2.Lerp(startSizeDelta, new Vector2(endSizeX, cardLayout.sizeDelta.y), timer / foldOutCardTime);
            yield return null;
        }
        cardLayout.sizeDelta = new Vector2(endSizeX, cardLayout.sizeDelta.y);
    }
}