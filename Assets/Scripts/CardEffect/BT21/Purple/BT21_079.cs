using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.BT21
{
    public class BT21_079 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Requirement
            if(timing == EffectTiming.None)
            {
                bool DigivolutionCondition(Permanent permanent)
                {
                    return permanent.Level == 5 && permanent.TopCard.ContainsCardName("Growlmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(DigivolutionCondition, 4, false, card, null));
            }
            #endregion

            #region Security attack
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(
                    changeValue: 1,
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Rule Text

            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as [ChaosGallantmon]", _ => true, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: ChangeCardNames);
                cardEffects.Add(changeCardNamesClass);

                List<string> ChangeCardNames(CardSource cardSource, List<string> cardNames)
                {
                    if (cardSource == card)
                    {
                        cardNames.Add("ChaosGallantmon");
                    }

                    return cardNames;
                }
            }

            #endregion

            #region End of attack
            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete all digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT21_079 EOA");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[End of Attack][Once Per Turn] Delete all Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnEndAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> DestroyTargetPermanents = GManager.instance.turnStateMachine.gameContext.Players.Map(player => player.GetBattleAreaDigimons()).Flat();
                    yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(DestroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                }
            }
            #endregion

            #region On Deletion
            if(timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play guilmon or growlmon in name", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Deletion] You may play 1 play cost 3 or lower Digimon card with [Guilmon] or [Growlmon] in its name from your trash without paying the cost. For every 10 cards in both players' trashes, add 2 to this effect's play cost maximum.";
                }

                int Count()
                {
                    return Mathf.FloorToInt((card.Owner.TrashCards.Count + card.Owner.Enemy.TrashCards.Count) / 10);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource card)
                {
                    return card.IsDigimon 
                        && (card.ContainsCardName("Guilmon") || card.ContainsCardName("Growlmon"))
                        && card.GetCostItself <= 3 + Count() * 2;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card) && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanSelectCardCondition));

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 card to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

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
            #endregion

            return cardEffects;
        }
    }
}