using System.Collections;
using System.Collections.Generic;

// Hjaðninger Waltz — Option
namespace DCGO.CardEffects.BT27
{
    public class BT27_100 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Give opponent's Digimon a forced attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false,
                    "[Main] Give 1 of your opponent's Digimon \"[Start of Your Main Phase] This Digimon attacks.\" until their turn ends. Then, place this card in the battle area.");
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable) => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                bool CanSelectPermanent(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanent))
                    {
                        Permanent target = null;
                        SelectPermanentEffect select = GManager.instance.GetComponent<SelectPermanentEffect>();
                        select.SetUp(selectPlayer: card.Owner, canTargetCondition: CanSelectPermanent,
                            canTargetCondition_ByPreSelecetedList: null, canEndSelectCondition: null,
                            maxCount: 1, canNoSelect: false, canEndNotMax: false,
                            selectPermanentCoroutine: SelectTarget, afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom, cardEffect: activateClass);
                        select.SetUpCustomMessage("Select an opponent's Digimon to give a forced attack.",
                            "The opponent is selecting a Digimon to give a forced attack.");
                        yield return ContinuousController.instance.StartCoroutine(select.Activate());

                        IEnumerator SelectTarget(Permanent permanent) { target = permanent; yield return null; }

                        if (target != null)
                        {
                            ActivateClass granted = new ActivateClass();
                            granted.SetUpICardEffect("[Start of Your Main Phase] This Digimon attacks.",
                                CanTriggerGranted, target.TopCard);
                            granted.SetUpActivateClass(CanActivateGranted, AttackCoroutine, -1, false,
                                "[Start of Your Main Phase] This Digimon attacks.");
                            granted.SetIsOptionEffect(true);
                            granted.SetEffectSourcePermanent(target);
                            target.UntilOwnerTurnEndEffects.Add(GetGrantedEffect);

                            ICardEffect GetGrantedEffect(EffectTiming effectTiming) =>
                                effectTiming == EffectTiming.OnStartMainPhase ? granted : null;

                            bool CanTriggerGranted(Hashtable ht) =>
                                CardEffectCommons.IsPermanentExistsOnBattleArea(target) &&
                                GManager.instance.turnStateMachine.gameContext.TurnPlayer == target.TopCard.Owner;

                            bool CanActivateGranted(Hashtable ht) =>
                                CardEffectCommons.IsPermanentExistsOnBattleArea(target) &&
                                !target.TopCard.CanNotBeAffected(activateClass) && target.CanAttack(granted);

                            IEnumerator AttackCoroutine(Hashtable ht)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(target) && target.CanAttack(granted))
                                {
                                    SelectAttackEffect attack = GManager.instance.GetComponent<SelectAttackEffect>();
                                    attack.SetUp(attacker: target, canAttackPlayerCondition: () => true,
                                        defenderCondition: permanent => true, cardEffect: granted);
                                    attack.SetCanNotSelectNotAttack();
                                    yield return ContinuousController.instance.StartCoroutine(attack.Activate());
                                }
                            }

                            if (!target.TopCard.CanNotBeAffected(activateClass))
                                yield return ContinuousController.instance.StartCoroutine(
                                    GManager.instance.GetComponent<Effects>().CreateDebuffEffect(target));
                        }
                    }
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            #endregion

            #region All Turns - Delay
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("De-Digivolve all, then delete lowest play cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true,
                    "[All Turns] When any of your Digimon with [Craniamon] in their names suspend, <Delay>. <De-Digivolve 1> all of your opponent's Digimon. Then, delete all of their Digimon with the lowest play cost.");
                cardEffects.Add(activateClass);

                bool SuspendedCraniamon(Permanent permanent) =>
                    CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                    permanent.TopCard.ContainsCardName("Craniamon");

                bool CanUseCondition(Hashtable hashtable) =>
                    CardEffectCommons.IsExistOnBattleArea(card) &&
                    CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, SuspendedCraniamon) &&
                    CardEffectCommons.CanDeclareOptionDelayEffect(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool delayPaid = false;
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent> { card.PermanentOfThisCard() },
                            activateClass: activateClass,
                            successProcess: permanents => DelayPaid(), failureProcess: null));
                    IEnumerator DelayPaid() { delayPaid = true; yield return null; }
                    if (!delayPaid) yield break;

                    // Snapshot first: de-digivolution may change the active board.
                    List<Permanent> targets = card.Owner.Enemy.GetBattleAreaDigimons().Clone();
                    foreach (Permanent target in targets)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(target, card) &&
                            !target.TopCard.CanNotBeAffected(activateClass))
                            yield return ContinuousController.instance.StartCoroutine(
                                new IDegeneration(target, 1, activateClass).Degeneration());
                    }

                    // Recalculate after de-digivolution; play costs may have changed.
                    List<Permanent> lowest = card.Owner.Enemy.GetBattleAreaDigimons().Filter(
                        permanent => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                                     CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, true));
                    yield return ContinuousController.instance.StartCoroutine(
                        new DestroyPermanentsClass(lowest, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete all lowest-cost Digimon and add this card to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false,
                    "[Security] Delete all of your opponent's Digimon with the lowest play cost. Then, add this card to the hand.");
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable) => CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> lowest = card.Owner.Enemy.GetBattleAreaDigimons().Filter(
                        permanent => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                                     CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, true));
                    yield return ContinuousController.instance.StartCoroutine(
                        new DestroyPermanentsClass(lowest, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
