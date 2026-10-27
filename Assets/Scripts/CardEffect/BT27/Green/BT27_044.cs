using System.Collections.Generic;

// Gekkomon
namespace DCGO.CardEffects.BT27
{
    public class BT27_044 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasGlowingDawnTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 0, false, card, null, level: 2));
            }
            #endregion

            #region Your Turn - Digivolution Cost Reduction
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent == card.PermanentOfThisCard();
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.CardColors.Contains(CardColor.Green)
                        && cardSource.HasBeatBreakTraits;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                cardEffects.Add(CardEffectFactory.ChangeDigivolutionCostStaticEffect(
                    changeValue: -1,
                    permanentCondition: PermanentCondition,
                    cardCondition: CardSourceCondition,
                    rootCondition: RootCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    setFixedCost: false));
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
