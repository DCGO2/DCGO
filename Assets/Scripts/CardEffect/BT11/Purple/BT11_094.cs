using System.Collections;
using System.Collections.Generic;

// Mirei Mikagura
namespace DCGO.CardEffects.BT11
{
    public class BT11_094 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Turn
            if (timing == EffectTiming.OnStartTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Start of Your Turn] Gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.CanAddMemory(activateClass)) yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                bool isAngewomon = true;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Angewomon] or [LadyDevimon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] When one of your Digimon digivolves into [Angewomon] or [LadyDevimon], if you have 1 or fewer Digimon in play, by suspending this Tamer, you may play 1 [Angewomon] or [LadyDevimon] with a different name than the Digimon you digivolved into from your hand without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                    && CardEffectCommons.IsOwnerTurn(card)
                    && CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition))
                    {
                        List<Permanent> permanents = CardEffectCommons.GetPlayedPermanentsFromEnterFieldHashtable(
                        hashtable: hashtable,
                        rootCondition: null);

                        if (permanents[0].CardNamesJustAfterDigivolved.Contains("LadyDevimon")) isAngewomon = false;

                        return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateSuspendCostEffect(card)
                        && card.Owner.GetBattleAreaDigimons().Count <= 1;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && (permanent.TopCard.EqualsCardName("Angewomon")
                            || permanent.TopCard.EqualsCardName("LadyDevimon"));
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return ((!isAngewomon && cardSource.EqualsCardName("Angewomon"))
                            || (isAngewomon && cardSource.EqualsCardName("LadyDevimon")))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass,
                        SuccessProcess,
                        null));

                    IEnumerator SuccessProcess(List<Permanent> suspendedPermanents)
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                canTargetCondition: CanSelectCardCondition,
                                SelectCardEffect.Root.Hand,
                                activateClass,
                                false
                            ));
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