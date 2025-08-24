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

            bool IsOpponentDigimonOrTamer(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && permanent.IsDigimon || permanent.IsTamer;
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

                if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentDigimonOrTamer))
                {
                    bool hasOpponentDeleted = false;
                    Permanent selectedPermanent = null;

                    #region Opponent Selects 1 Digimon or Tamer

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsOpponentDigimonOrTamer));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner.Enemy,
                        canTargetCondition: IsOpponentDigimonOrTamer,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    #endregion

                    #region Attempt to delete selected permanent

                    IEnumerator SuccessProcess(List<Permanent> deletedPermanents)
                    {
                        hasOpponentDeleted = true;
                        yield return null;
                    }

                    if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent> { selectedPermanent },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    #endregion

                    if (!hasOpponentDeleted)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(
                            player: card.Owner,
                            AddLifeCount: 1,
                            cardEffect: activateClass).Recovery());
                    }
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
                    return "All Turns] [Once Per Turn] When this Digimon would leave the battle area, your opponent may delete 1 of their Digimon or Tamers. If this effect didn't delete, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionPermanent(IsOpponentDigimonOrTamer);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentDigimonOrTamer))
                    {
                        bool hasOpponentDeleted = false;
                        Permanent selectedPermanent = null;

                        #region Opponent Selects 1 Digimon or Tamer

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsOpponentDigimonOrTamer));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner.Enemy,
                            canTargetCondition: IsOpponentDigimonOrTamer,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion

                        #region Attempt to delete selected permanent

                        IEnumerator SuccessProcess(List<Permanent> deletedPermanents)
                        {
                            hasOpponentDeleted = true;
                            yield return null;
                        }

                        if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent> { selectedPermanent },
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                        #endregion

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
            }

            #endregion

            return cardEffects;
        }
    }
}