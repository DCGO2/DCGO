using System.Collections;
using System.Collections.Generic;

// Gemmon
namespace DCGO.CardEffects.BD01
{
    public class BD01_02 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region All Turns - ESS
            if (timing == EffectTiming.None)
            {
                ChangeDPDeleteEffectMaxDPClass changeDPDeleteEffectMaxDPClass = new ChangeDPDeleteEffectMaxDPClass();
                changeDPDeleteEffectMaxDPClass.SetUpICardEffect("Maximum DP of DP-based deletion effects gets +1000 DP for this [ALYSIAN] trait Digimon", CanUseCondition, card);
                changeDPDeleteEffectMaxDPClass.SetUpChangeDPDeleteEffectMaxDPClass(changeMaxDP: ChangeMaxDP);
                changeDPDeleteEffectMaxDPClass.SetIsInheritedEffect(true);
                cardEffects.Add(changeDPDeleteEffectMaxDPClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.PermanentOfThisCard().TopCard.HasAlysianTraits;
                }

                int ChangeMaxDP(int maxDP, ICardEffect cardEffect)
                {
                    if (cardEffect != null
                        && cardEffect.EffectSourceCard != null
                        && cardEffect.EffectSourceCard.PermanentOfThisCard() != null
                        && cardEffect.EffectSourceCard.PermanentOfThisCard() == card.PermanentOfThisCard())
                    {
                        maxDP += 1000;
                    }

                    return maxDP;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
