using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT15
{
    public class BT15_081 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(
                    changeValue: 2,
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.CardNames.Contains("Leviamon"))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region All Turns - When opponent plays digimon/tamer

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve 1 [Leviamon (X Antibody)] from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Trash] [All Turns] When an effect play an opponent's Digimon or Tamer, 1 of your [Leviamon] or 1 of your Digimon with [X Antibody] in its digivolution cards may digivolve into this card without paying the cost.";
                }

                #region Opponent Digimon/Tamer Played/card is in trash condition

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.IsDigimon || permanent.TopCard.IsTamer)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnTrash(card))
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

                #endregion

                #region Selectable Digimon Conditions

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (CanSelectCardCondition(permanent.TopCard))
                        {
                            if (card.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass, root: SelectCardEffect.Root.Trash))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource))
                    {
                        if (cardSource.CardNames.Contains("Leviamon"))
                        {
                            return true;
                        }

                        if (cardSource.PermanentOfThisCard().DigivolutionCards.Count((CS) => CS.CardNames.Contains("X Antibody") || CS.CardNames.Contains("XAntibody")) >= 1)
                        {
                            return cardSource.Level == 5 && cardSource.CardColors.Contains(CardColor.Purple);
                        }
                    }

                    return false;
                }

                #endregion

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnTrash(card))
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
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will digivolve.", "The opponent is selecting 1 Digimon that will digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: selectedPermanent,
                                cardCondition: null,
                                payCost: false,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: false,
                                activateClass: activateClass,
                                successProcess: null,
                                ignoreSelection: true));
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Tamer and 1 level 3, 5, and 7  Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If your opponent has as many or more total Digimon and Tamers as you, delete 1 of your opponent's Tamers. Then, delete 1 of your opponent's level 3, 1 of their level 5 and 1 of their level 7 Digimon.";
                }

                #region Card select conditions

                bool CanSelectTamerCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.IsTamer)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectLevel3Condition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasLevel)
                        {
                            if (permanent.Level == 3)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanSelectLevel5Condition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasLevel)
                        {
                            if (permanent.Level == 5)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanSelectLevel7Condition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasLevel)
                        {
                            if (permanent.Level == 7)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                #endregion

                bool PermanentCondition(Permanent permanent)
                {
                    return (permanent.IsDigimon || permanent.IsTamer);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        List<Permanent> selectedPermanents = new List<Permanent>();

                        if (card.Owner.GetBattleAreaPermanents().Count(PermanentCondition) <= card.Owner.Enemy.GetBattleAreaPermanents().Count(PermanentCondition))
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerCondition))
                            {
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectTamerCondition));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectTamerCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to delete.", "The opponent is selecting 1 Tamer to delete.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            }
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectLevel3Condition))
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectLevel3Condition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectLevel3Condition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 level 3 Digimon to delete.", "The opponent is selecting 1 level 3 Digimon to delete.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectLevel5Condition))
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectLevel5Condition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectLevel5Condition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 level 5 Digimon to delete.", "The opponent is selecting 1 level 5 Digimon to delete.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectLevel7Condition))
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectLevel7Condition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectLevel7Condition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 level 7 Digimon to delete.", "The opponent is selecting 1 level 7 Digimon to delete.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanents.Add(permanent);
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                            selectedPermanents,
                            CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}