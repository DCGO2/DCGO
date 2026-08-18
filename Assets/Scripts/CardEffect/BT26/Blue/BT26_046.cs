using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Gryphonmon
namespace DCGO.CardEffects.BT26
{
    public class BT26_046 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Reduce Play Cost
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce play cost by 4", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetHashString("BT26_046_ReducePlayCost");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "When this card would be played, if there are 2 or more suspended Digimon, reduce the cost by 4.";

                bool CardCondition(CardSource cardSource) => cardSource == card;

                int SuspendedDigimonCount()
                {
                    int count = 0;
                    count += card.Owner.GetBattleAreaDigimons().Count(permanent => permanent.IsSuspended);
                    count += card.Owner.Enemy.GetBattleAreaDigimons().Count(permanent => permanent.IsSuspended);
                    return count;
                }

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => SuspendedDigimonCount() >= 2
                        && card.Owner.CanReduceCost(null, card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect("Play Cost -4", CanUseCondition1, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardCondition, rootCondition: RootCondition, isUpDown: IsUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);

                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                    card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                    bool CanUseCondition1(Hashtable hashtable) => true;

                    bool RootCondition(SelectCardEffect.Root root) => true;

                    bool IsUpDown() => true;

                    int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    {
                        if (CardCondition(cardSource) && SuspendedDigimonCount() >= 2)
                        {
                            cost -= 4;
                        }

                        return cost;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));
                }
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Vortex
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.VortexSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName()
                => "Suspend 1 opponent's Digimon/Tamer, can't unsuspend, then 1 of your Digimon can't be deleted in battle";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Suspend 1 of your opponent's Digimon or Tamers. 1 of their Digimon or Tamers can't unsuspend until their turn ends. Then, 1 of your Digimon can't be deleted in battle until their turn ends.";

            bool CanSelectSuspendTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer)
                    && !permanent.IsSuspended && permanent.CanSuspend;

            bool CanSelectCantUnsuspendCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);

            bool CanSelectProtectTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

            bool SharedCanActivateCondition(Hashtable hashtable, ICardEffect activateClass)
                => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                    && CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendTargetCondition);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendTargetCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectSuspendTargetCondition));

                    SelectPermanentEffect selectSuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectSuspendEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectSuspendTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectSuspendEffect.SetUpCustomMessage("Select 1 Digimon or Tamer to suspend.", "The opponent is selecting 1 Digimon or Tamer to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectSuspendEffect.Activate());
                }

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectCantUnsuspendCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectCantUnsuspendCondition));

                    SelectPermanentEffect selectCantUnsuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectCantUnsuspendEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCantUnsuspendCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectCantUnsuspendEffect.SetUpCustomMessage("Select 1 Digimon or Tamer that can't unsuspend.", "The opponent is selecting 1 Digimon or Tamer that can't unsuspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectCantUnsuspendEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspend(permanent, EffectDuration.UntilOpponentTurnEnd, activateClass, null, "Can't unsuspend"));
                    }
                }

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectProtectTargetCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectProtectTargetCondition));

                    SelectPermanentEffect selectProtectEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectProtectEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectProtectTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectProtectEffect.SetUpCustomMessage("Select 1 Digimon that can't be deleted in battle.", "The opponent is selecting 1 Digimon that can't be deleted in battle.");

                    yield return ContinuousController.instance.StartCoroutine(selectProtectEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        bool CanNotBeDestroyedByBattleCondition(Permanent permanent1, Permanent attackingPermanent, Permanent defendingPermanent, CardSource defendingCard)
                            => permanent1 == attackingPermanent || permanent1 == defendingPermanent;

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBeDeletedByBattle(
                            targetPermanent: permanent,
                            canNotBeDestroyedByBattleCondition: CanNotBeDestroyedByBattleCondition,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            effectName: "Can't be deleted in battle"));
                    }
                }
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }
            #endregion

            return cardEffects;
        }
    }
}
