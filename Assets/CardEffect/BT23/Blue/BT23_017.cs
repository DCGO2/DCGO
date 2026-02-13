using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Betamon
namespace DCGO.CardEffects.BT23
{
    public class BT23_017 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasCSTraits
                        && targetPermanent.TopCard.HasLevel
                        && targetPermanent.TopCard.IsLevel2;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 card, return 1 non digi-egg [CS] card from trash to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By trashing 1 card in your hand, you may return 1 non-Digi-Egg card with the [CS] trait from your trash to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return !cardSource.IsDigiEgg
                        && cardSource.HasCSTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool trashed = false;
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: (cardSource) => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Any()) trashed = true;
                        yield return null;
                    }

                    selectHandEffect.SetUpCustomMessage("Select 1 card to trash", "Your opponent is selecting 1 card to trash");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");
                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    if (trashed && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectCardCondition));
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 [CS] card to add to hand.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.AddHand,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 [CS] card to add to hand.", "The opponent is selecting 1 card to add to hand.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }
                }
            }

            #endregion

            #region ESS - When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 5 cost or less [Hudie] digimon from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT23_017_WA");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] You may play 1 play cost 5 or lower Digimon card with the [Hudie] trait from your hand without paying the cost. The Digimon this effect played can't digivolve and is deleted at the end of your opponent's turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasPlayCost && cardSource.BasePlayCostFromEntity <= 5
                        && cardSource.HasHudieTraits
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                    {
                        CardSource selectedCard = null;
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

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        selectHandEffect.SetUpCustomMessage("Select 1 [Hudie] digimon to play", "Your opponent is selecting 1 card to play");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");
                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                        if (selectedCard != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Hand,
                                activateETB: true));

                            yield return new WaitForSeconds(0.2f);

                            Permanent playedDigimon = selectedCard.PermanentOfThisCard();

                            #region Can't Digivolve/Delete EoOT
                            if (playedDigimon != null)
                            {
                                #region Can't Digivolve
                                CanNotDigivolveClass canNotEvolveClass = new CanNotDigivolveClass();
                                canNotEvolveClass.SetUpICardEffect("Can't digivolve", CanUseCantEvoCondition, card);
                                canNotEvolveClass.SetUpCanNotEvolveClass(permanentCondition: PermanentCondition, cardCondition: CardCondition);
                                playedDigimon.PermanentEffects.Add((_timing) => canNotEvolveClass);

                                bool CanUseCantEvoCondition(Hashtable hashtable)
                                {
                                    return CardEffectCommons.IsPermanentExistsOnBattleArea(playedDigimon);
                                }

                                bool PermanentCondition(Permanent permanent)
                                {
                                    return permanent == playedDigimon;
                                }

                                bool CardCondition(CardSource cardSource)
                                {
                                    return true;
                                }
                                #endregion

                                #region Delete Digimon Played

                                ActivateClass activateClass1 = new ActivateClass();
                                activateClass1.SetUpICardEffect("Delete the Digimon", CanUseCondition2, playedDigimon.TopCard);
                                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                                activateClass1.SetEffectSourcePermanent(playedDigimon);
                                playedDigimon.UntilOpponentTurnEndEffects.Add(GetCardEffect);

                                if (!playedDigimon.TopCard.CanNotBeAffected(activateClass))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(playedDigimon));
                                }

                                string EffectDiscription1()
                                {
                                    return "[End of Opponents Turn] Delete this Digimon.";
                                }

                                bool CanUseCondition2(Hashtable hashtable1)
                                {
                                    if (CardEffectCommons.IsOpponentTurn(card))
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(playedDigimon, playedDigimon.TopCard))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                bool CanActivateCondition1(Hashtable hashtable1)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(playedDigimon))
                                    {
                                        if (!playedDigimon.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(playedDigimon))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                                        new List<Permanent>() { playedDigimon },
                                        CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                                    }
                                }

                                ICardEffect GetCardEffect(EffectTiming _timing)
                                {
                                    if (_timing == EffectTiming.OnEndTurn)
                                    {
                                        return activateClass1;
                                    }

                                    return null;
                                }

                                #endregion
                            }
                            #endregion
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}