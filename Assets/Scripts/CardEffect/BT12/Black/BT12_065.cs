using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT12
{
    public class BT12_065 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Mercurymon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return card.Owner.HandCards.Contains(card);
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardColors.Contains(CardColor.Black) && targetPermanent.IsTamer;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: Condition));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent's 1 Digimon get effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Until the end of your opponent's turn, 1 of your opponent's Digimon gains \"[Start of Your Main Phase] Attack with this Digimon.\"";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
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
                    if (isExistOnField(card))
                    {
                        Permanent selectedPermanent = null;

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

                                selectPermanentEffect.SetUpCustomMessage("Selcect 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    selectedPermanent = permanent;

                                    yield return null;
                                }

                                if (selectedPermanent != null)
                                {
                                    ActivateClass activateClass1 = new ActivateClass();
                                    activateClass1.SetUpICardEffect("Attack with this Digimon", CanUseCondition1, selectedPermanent.TopCard);
                                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                                    activateClass1.SetEffectSourcePermanent(selectedPermanent);
                                    selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                                    }

                                    string EffectDiscription1()
                                    {
                                        return "[Start of Your Main Phase] Attack with this Digimon.";
                                    }

                                    bool CanUseCondition1(Hashtable hashtable1)
                                    {
                                        if (selectedPermanent.TopCard != null)
                                        {
                                            if (selectedPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(selectedPermanent))
                                            {
                                                if (GManager.instance.turnStateMachine.gameContext.TurnPlayer == selectedPermanent.TopCard.Owner)
                                                {
                                                    return true;
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    bool CanActivateCondition1(Hashtable hashtable1)
                                    {
                                        if (selectedPermanent.TopCard != null)
                                        {
                                            if (selectedPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(selectedPermanent))
                                            {
                                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                                {
                                                    if (selectedPermanent.CanAttack(activateClass1))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                    {
                                        if (selectedPermanent.TopCard != null)
                                        {
                                            if (selectedPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(selectedPermanent))
                                            {
                                                if (selectedPermanent.CanAttack(activateClass1))
                                                {
                                                    SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                                    selectAttackEffect.SetUp(
                                                        attacker: selectedPermanent,
                                                        canAttackPlayerCondition: () => true,
                                                        defenderCondition: (permanent) => true,
                                                        cardEffect: activateClass1);

                                                    selectAttackEffect.SetCanNotSelectNotAttack();

                                                    Hashtable hashtable = new Hashtable();
                                                    hashtable.Add("CardEffect", activateClass1);

                                                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                                }
                                            }
                                        }
                                    }

                                    ICardEffect GetCardEffect(EffectTiming _timing)
                                    {
                                        if (_timing == EffectTiming.OnStartMainPhase)
                                        {
                                            return activateClass1;
                                        }

                                        return null;
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
}