using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

//ST20-03 Birdramon
namespace DCGO.CardEffects.ST20
{
    public class ST20_03 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasAdventureTraits && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 3;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region On-Play/When-Digivolving Shared
            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    List<CardSource> tamerCards = new List<CardSource>();

                    foreach (Permanent permanent in card.Owner.GetBattleAreaPermanents())
                    {
                        if (permanent.IsTamer && permanent.TopCard.HasAdventureTraits)
                        {
                            tamerCards.Add(permanent.TopCard);
                        }
                    }

                    return Combinations.GetDifferenetColorCardCount(tamerCards) >= 3;
                }
                return false;
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If your Tamers with the [ADVENTURE] trait have 3 or more total colors, this Digimon may digivolve into a Digimon card with the [ADVENTURE] trait in the hand without paying the cost.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] If your Tamers with the [ADVENTURE] trait have 3 or more total colors, this Digimon may digivolve into a Digimon card with the [ADVENTURE] trait in the hand without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
                

                bool CanDigivolveIntoCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.HasAdventureTraits;
                }


                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: card.PermanentOfThisCard(),
                            cardCondition: CanDigivolveIntoCardCondition,
                            payCost: false,
                            reduceCostTuple: null,
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null));
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If your Tamers with the [ADVENTURE] trait have 3 or more total colors, this Digimon may digivolve into a Digimon card with the [ADVENTURE] trait in the hand without paying the cost.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If your Tamers with the [ADVENTURE] trait have 3 or more total colors, this Digimon may digivolve into a Digimon card with the [ADVENTURE] trait in the hand without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanDigivolveIntoCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.HasAdventureTraits;
                }


                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: card.PermanentOfThisCard(),
                            cardCondition: CanDigivolveIntoCardCondition,
                            payCost: false,
                            reduceCostTuple: null,
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null));
                }
            }
            #endregion

            #region Your Turn - ESS

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
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