using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tentomon
namespace DCGO.CardEffects.BT23
{
    public class BT23_037 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel2 && targetPermanent.TopCard.HasCSTraits;
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

            #region Your Turn

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsOwnerTurn(card) && CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent == card.PermanentOfThisCard();
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource.HasCSTraits;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                cardEffects.Add(CardEffectFactory.ChangeDigivolutionCostStaticEffect(
                    changeValue: -1,
                    permanentCondition: PermanentCondition,
                    cardCondition: CardSourceCondition,
                    rootCondition: RootCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    setFixedCost: false));
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
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasPlayCost && cardSource.BasePlayCostFromEntity <= 5
                        && cardSource.EqualsTraits("Hudie")
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

                                if (!playedDigimon.TopCard.CanNotBeAffected(activateClass1))
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