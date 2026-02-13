using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT8_031 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Trash digivolution cards and return 1 Digimon to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Trash the top digivolution card of 1 of your opponent's Digimon. Then, return 1 of your opponent's Digimon with no digivolution cards to its owner's hand.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    return true;
                }

                return false;
            }

            bool CanSelectPermanentCondition1(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.HasNoDigivolutionCards)
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
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (isExistOnField(card))
                {
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
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
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will trash digivolution cards.", "The opponent is selecting 1 Digimon that will trash digivolution cards.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                Permanent selectedPermanent = permanent;

                                if (selectedPermanent != null)
                                {
                                    if (selectedPermanent.DigivolutionCards.Count >= 1 && !selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        List<CardSource> selectedCards = new List<CardSource>();

                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: 1, isFromTop: true, activateClass: activateClass));
                                    }
                                }
                            }
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition1,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Bounce,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            AddSkillClass addSkillClass = new AddSkillClass();
            addSkillClass.SetUpICardEffect("Opponent's Digimon trashes its digivolution card when it attacks", CanUseCondition, card);
            addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);

            cardEffects.Add(addSkillClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOpponentTurn(card))
                    {
                        return true;
                    }
                }

                return false;
            }



            bool PermanentCondition(Permanent permanent)
            {
                if (permanent != null)
                {
                    if (permanent.TopCard != null)
                    {
                        if (permanent.TopCard.Owner.GetBattleAreaDigimons().Contains(permanent))
                        {
                            if (!permanent.TopCard.CanNotBeAffected(addSkillClass))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                if (cardSource.PermanentOfThisCard() != null)
                {
                    if (cardSource.Owner == card.Owner.Enemy)
                    {
                        if (cardSource == cardSource.PermanentOfThisCard().TopCard)
                        {
                            if (PermanentCondition(cardSource.PermanentOfThisCard()))
                            {
                                if (cardSource.Owner.GetBattleAreaDigimons().Contains(cardSource.PermanentOfThisCard()))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }

            List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
            {
                if (_timing == EffectTiming.OnAllyAttack)
                {
                    ActivateClass activateClass1 = new ActivateClass();
                    activateClass1.SetUpICardEffect("Trash the bottom digivolution card", CanUseCondition1, cardSource);
                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription());
                    activateClass1.SetRootCardEffect(addSkillClass);
                    activateClass1.SetHashString("TrashDigivolutionCards_BT8_031");
                    cardEffects.Add(activateClass1);

                    string EffectDiscription()
                    {
                        return "[When Attacking] Trash the bottom digivolution card of this Digimon.";
                    }

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        if (CardSourceCondition(cardSource))
                        {
                            if (isExistOnField(cardSource))
                            {
                                if (cardSource.Owner.GetBattleAreaDigimons().Contains(cardSource.PermanentOfThisCard()))
                                {
                                    if (GManager.instance.turnStateMachine.gameContext.TurnPlayer == cardSource.Owner)
                                    {
                                        if (GManager.instance.attackProcess.AttackingPermanent == cardSource.PermanentOfThisCard())
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }

                        return false;
                    }

                    bool CanActivateCondition1(Hashtable hashtable)
                    {
                        if (CardSourceCondition(cardSource))
                        {
                            if (isExistOnField(cardSource))
                            {
                                if (cardSource.Owner.GetBattleAreaDigimons().Contains(cardSource.PermanentOfThisCard()))
                                {
                                    if (!cardSource.CanNotBeAffected(activateClass1))
                                    {
                                        if (cardSource.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                                        {
                                            if (!cardSource.PermanentOfThisCard().DigivolutionCards[cardSource.PermanentOfThisCard().DigivolutionCards.Count - 1].CanNotTrashFromDigivolutionCards(activateClass1))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        return false;
                    }

                    IEnumerator ActivateCoroutine1(Hashtable _hashtable)
                    {
                        if (CardSourceCondition(cardSource))
                        {
                            if (isExistOnField(cardSource))
                            {
                                if (cardSource.Owner.GetBattleAreaDigimons().Contains(cardSource.PermanentOfThisCard()))
                                {
                                    if (cardSource.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                                    {
                                        Permanent selectedPermanent = cardSource.PermanentOfThisCard();

                                        if (selectedPermanent != null)
                                        {
                                            if (selectedPermanent.DigivolutionCards.Count >= 1)
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: 1, isFromTop: false, activateClass: activateClass1));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return cardEffects;
            }
        }

        return cardEffects;
    }
}
