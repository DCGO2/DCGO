using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


public static class Combinations
{
    public static void Sample()
    {
        List<string[]> sourceList = new List<string[]>(3);
        sourceList.Add(new string[] { "a", "b" });
        sourceList.Add(new string[] { "c", "d", "e" });
        sourceList.Add(new string[] { "f", "g", "h", "i" });
        List<string[]> resultList = GetCombinations(sourceList);

        foreach (string[] item in resultList)
        {
            Debug.Log(string.Join(",", item));
        }
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

    //カードリストの内、異なる色を持つカードの枚数
    public static int GetDifferenetColorCardCount(List<CardSource> cardSources)
    {
        List<CardColor[]> cardColors = new List<CardColor[]>();

        foreach (CardSource cardSource in cardSources)
        {
            cardColors.Add(cardSource.CardColors.ToArray());
        }

        List<CardColor[]> colorCombinations = Combinations.GetCombinations(cardColors);

        int maxColorCount = 0;

        foreach (CardColor[] cardColorArray in colorCombinations)
        {
            //赤～白に対応するカード1枚を各色毎に格納する配列
            CardSource[] cardsCorrespondingToColor = new CardSource[System.Enum.GetValues(typeof(CardColor)).Length - 1];

            for (int i = 0; i < cardsCorrespondingToColor.Length; i++)
            {
                cardsCorrespondingToColor[i] = null;
            }

            if (cardColorArray.Length == cardSources.Count)
            {
                for (int i = 0; i < cardColorArray.Length; i++)
                {
                    CardSource cardSource = cardSources[i];

                    bool skip = false;

                    for (int j = 0; j < cardsCorrespondingToColor.Length; j++)
                    {
                        if (cardsCorrespondingToColor[j] != null)
                        {
                            //既に同じ組み合わせの色のカードが配列に格納されている場合
                            if (Enumerable.SequenceEqual(cardSource.CardColors.OrderBy(e => e), cardsCorrespondingToColor[j].CardColors.OrderBy(e => e)))
                            {
                                skip = true;
                                break;
                            }
                        }
                    }

                    if (skip)
                    {
                        continue;
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

            int colorCount = cardsCorrespondingToColor.ToList().Count((cardSource) => cardSource != null);

            if (colorCount >= maxColorCount)
            {
                maxColorCount = colorCount;
            }
        }

        return maxColorCount;
    }
}