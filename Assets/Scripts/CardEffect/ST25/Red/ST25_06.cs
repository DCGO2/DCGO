using System.Collections;
using System.Collections.Generic;

// Kanata Hondo
namespace DCGO.CardEffects.ST25
{
    public class ST25_06 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Turn
            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }
            #endregion

            #region Your Turn - [ALYSION] Digimon played or digivolve
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending this Tamer, 1 of your Digimon isn't affected by opponent's Option effects and gets +3000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] When your Digimon are played or digivolve, if any of them have the [ALYSION] trait, by suspending this Tamer, for the turn, 1 of your Digimon isn't affected by your opponent's Option effects and gets +3000 DP.";

                bool OwnDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, OwnDigimonCondition)
                            || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, OwnDigimonCondition));

                bool CanActivateCondition(Hashtable hashtable)
                {
                    List<Permanent> permanents = CardEffectCommons.GetPlayedPermanentsFromEnterFieldHashtable(
                        hashtable: hashtable,
                        rootCondition: null);

                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && permanents != null
                        && permanents.Some(permanent => OwnDigimonCondition(permanent) && permanent.TopCard.EqualsTraits("ALYSION"))
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<Permanent> _permanents)
                    {
                        if (!CardEffectCommons.HasMatchConditionPermanent(OwnDigimonCondition)) yield break;

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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will gain Option immunity and DP +3000.", "The opponent is selecting 1 Digimon that will gain Option immunity and DP +3000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            permanent.UntilEachTurnEndEffects.Add(_timing => PermanentEffectFactory.OptionEffectImmunity(permanent));
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(permanent));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: permanent, changeValue: 3000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
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
