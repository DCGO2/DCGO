using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.Tokens
{
    public class BT20_017_token : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Decoy

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                string DecoyDiscription()
                {
                    return "<Decoy (Red)/(Black)> (When one of your other Red or Black Digimon would be deleted by an opponent's effect, you may delete this Digimon to prevent that deletion.)";
                }

                bool CanSelectDecoyPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent != card.PermanentOfThisCard())
                        {
                            if (permanent.TopCard.CardColors.Contains(CardColor.Red) || permanent.TopCard.CardColors.Contains(CardColor.Black))
                            {
                                if (permanent.willBeRemoveField)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.DecoySelfEffect(isInheritedEffect: false, card: card, condition: null, permanentCondition: CanSelectDecoyPermanentCondition, effectName: "Decoy (Red/Black)", effectDiscription: DecoyDiscription()));
            }

            #endregion

            return cardEffects;
        }
    }
}