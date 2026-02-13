using System.Collections.Generic;

namespace DCGO.CardEffects.ST19
{
    public class ST19_02 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Decoy

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                string DecoyDescription()
                {
                    return
                        "<Decoy ([Puppet] trait)> (When one of your other [Puppet] trait Digimon would be deleted by an opponent's effect, you may delete this Digimon to prevent that deletion.)";
                }

                bool CanSelectDecoyPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent != card.PermanentOfThisCard() &&
                           permanent.TopCard.ContainsTraits("Puppet") &&
                           permanent.willBeRemoveField;
                }

                cardEffects.Add(CardEffectFactory.DecoySelfEffect(isInheritedEffect: false, card: card, condition: null,
                    permanentCondition: CanSelectDecoyPermanentCondition, effectName: "Decoy ([Puppet] trait)",
                    effectDiscription: DecoyDescription()));
            }

            #endregion

            #region Barrier - ESS

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(
                    CardEffectFactory.BarrierSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}