using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// ClavisAngemon
namespace DCGO.CardEffects.BT25
{
    public class BT25_042 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Angel") ||
                        targetPermanent.TopCard.EqualsTraits("Archangel") ||
                        targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null, level: 5));
            }
            #endregion

            #region OP/WD/WA Shared

            string SharedHashString = "BT25_042_OP_WD_WA";

            string SharedEffectName = "By trashing top or bottom security, this digimon is immune to digimon effect until opponent turn ends";

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] By trashing your top or bottom security card, your opponent's Digimon effects don't affect this Digimon until their turn ends.";

            bool AdditionalCanUseCondition(Hashtable hashtable)
            {
                return card.Owner.SecurityCards.Count >= 1;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isUsed = false;

                if (card.Owner.SecurityCards.Count >= 1)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                    {
                            new(message: $"Trash Top Security card", value: 1, spriteIndex: 0),
                            new(message: $"Trash Bottom Security card", value: 2, spriteIndex: 0),
                            new(message: $"Don't trash security", value: 3, spriteIndex: 1)
                    };
                    string selectPlayerMessage = "Will you trash a security card?";
                    string notSelectPlayerMessage = "The opponent is choosing if they will trash a security card.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool doTrash = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                    bool topSecurity = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                    if (doTrash)
                    {
                        if (topSecurity)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashSecurityAndProcessAccordingToResult(
                                player: card.Owner,
                                trashAmount: 1,
                                activateClass: activateClass,
                                fromTop: true,
                                successProcess: SuccessProcess,
                                failureProcess: null));
                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashSecurityAndProcessAccordingToResult(
                                player: card.Owner,
                                trashAmount: 1,
                                activateClass: activateClass,
                                fromTop: false,
                                successProcess: SuccessProcess,
                                failureProcess: null));
                        }

                        IEnumerator SuccessProcess(List<CardSource> cardSources)
                        {
                            isUsed = true;
                            Permanent thisPermanent = card.PermanentOfThisCard();

                            thisPermanent.UntilOpponentTurnEndEffects.Add((_timing) => PermanentEffectFactory.DigimonEffectImmunity(thisPermanent));
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(thisPermanent));
                        }

                    }
                }
                if (!isUsed) activateClass.RemoveUse();
            }

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    maxCountPerTurn: 1,
                    hashValue: SharedHashString,
                    onPlay: true,
                    whenDigivolving: true,
                    whenAttacking: true,
                    additionalUseCondition: AdditionalCanUseCondition);

            #endregion

            #region All Turns
            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 level 4 or lower [Angel]/[Illad] trait card from hand without cost, then 2 digimon gain reboot & blocker untill opponent turn end", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT25_042_AT");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When your security stack is removed from, you may play 1 level 4 or lower [Angel] or [Iliad] trait card from your hand without paying the cost. Then, 2 of your Digimon gain <Reboot> and <Blocker> until your opponent's turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnTrashSecurity(hashtable, null, trashedCard => trashedCard.Owner == card.Owner);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanPlayAngelCardCondition(CardSource cardSource)
                    => cardSource.Level <= 4
                       && cardSource.EqualsTraits("Angel")
                       && CardEffectCommons.CanPlayAsNewPermanent(card, false, activateClass);

                bool CanPlayIlladCardCondition(CardSource cardSource)
                    => cardSource.Level <= 4
                       && cardSource.HasIliadTraits
                       && CardEffectCommons.CanPlayAsNewPermanent(card, false, activateClass);

                bool IsOwnedDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canPlayAngelCard = CardEffectCommons.HasMatchConditionOwnersHand(card, CanPlayAngelCardCondition);
                    bool canPlayIlladCard = CardEffectCommons.HasMatchConditionOwnersHand(card, CanPlayIlladCardCondition);

                    if (canPlayAngelCard || canPlayIlladCard)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                        if (canPlayAngelCard) selectionElements.Add(new SelectionElement<int>(message: $"Play Angel card", value: 1, spriteIndex: 0));
                        if (canPlayIlladCard) selectionElements.Add(new SelectionElement<int>(message: $"Play Illad card", value: 2, spriteIndex: 0));
                        selectionElements.Add(new SelectionElement<int>(message: $"Don't play card", value: 3, spriteIndex: 1));

                        string selectPlayerMessage = "Will you play a card?";
                        string notSelectPlayerMessage = "The opponent is choosing if they will play a card.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool doPlay = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                        bool playAngel = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                        if (doPlay)
                        {
                            if (playAngel)
                            {
                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, CanPlayAngelCardCondition));

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanPlayAngelCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.PlayForFree,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                            }
                            else
                            {
                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, CanPlayIlladCardCondition));

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanPlayIlladCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.PlayForFree,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                            }
                        }
                    }

                    List<Permanent> selectedPermaments = new List<Permanent>();
                    int digimonCount = CardEffectCommons.MatchConditionOwnersPermanentCount(card, IsOwnedDigimon);

                    if (digimonCount > 2)
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = Math.Min(2, CardEffectCommons.MatchConditionOwnersPermanentCount(card, IsOwnedDigimon));

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOwnedDigimon,
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
                            selectedPermaments.Add(permanent);
                            yield break;
                        }

                        selectPermanentEffect.SetUpCustomMessage($"Select 2 digimon to gain reboot and blocker.", $"The opponent is selecting 2 digimon to gain reboot and blocker.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                    else if (digimonCount == 2) selectedPermaments.AddRange(card.Owner.GetBattleAreaDigimons());
                    else if (digimonCount == 1) selectedPermaments.Add(card.Owner.GetBattleAreaDigimons()[0]);

                    if (selectedPermaments.Any())
                    {
                        foreach (Permanent permanent in selectedPermaments)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainReboot(
                                targetPermanent: permanent,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(
                                targetPermanent: permanent,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}