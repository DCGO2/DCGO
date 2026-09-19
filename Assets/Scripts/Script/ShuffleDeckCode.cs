using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ShuffleDeckCode : MonoBehaviour
{
    [Header("シャッフルされた文字番号")]
    public int[] ShuffledNumberIDs;

    //文字列を逆から読む
    public string[] ReversNumberIDs(string[] numbers)
    {
        return numbers.Reverse().ToArray();
    }

    //1文字をConvertBinaryNumber→乱数文字に変換
    public string ConvertString(string s)
    {
        string t = "";

        if(ConvertBinaryNumber.numbers.Contains(s))
        {
            int index = Array.IndexOf(ConvertBinaryNumber.numbers, s);

            int convertIndex = Array.IndexOf(ShuffledNumberIDs, index);

            if(0 <= convertIndex && convertIndex < ConvertBinaryNumber.numbers.Length)
            {
                t = ConvertBinaryNumber.numbers[convertIndex];
            }
        }

        else
        {

        }

        return t;
    }

    //1文字を乱数文字→ConvertBinaryNumberに変換
    public string ReturnConvertString(string t)
    {
        string s = "";

        if (ConvertBinaryNumber.numbers.Contains(t))
        {
            int index = Array.IndexOf(ConvertBinaryNumber.numbers, t);

            int convertIndex = ShuffledNumberIDs[index];

            if (0 <= convertIndex && convertIndex < ConvertBinaryNumber.numbers.Length)
            {
                s = ConvertBinaryNumber.numbers[convertIndex];
            }
        }

        return s;
    }

    //index番目の文字を変換する回数
    int ConvFactor(int index)
    {
        //変換パラメータ
        float parameter1 = 7.2f;
        float parameter2 = 13.7f;
        float parameter3 = 5.6f;
        float parameter4 = 0.7f;

        return ((int)(parameter3 * Mathf.Pow(index + 1, 3) + parameter2 * Mathf.Pow(index + 1, 2) + parameter1 * Mathf.Pow(index + 1, 1) + parameter4)) % 13;
    }

    //DeckCodeを暗号文字列化
    public string GetConvertDeckCode(string DeckCode)
    {
        string ConvertedDeckCode = "";

        string[] parseByComma = DeckCode.Split(',');

        if (parseByComma.Length >= 3)
        {
            parseByComma[1] = GetConvertParseByComma(parseByComma[1]);

            parseByComma[2] = GetConvertParseByComma(parseByComma[2]);

            ConvertedDeckCode = $"{parseByComma[0]},{parseByComma[1]},{parseByComma[2]},";

            string GetConvertParseByComma(string text)
            {
                string[] SplitText = SplitClass.Split(text, 1);

                SplitText = ReversNumberIDs(SplitText);

                for (int i = 0; i < SplitText.Length; i++)
                {
                    string t = SplitText[i];

                    for (int j = 0; j < ConvFactor(i); j++)
                    {
                        t = ConvertString(t);
                    }

                    SplitText[i] = t;
                }

                string ConvertedParseByComma = "";

                for (int i = 0; i < SplitText.Length; i++)
                {
                    ConvertedParseByComma += SplitText[i];
                }

                return ConvertedParseByComma;
            }
        }
            
        return ConvertedDeckCode;
    }

    //暗号文字列をDeckCodeに復号
    public string GetDeckCode(string ConvertedDeckCode)
    {
        string DeckCode = "";

        string[] parseByComma = ConvertedDeckCode.Split(',');

        if(parseByComma.Length == 3 || parseByComma.Length == 4)
        {
            parseByComma[1] = GetConvertParseByComma(parseByComma[1]);

            parseByComma[2] = GetConvertParseByComma(parseByComma[2]);

            DeckCode = $"{parseByComma[0]},{parseByComma[1]},{parseByComma[2]},";

            string GetConvertParseByComma(string text)
            {
                string[] SplitText = SplitClass.Split(text, 1);

                for (int i = 0; i < SplitText.Length; i++)
                {
                    string t = SplitText[i];

                    for (int j = 0; j < ConvFactor(i); j++)
                    {
                        t = ReturnConvertString(t);
                    }

                    SplitText[i] = t;
                }

                SplitText = ReversNumberIDs(SplitText);

                string ConvertedParseByComma = "";

                for (int i = 0; i < SplitText.Length; i++)
                {
                    ConvertedParseByComma += SplitText[i];
                }

                return ConvertedParseByComma;
            }
        }
        
        return DeckCode;      
    }
}
