using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Lucemon: Chaos Mode
namespace DCGO.CardEffects.EX10
{
    public class EX10_052 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared Methods

            bool CanSelectPermanentConditionShared(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                {
                    if (permanent.IsDigimon || permanent.IsTamer)
                    {
                        return true;
                    }
                }
                return false;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Discard 1 hand card

                int discardCount = Math.Min(1, card.Owner.HandCards.Count);

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: _ => true,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: discardCount,
                    canNoSelect: false,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: null,
                    mode: SelectHandEffect.Mode.Discard,
                    cardEffect: activateClass);

                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                #endregion

                List<Permanent> deleteTargetPermanents = new List<Permanent>();
                bool attemptedToDelete = false;
                bool failedToDelete = false;

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentConditionShared))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentConditionShared));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner.Enemy,
                        canTargetCondition: CanSelectPermanentConditionShared,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer to delete.", "The opponent is selecting 1 Digimon or Tamer to delete.");

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
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: deleteTargetPermanents,
                        activateClass: activateClass,
                        successProcess: null,
                        failureProcess: FailureProcess));

                    IEnumerator FailureProcess()
                    {
                        failedToDelete = true;
                        yield return null;
                    }
                }
                if (!attemptedToDelete || failedToDelete)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IRecovery(
                        player: card.Owner,
                        AddLifeCount: 1,
                        cardEffect: activateClass).Recovery()
                    );
                }
            }

            #endregion

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel3 && targetPermanent.TopCard.EqualsCardName("Lucemon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 1 card in hand, your opponent may delete a digimon or tamer. if they didnt Recover +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By trashing 1 card in your hand, your opponent may delete 1 of their Digimon or Tamers. If this effect didn't delete, <Recovery +1 (Deck)> (Place the top card of your deck on top of your security stack.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.Owner.HandCards.Any();
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 1 card in hand, your opponent may delete a digimon or tamer. if they didnt Recover +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] By trashing 1 card in your hand, your opponent may delete 1 of their Digimon or Tamers. If this effect didn't delete, <Recovery +1 (Deck)> (Place the top card of your deck on top of your security stack.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.Owner.HandCards.Any();
                }
            }

            #endregion

            #region All Turns - Once Per Turn

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Your opponent may delete a digimon or tamer, if they dont this cards doesnt leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("EX10_052_RemoveField");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon would leave the battle area, your opponent may delete 1 of their Digimon or Tamers. If this effect didn't delete, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool hasOpponentDeleted = false;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentConditionShared))
                    {
                        Permanent selectedPermanent = null;

                        #region Opponent Selects 1 Digimon or Tamer

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentConditionShared));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner.Enemy,
                            canTargetCondition: CanSelectPermanentConditionShared,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine (Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }
                        #endregion

                        #region Attempt to delete selected permanent

                        IEnumerator SuccessProcess(List<Permanent> deletedPermanents)
                        {
                            hasOpponentDeleted = true;
                            UnityEngine.Debug.Log($"DELETED SUCCESSFULLY: {hasOpponentDeleted}");
                            yield return null;
                        }

                        if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent> { selectedPermanent },
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                        #endregion
                    }

                    UnityEngine.Debug.Log($"FINAL CHECK: {hasOpponentDeleted}");
                    if (!hasOpponentDeleted)
                    {
                        Permanent thisPermament = card.PermanentOfThisCard();

                        #region Remove Events from Permanent

                        thisPermament.HideDeleteEffect();
                        thisPermament.HideHandBounceEffect();
                        thisPermament.HideDeckBounceEffect();
                        thisPermament.HideWillRemoveFieldEffect();

                        thisPermament.DestroyingEffect = null;
                        thisPermament.IsDestroyedByBattle = false;
                        thisPermament.HandBounceEffect = null;
                        thisPermament.LibraryBounceEffect = null;
                        thisPermament.willBeRemoveField = false;

                        #endregion
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}