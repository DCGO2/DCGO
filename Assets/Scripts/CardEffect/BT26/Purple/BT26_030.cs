using System;
using System.Collections;
using System.Collections.Generic;

// Pumpkinmon
namespace DCGO.CardEffects.BT26
{
    public class BT26_030 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Angel]/[TS] card cost 4 or less from hand or trash free", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Security] You may play 1 card with the [Angel] or [TS] trait and a play cost of 4 or less from your hand or trash without paying the cost.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => (cardSource.ContainsTraits("Angel") || cardSource.HasTSTraits)
                        && cardSource.HasPlayCost && cardSource.GetCostItself <= 4;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool CanSelectHandCardCondition(CardSource cardSource)
                        => CanSelectCardCondition(cardSource) && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

                    bool CanSelectTrashCardCondition(CardSource cardSource)
                        => CanSelectCardCondition(cardSource) && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

                    bool validHandCard = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCardCondition);
                    bool validTrashCard = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectTrashCardCondition);

                    if (validHandCard || validTrashCard)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                        if (validHandCard) selectionElements.Add(new SelectionElement<int>(message: "Play from hand", value: 1, spriteIndex: 0));
                        if (validTrashCard) selectionElements.Add(new SelectionElement<int>(message: "Play from trash", value: 2, spriteIndex: 0));
                        selectionElements.Add(new SelectionElement<int>(message: "Don't play", value: 3, spriteIndex: 1));

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: "Will you play a card?", notSelectPlayerMessage: "The opponent is choosing whether to play a card.");
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                        int selected = GManager.instance.userSelectionManager.SelectedIntValue;

                        if (selected == 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                canTargetCondition: CanSelectHandCardCondition,
                                root: SelectCardEffect.Root.Hand,
                                cardEffect: activateClass,
                                payCost: false));
                        }
                        else if (selected == 2)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                canTargetCondition: CanSelectTrashCardCondition,
                                root: SelectCardEffect.Root.Trash,
                                cardEffect: activateClass,
                                payCost: false));
                        }
                    }
                }
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName()
                => "By trashing 1 hand card, 1 [Iliad] Digimon gains Execute and Ascension for the turn";

            string SharedEffectDescription(string tag)
                => $"[{tag}] By trashing 1 card in your hand, 1 of your Digimon with the [Iliad] trait gains <Execute> and <Ascension> for the turn. (When this Digimon is deleted, you may place this card as the top security card.)";

            bool CanSelectGrantTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.ContainsTraits("Iliad");

            bool SharedCanActivateCondition(Hashtable hashtable, ICardEffect activateClass)
                => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                    && card.Owner.HandCards.Count >= 1
                    && CardEffectCommons.HasMatchConditionPermanent(CanSelectGrantTargetCondition);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.HandCards.Count >= 1)
                {
                    CardSource selectedCardToTrash = null;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: _ => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCardToTrash = cardSource;
                        yield return null;
                    }

                    selectHandEffect.SetUpCustomMessage("Select 1 card to trash.", "The opponent is selecting 1 card to trash.");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    if (selectedCardToTrash != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashHandAndProcessAccordingToResult(
                            player: card.Owner,
                            hashtable: hashtable,
                            cardToTrash: selectedCardToTrash,
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                        IEnumerator SuccessProcess(CardSource cardSource)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectGrantTargetCondition))
                            {
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectGrantTargetCondition));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectGrantTargetCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 [Iliad] Digimon to gain Execute and Ascension.", "The opponent is selecting 1 [Iliad] Digimon to gain Execute and Ascension.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainExecute(permanent, EffectDuration.UntilEachTurnEnd, activateClass));
                                    yield return ContinuousController.instance.StartCoroutine(GainAscension(permanent, EffectDuration.UntilEachTurnEnd, activateClass));
                                }
                            }
                        }
                    }
                }
            }

            // No CardEffectCommons.GainAscension helper exists yet (only the self-targeted AscensionSelfEffect) -
            // built inline here mirroring CardEffectCommons.GainExecute's shape, since CanTriggerAscension/
            // CanActivateAscension/AscensionProcess all already take an arbitrary CardSource, not just the effect's own card.
            IEnumerator GainAscension(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect grantingActivateClass)
            {
                if (targetPermanent == null) yield break;
                if (!CardEffectCommons.IsPermanentExistsOnBattleArea(targetPermanent)) yield break;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerAscension(hashtable, targetPermanent.TopCard, grantingActivateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.CanActivateAscension(targetPermanent.TopCard, grantingActivateClass);

                IEnumerator AscensionActivateCoroutine(Hashtable hashtable)
                    => CardEffectCommons.AscensionProcess(hashtable, grantingActivateClass, targetPermanent.TopCard);

                ActivateClass ascension = new ActivateClass();
                ascension.SetUpICardEffect("Ascension", CanUseCondition, targetPermanent.TopCard);
                ascension.SetUpActivateClass(CanActivateCondition, AscensionActivateCoroutine, -1, false, DataBase.AscensionEffectDescription());

                CardEffectCommons.AddEffectToPermanent(
                    targetPermanent: targetPermanent,
                    effectDuration: effectDuration,
                    card: targetPermanent.TopCard,
                    cardEffect: ascension,
                    timing: EffectTiming.OnDestroyedAnyone);

                if (!targetPermanent.TopCard.CanNotBeAffected(grantingActivateClass))
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
                }
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }
            #endregion

            return cardEffects;
        }
    }
}
