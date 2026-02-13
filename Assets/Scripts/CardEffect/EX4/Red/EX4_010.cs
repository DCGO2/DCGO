using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX4
{
    public class EX4_010 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Growlmon") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Both players trash the top 3 cards of their decks and delete Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Trash the top 3 cards of both players' decks. Then, choose any number of your opponent's Digimon so that their DP total is up to 3000 and delete them. For every 10 total cards in both players' trashes, add 2000 to the maximum this DP-based deletion effect can delete.";
                }

                int maxDP()
                {
                    int maxDP = 3000;

                    int trashSum = 0;

                    foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                    {
                        trashSum += player.TrashCards.Count;
                    }

                    maxDP += 2000 * (trashSum / 10);

                    return maxDP;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(maxDP(), activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            if (GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer.Count((player) => player.LibraryCards.Count >= 1) >= 1)
                            {
                                return true;
                            }

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                            {
                                if (player.LibraryCards.Count >= 1)
                                {
                                    IAddTrashCardsFromLibraryTop addTrashCard = new IAddTrashCardsFromLibraryTop(3, player, activateClass);
                                    addTrashCard.SetNotShowCards();

                                    yield return ContinuousController.instance.StartCoroutine(addTrashCard.AddTrashCardsFromLibraryTop());

                                    if (player.isYou)
                                    {
                                        ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(addTrashCard.discardedCards, "Your Discarded Cards", true, true));
                                    }

                                    else
                                    {
                                        ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(addTrashCard.discardedCards, "Opponent's Discarded Cards", true, true));
                                    }
                                }
                            }

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                int maxCount = Math.Min(card.Owner.Enemy.GetBattleAreaDigimons().Count(CanSelectPermanentCondition), CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                    canEndSelectCondition: CanEndSelectCondition,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: true,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Destroy,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                bool CanEndSelectCondition(List<Permanent> permanents)
                                {
                                    if (permanents.Count <= 0)
                                    {
                                        return false;
                                    }

                                    int sumDP = 0;

                                    foreach (Permanent permanent1 in permanents)
                                    {
                                        sumDP += permanent1.DP;
                                    }

                                    if (sumDP > card.Owner.MaxDP_DeleteEffect(maxDP(), activateClass))
                                    {
                                        return false;
                                    }

                                    return true;
                                }

                                bool CanTargetCondition_ByPreSelecetedList(List<Permanent> permanents, Permanent permanent)
                                {
                                    int sumDP = 0;

                                    foreach (Permanent permanent1 in permanents)
                                    {
                                        sumDP += permanent1.DP;
                                    }

                                    sumDP += permanent.DP;

                                    if (sumDP > card.Owner.MaxDP_DeleteEffect(maxDP(), activateClass))
                                    {
                                        return false;
                                    }

                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}