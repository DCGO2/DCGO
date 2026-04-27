using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

// Habakirimon/Habakiri
namespace DCGO.CardEffects.BT25
{
    public class BT25_043 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects
            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.HasGlowingDawnTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null, level: 5));
            }
            #endregion

            #region Shared WD/WA
            string SharedHashString = "BT25_043_WD_WA";

            string SharedEffectName = "<Recovery +1>, by trashing top sec of player with most sec cards, unsuspend";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    maxCountPerTurn: 1,
                    hashValue: SharedHashString,
                    whenDigivolving: true,
                    whenAttacking: true);

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] <Recovery +1>. Then, by trashing the top security card of 1 player with the most security cards, this Digimon unsuspends.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool validOwnerSecurity = card.Owner.SecurityCards.Count > 0 && card.Owner.SecurityCards.Count >= card.Owner.Enemy.SecurityCards.Count;
                bool validEnemySecurity = card.Owner.Enemy.SecurityCards.Count > 0 && card.Owner.Enemy.SecurityCards.Count >= card.Owner.SecurityCards.Count;

                if (validOwnerSecurity || validEnemySecurity)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    if (validOwnerSecurity)
                    {
                        selectionElements.Add(new(message: $"Trash your own top security card", value: 1, spriteIndex: 0));
                    }
                    if (validEnemySecurity)
                    {
                        selectionElements.Add(new(message: $"Trash your opponent's top security card", value: 2, spriteIndex: 0));
                    }
                    selectionElements.Add(new(message: $"Don't trash security", value: 3, spriteIndex: 1));

                    string selectPlayerMessage = "Will you trash 1 player's security to unsuspend?";
                    string notSelectPlayerMessage = "The opponent is choosing if they will trash a security.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool doTrash = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                    bool ownSecurity = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                    if (doTrash)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashSecurityAndProcessAccordingToResult(
                            player: ownSecurity ? card.Owner : card.Owner.Enemy,
                            trashAmount: 1,
                            activateClass: activateClass,
                            fromTop: true,
                            successProcess: SuccessProcess,
                            failureProcess: null
                        ));

                        IEnumerator SuccessProcess(List<CardSource> cardSources)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                        }                   
                    }
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash your top sec to prevent them from leaving", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT25_043_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns][Once Per Turn] When any of your [Glowing Dawn] trait Digimon would leave the battle area, by trashing your top security card, they don't leave.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasGlowingDawnTraits;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && card.Owner.SecurityCards.Count >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> removedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable);

                    removedPermanents = removedPermanents.Filter(PermanentCondition);
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());
                            
                    foreach (Permanent permanent in removedPermanents)
                    {
                        permanent.willBeRemoveField = false;
                        permanent.HideDeleteEffect();
                        permanent.HideHandBounceEffect();
                        permanent.HideDeckBounceEffect();
                        permanent.HideWillRemoveFieldEffect();
                    }
                }
            }
            #endregion
            #endregion

            #region Option Effects
            #region Ignore Colour Requirement
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.HasGlowingDawnTraits;
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 enemy Digimon gets -8K DP for turn, then by trashing your top sec, all enemy digimon get -5K DP for turn", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] 1 of your opponent's Digimon gets -8000 DP for the turn. Then, by trashing your top security card, all of your opponent's Digimon get -5000 DP for the turn.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region DP minus single target
                    Permanent selectedPermament = null;

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, CanSelectPermanentCondition));
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to -8K DP", "The opponent is selecting 1 Digimon to -8K DP");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (selectedPermament != null) yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.ChangeDigimonDP(selectedPermament, -8000, EffectDuration.UntilEachTurnEnd, activateClass));
                    }
                    #endregion

                    #region DP minus enemy board
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Will you trash your top security card to give all enemy Digimon -5K DP for turn?";
                        string notSelectPlayerMessage = "The opponent is choosing whether or not to trash their top security card to give all your Digimon -5K DP for turn.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool trash = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (trash)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                player: card.Owner,
                                destroySecurityCount: 1,
                                cardEffect: activateClass,
                                fromTop: true).DestroySecurity());

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDPPlayerEffect(
                                permanentCondition: CanSelectPermanentCondition,
                                changeValue: -5000,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                        }
                    }
                    #endregion
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion
            #endregion

            return cardEffects;
        }
    }
}
