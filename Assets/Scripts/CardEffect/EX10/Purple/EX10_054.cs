using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

// VenomMyotismon
namespace DCGO.CardEffects.EX10
{
    public class EX10_054 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Trash - Main

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play for reduced cost of 7", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsDigimonEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Trash] [Main] By deleting 1 of your level 5 Digimon with [Myotismon] in its text, play this card with the play cost reduced by 7.";
                }

                bool IsLvl5Myotismon(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card) &&
                           targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5 &&
                           targetPermanent.TopCard.HasText("Myotismon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card) &&
                           CardEffectCommons.MatchConditionPermanentCount(IsLvl5Myotismon) >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent targetPermament = null;
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsLvl5Myotismon));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsLvl5Myotismon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        targetPermament = permanent;
                        yield return null;
                    }

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    if (targetPermament != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent>() { targetPermament },
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                    IEnumerator SuccessProcess(List<Permanent> permanents)
                    {
                        #region reduce play cost

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect("Play Cost -7", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                        card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                        ICardEffect GetCardEffect(EffectTiming _timing)
                        {
                            if (_timing == EffectTiming.None)
                            {
                                return changeCostClass;
                            }

                            return null;
                        }

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return true;
                        }

                        int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                        {
                            if (CardSourceCondition(cardSource))
                            {
                                if (RootCondition(root))
                                {
                                    if (PermanentsCondition(targetPermanents))
                                    {
                                        Cost -= 7;
                                    }
                                }
                            }

                            return Cost;
                        }

                        bool PermanentsCondition(List<Permanent> targetPermanents)
                        {
                            if (targetPermanents == null)
                            {
                                return true;
                            }
                            else
                            {
                                if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    if (cardSource == card)
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        bool RootCondition(SelectCardEffect.Root root)
                        {
                            return true;
                        }

                        bool isUpDown()
                        {
                            return true;
                        }

                        #endregion

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                            cardSources: new List<CardSource> { card },
                                            activateClass: activateClass,
                                            payCost: true,
                                            isTapped: false,
                                            root: SelectCardEffect.Root.Trash,
                                            activateETB: true));

                        #region release reducing play cost

                        card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);

                        #endregion
                    }
                }
            }

            #endregion

            #region On Play/When Digivolving Shared

            const string OnPlayWhenDigiEffectDescription = "You may suspend 2 of your opponent's Digimon or Tamers. Then, 2 of their Digimon or Tamers can't unsuspend until their turn ends.";

            bool CanSelectPermanentConditionShared(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    || CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaTamer(permanent, card);
            }

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                       CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentConditionShared);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentConditionShared))
                {
                    int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentConditionShared));
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentConditionShared,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentConditionShared,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: true,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 2 Digimon or Tamers that will be unable to unsuspend.",
                        "The opponent is selecting 2 Digimon or Tamers that will be unable to unsuspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendUntilOpponentTurnEnd(
                            targetPermanent: permanent,
                            activateClass: activateClass
                        ));
                    }
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 2 opponent Digimon or Tamers", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] " + OnPlayWhenDigiEffectDescription;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 2 opponent Digimon or Tamers", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] " + OnPlayWhenDigiEffectDescription;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }

            #endregion

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 suspended Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] Delete 1 of your opponent's suspended Digimon.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.IsSuspended)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletion(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}