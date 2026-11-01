using System;
using System.Collections.Generic;

public partial class CardEffectFactory
{

    public static string NAME_SEPARATOR = "§";
    public static ChangeCardNamesClass NameTreatedAsIncluding(CardSource card, string otherName)
    {
        ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
        changeCardNamesClass.SetUpICardEffect($"Also treated as including [{otherName}]", _ => true, card);
        changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);

        return changeCardNamesClass;

        List<string> changeCardNames(CardSource cardSource, List<string> cardNames)
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

    public static ChangeCardNamesClass NameIsAlso(CardSource card, params string[] otherNames)
    {
        string nameDisplay = string.Join("/", otherNames);
        ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
        changeCardNamesClass.SetUpICardEffect($"Also treated as [{nameDisplay}]", _ => true, card);
        changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);

        return changeCardNamesClass;

        List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
        {
            if (cardSource == card)
            {
                CardNames.AddRange(otherNames);
            }

            return CardNames;
        }
    }
}