using System.Collections.Generic;

public partial class CardEffectCommons
{
    public enum CardLocation
    {
        BATTLE_AREA,
        HAND,
        TRASH,
    }

    public static bool CanDnaDigivolve(CardSource cardSource, List<CardLocation> cardLocations)
    {
        if (cardSource.jogressCondition.Count == 0) return false;

        for (int i = 0; i < cardSource.jogressCondition.Count; i++)
        {
            var jogressCondition = cardSource.jogressCondition[1].elements[i];
            var location = cardLocations[i];

            if (location == CardLocation.BATTLE_AREA) return HasMatchConditionOwnersPermanent(cardSource, jogressCondition.EvoRootCondition);
            if (location == CardLocation.HAND) return HasMatchConditionOwnersHand(cardSource, cs => jogressCondition.EvoRootCondition(cs.PermanentOfThisCard()));
            if (location == CardLocation.TRASH) return HasMatchConditionOwnersCardInTrash(cardSource, cs => jogressCondition.EvoRootCondition(cs.PermanentOfThisCard()));
        }
        return true;
    }
}