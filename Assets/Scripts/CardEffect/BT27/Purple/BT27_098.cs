using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Akihiro Kurata — BT27-098
namespace DCGO.CardEffects.BT27
{
    public class BT27_098 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region All Turns
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend: place top 2 deck cards face down under this Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    "[All Turns] When any Digimon are deleted, by suspending this Tamer, you may place the top 2 cards of your deck face down under this Tamer.");
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, p => p.IsDigimon, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent> { card.PermanentOfThisCard() },
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                    IEnumerator SuccessProcess(List<Permanent> suspendedPermanents)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>
                        {
                            new(message: $"Suspend and place 2 cards from deck under this card", value: 1, spriteIndex: 0),
                            new(message: $"Only suspend without placing cards", value: 2, spriteIndex: 0),
                            new(message: $"No", value: 3, spriteIndex: 1)
                        };

                        string selectPlayerMessage = "Will you suspend this tamer to place the top 2 cards from your deck under this Tamer face down?";
                        string notSelectPlayerMessage = "The opponent is choosing whether to place the top 2 cards from their deck under their Tamer face down.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        int answer = GManager.instance.userSelectionManager.SelectedIntValue;

                        if (answer != 3)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                                new List<Permanent>() { card.PermanentOfThisCard() },
                                activateClass,
                                SuccessProcess,
                                null));
                        }

                        IEnumerator SuccessProcess(List<Permanent> suspendedPermaments)
                        {
                            if (answer == 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                        new List<CardSource> { card.Owner.LibraryCards[0], card.Owner.LibraryCards[1] }, activateClass, isFacedown: true));
                            }
                        }
                    }
                }
            }
            #endregion

            #region End of All Turns
            if (timing == EffectTiming.OnEndTurn)
            {
                int faceDownSourceCount = 0;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 5 Tamer sources to play [Gizmon]/[Belphemon] in name from hand/trash for free, then place this Tamer under it", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[End of All Turns] By trashing 5 bottom face-down cards from under any of your Tamers, you may play 1 Digimon card with [Gizmon] or [Belphemon] in its name from your hand or trash without paying the cost. After, place this Tamer as its bottom digivolution card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    faceDownSourceCount = card.Owner.GetBattleAreaPermanents()
                        .Filter(TamerWithOneOrMoreFaceDownSource)
                        .Sum(permanent => permanent.DigivolutionCards.Count(CanSelectTrashSourceCardCondition));

                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && faceDownSourceCount >= 5;
                }

                bool TamerWithOneOrMoreFaceDownSource(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Any(CanSelectTrashSourceCardCondition);
                }

                bool CanSelectTrashSourceCardCondition(CardSource cardSource)
                {
                    return cardSource.IsFaceDown;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool trashed = false;

                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                    int maxCount1 = Math.Min(5, CardEffectCommons.MatchConditionPermanentCount(TamerWithOneOrMoreFaceDownSource));

                    selectPermanentEffect1.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TamerWithOneOrMoreFaceDownSource,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: maxCount1,
                        canNoSelect: true,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect1.SetUpCustomMessage("Select all Tamer(s) to trash bottom face-down cards from", "The opponent is selecting Tamer(s) to trash bottom face-down cards from");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    bool CanEndSelectCondition(List<Permanent> permanents)
                    {
                        if (permanents.Count > 0)
                        {
                            int faceDownCount = 0;

                            foreach (Permanent permanent in permanents)
                            {
                                faceDownCount += permanent.DigivolutionCards.Count(CanSelectTrashSourceCardCondition);
                            }

                            if (faceDownCount >= 5)
                            {
                                return true;
                            }
                        }
                        return false;
                    }

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents.Count == faceDownSourceCount)
                        {
                            trashed = true;
                            foreach (Permanent selectedPermanent in permanents)
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: 1, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));
                        }
                        else if (permanents.Count == 1)
                        {
                            trashed = true;
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 5, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));
                        }
                        else
                        {
                            Permanent trash2Tamer = null;

                            SelectPermanentEffect selectPermanentEffect2 = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect2.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: permanent => permanents.Contains(permanent) && permanent.DigivolutionCards.Count(CanSelectTrashSourceCardCondition) >= 2,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: CanEndSelectCondition,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect2.SetUpCustomMessage("Select Tamer to trash 2 from", "The opponent is selecting Tamer(s) to trash bottom face-down cards from");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect2.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                trash2Tamer = permanent;

                                yield return null;
                            }

                            Permanent trash1Tamer = permanents.FirstOrDefault(permanent => permanent != trash2Tamer);

                            trashed = true;
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: trash2Tamer, trashCount: 2, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: trash1Tamer, trashCount: 1, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));
                        }

                        yield return null;
                    }

                    if (trashed)
                    {

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
