using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT18
{
    public class BT18_082 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.EqualsCardName("Lucemon"))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition,
                    digivolutionCost: 6, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region On Play/ When Digivolving Shared

            bool CanSelectPermanentConditionShared(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card) &&
                       (permanent.IsDigimon || permanent.IsTamer);
            }

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent deletes 1 Digimon or Tamer/Trash and Recover", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] Your opponent may delete 1 of their Digimon or Tamers. If this effect didn't delete, trash the top card of your opponents security stack and <Recovery +1>.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> deleteTargetPermanents = new List<Permanent>();

                    bool attemptedToDelete = false;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentConditionShared))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner.Enemy,
                            canTargetCondition: CanSelectPermanentConditionShared,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer to delete.",
                            "The opponent is selecting 1 Digimon or Tamer to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            deleteTargetPermanents = permanents.Clone();

                            attemptedToDelete = true;

                            yield return null;
                        }
                    }

                    if (attemptedToDelete)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargetPermanents,
                                activateClass: activateClass, successProcess: null, failureProcess: FailureProcess));

                        IEnumerator FailureProcess()
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                player: card.Owner.Enemy,
                                destroySecurityCount: 1,
                                cardEffect: activateClass,
                                fromTop: true).DestroySecurity());

                            yield return ContinuousController.instance.StartCoroutine(
                                new IRecovery(card.Owner, 1, activateClass).Recovery());
                        }
                    }
                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner.Enemy,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());

                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent deletes 1 Digimon or Tamer/Trash and Recover", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] Your opponent may delete 1 of their Digimon or Tamers. If this effect didn't delete, trash the top card of your opponents security stack and <Recovery +1>.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> deleteTargetPermanents = new List<Permanent>();

                    bool attemptedToDelete = false;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentConditionShared))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner.Enemy,
                            canTargetCondition: CanSelectPermanentConditionShared,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer to delete.",
                            "The opponent is selecting 1 Digimon or Tamer to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            deleteTargetPermanents = permanents.Clone();

                            attemptedToDelete = true;

                            yield return null;
                        }
                    }

                    if (attemptedToDelete)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargetPermanents,
                                activateClass: activateClass, successProcess: null, failureProcess: FailureProcess));

                        IEnumerator FailureProcess()
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                player: card.Owner.Enemy,
                                destroySecurityCount: 1,
                                cardEffect: activateClass,
                                fromTop: true).DestroySecurity());

                            yield return ContinuousController.instance.StartCoroutine(
                                new IRecovery(card.Owner, 1, activateClass).Recovery());
                        }
                    }
                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner.Enemy,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());

                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash the bottom card of your security stack to not leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("Protection_BT18-082");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] (Once Per Turn) When this Digimon would leave the battle area, by trashing the bottom card of your security stack, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.Owner.SecurityCards.Count >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        card.Owner,
                        1,
                        activateClass,
                        false).DestroySecurity());
                    
                    Permanent thisCardPermanent = card.PermanentOfThisCard();
                    
                    thisCardPermanent.willBeRemoveField = false;
                    
                    thisCardPermanent.HideDeleteEffect();
                    thisCardPermanent.HideHandBounceEffect();
                    thisCardPermanent.HideDeckBounceEffect();
                    thisCardPermanent.HideWillRemoveFieldEffect();
                }
            }

            #endregion

            return cardEffects;
        }
    }
}