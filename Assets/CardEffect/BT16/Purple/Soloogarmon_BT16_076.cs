using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT16
{
    public class Soloogarmon_BT16_076 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolve Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsTraits("SoC") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete an opponent's Digimon with 6000 DP or less, if the effect didn't delete, play a level 4 or lower [SoC] card.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By trashing 2 cards in your hand, delete 1 of your opponent's Digimon with 6000 DP or less. If this effect didn't delete, you may play 1 level 4 or lower card with the [SoC] trait from your trash without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable,card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if(CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.IsDigimon)
                        {
                            if(permanent.HasDP && permanent.DP <= 6000)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.IsExistOnTrash(cardSource))
                    {
                        if (cardSource.Level <= 4 && cardSource.ContainsTraits("SoC") && (cardSource.IsDigimon || cardSource.IsDigiEgg))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count >= 2)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    bool discarded = false;
                    List<Permanent> deleteTargetPermanents = new List<Permanent>();

                    if (card.Owner.HandCards.Count >= 2)
                    {
                        int discardCount = 2;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 2)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }

                        if (discarded)
                        {
                            if(CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentCondition))
                            {
                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    canTargetCondition: CanSelectPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                yield return StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                                {
                                    deleteTargetPermanents = permanents.Clone();

                                    yield return null;
                                }
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                targetPermanents: deleteTargetPermanents,
                                activateClass: activateClass,
                                successProcess: null,
                                failureProcess: FailureProcess));

                            IEnumerator FailureProcess()
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                                {
                                    int maxCount1 = 1;

                                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                                canTargetCondition: CanSelectCardCondition,
                                                canTargetCondition_ByPreSelecetedList: null,
                                                canEndSelectCondition: null,
                                                canNoSelect: () => true,
                                                selectCardCoroutine: SelectCardCoroutine,
                                                afterSelectCardCoroutine: null,
                                                message: "Select 1 card to play.",
                                                maxCount: maxCount1,
                                                canEndNotMax: false,
                                                isShowOpponent: true,
                                                mode: SelectCardEffect.Mode.Custom,
                                                root: SelectCardEffect.Root.Custom,
                                                customRootCardList: card.Owner.TrashCards,
                                                canLookReverseCard: true,
                                                selectPlayer: card.Owner,
                                                cardEffect: activateClass);

                                    selectCardEffect.SetUpCustomMessage("Select 1 trash card to play.", "The opponent is selecting 1 trash card to play.");
                                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");


                                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                                    {
                                        selectedCards.Add(cardSource);

                                        yield return null;
                                    }

                                    yield return StartCoroutine(selectCardEffect.Activate());

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: selectedCards,
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Trash,
                                    activateETB: true));
                                }
                            }                            
                        }
                    }

                    yield return null;
                }
            }
            #endregion

            #region End of Attack - Inherited Effect
            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Unsuspend_BT16_076");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Attack] [Once Per Turn] If your opponent has 1 or more memory, unsuspend this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.Enemy.MemoryForPlayer >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {

                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());

                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [FenriLoogamon] from your trash.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When one of your other Digimon with the [SoC] trait is deleted, this Digimon with a Tamer with the [SoC] trait in its digivolution cardsmay digivolve into [FenriLoogamon] from your trash without paying the cost.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent != card.PermanentOfThisCard())
                        {
                            if (permanent.TopCard.CardTraits.Contains("SoC"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectFenriInTrash(CardSource cardSource)
                {
                    if (cardSource.CardNames.Contains("FenriLoogamon"))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => (cardSource.CardTraits.Contains("SoC") && cardSource.IsTamer)) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                    targetPermanent: card.PermanentOfThisCard(),
                    cardCondition: cardSource => CanSelectFenriInTrash(cardSource),
                    payCost: false,
                    reduceCostTuple: null,
                    fixedCostTuple: null,
                    ignoreDigivolutionRequirementFixedCost: -1,
                    isHand: false,
                    activateClass: activateClass,
                    successProcess: null));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}