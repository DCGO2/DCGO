using System.Collections;
using System.Collections.Generic;

public partial class CardEffectCommons
{
    #region Change target's original name to [Sukamon]
    public static IEnumerator ChangeDigimonNameToSukamon(Permanent selectedPermanent, CardSource card, ICardEffect activateClass)
    {
        string _separator = CardEffectFactory.NAME_SEPARATOR;

        if (selectedPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(selectedPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        ChangeCardNamesClass ChangeCardNamesClass = new ChangeCardNamesClass();
        ChangeCardNamesClass.SetUpICardEffect("Original card name is [Sukamon]", CanUseCondition1, card);
        ChangeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: ChangeCardNames);
        selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => ChangeCardNamesClass);

        bool CanUseCondition1(Hashtable hashtable)
        {
            return selectedPermanent.TopCard != null
                && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
        }

        List<string> ChangeCardNames(CardSource cardSource, List<string> CardNames)
        {
            if (cardSource == selectedPermanent.TopCard)
            {
                for (int i = 0; i < CardNames.Count; i++)
                {
                    if (CardNames[i].Contains(_separator))
                    {
                        int separatorIndex = CardNames[i].IndexOf(_separator);

                        CardNames[i] = "Sukamon" + CardNames[i][separatorIndex..];
                    }
                    else CardNames[i] = "Sukamon";
                }
            }

            return CardNames;
        }
    }
    #endregion
}