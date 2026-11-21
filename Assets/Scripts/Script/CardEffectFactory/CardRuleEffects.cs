using System.Collections.Generic;

public partial class CardEffectFactory
{
    public static string NAME_SEPARATOR = "§";
    public static ChangeBaseCardNameClass NameTreatedAsIncluding(CardSource card, string otherName)
    {
        ChangeBaseCardNameClass ChangeBaseCardNameClass = new ChangeBaseCardNameClass();
        ChangeBaseCardNameClass.SetUpICardEffect($"Also treated as including [{otherName}]", _ => true, card);
        ChangeBaseCardNameClass.SetUpChangeBaseCardNamesClass(changeBaseCardNames: ChangeBaseCardNames);

        return ChangeBaseCardNameClass;

        List<string> ChangeBaseCardNames(CardSource cardSource, List<string> cardNames)
        {
            if (cardSource == card)
            {
                for (int i = 0; i < cardNames.Count; i++)
                {
                    if (!cardNames[i].Contains(otherName))
                    {
                        cardNames[i] = $"{cardNames[i]}{NAME_SEPARATOR}{otherName}";
                    }
                }
            }

            return cardNames;
        }
    }

    public static ChangeBaseCardNameClass NameIsAlso(CardSource card, params string[] otherNames)
    {
        string nameDisplay = string.Join("/", otherNames);
        ChangeBaseCardNameClass ChangeBaseCardNameClass = new ChangeBaseCardNameClass();
        ChangeBaseCardNameClass.SetUpICardEffect($"Also treated as [{nameDisplay}]", _ => true, card);
        ChangeBaseCardNameClass.SetUpChangeBaseCardNamesClass(changeBaseCardNames: ChangeBaseCardNames);

        return ChangeBaseCardNameClass;

        List<string> ChangeBaseCardNames(CardSource cardSource, List<string> CardNames)
        {
            if (cardSource == card)
            {
                CardNames.AddRange(otherNames);
            }

            return CardNames;
        }
    }
}