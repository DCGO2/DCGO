using System.Collections;
using System.Collections.Generic;

//ST20-10 Agumon
namespace DCGO.CardEffects.ST20
{
    public class ST20_10 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return (targetPermanent.TopCard.HasAdventureTraits || targetPermanent.TopCard.HasHeroTraits) && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 2;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Warp Digivolution
            if (timing == EffectTiming.None)
            {
                bool enoughTamerColours()
                {
                    List<CardSource> tamerCards = new List<CardSource>();

                    foreach (Permanent permanent in card.Owner.GetBattleAreaPermanents())
                    {
                        if (permanent.IsTamer)
                        {
                            tamerCards.Add(permanent.TopCard);
                        }
                    }
                    return Combinations.GetDifferenetColorCardCount(tamerCards) >= 3;
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsOwnerTurn(card) && CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, (permanent) => permanent.IsDigimon && permanent.HasDP && permanent.DP >= 10000))
                        {
                            return true;
                        }
                        return enoughTamerColours();
                    }
                    return false;
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent == card.PermanentOfThisCard();
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        //compared to the code I was copying from (ST8-04), removed a check that owner matched between cardsource and card, as that should be handled by whose hand the card is in. If this breaks, try putting it back.
                        if (card.Owner.HandCards.Contains(cardSource))
                        {
                            return cardSource.EqualsCardName("WarGreymon");
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: true, card: card, condition: Condition, cardCondition: CardCondition));
            }
            #endregion

            #region Reboot - ESS
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}