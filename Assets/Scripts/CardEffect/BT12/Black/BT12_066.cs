using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT12
{
    public class BT12_066 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Sephirothmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
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

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: Condition));
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
                activateClass.SetUpICardEffect("Your 1 Digimon gains Blocker", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] 1 of your Digimon gains <Blocker> until the end of your opponent's turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get Blocker.", "The opponent is selecting 1 Digimon that will get Blocker.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(targetPermanent: permanent, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Your 1 Digimon gains Blocker", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] 1 of your Digimon gains <Blocker> until the end of your opponent's turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get Blocker.", "The opponent is selecting 1 Digimon that will get Blocker.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(targetPermanent: permanent, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                    }
                }
            }

            return cardEffects;
        }
    }
}