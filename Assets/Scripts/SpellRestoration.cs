using UnityEngine;
using System.Text;

public class SpellRestoration
{
    private string[,] convertTable = new string[65, 2] {
        { "A", "あ" },
        { "B", "い" },
        { "C", "う" },
        { "D", "え" },
        { "E", "お" },
        { "F", "か" },
        { "G", "き" },
        { "H", "く" },
        { "I", "け" },
        { "J", "こ" },
        { "K", "さ" },
        { "L", "し" },
        { "M", "す" },
        { "N", "せ" },
        { "O", "そ" },
        { "P", "た" },
        { "Q", "ち" },
        { "R", "つ" },
        { "S", "て" },
        { "T", "と" },
        { "U", "な" },
        { "V", "に" },
        { "W", "ぬ" },
        { "X", "ね" },
        { "Y", "の" },
        { "Z", "は" },
        { "a", "ひ" },
        { "b", "ふ" },
        { "c", "へ" },
        { "d", "ほ" },
        { "e", "ま" },
        { "f", "み" },
        { "g", "む" },
        { "h", "め" },
        { "i", "も" },
        { "j", "や" },
        { "k", "ゆ" },
        { "l", "よ" },
        { "m", "ら" },
        { "n", "り" },
        { "o", "る" },
        { "p", "れ" },
        { "q", "ろ" },
        { "r", "わ" },
        { "s", "が" },
        { "t", "ぎ" },
        { "u", "ぐ" },
        { "v", "げ" },
        { "w", "ご" },
        { "x", "ざ" },
        { "y", "じ" },
        { "z", "ず" },
        { "0", "ぜ" },
        { "1", "ぞ" },
        { "2", "だ" },
        { "3", "ぢ" },
        { "4", "づ" },
        { "5", "で" },
        { "6", "ど" },
        { "7", "ば" },
        { "8", "び" },
        { "9", "ぶ" },
        { "+", "べ" },
        { "/", "ぼ" },
        { "=", "ん" },
    };

    public string ToSpellRestoration(object data)
    {
        string json = JsonUtility.ToJson(data);
        string base64string = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        string spellRestoration = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        return spellRestoration;
    }

    public T FromSpellRestoration<T>(string spellRestoration)
    {
        try
        {
            byte[] byteArray = System.Convert.FromBase64String(spellRestoration);
            string jsonBack = Encoding.UTF8.GetString(byteArray);
            var restoreData = JsonUtility.FromJson<T>(jsonBack);

            return restoreData;
        }
        catch (System.FormatException exception)
        {
            return default(T);
        }
    }
}