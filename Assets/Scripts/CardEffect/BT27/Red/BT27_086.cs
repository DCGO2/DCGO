using System.Collections;
using System.Collections.Generic;

// Kanata Hondo
namespace DCGO.CardEffects.BT27
{
    public class BT27_086 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Start of Your Main Phase] If you have a Digimon with [Gemmon] in its text, gain 1 memory.";

                bool GemmonDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasText("Gemmon");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(GemmonDigimonCondition)
                        && card.Owner.CanAddMemory(activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            #region Your Turn - [ALYSION] Digimon played or digivolve
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending this Tamer, 1 of your Digimon gets +3000 DP, then if you have an 8000 DP+ Digimon, <Draw 1>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] When your Digimon are played or digivolve, if any of them have the [ALYSION] trait, by suspending this Tamer, 1 of your Digimon gets +3000 DP for the turn. After, if you have a Digimon with 8000 DP or more, <Draw 1>.";

                bool AlysionDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("ALYSION");

                bool OwnDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool HighDPDigimonCondition(Permanent permanent)
                    => OwnDigimonCondition(permanent)
                        && permanent.HasDP
                        && permanent.DP >= 8000;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, AlysionDigimonCondition)
                            || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, AlysionDigimonCondition));

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<Permanent> _permanents)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(OwnDigimonCondition))
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: OwnDigimonCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP +3000.", "The opponent is selecting 1 Digimon that will get DP +3000.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                    targetPermanent: permanent, changeValue: 3000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                            }
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(HighDPDigimonCondition))
                        {
                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
