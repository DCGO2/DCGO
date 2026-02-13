using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "When this digivolution card is trashed due to activating this Digimon's <Digi-Burst>" effect
    public static bool CanTriggerOnTrashBySelfDigiBurst(Hashtable hashtable, CardSource card)
    {
        bool CardEffectCondition(ICardEffect cardEffect)
        {
            if (cardEffect != null)
            {
                if (!string.IsNullOrEmpty(cardEffect.EffectDiscription))
                {
                    if (cardEffect.EffectDiscription.Contains("Digi-Burst"))
                    {
                        if (cardEffect.EffectSourceCard != null)
                        {
                            if (IsExistOnBattleArea(cardEffect.EffectSourceCard))
                            {
                                if (cardEffect.EffectSourceCard.PermanentOfThisCard().cardSources.Contains(card))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        return CanTriggerOnTrashSelfDigivolutionCard(hashtable, CardEffectCondition, card);

    }
    #endregion
}