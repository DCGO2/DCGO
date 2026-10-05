using System;
using System.Collections;
using System.Collections.Generic;

// Davis Motomiya & Ken Ichijoji
namespace DCGO.CardEffects.EX13
{
    public class EX13_070 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Turn
            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }
            #endregion

            #region End of Your Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending this Tamer, 1 Digimon may digivolve or 2 Digimon may DNA digivolve", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetHashString("EX13_070_EOT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[End of Your Turn] By suspending this Tamer, activate 1 of the effects below: - 1 of your Digimon may digivolve into a Digimon card with [Imperialdramon] in its name or the [Free] trait in the hand. Reduce this effect's paid cost by 1 for each of your opponent's Digimon. - 2 of your Digimon may DNA digivolve into a [Free] trait Digimon card in the hand.";

                bool IsOwnerDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool IsImperialdramonOrFreeDigimonCard(CardSource cardSource)
                    => cardSource.IsDigimon
                        && (cardSource.ContainsCardName("Imperialdramon") || cardSource.EqualsTraits("Free"));

                bool CanSelectDNACardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && cardSource.EqualsTraits("Free")
                        && cardSource.CanPlayJogress(true);

                bool CanDigivolve()
                    => CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsOwnerDigimon)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, IsImperialdramonOrFreeDigimonCard);

                bool CanDNADigivolve()
                    => CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectDNACardCondition);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass,
                        SuccessProcess,
                        null));

                    IEnumerator SuccessProcess(List<Permanent> suspendedPermanents)
                    {
                        bool canDigivolve = CanDigivolve();
                        bool canDNADigivolve = CanDNADigivolve();

                        if (!canDigivolve && !canDNADigivolve) yield break;

                        if (canDigivolve && canDNADigivolve)
                        {
                            List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                            {
                                new(message: "1 of your Digimon may digivolve into a Digimon card with [Imperialdramon] in its name or the [Free] trait in the hand. Reduce this effect's paid cost by 1 for each of your opponent's Digimon.", value: 1, spriteIndex: 0),
                                new(message: "2 of your Digimon may DNA digivolve into a [Free] trait Digimon card in the hand.", value: 2, spriteIndex: 0),
                            };

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: "Select 1 effect to activate.", notSelectPlayerMessage: "The opponent is selecting 1 effect to activate.");
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetInt(canDigivolve ? 1 : 2);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(DigivolveCoroutine());
                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                                CanSelectDNACardCondition,
                                payCost: true,
                                isHand: true,
                                activateClass));
                        }
                    }

                    IEnumerator DigivolveCoroutine()
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOwnerDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that may digivolve.", "The opponent is selecting 1 Digimon that may digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermanent == null) yield break;

                        int opponentDigimonCount = card.Owner.Enemy.GetBattleAreaDigimons().Count;

                        (int reduceCost, Func<CardSource, bool> reduceCostCardCondition)? reduceCostTuple = null;

                        if (opponentDigimonCount > 0)
                        {
                            reduceCostTuple = (reduceCost: opponentDigimonCount, reduceCostCardCondition: null);
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: selectedPermanent,
                            cardCondition: IsImperialdramonOrFreeDigimonCard,
                            payCost: true,
                            reduceCostTuple: reduceCostTuple,
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null));
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
