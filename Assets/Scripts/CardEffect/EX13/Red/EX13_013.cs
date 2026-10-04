using System.Collections;
using System.Collections.Generic;

// WarGrowlmon
namespace DCGO.CardEffects.EX13
{
    public class EX13_013 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Engage
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.EngageSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared WD/WA
            string SharedEffectName = "Delete 1 enemy Digimon with 5K DP or less, if did non, gain <Piercing> and 3K DP for turn";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    whenDigivolving: true,
                    whenAttacking: true);

            string SharedEffectDescription(string tag) => $"[{tag}] Delete 1 of your opponent's Digimon with 5000 DP or less. If this effect didn't delete, this Digimon gains <Piercing> and +3000 DP for the turn.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                List<Permanent> deleteTargetPermanents = new List<Permanent>();

                bool failedToDelete = true;

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                            && permanent.DP <= card.Owner.MaxDP_DeleteEffect(5000, activateClass);
                }

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        deleteTargetPermanents = permanents.Clone();

                        yield return null;
                    }
                }

                if (deleteTargetPermanents.Count > 0)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargetPermanents, activateClass: activateClass, successProcess: SuccessDeleteProcess, failureProcess: null));

                    IEnumerator SuccessDeleteProcess(List<Permanent> permanents)
                    {
                        if (permanents.Count > 0)
                            failedToDelete = false;

                        yield return null;
                    }
                }

                if (failedToDelete
                && CardEffectCommons.IsExistOnBattleAreaDigimon(card.PermanentOfThisCard().TopCard))
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(
                        targetPermanent: card.PermanentOfThisCard(),
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                        targetPermanent: card.PermanentOfThisCard(),
                        changeValue: 3000,
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));
                }
            }
            #endregion

            #region EoA/OD
            string SharedEffectName2 = "Play tamer with [Guilmon] text from hand/trash";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName2,
                    SharedActivateCoroutine2,
                    SharedEffectDescription2,
                    additionalActivateCondition: AdditionalActivateCondition,
                    optional: false,
                    isSkippable: true,
                    endOfAttack: true,
                    onDeletion: true);

            string SharedEffectDescription2(string tag) => $"[{tag}] You may play 1 Tamer card with [Guilmon] in its text from your hand or trash without paying the cost.";

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanSelectCardCondition2(cardSource, activateClass))
                        || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, cardSource => CanSelectCardCondition2(cardSource, activateClass));
            }

            bool CanSelectCardCondition2(CardSource cardSource, ActivateClass activateClass)
            {
                return cardSource.IsTamer
                    && cardSource.HasText("Guilmon")
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
            }

            IEnumerator SharedActivateCoroutine2(Hashtable hashtable, ActivateClass activateClass)
            {
                bool canPlayFromHand = CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanSelectCardCondition2(cardSource, activateClass));
                bool canPlayFromTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, cardSource => CanSelectCardCondition2(cardSource, activateClass));

                bool shouldPlay = canPlayFromHand || canPlayFromTrash;
                SelectCardEffect.Root root = canPlayFromTrash && !canPlayFromHand ? SelectCardEffect.Root.Trash : SelectCardEffect.Root.Hand;

                if (canPlayFromHand && canPlayFromTrash)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "Play from hand", value: 1, spriteIndex: 0),
                            new(message: "Play from trash", value: 2, spriteIndex: 0),
                            new(message: "Don't play a card", value: 3, spriteIndex: 1),
                        };

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: "Will you play a card?", notSelectPlayerMessage: "The opponent is choosing whether to play a card.");
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    int selected = GManager.instance.userSelectionManager.SelectedIntValue;

                    shouldPlay = selected != 3;
                    root = selected == 1 ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                }

                if (shouldPlay)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: cardSource => CanSelectCardCondition2(cardSource, activateClass),
                        root: root,
                        cardEffect: activateClass,
                        payCost: false));
                }
            }
            #endregion

            #region Inherited AT
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash enemy's top sec", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX13_013_Inherited");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When any of your opponent's Digimon are deleted, if this Digimon has [Gallantmon] in its name, trash their top security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, IsOpponentDigimon, activateClass)
                        && CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && card.PermanentOfThisCard().TopCard.ContainsCardName("Gallantmon");
                }

                bool IsOpponentDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
