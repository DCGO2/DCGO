using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ConvertBinaryNumber
{
    public static string[] numbers = new string[]
    {
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f" , "g" , "h" , "i" , "j",
        "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" , "A" , "B" , "C" , "D",
        "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T" , "U" , "V" , "W" , "X",
        "Y", "Z", "!", "#", "$", "%", "&", "'", "ザ", "ジ", "=", "~", "`", "@", "ズ", "ゼ" , "ゾ" , "ダ" , "ｭ" , ":",
        "ド", "ヅ", "デ", "ヂ", "/", "?", "ｬ", "ゲ", "ゴ", "あ", "い", "う", "え", "お", "か", "き" , "く" , "け" , "こ" , "さ",
        "し", "す", "せ", "そ", "た", "ち", "つ", "て", "と", "な", "に", "ぬ", "ね", "の", "は", "ひ" , "ふ" , "へ" , "ほ" , "ま",
        "み", "む", "め", "も", "や", "ゆ", "よ", "ら", "り", "る", "れ", "ろ", "わ", "を", "ん", "っ" , "ゃ" , "ゅ" , "ょ" , "ぁ",
        "ぃ", "ぅ", "ぇ", "ぉ", "ア", "イ", "ウ", "エ", "オ", "カ", "キ", "ク", "ケ", "コ", "サ", "シ" , "ス" , "セ" , "ソ" , "タ",
        "チ", "ツ", "テ", "ト", "ナ", "ニ", "ヌ", "ネ", "ノ", "ハ", "ヒ", "フ", "ヘ", "ホ", "マ", "ミ" , "ム" , "メ" , "モ" , "ヤ",
        "ユ", "ヨ", "ラ", "リ", "ル", "レ", "ロ", "ワ", "ヲ", "ン", "ッ", "ャ", "ュ", "ョ", "ァ", "ィ" , "ゥ" , "ェ" , "ォ" , "が",
        "ぎ", "ぐ", "げ", "ご", "ざ", "じ", "ず", "ぜ", "ぞ", "だ", "ぢ", "づ", "で", "ど", "ば", "び" , "ぶ" , "べ" , "ぼ" , "ぱ",
        "α", "β", "γ", "ｧ", "δ", "ε", "ζ", "η", "θ", "ι", "κ", "λ", "μ", "ν", "ξ", "ο" , "π" , "ρ" , "σ" , "τ",
        "υ", "φ", "ψ", "ω", "Ω", "Δ", "Γ", "Θ", "Λ", "Ξ", "Π", "Σ", "Φ", "ぴ", "ぷ", "ぺ" , "ぽ" , "ガ" , "ギ" , "グ",
    };

    //Convert n-decimal number to n^k-decimal number
    public static string NStringToNKString(string x, int n, int k)
    {
        //k letters are cut out one by one
        string[] SplitText = SplitClass.Split(x, k);

        string result = null;

        for (int i = 0; i < SplitText.Count(); i++)
        {
            //Convert to decimal
            int x_10 = NStringToInt(SplitText[i], n);

            //Convert to n^k decimal
            string x_nk = IntToNString(x_10, (int)Mathf.Pow(n, k));

            result += x_nk;
        }

        return result;
    }

    //Convert n^k decimal numbers to n decimal numbers
    public static string NKStringToNString(string x, int n, int k)
    {
        //Cutting one character at a time
        string[] SplitText = SplitClass.Split(x, 1);

        string result = null;

        for (int i = 0; i < SplitText.Length; i++)
        {
            //Convert to decimal
            int x_10 = NStringToInt(SplitText[i], n);

            if (x_10 == 114514)
            {
                return "114514";
            }

            //Converted to n decimal
            string x_n = IntToNString(x_10, n);

            //Cutting one character at a time
            string[] Split_x_n = SplitClass.Split(x_n, 1);

            string[] x_n_i = new string[k];

            //Put initial value 0
            for (int j = 0; j < k; j++)
            {
                x_n_i[j] = "0";
            }

            for (int j = 0; j < Split_x_n.Length; j++)
            {
                x_n_i[j + k - Split_x_n.Length] = Split_x_n[j];
            }

            for (int j = 0; j < k; j++)
            {
                result += x_n_i[j];
            }
        }

        return result;
    }

    //Convert decimal int to n-decimal string
    public static string IntToNString(int x, int n)
    {
        if (x < 0)
        {
            return null;
        }

        if (x == 0)
        {
            return "0";
        }

        var nstring = "";
        int r = 1;  // over
        int q = x;  // quotient

        // Loop until quotient reaches 0
        while (q > 0)
        {
            r = q % n;
            nstring += numbers[r];
            q = q / n;
        }

        return string.Join("", nstring.Reverse());
    }

    //Convert n-decimal string to decimal int
    public static int NStringToInt(string x, int n)
    {
        //Cutting one character at a time
        string[] SplitText = SplitClass.Split(x, 1);
        int _x = 0;

        for (int i = 0; i < SplitText.Reverse().Count(); i++)
        {
            string SplitedText = SplitText.Reverse().ToArray()[i];

            int delta = Array.IndexOf(numbers, SplitedText) * (int)Mathf.Pow(n, i);

            if (delta == -1)
            {
                return 114514;
            }

            _x += delta;
        }

        return _x;
    }
}