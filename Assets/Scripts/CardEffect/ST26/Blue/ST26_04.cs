using System.Collections;
using System.Collections.Generic;

// Sekkamon
namespace DCGO.CardEffects.ST26
{
    public class ST26_04 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Iceclad
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.IcecladSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "Trash any 3 opponent's sources, then 1 of their Digimon or Tamers can't suspend";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Trash any 3 digivolution cards from your opponent's Digimon. Then, 1 of their Digimon or Tamers can't suspend until their turn ends.";

            bool OpponentDigimonWithSourcesCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionCards.Count >= 1;

            bool OpponentDigimonOrTamerCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                    permanentCondition: OpponentDigimonWithSourcesCondition,
                    cardCondition: null,
                    maxCount: 3,
                    canNoTrash: false,
                    isFromOnly1Permanent: false,
                    activateClass: activateClass));

                if (!CardEffectCommons.HasMatchConditionPermanent(OpponentDigimonOrTamerCondition)) yield break;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: OpponentDigimonOrTamerCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer that can't suspend.", "The opponent is selecting 1 Digimon or Tamer that can't suspend.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotSuspend(
                        targetPermanent: permanent,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass,
                        condition: null,
                        effectName: "Can't suspend"));
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);
            #endregion

            #region End of Your Turn - OPT - Inherited
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash the bottom 2 sources of 1 opponent's Digimon, if not, this Digimon may unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("ST26_04_Inherited");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[End of Your Turn] [Once Per Turn] Trash the bottom 2 digivolution cards of 1 of your opponent's Digimon. If this effect didn't trash, this [Ice-Snow] trait Digimon may unsuspend.";

                bool OpponentDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool trashed = false;

                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentDigimonCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentDigimonCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to trash its bottom 2 digivolution cards.", "The opponent is selecting 1 Digimon to trash its bottom 2 digivolution cards.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            int countBefore = permanent.DigivolutionCards.Count;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                                targetPermanent: permanent, trashCount: 2, isFromTop: false, activateClass: activateClass));

                            trashed = permanent.DigivolutionCards.Count < countBefore;
                        }
                    }

                    if (trashed) yield break;

                    Permanent thisPermanent = card.PermanentOfThisCard();

                    if (thisPermanent == null
                        || !thisPermanent.TopCard.EqualsTraits("Ice-Snow")
                        || !thisPermanent.IsSuspended
                        || !CardEffectCommons.CanUnsuspend(thisPermanent)) yield break;

                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: "Unsuspend", value: true, spriteIndex: 0),
                        new SelectionElement<bool>(message: "Don't unsuspend", value: false, spriteIndex: 1),
                    };

                    GManager.instance.userSelectionManager.SetBoolSelection(
                        selectionElements: selectionElements,
                        selectPlayer: card.Owner,
                        selectPlayerMessage: "Will this Digimon unsuspend?",
                        notSelectPlayerMessage: "The opponent is choosing whether to unsuspend their Digimon.");

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    if (GManager.instance.userSelectionManager.SelectedBoolValue)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                            new List<Permanent>() { thisPermanent }, activateClass).Unsuspend());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
