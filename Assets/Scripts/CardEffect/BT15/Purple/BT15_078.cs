using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT15
{
    public class BT15_078 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region All Turns - Effect plays opponents digimon

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("All Opponents digimon, gain Memory -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("LoseMemory_BT15_078");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When an effect plays an opponent's Digimon, until the end of their turn, all of your opponent's Digimon gain \"[On Deletion] Lose 1 memory.\"";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition))
                        {
                            if (CardEffectCommons.IsByEffect(hashtable, null))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool PermanentCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                        {
                            if (!permanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaPermanents())
                    {
                        if (PermanentCondition(permanent))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));
                        }
                    }

                    AddSkillClass addSkillClass = new AddSkillClass();
                    addSkillClass.SetUpICardEffect("Memory -1", CanUseCondition1, card);
                    addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);

                    card.Owner.UntilOpponentTurnEndEffects.Add((_timing) => addSkillClass);

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        if (PermanentCondition(cardSource.PermanentOfThisCard()))
                        {
                            if (cardSource == cardSource.PermanentOfThisCard().TopCard)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                    {
                        if (_timing == EffectTiming.OnDestroyedAnyone)
                        {
                            ActivateClass activateClass1 = new ActivateClass();
                            activateClass1.SetUpICardEffect("Memory -1", CanUseCondition2, cardSource);
                            activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                            cardEffects.Add(activateClass1);

                            if (cardSource.PermanentOfThisCard() != null)
                            {
                                activateClass1.SetEffectSourcePermanent(cardSource.PermanentOfThisCard());
                            }

                            string EffectDiscription1()
                            {
                                return "[On Deletion] Lose 1 memory.";
                            }

                            bool CanUseCondition2(Hashtable hashtable)
                            {
                                if (CardSourceCondition(cardSource))
                                {
                                    if (CardEffectCommons.CanTriggerOnDeletion(hashtable, cardSource))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool CanActivateCondition1(Hashtable hashtable)
                            {
                                if (CardEffectCommons.CanActivateOnDeletion(cardSource))
                                {
                                    return true;
                                }

                                return false;
                            }

                            IEnumerator ActivateCoroutine1(Hashtable _hashtable)
                            {
                                yield return ContinuousController.instance.StartCoroutine(cardSource.Owner.AddMemory(-1, activateClass1));
                            }
                        }

                        return cardEffects;
                    }
                }
            }

            #endregion

            #region When Attacking - opponent plays a digimon

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent plays 1  Digimon from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] Your opponent plays 1 level 4 or lower Digimon card from their trash suspended without paying the cost. [On Play] effects on Digimon played by this effect don't activate. Then, you may switch the target of attack to that Digimon.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.Level <= 4)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                if (cardSource.HasLevel)
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
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOpponentsCardInTrash(card, CanSelectCardCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsCardInTrash(card, CanSelectCardCondition))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = Math.Min(1, card.Owner.Enemy.TrashCards.Count(CanSelectCardCondition));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
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
                                    selectPlayer: card.Owner.Enemy,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        if (selectedCards.Count > 0)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                                    cardSources: selectedCards,
                                                    activateClass: activateClass,
                                                    payCost: false,
                                                    isTapped: true,
                                                    root: SelectCardEffect.Root.Trash,
                                                    activateETB: false));

                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: $"Redirect Attack", value : true, spriteIndex: 0),
                        new SelectionElement<bool>(message: $"Continue Attack", value : false, spriteIndex: 1),
                    };

                            string selectPlayerMessage = "Would you like to redirect the attack?";
                            string notSelectPlayerMessage = "The opponent is selecting whether to redirect the attack.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            if (GManager.instance.userSelectionManager.SelectedBoolValue)
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(
                                    activateClass,
                                    false,
                                    selectedCards[0].PermanentOfThisCard()));
                            }
                        }
                    }
                }
            }

            #endregion

            //Inherited Effect
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }

            return cardEffects;
        }
    }
}