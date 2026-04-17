using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Monarchlizamon
namespace DCGO.CardEffects.ST23
{
    public class ST23_08 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Glowing Dawn");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 4, permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "Gain +3K until end of opponent's turn, if your turn by trashing bottom face-down card from your Tamer, may play/use 1 [Glowing Dawn] card with the cost reduced by 3";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    maxCountPerTurn: -1,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag)
                => $"[{tag}] This Digimon gets +3000 DP until your opponent's turn ends. Then, if it's your turn, by trashing the bottom face-down card from under any of your Tamers, you may play or use 1 [Glowing Dawn] trait card from your hand with the cost reduced by 3.";

            bool TamerWithOneFaceDownSource(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Any(CanSelectTrashSourceCardCondition);
            }

            bool CanSelectTrashSourceCardCondition(CardSource cardSource)
            {
                return cardSource.IsFlipped;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: card.PermanentOfThisCard(), changeValue: 3000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));

                if (CardEffectCommons.MatchConditionPermanentCount(TamerWithOneFaceDownSource) > 0 && CardEffectCommons.IsOwnerTurn(card))
                {
                    bool trash = false;

                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect1.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TamerWithOneFaceDownSource,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect1.SetUpCustomMessage("Select 1 Tamer to trash 1 bottom face-down card from", "The opponent is selecting 1 Tamer to trash 1 bottom face-down card from");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 1, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));

                        trash = true;
                    }

                    if (trash)
                    {
                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource.EqualsTraits("Glowing Dawn")
                                && ((cardSource.IsOption
                                && !cardSource.CanNotPlayThisOption)
                                    || (cardSource.HasPlayCost
                                && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass, fixedCost: cardSource.GetCostItself - 3)));
                        }

                        CardSource selectCard = null;
                        List<CardSource> selectedCards = null;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 card to play/use.", "The opponent is selecting 1 card to play/use.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectCard = cardSource;
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                        if (selectCard != null)
                        {
                            #region reduce use cost
                            ChangeCostClass changeCostClass = new ChangeCostClass();
                            changeCostClass.SetUpICardEffect($"Play/Use Cost -3", CanUseCondition1, card);
                            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: PlayOrUseCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
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

                            bool PlayOrUseCondition(CardSource cardSource)
                            {
                                return true;
                            }

                            int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                            {
                                if (PlayOrUseCondition(cardSource))
                                {
                                    if (RootCondition(root))
                                    {
                                        if (PermanentsCondition(targetPermanents))
                                        {
                                            Cost -= 3;
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

                            bool RootCondition(SelectCardEffect.Root root)
                            {
                                return true;
                            }

                            bool isUpDown()
                            {
                                return true;
                            }
                            #endregion

                            if (selectCard.IsOption)
                            {                       
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                    cardSources: selectedCards,
                                    activateClass: activateClass,
                                    payCost: true,
                                    root: SelectCardEffect.Root.Hand));                            
                            }
                            else
                            {                            
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    new List<CardSource>() { selectCard },
                                    activateClass: activateClass,
                                    payCost: true,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Hand,
                                    activateETB: true));
                            }

                            #region release effect
                            card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                            #endregion
                        }
                    }
                }
            }
            #endregion

            #region End of Attack = ESS
            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing a face-down card from under a tamer, Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Unsuspend_ST24_08");
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Attack] [Once Per Turn] By trashing the bottom face-down card from under any of your Tamers, this [Glowing Dawn] trait Digimon unsuspends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, TamerWithOneFaceDownSource);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool trash = false;

                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect1.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TamerWithOneFaceDownSource,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect1.SetUpCustomMessage("Select 1 Tamer to trash 1 bottom face-down card from to unsuspend this Digimon", "The opponent is selecting 1 Tamer to trash 1 bottom face-down card from");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 1, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));

                        trash = true;
                    }

                    if (trash)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                    }
                    else
                    {
                        activateClass.RemoveUse();
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
