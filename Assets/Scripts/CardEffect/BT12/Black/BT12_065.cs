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

            if(timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("As if it were a level 3 digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, "");
                activateClass.SetIsBackgroundProcess(true);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnHand(card))
                    {
                        bool PermanentCondition(Permanent targetPermanent)
                        {
                            return targetPermanent.TopCard.CardColors.Contains(CardColor.Black) && targetPermanent.IsTamer;
                        }

                        bool CardCondition(CardSource cardSource)
                        {
                            return cardSource == card;
                        }

                        if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentCondition, CardCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = CardEffectCommons.GetPermanentsFromHashtable(_hashtable)[0];

                    bool CanUseChangeCondition(Hashtable ccHashtable)
                    {
                        if (selectedPermanent.TopCard != null)
                        {
                            if (card.Owner.GetBattleAreaPermanents().Contains(selectedPermanent))
                            {
                                if (card == selectedPermanent.TopCard)
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }


                    ChangePermanentLevelClass changePermanentLevelClass = new ChangePermanentLevelClass();
                    changePermanentLevelClass.SetUpICardEffect($"Treated as level 3", CanUseChangeCondition, card);
                    changePermanentLevelClass.SetUpChangePermanentLevelClass(GetLevel: GetLevel);
                    changePermanentLevelClass.SetNotShowUI(true);

                    int GetLevel(Permanent permanent, int level)
                    {
                        if (selectedPermanent.TopCard != null)
                        {
                            if (permanent == selectedPermanent)
                            {
                                level = 3;
                            }
                        }

                        return level;
                    }


                    TreatAsDigimonClass treatAsDigimonClass = new TreatAsDigimonClass();
                    treatAsDigimonClass.SetUpICardEffect($"Treated as Digimon", CanUseChangeCondition, card);
                    treatAsDigimonClass.SetUpTreatAsDigimonClass(
                        permanentCondition: PermanentCondition);
                    treatAsDigimonClass.SetNotShowUI(true);

                    bool PermanentCondition(Permanent permanent)
                    {
                        if (selectedPermanent.TopCard != null)
                        {
                            if (permanent == selectedPermanent)
                            {
                                return true;
                            }
                        }

                        return false;
                    }


                    DontHaveDPClass dontHaveDPClass = new DontHaveDPClass();
                    dontHaveDPClass.SetUpICardEffect("Don't have DP", CanUseChangeCondition, card);
                    dontHaveDPClass.SetUpDontHaveDPClass(PermanentCondition: PermanentCondition);
                    dontHaveDPClass.SetNotShowUI(true);

                    List<Func<EffectTiming, ICardEffect>> getCardEffects =
                        new List<Func<EffectTiming, ICardEffect>>()
                        {
                                                    _ => changePermanentLevelClass,
                                                    _ => treatAsDigimonClass,
                                                    _ => dontHaveDPClass,
                        };

                    foreach (Func<EffectTiming, ICardEffect> getCardEffect in getCardEffects)
                    {
                    card.Owner.UntilAfterPlayEffect.Add(getCardEffect);
                    }

                    yield return null;
                }
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

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");
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