using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Apollomon
namespace DCGO.CardEffects.P
{
    public class P_191 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel5 &&
                           targetPermanent.TopCard.HasLightFangOrNightClawTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region BlastDigivolve

            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"-{DPChangeValue()}DP to 1 digimon for the turn, then delete up to 7k worth of digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] To 1 of your opponent’s Digimon, give -4000 DP for the turn for each of your Digimon with the [Olympos XII] trait. Then, delete up to 7000 DP total worth of their Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                           && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                int DPChangeValue() => card.Owner.GetBattleAreaDigimons().Filter(x => x.TopCard.EqualsTraits("Olympos XII")).Count * 4000;
                int DeletionMaxDP() => 7000;

                bool CanSelectDeletePermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(DeletionMaxDP(), activateClass))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentCondition))
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
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage(customMessageArray: CardEffectCommons.customPermanentMessageArray_ChangeDP(changeValue: -DPChangeValue(), maxCount: maxCount));

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: selectedPermanent, changeValue: -DPChangeValue(), effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeletePermanentCondition))
                            {
                                int maxCount1 = Math.Min(card.Owner.Enemy.GetBattleAreaDigimons().Count(CanSelectDeletePermanentCondition),
                                    CardEffectCommons.MatchConditionPermanentCount(CanSelectDeletePermanentCondition));

                                SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect1.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectDeletePermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: CanTargetConditionByPreSelectedList,
                                    canEndSelectCondition: CanEndSelectCondition,
                                    maxCount: maxCount1,
                                    canNoSelect: false,
                                    canEndNotMax: true,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Destroy,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Choose digimon to delete", "Opponent is choosing digimon to delete");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                bool CanEndSelectCondition(List<Permanent> permanents)
                                {
                                    if (permanents.Count <= 0)
                                    {
                                        return false;
                                    }

                                    int sumDP = 0;

                                    foreach (Permanent permanent in permanents)
                                    {
                                        sumDP += permanent.DP;
                                    }

                                    if (sumDP > card.Owner.MaxDP_DeleteEffect(DeletionMaxDP(), activateClass))
                                    {
                                        return false;
                                    }

                                    return true;
                                }

                                bool CanTargetConditionByPreSelectedList(List<Permanent> permanents, Permanent permanent)
                                {
                                    int sumDP = 0;

                                    foreach (Permanent permanent1 in permanents)
                                    {
                                        sumDP += permanent1.DP;
                                    }

                                    sumDP += permanent.DP;

                                    if (sumDP > card.Owner.MaxDP_DeleteEffect(DeletionMaxDP(), activateClass))
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

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"-{DPChangeValue()}DP to 1 digimon for the turn, then delete up to 7k worth of digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] To 1 of your opponent’s Digimon, give -4000 DP for the turn for each of your Digimon with the [Olympos XII] trait. Then, delete up to 7000 DP total worth of their Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                           && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                int DPChangeValue() => card.Owner.GetBattleAreaDigimons().Filter(x => x.TopCard.EqualsTraits("Olympos XII")).Count * 4000;
                int DeletionMaxDP() => 7000;

                bool CanSelectDeletePermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(DeletionMaxDP(), activateClass))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentCondition))
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
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage(customMessageArray: CardEffectCommons.customPermanentMessageArray_ChangeDP(changeValue: -DPChangeValue(), maxCount: maxCount));

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: selectedPermanent, changeValue: -DPChangeValue(), effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeletePermanentCondition))
                            {
                                int maxCount1 = Math.Min(card.Owner.Enemy.GetBattleAreaDigimons().Count(CanSelectDeletePermanentCondition),
                                    CardEffectCommons.MatchConditionPermanentCount(CanSelectDeletePermanentCondition));

                                SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect1.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectDeletePermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: CanTargetConditionByPreSelectedList,
                                    canEndSelectCondition: CanEndSelectCondition,
                                    maxCount: maxCount1,
                                    canNoSelect: false,
                                    canEndNotMax: true,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Destroy,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Choose digimon to delete", "Opponent is choosing digimon to delete");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                bool CanEndSelectCondition(List<Permanent> permanents)
                                {
                                    if (permanents.Count <= 0)
                                    {
                                        return false;
                                    }

                                    int sumDP = 0;

                                    foreach (Permanent permanent in permanents)
                                    {
                                        sumDP += permanent.DP;
                                    }

                                    if (sumDP > card.Owner.MaxDP_DeleteEffect(DeletionMaxDP(), activateClass))
                                    {
                                        return false;
                                    }

                                    return true;
                                }

                                bool CanTargetConditionByPreSelectedList(List<Permanent> permanents, Permanent permanent)
                                {
                                    int sumDP = 0;

                                    foreach (Permanent permanent1 in permanents)
                                    {
                                        sumDP += permanent1.DP;
                                    }

                                    sumDP += permanent.DP;

                                    if (sumDP > card.Owner.MaxDP_DeleteEffect(DeletionMaxDP(), activateClass))
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

            #endregion

            #region End of Your Turn

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DNA into [GraceNovamon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] 2 of your Digimon may DNA digivolve into [GraceNovamon] in the hand. Then, 1 of your Digimon may attack.";
                }

                bool CanSelectDNACardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.EqualsCardName("GraceNovamon") &&
                           cardSource.CanPlayJogress(true);
                }

                bool IsYourAttackingDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.CanAttack(activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                                         CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                                             CanSelectDNACardCondition,
                                             payCost: true,
                                             isHand: true,
                                             activateClass
                                         ));

                    if (CardEffectCommons.HasMatchConditionPermanent(IsYourAttackingDigimon))
                    {
                        Permanent selectedPermanent = null;
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsYourAttackingDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to attack.", "The opponent is selecting 1 Digimon to attack.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: selectedPermanent,
                                canAttackPlayerCondition: () => true,
                                defenderCondition: _ => true,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }
                    }
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 digimon may attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("P_191_EndOfYourTurn");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] [Once Per Turn] 1 of your Digimon may attack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool IsYourAttackingDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.CanAttack(activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsYourAttackingDigimon))
                    {
                        Permanent selectedPermanent = null;
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsYourAttackingDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to attack.", "The opponent is selecting 1 Digimon to attack.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: selectedPermanent,
                                canAttackPlayerCondition: () => true,
                                defenderCondition: _ => true,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}