using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.BT17
{
    public class BT17_048 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Argomon") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Opponents Turn
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsTamer)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (!CardEffectCommons.IsOwnerTurn(card))
                            return true;
                    }

                    return false;
                }

                string effectName = "[Opponent's Turn] None of your opponent's Tamers can unsuspend.";

                cardEffects.Add(CardEffectFactory.CantUnsuspendStaticEffect(
                    permanentCondition: PermanentCondition,
                    isInheritedEffect: false,
                    card: card, condition: CanUseCondition,
                    effectName: effectName));
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Level 6 [Argomon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] If you have 4 or more [Argomon] in the trash, you may play 1 level 6 [Argomon] from your hand without paying the cost.";
                }

                bool IsArgomon(CardSource source)
                {
                    return source.EqualsCardName("Argomon");
                }

                bool IsLevel6Argomon(CardSource source)
                {
                    if (source.IsDigimon)
                    {
                        if(source.HasLevel &&  source.Level == 6)
                        {
                            if (source.EqualsCardName("Argomon"))
                            {
                                if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: source, payCost: false, cardEffect: activateClass))
                                {
                                    return true;
                                }
                            }
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
                        if(CardEffectCommons.HasMatchConditionOwnersHand(card, IsLevel6Argomon))
                        {
                            if (CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, IsArgomon) >= 4)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsLevel6Argomon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: SelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if(cardSources != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: cardSources,
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Hand,
                                activateETB: true));
                        }
                        
                    }
                }
            }
            #endregion

            #region When Attacking - ESS
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Unsuspend_BT17_048");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] By suspending 1 of your [Rhythm], unsuspend this Digimon.";
                }

                bool HasUnsuspendedRhythm(Permanent permanent)
                {
                    if(CardEffectCommons.IsOwnerPermanent(permanent, card))
                    {
                        if (permanent.IsTamer)
                        {
                            if (permanent.TopCard.EqualsCardName("Rhythm"))
                            {
                                if (CardEffectCommons.CanActivateSuspendCostEffect(permanent.TopCard))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                   
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasUnsuspendedRhythm);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(HasUnsuspendedRhythm));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: HasUnsuspendedRhythm,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: SelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents != null)
                            yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent> { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                    }
                }
            }
            #endregion

            #region Cost Reduction
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce Digivolution Cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("DigivolutionCost-_BT17_048");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When digivolving into this card, by suspending up to 5 Tamers, for each Tamer suspended by this effect, reduce the digivolution cost by 1.";
                }

                bool CanSelectTamerCondition(Permanent permanent)
                {
                    if (permanent.IsTamer)
                    {
                        if (!permanent.IsSuspended)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentCondition, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerCondition))
                    {
                        int maxCount = Math.Min(5, CardEffectCommons.MatchConditionPermanentCount(CanSelectTamerCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectTamerCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: true,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectCardCoroutine,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select up to 5 Tamers to suspend.", "The opponent is selecting up to 5 Tamers to suspend.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<Permanent> permanents)
                        {
                            if (permanents.Count >= 1)
                            {
                                int minusCount = permanents.Count;

                                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                                ChangeCostClass changeCostClass = new ChangeCostClass();
                                changeCostClass.SetUpICardEffect($"Digivolution Cost -{minusCount}", CanUseCondition1, card);
                                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                                card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

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
                                                Cost -= minusCount;
                                            }
                                        }
                                    }

                                    return Cost;
                                }

                                bool PermanentsCondition(List<Permanent> targetPermanents)
                                {
                                    if (targetPermanents != null)
                                    {
                                        if (targetPermanents.Count(PermanentCondition) >= 1)
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                bool PermanentCondition(Permanent targetPermanent)
                                {
                                    if (targetPermanent.TopCard != null)
                                    {
                                        if (targetPermanent.TopCard.Owner == card.Owner)
                                        {
                                            if (targetPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(targetPermanent))
                                            {
                                                return true;
                                            }
                                        }
                                    }

                                    return false;
                                }

                                bool CardSourceCondition(CardSource cardSource)
                                {
                                    if (cardSource != null)
                                    {
                                        if (cardSource == card)
                                        {
                                            return true;
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
                            }
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}