using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class KoHagurumon_BT16_052_token : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            string DecoyDiscription()
            {
                return "<Decoy (Black)> (When one of your other Black Digimon would be deleted by an opponent's effect, you may delete this Digimon to prevent that deletion.)";
            }

            bool CanSelectDecoyPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent != card.PermanentOfThisCard())
                    {
                        if (permanent.TopCard.CardColors.Contains(CardColor.Black))
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

            cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(false, card, null));            
            cardEffects.Add(CardEffectFactory.DecoySelfEffect(isInheritedEffect: false, card: card, condition: null, permanentCondition: CanSelectDecoyPermanentCondition, effectName: "Decoy ([Bagra Army])", effectDiscription: DecoyDiscription()));

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.CanNotAttackSelfStaticEffect(defenderCondition: null, isInheritedEffect: false, card: card, condition: Condition, effectName: "Can't Attack"));
            }

            return cardEffects;
        }
    }
}