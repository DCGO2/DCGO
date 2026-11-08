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
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanent,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get effects.",
                            "The opponent is selecting 1 Digimon that will get effects.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            ActivateClass activateClassDebuff = new ActivateClass();
                            activateClassDebuff.SetUpICardEffect("Attack with this Digimon", CanUseConditionDebuff, selectedPermanent.TopCard);
                            activateClassDebuff.SetUpActivateClass(CanActivateConditionDebuff, ActivateCoroutineDebuff, -1, false, EffectDescriptionDebuff());
                            activateClassDebuff.SetEffectSourcePermanent(selectedPermanent);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                            }

                            string EffectDescriptionDebuff()
                            {
                                return "[Start of Your Main Phase] Attack with this Digimon.";
                            }

                            bool CanUseConditionDebuff(Hashtable hashtable1)
                            {
                                return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(selectedPermanent) &&
                                        selectedPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(selectedPermanent) &&
                                        !selectedPermanent.TopCard.CanNotBeAffected(activateClass) &&
                                        GManager.instance.turnStateMachine.gameContext.TurnPlayer == selectedPermanent.TopCard.Owner;
                            }

                            bool CanActivateConditionDebuff(Hashtable hashtable1)
                            {
                                return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(selectedPermanent) &&
                                        !selectedPermanent.TopCard.CanNotBeAffected(activateClass) &&
                                        selectedPermanent.CanAttack(activateClassDebuff);
                            }

                            IEnumerator ActivateCoroutineDebuff(Hashtable hashtableDebuff)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent) &&
                                    selectedPermanent.CanAttack(activateClassDebuff))
                                {
                                    SelectAttackEffect selectAttackEffect =
                                        GManager.instance.GetComponent<SelectAttackEffect>();

                                    selectAttackEffect.SetUp(
                                        attacker: selectedPermanent,
                                        canAttackPlayerCondition: () => true,
                                        defenderCondition: _ => true,
                                        cardEffect: activateClassDebuff);

                                    selectAttackEffect.SetCanNotSelectNotAttack();

                                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                }
                            }

                            ICardEffect GetCardEffect(EffectTiming timingDebuff)
                            {
                                return timingDebuff == EffectTiming.OnStartMainPhase ? activateClassDebuff : null;
                            }
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

                    yield return ContinuousController.instance.StartCoroutine(new IMassDegeneration(card.Owner.Enemy.GetBattleAreaDigimons(), 1, activateClass).Degeneration());

                    List<Permanent> lowest = card.Owner.Enemy.GetBattleAreaDigimons().Filter(
                        permanent => CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, true));

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
                        permanent => CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, true));
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
