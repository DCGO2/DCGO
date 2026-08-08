using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.RB1
{
    public class RB1_009 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Gammamon") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return card.Owner.HandCards.Contains(card);
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(targetPermanent))
                    {
                        if (targetPermanent.TopCard.CardNames.Contains("Gammamon"))
                        {
                            if (targetPermanent.DigivolutionCards.Count(cardSource => cardSource.ContainsCardName("Gammamon")) >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: true, card: card, condition: Condition));
            }

            #region All Turns Copy effects of Gammamon in digivolution cards
            if (timing == EffectTiming.None)
            {
                bool CopyCardCondition(CardSource cardSource) => cardSource.ContainsCardName("Gammamon");

                cardEffects.Add(CardEffectFactory.CopyDigivolutionCardEffects(ref cardEffects, timing, card, cardCondition: CopyCardCondition));
                cardEffects.Add(CardEffectFactory.CopyDigivolutionCardEffects(ref cardEffects, timing, card, isInheritedEffect: true, cardCondition: CopyCardCondition));
            }
            #endregion

            return cardEffects;
        }
    }
}