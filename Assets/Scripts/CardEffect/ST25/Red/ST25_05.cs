using System;
using System.Collections;
using System.Collections.Generic;

// Rubeusdramon / Burning Drive
namespace DCGO.CardEffects.ST25
{
    public class ST25_05 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            Permanent thisPermanent = card.PermanentOfThisCard();

            #region Digimon Side
            #region Alternate Digivolution Requirement - Lv.5 w/[Gemmon] in text
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasText("Gemmon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Progress
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ProgressSelfStaticEffect(false, card, null));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete up to 7000 DP total of opponent's Digimon, +4000 per 8000 DP of this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Digivolving] Delete up to 7000 DP total worth of your opponent's Digimon. For every 8000 DP this Digimon has, add 4000 to this DP deletion effect's maximum.";

                int MaxTotalDP()
                {
                    int bonus = thisPermanent != null && thisPermanent.HasDP ? thisPermanent.DP / 8000 * 4000 : 0;

                    return card.Owner.MaxDP_DeleteEffect(7000 + bonus, activateClass);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.HasDP
                        && permanent.DP <= MaxTotalDP();

                int SumDP(List<Permanent> permanents)
                {
                    int sumDP = 0;

                    foreach (Permanent permanent in permanents)
                    {
                        sumDP += permanent.DP;
                    }

                    return sumDP;
                }

                bool CanTargetConditionByPreSelectedList(List<Permanent> permanents, Permanent permanent)
                    => SumDP(permanents) + permanent.DP <= MaxTotalDP();

                bool CanEndSelectCondition(List<Permanent> permanents)
                    => permanents.Count >= 1
                        && SumDP(permanents) <= MaxTotalDP();

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition)) yield break;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: CanTargetConditionByPreSelectedList,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition),
                        canNoSelect: false,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage($"Select Digimon to delete (up to {MaxTotalDP()} DP total).", "The opponent is selecting Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            #region Your Turn - Security A. +1 per 8000 DP
            if (timing == EffectTiming.None)
            {
                int SecurityAttackBonus()
                {
                    return thisPermanent != null
                        && thisPermanent.HasDP ? thisPermanent.DP / 8000 : 0;
                }

                bool Condition()
                {
                    return CardEffectCommons.IsOwnerTurn(card)
                        && thisPermanent != null
                        && thisPermanent.HasDP
                        && thisPermanent.DP >= 8000;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(
                    changeValue: (Func<int>)SecurityAttackBonus,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition));
            }
            #endregion
            #endregion

            #region Option Side
            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 of your [ALYSION] Digimon gets +10000 DP, then delete 1 opponent's Digimon with DP up to 1 of yours", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] 1 of your [ALYSION] trait Digimon gets +10000 DP for the turn. Then, delete 1 of your opponent's Digimon with as much DP as 1 of your Digimon or less.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool AlysionDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("ALYSION");

                bool CanSelectOwnerDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(AlysionDigimonCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: AlysionDigimonCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 [ALYSION] Digimon that will get DP +10000.", "The opponent is selecting 1 Digimon that will get DP +10000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: permanent, changeValue: 10000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                        }
                    }

                    Permanent selectedOwnerDigimon = null;

                    bool CanSelectOpponentDigimon(Permanent permanent, Permanent ownersPermanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                                permanent.DP <= ownersPermanent.DP;
                    }

                    bool HasDigimonAbleToDelete()
                    {
                        return card.Owner.GetBattleAreaDigimons().Some(ownersPermanent => CardEffectCommons.HasMatchConditionOpponentsPermanent(card, permanent => CanSelectOpponentDigimon(permanent, ownersPermanent)));
                    }

                    if (HasDigimonAbleToDelete())
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOwnerDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 of your Digimon. You will next be deleting a Digimon with less DP than this to delete.", "The opponent is selecting 1 Digimon");

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedOwnerDigimon = permanent;
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if (selectedOwnerDigimon != null && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, permanent => CanSelectOpponentDigimon(permanent, selectedOwnerDigimon)))
                    {
                        SelectPermanentEffect deleteSelectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        deleteSelectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: permanent => CanSelectOpponentDigimon(permanent, selectedOwnerDigimon),
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        deleteSelectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(deleteSelectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion
            #endregion

            return cardEffects;
        }
    }
}
