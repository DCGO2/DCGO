using System.Collections;
using System.Collections.Generic;

public partial class CardEffectCommons
{
    #region Change target's original name to [Sukamon]
    public static IEnumerator ChangeDigimonName(Permanent selectedPermanent, CardSource card, ICardEffect activateClass, string newName)
    {
        string _separator = CardEffectFactory.NAME_SEPARATOR;

        if (selectedPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(selectedPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        OverrideBaseCardNameClass OverrideBaseCardNameClass = new OverrideBaseCardNameClass();
        OverrideBaseCardNameClass.SetUpICardEffect($"Original card name is [{newName}]", CanUseCondition1, card);
        OverrideBaseCardNameClass.SetUpOverrideBaseCardNamesClass(changeBaseCardNames: ChangeCardNames);
        selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => OverrideBaseCardNameClass);

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

                        CardNames[i] = newName + CardNames[i][separatorIndex..];
                    }
                    else CardNames[i] = newName;
                }
            }

            return CardNames;
        }
    }
    #endregion
}