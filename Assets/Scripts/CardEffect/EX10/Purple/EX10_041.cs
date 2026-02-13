using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Wizardmon
namespace DCGO.CardEffects.EX10
{
    public class EX10_041 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel3 && targetPermanent.TopCard.HasEvilTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region When trashed from deck

            if (timing == EffectTiming.OnDiscardLibrary)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Give 1 digimon Sec Atk -1 until their turn ends", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When effects trash this card from the deck or security stack, give 1 of your opponent's Digimon <Security A. -1> (This Digimon checks 1 additional security card.) until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenSelfDiscardLibrary(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card)
                        && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, SharedCanSelectPermanentCondition);
                }
            }

            #endregion

            #region When trashed from security

            if (timing == EffectTiming.OnDiscardSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Give 1 digimon Sec Atk -1 until their turn ends", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When effects trash this card from the deck or security stack, give 1 of your opponent's Digimon <Security A. -1> (This Digimon checks 1 additional security card.) until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnTrashSelfSecurity(hashtable, null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card)
                        && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, SharedCanSelectPermanentCondition);
                }
            }

            #endregion

            #region Trash from Deck/Security Shared

            bool SharedCanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable _hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, SharedCanSelectPermanentCondition))
                {
                    Permanent selectedPermanent = null;

                    #region Select Enemy Digimon

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, SharedCanSelectPermanentCondition));

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SharedCanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to give -1 Security Attack", "The opponent is selecting 1 Digimon to give -1 Security Attack");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    #endregion

                    if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                        targetPermanent: selectedPermanent,
                        changeValue: -1,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass));
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing your top security, trash top 2 cards of your deck and all opponent digimon get -3K DP for the turn", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By trashing your top security card, trash the top 2 cards of your deck and all of your opponent's Digimon get -3000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.Owner.SecurityCards.Any();
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashSecurityAndProcessAccordingToResult(
                        player: card.Owner,
                        trashAmount: 1,
                        activateClass: activateClass,
                        fromTop: true,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<CardSource> trashedSecurityCards)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(
                            addTrashCount: 2,
                            player: card.Owner,
                            cardEffect: activateClass).AddTrashCardsFromLibraryTop());

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDPPlayerEffect(
                            permanentCondition: SharedCanSelectPermanentCondition,
                            changeValue: -3000,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing your top security, trash top 2 cards of your deck and all opponent digimon get -3K DP for the turn", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By trashing your top security card, trash the top 2 cards of your deck and all of your opponent's Digimon get -3000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.Owner.SecurityCards.Any();
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashSecurityAndProcessAccordingToResult(
                        player: card.Owner,
                        trashAmount: 1,
                        activateClass: activateClass,
                        fromTop: true,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<CardSource> trashedSecurityCards)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(
                            addTrashCount: 2,
                            player: card.Owner,
                            cardEffect: activateClass).AddTrashCardsFromLibraryTop());

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDPPlayerEffect(
                            permanentCondition: SharedCanSelectPermanentCondition,
                            changeValue: -3000,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));
                    }
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}