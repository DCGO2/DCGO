using System;
using System.Collections;
using System.Collections.Generic;

// Salamon
namespace DCGO.CardEffects.BT26
{
    public class BT26_066 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 2));
            }
            #endregion

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.StartOfYourMainPhaseClass(
                    card,
                    "If 5 or fewer hand cards, 1 [Titan] Digimon may digivolve into [Titan] trash card for 2 less",
                    ActivateCoroutine,
                    EffectDescription(),
                    additionalActivateCondition: AdditionalActivateCondition,
                    optional: false,
                    isSkippable: true
                ));

                string EffectDescription()
                    => "[Start of Your Main Phase] If your hand has 5 or fewer cards, 1 of your Digimon with the [Titan] trait may digivolve into a Digimon card with the [Titan] trait in the trash with the cost reduced by 2.";

                bool CanSelectTargetPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.ContainsTraits("Titan");

                bool CardCondition(CardSource cardSource)
                    => cardSource.IsDigimon && cardSource.ContainsTraits("Titan");

                bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                    => card.Owner.HandCards.Count <= 5
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectTargetPermanentCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTargetPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectTargetPermanentCondition));

                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectTargetPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 [Titan] Digimon to digivolve.", "The opponent is selecting 1 [Titan] Digimon to digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: selectedPermanent,
                                cardCondition: CardCondition,
                                payCost: true,
                                reduceCostTuple: (2, CardCondition),
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: false,
                                activateClass: activateClass,
                                successProcess: null,
                                isOptional: true));
                        }
                    }
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.OnDiscardHand)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [Titamon]/[Titan] trash card for 1 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT26_066_Inherit");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] [Once Per Turn] When your hand is trashed from, this [Titan] trait Digimon may digivolve into [Titamon] or a Digimon card with the [Titan] trait in the trash with the cost reduced by 1.";

                bool CardCondition(CardSource cardSource)
                    => cardSource.IsDigimon && (cardSource.EqualsCardName("Titamon") || cardSource.ContainsTraits("Titan"));

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerOnTrashHand(hashtable, null, cardSource => cardSource.Owner == card.Owner);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: CardCondition,
                        payCost: true,
                        reduceCostTuple: (1, CardCondition),
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: false,
                        activateClass: activateClass,
                        successProcess: null,
                        isOptional: true));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
