using System;
using System.Collections.Generic;

public partial class CardEffectCommons
{
    #region Create Jogress Conditions from Permanent Conditions

    public static DigiXrosCondition GetDigiXrosConditionsFromNames(CardSource card, int CostReduction, Func<List<CardSource>, CardSource, bool> CanTargetCondition_ByPreSelecetedList, params string[] names)
    {
        
        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>();

        foreach(string name in names)
        {
            elements.Add(new DigiXrosConditionElement(cardSource => CardCondition(cardSource, name), name));
        }

        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, CanTargetCondition_ByPreSelecetedList, CostReduction);

        return digiXrosCondition;

        bool CardCondition(CardSource cardSource, string name)
        {
            return cardSource != null
                && cardSource.Owner == card.Owner
                && cardSource.IsDigimon
                && cardSource.CardNames_DigiXros.Contains(name);
        }
    }

    #endregion
}