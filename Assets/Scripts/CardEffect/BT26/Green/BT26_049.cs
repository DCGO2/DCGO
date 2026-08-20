using System;
using System.Collections;
using System.Collections.Generic;

// Rosemon
namespace DCGO.CardEffects.BT26
{
    public class BT26_049 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - [Lilamon]
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Lilamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Alternate Digivolution Requirement - Lv.5 w/[DATA SQUAD] trait
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsTraits("DATA SQUAD");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Shared When Digivolving / When Attacking

            string SharedEffectName() => "Suspend 2 opponent's Digimon or Tamers";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] Suspend 2 of your opponent's Digimon or Tamers.";

            bool CanSelectSuspendTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);

            bool SharedCanActivateCondition(Hashtable hashtable, ICardEffect activateClass)
                => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                    && CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendTargetCondition);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendTargetCondition))
                {
                    int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectSuspendTargetCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
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

                    selectPermanentEffect.SetUpCustomMessage("Select up to 2 Digimon or Tamers to suspend.", "The opponent is selecting up to 2 Digimon or Tamers to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), 1, false, SharedEffectDescription("When Digivolving"));
                activateClass.SetHashString("BT26_049_Shared");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), 1, false, SharedEffectDescription("When Attacking"));
                activateClass.SetHashString("BT26_049_Shared");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
            }
            #endregion

            #region All Turns - Reactive Play
            if (timing == EffectTiming.OnTappedAnyone || timing == EffectTiming.OnDigivolutionCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May play/use 1 [DATA SQUAD] card cost 3 (+1 per suspended opponent) or lower from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When any of your opponent's Digimon or Tamers suspend, or effects trash cards from under your Tamers, you may play or use 1 play or use cost 3 or lower [DATA SQUAD] trait card from your hand without paying the cost. For each suspended Digimon or Tamer, add 1 to the cost maximum.";

                bool OpponentPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer);

                bool OwnTamerCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card);

                bool IsSuspendedOpponentPermanent(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer)
                        && permanent.IsSuspended;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, OpponentPermanentCondition)
                            || CardEffectCommons.CanTriggerOnTrashDigivolutionCard(hashtable, OwnTamerCondition, null, _ => true));

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    int maxCost = 3 + card.Owner.Enemy.GetFieldPermanents().Filter(IsSuspendedOpponentPermanent).Count;

                    bool CanSelectCardCondition(CardSource cardSource)
                        => cardSource.ContainsTraits("DATA SQUAD")
                            && cardSource.HasCost && cardSource.GetCostItself <= maxCost
                            && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, isPlayOption: true);

                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                            canTargetCondition: CanSelectCardCondition,
                            root: SelectCardEffect.Root.Hand,
                            cardEffect: activateClass,
                            payCost: false));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
