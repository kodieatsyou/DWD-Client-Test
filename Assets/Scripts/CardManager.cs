using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Suit
{
    Heart,
    Diamond,
    Club,
    Spade
}

public class Card : IComparable<Card>
{
    private Suit suit;
    private int value;
    private Sprite cardSprite;
    private RectTransform cardTransform;

    public Card(Suit suit, int value)
    {
        this.suit = suit;
        this.value = value;
        this.cardSprite = CardManager.instance.cardSprites[(this.suit, this.value)];
    }

    public int CompareTo(Card other)
    {
        if(other == null)
        {
            throw new ArgumentNullException(nameof(other));
        }

        int compareSuit = suit.CompareTo(other.suit);
        if (compareSuit != 0)
        {
            return compareSuit;
        }

        return other.value.CompareTo(value);
    }

    public override string ToString()
    {
        return suit.ToString() + value;
    }

    public Sprite GetCardSprite() 
    {
        return cardSprite;
    }

    public void SetTransform(RectTransform cardTransform)
    {
        this.cardTransform = cardTransform;
    }

    public RectTransform GetCardTransform()
    {
        return cardTransform;
    }
}

public class CardManager : MonoBehaviour
{

    public static CardManager instance;
    
    public Dictionary<(Suit, int), Sprite> cardSprites = new Dictionary<(Suit, int), Sprite> ();

    public Card[] cards = new Card[8];

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(instance);
        }
        instance = this;

        LoadCardSprites();
    }

    private void LoadCardSprites()
    {
        foreach(Suit s in Enum.GetValues(typeof(Suit)))
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>($"CardSprites/{s.ToString()}");
            for(int i = 0; i < sprites.Length; i++)
            {
                //For ace
                if(i == 0)
                {
                    cardSprites.Add((s, 14), sprites[i]);
                }
                cardSprites.Add((s, i+1), sprites[i]);
            }
        }
    }

    private void Start()
    {
        
        SetStartingHand();
    }

    private void SetStartingHand()
    {
        cards[0] = new Card(Suit.Heart, 13);
        cards[1] = new Card(Suit.Heart, 2);
        cards[2] = new Card(Suit.Spade, 11);
        cards[3] = new Card(Suit.Spade, 14);
        cards[4] = new Card(Suit.Diamond, 9);
        cards[5] = new Card(Suit.Diamond, 12);
        cards[6] = new Card(Suit.Club, 12);
        cards[7] = new Card(Suit.Club, 4);

        Array.Sort(cards);
    }
}
