using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Xml;


public static class Combinations
{
    public static void Sample()
    {
        List<string[]> sourceList = new List<string[]>(4);
        sourceList.Add(new string[] { "red", "yellow" });
        sourceList.Add(new string[] { "purple", "red"});
        sourceList.Add(new string[] { "purple", "red" });
        sourceList.Add(new string[] { "purple", "red" });
        List<string[]> resultList = GetCombinations(sourceList);

        HighestValue(resultList);
    }

    public static void NameSample()
    {
        List<string[]> sourceList = new List<string[]>(4);
        sourceList.Add(new string[] { "Takuya Kanbara & Koji Minamoto", "Takuya Kanbara", "Koji Minamoto" });
        sourceList.Add(new string[] { "Takuya Kanbara"});
        sourceList.Add(new string[] { "Takuya Kanbara & Koji Minamoto", "Takuya Kanbara", "Koji Minamoto" });
        sourceList.Add(new string[] { "Koji Minamoto" });
        List<string[]> resultList = GetCombinations(sourceList);

        HighestValue(resultList);
    }
    public static List<T[]> GetCombinations<T>(List<T[]> sourceList)
    {
        List<T[]> resultList = new List<T[]>();
        Stack<T> stack = new Stack<T>();
        GetCombinationsCore(stack, resultList, sourceList);

        return resultList;
    }

    private static void GetCombinationsCore<T>(Stack<T> stack, List<T[]> resultList, List<T[]> sourceList)
    {
        int dimension = stack.Count;
        if (sourceList.Count <= dimension)
        {
            T[] array = stack.ToArray();
            Array.Reverse(array);
            resultList.Add(array);
            return;
        }
        else
        {
            foreach (T item in sourceList[dimension])
            {
                stack.Push(item);
                GetCombinationsCore(stack, resultList, sourceList);
                stack.Pop();
            }
        }
    }

    static int HighestValue(List<string[]> resultList)
    {
        int highestCount = 0;

        foreach (string[] item in resultList)
        {
            Debug.Log(string.Join(",", item));

            int count = item.Distinct().ToArray().Length;

            if (count > highestCount)
            {
                highestCount = count;
                Debug.Log($"New Highest Count: {highestCount}");
            }
        }

        return highestCount;
    }

    //GetUniqueNameCardCount
    public static int GetUniqueNameCardCount(List<CardSource> cardSources)
    {
        List<string[]> cardNames = new List<string[]>();

        foreach (CardSource cardSource in cardSources)
            cardNames.Add(cardSource.CardNames.ToArray());

        return HighestValue(GetCombinations(cardNames));
    }

    //GetUniqueColorCardCount
    public static int GetUniqueColorCardCount(List<CardSource> cardSources)
    {
        List<string[]> cardColors = new List<string[]>();

        foreach (CardSource cardSource in cardSources)
            cardColors.Add(cardSource.CardColors.Map(x => x.ToString()).ToArray());

        return HighestValue(GetCombinations(cardColors));
    }

    //GetDifferenetColorCardCount
    public static int GetDifferenetColorCardCount(List<CardSource> cardSources, bool allowSkip = false)
    {
        List<CardColor[]> cardColors = new List<CardColor[]>();

        foreach (CardSource cardSource in cardSources)
        {
            cardColors.Add(cardSource.CardColors.ToArray());
        }

        List<CardColor[]> colorCombinations = Combinations.GetCombinations(cardColors);

        int maxColorCount = 0;

        //赤～白に対応するカード1枚を各色毎に格納する配列
        CardSource[] cardsCorrespondingToColor = new CardSource[System.Enum.GetValues(typeof(CardColor)).Length - 1];

        for (int i = 0; i < cardsCorrespondingToColor.Length; i++)
        {
            cardsCorrespondingToColor[i] = null;
        }

        foreach (CardColor[] cardColorArray in colorCombinations)
        {
            if (cardColorArray.Length == cardSources.Count)
            {
                for (int i = 0; i < cardColorArray.Length; i++)
                {
                    CardSource cardSource = cardSources[i];

                    if (allowSkip)
                    {
                        bool skip = false;

                        for (int j = 0; j < cardsCorrespondingToColor.Length; j++)
                        {
                            if (cardsCorrespondingToColor[j] != null)
                            {
                                //既に同じ組み合わせの色のカードが配列に格納されている場合
                                if (Enumerable.SequenceEqual(cardSource.CardColors.OrderBy(e => e), cardsCorrespondingToColor[j].CardColors.OrderBy(e => e)))
                                {
                                    UnityEngine.Debug.Log($"SKIPPING: {cardSource.BaseENGCardNameFromEntity}");
                                    skip = true;
                                    break;
                                }
                            }
                        }

                        if (skip)
                        {
                            continue;
                        }
                    }                    

                    CardColor cardColor = cardColorArray[i];

                    int colorIndex = (int)cardColor;

                    if (0 <= colorIndex && colorIndex <= cardsCorrespondingToColor.Length - 1)
                    {
                        if (cardsCorrespondingToColor[colorIndex] == null)
                        {
                            cardsCorrespondingToColor[colorIndex] = cardSource;
                        }
                    }
                }
            }
        }

        int colorCount = cardsCorrespondingToColor.ToList().Count((cardSource) => cardSource != null);

        if (colorCount >= maxColorCount)
        {
            maxColorCount = colorCount;
        }

        return maxColorCount;
    }
}