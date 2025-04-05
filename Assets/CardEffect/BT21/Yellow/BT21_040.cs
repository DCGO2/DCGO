using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT21
{
    public class BT21_040 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return (targetPermanent.TopCard.EqualsCardName("Koromon") || (targetPermanent.TopCard.HasHeroTraits) && targetPermanent.TopCard.IsLevel2);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsOwnerTurn(card) && CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, (permanent) => permanent.IsDigimon && permanent.Level >= 6 && permanent.TopCard.HasLevel))
                        {
                            return true;
                        }

                        if (card.Owner.GetBattleAreaPermanents().Count((permanent) => permanent.IsTamer && permanent.TopCard.HasHeroTraits) >= 3)
                        {
                            if (card.Owner.LibraryCards.Count >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent == card.PermanentOfThisCard();
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource.Owner.HandCards.Contains(cardSource))
                    {
                        if (cardSource.EqualsCardName("ShineGreymon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                string effectName = $"Can digivolve to [ShineGreymon]";

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: true, card: card, condition: Condition, effectName: effectName, cardCondition: CardCondition));
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 2000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}