using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Ghoulmon
namespace DCGO.CardEffects.BT25
{
    public class BT25_076 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Would be Played
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 of your play cost 11 or lower [Negamon] in text with [Negamon] in sources to reduce by it's play cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                activateClass.SetIsDigimonEffect(true);

                string EffectDescription()
                {
                    return
                        "When this card would be played, by deleting 1 of your play cost 11 or lower Digimon with [Negamon] in its digivolution cards and [Negamon] in its text, reduce the cost by the deleted Digimon's play cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasPlayCost
                        && permanent.TopCard.GetCostItself <= 11
                        && permanent.TopCard.HasText("Negamon")
                        && permanent.DigivolutionCards.Count((digiCard) => digiCard.EqualsCardName("Negamon")) > 0
                        && !permanent.TopCard.CanNotBeAffected(activateClass)
                        && card.PayingCost(SelectCardEffect.Root.Hand, null, checkAvailability: false) - permanent.TopCard.GetCostItself <= card.Owner.MaxMemoryCost;//Check the permanent would sufficiently reduce cost to make a valid play
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canNoSelect = true;

                    if (card.PayingCost(SelectCardEffect.Root.Hand, null, checkAvailability: false) >
                        card.Owner.MaxMemoryCost)
                    {
                        canNoSelect = false;
                    }

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: canNoSelect,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.",
                        "The opponent is selecting 1 Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents.Count == 1)
                        {
                            int reducedCost = permanents[0].TopCard.GetCostItself;
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                targetPermanents: permanents,
                                activateClass: activateClass,
                                successProcess: SuccessProcess,
                                failureProcess: null));

                            IEnumerator SuccessProcess(List<Permanent> permanents)
                            {
                                if (card.Owner.CanReduceCost(null, card))
                                {
                                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                                }

                                ChangeCostClass changeCostClass = new ChangeCostClass();
                                changeCostClass.SetUpICardEffect($"Play Cost -{reducedCost}", CanUseCondition1, card);
                                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition,
                                    rootCondition: RootCondition, isUpDown: IsUpDown, isCheckAvailability: () => false,
                                    isChangePayingCost: () => true);
                                card.Owner.UntilCalculateFixedCostEffect.Add(_ => changeCostClass);

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(hashtable));

                                bool CanUseCondition1(Hashtable hashtable1)
                                {
                                    return true;
                                }

                                int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root,
                                    List<Permanent> targetPermanents)
                                {
                                    if (CardSourceCondition(cardSource) &&
                                        RootCondition(root) &&
                                        PermanentsCondition(targetPermanents))
                                    {
                                        cost -= reducedCost;
                                    }

                                    return cost;
                                }

                                bool PermanentsCondition(List<Permanent> targetPermanents)
                                {
                                    return targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0;
                                }

                                bool CardSourceCondition(CardSource cardSource)
                                {
                                    return cardSource == card;
                                }

                                bool RootCondition(SelectCardEffect.Root root)
                                {
                                    return true;
                                }

                                bool IsUpDown()
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Reduce Play Cost - Not Shown
            if (timing == EffectTiming.None && !CardEffectCommons.IsExistOnField(card))//do not apply this effect if the card is already in play anywhere to avoid stack overflow on GetCostItself
            {
                List<Permanent> permanents = card.Owner.GetBattleAreaDigimons().Filter(PermanentCondition);

                int reducedCost = permanents.Count > 0 ? permanents.Max(p => p.TopCard.GetCostItself): 0;

                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect($"Play Cost -{reducedCost}", CanUseCondition1, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition,
                    rootCondition: RootCondition, isUpDown: IsUpDown, isCheckAvailability: () => true,
                    isChangePayingCost: () => true);

                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition1(Hashtable hashtable1)
                {
                    return CardEffectCommons.HasMatchConditionPermanent(PermanentCondition);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasPlayCost
                        && permanent.TopCard.BasePlayCostFromEntity <= 11 // May cause inaccurate number that ignores Pyramidamon effect, but GetCostItself causes stack overflow
                        && permanent.TopCard.HasText("Negamon")
                        && permanent.DigivolutionCards.Count((cardSource) => cardSource.EqualsCardName("Negamon")) > 0;
                }

                int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root,
                    List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource) &&
                        RootCondition(root) &&
                        PermanentsCondition(targetPermanents))
                    {
                        cost -= reducedCost;
                    }

                    return cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    return targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                bool IsUpDown()
                {
                    return true;
                }
            }
            #endregion

            #region Rush
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

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

            #region Shared OP/WA/OD
            string SharedEffectName = "Delete 1 enemy Digimon with lowest play cost, if didn't delete, trash enemy security";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenAttacking: true,
                onDeletion: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] Delete 1 of your opponent's Digimon with the lowest play cost. If this effect didn't delete, trash your opponent's top security card.";
            }

            bool CanSelectMinCostCondition(Permanent permanent)
            {
                return CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, true);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                List<Permanent> deleteTargetPermanents = new List<Permanent>();

                bool failedToDelete = true;

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectMinCostCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectMinCostCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        deleteTargetPermanents = permanents.Clone();

                        yield return null;
                    }
                }

                if (deleteTargetPermanents.Count > 0)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargetPermanents, activateClass: activateClass, successProcess: SuccessDeleteProcess, failureProcess: null));

                    IEnumerator SuccessDeleteProcess(List<Permanent> permanents)
                    {
                        if (permanents.Count > 0)
                            failedToDelete = false;

                        yield return null;
                    }
                }

                if (failedToDelete)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner.Enemy,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
