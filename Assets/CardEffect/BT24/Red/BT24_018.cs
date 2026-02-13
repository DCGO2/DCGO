using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Styracomon
namespace DCGO.CardEffects.BT24
{
    public class BT24_018: CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasOwenDreadnought);
                }

                bool HasOwenDreadnought(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card) &&
                           targetPermanent.TopCard.EqualsCardName("Owen Dreadnought");
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card)
                        && targetPermanent.TopCard.EqualsCardName("Lamiamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 6,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: Condition)
                );
            }
            #endregion

            #region Progress
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ProgressSelfStaticEffect(false, card, null));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(false, card, null));
            }
            #endregion

            #region Armor Purge
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.ArmorPurgeEffect(card));
            }
            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May trash 1 opponent's security. Then, this may unsuspend.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Digivolving] You may trash any 1 of your opponent's security cards. Then, this Digimon may unsuspend.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region Trash Security
                    if (card.Owner.Enemy.SecurityCards.Count >= 1)
                    {
                        int maxCount = 1;
                    
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                    
                        CardSource selectedCard = null;
                    
                        selectCardEffect.SetUp(
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Select 1 card to trash.\n (cards to the left are at the top and cards to the right are at the bottom)",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Security,
                            customRootCardList: card.Owner.Enemy.SecurityCards,
                            canLookReverseCard: false,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);
                    
                        selectCardEffect.SetUpCustomMessage_ShowCard("Send to trash");

                        selectCardEffect.SetUseFaceDown();
                    
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    
                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }
                    
                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                    player: card.Owner.Enemy,
                                    refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
                            }
                        }

                        if (selectedCard != null)
                        {
                            #region
                            selectedCard.Owner.securityObject.securityBreakGlass.ShowBlueMatarial();

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().BreakSecurityEffect(selectedCard.Owner));

                            yield return new WaitForSeconds(0.1f);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().EnterSecurityCardEffect(selectedCard));

                            yield return new WaitForSeconds(0.5f);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().DestroySecurityEffect(selectedCard));
                            #endregion

                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(selectedCard));
                        }
                    }
                    #endregion

                    #region Unsuspend

                    if (card.PermanentOfThisCard().IsSuspended)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Will you unsuspend this digimon?";
                        string notSelectPlayerMessage = "The opponent is choosing if they will unsuspend.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                        var selectedOption = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (selectedOption)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                        new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() },
                            activateClass).Unsuspend());
                        }
                    }

                    #endregion
                }
            }

            #endregion

            #region All Turns when Security is removed, delete 1

            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 of your opponent's Digimon?", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT24_18_AT_Sec_Removed");
                cardEffects.Add(activateClass);

                string EffectDescription()
                 => "[All Turns] [Once Per Turn] When your opponent's security stack is removed from, you may delete 1 of their Digimon.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => player == card.Owner.Enemy);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                        CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to send to delete.", "Your opponent is selecting 1 digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            #region All Turns When Reptile or Dragonkin would leave Battle Area, delete opponent's digimon to prevent
            if (timing == EffectTiming.WhenRemoveField)
            {
                List<Permanent> removedPermanents = new List<Permanent>();

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delte an opponent's Digimon, to prevent [Reptile] or [Dragonkin] trait digimon from leaving the battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT24_018_AT_Prevent_Deletion");
                cardEffects.Add(activateClass);

                string EffectDescription()
                 => "[All Turns] [Once Per Turn] When any of your [Reptile] or [Dragonkin] trait Digimon would leave the battle area, by deleting 1 of your opponent's lowest DP Digimon, they don't leave.";

                bool IsReptileKin(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.EqualsTraits("Reptile") || 
                            permanent.TopCard.EqualsTraits("Dragonkin"));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                        CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, IsReptileKin);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    removedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(IsReptileKin);

                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                        removedPermanents.Count > 0;
                }

                bool CanDeleteCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region By Deleting Lowest Opponent's DP Digimon
                    bool deletedCard = false;
                    if (CardEffectCommons.HasMatchConditionPermanent(CanDeleteCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanDeleteCondition));

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanDeleteCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to send to delete.", "Your opponent is selecting 1 digimon to delete.");

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { permanent }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                            #endregion

                            #region They don't leave
                            IEnumerator SuccessProcess()
                            {
                                foreach (Permanent removed in removedPermanents)
                                {
                                    removed.willBeRemoveField = false;

                                    removed.HideHandBounceEffect();
                                    removed.HideDeckBounceEffect();
                                    removed.HideDeleteEffect(); 
                                    removed.HideWillRemoveFieldEffect();
                                }

                                yield return null;
                            }
                        }
                    }
                    #endregion
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
