using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.BT19
{
    public class BT19_094 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Trash
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash opponents top security card or Recovery +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Trash][Your Turn] When any of your Digimon digivolve into [Lucemon (X Antibody)], by returning this card to the bottom of the deck, your opponent may trash their top security card. If this effect didn't trash, <Recovery +1>";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.EqualsCardName("Lucemon (X Antibody)"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnTrash(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition))
                        {
                            if (CardEffectCommons.IsOwnerTurn(card))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnTrash(card))
                    {
                        List<CardSource> cardSources = new List<CardSource>() { card };

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(cardSources));

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(cardSources, "Deck Bottom Card", true, true));

                        if (card.Owner.Enemy.SecurityCards.Count > 0)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Discard", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Not Discard", value : false, spriteIndex: 1),
                        };

                            string selectPlayerMessage = "Will you discard the top card of your security?";
                            string notSelectPlayerMessage = "The opponent is choosing whether to discard security.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner.Enemy, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(false);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool doDiscard = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (!doDiscard)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());                           
                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner.Enemy,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());
                        }
                    }
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Delete your opponent's Digimon until they have as many as the number of your security cards. If this effect deleted, <Recovery +1>.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card) &&
                           card.Owner.Enemy.GetBattleAreaDigimons().Count > card.Owner.SecurityCards.Count;
                }
                
                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Mathf.Max(0, card.Owner.Enemy.GetBattleAreaDigimons().Count - card.Owner.SecurityCards.Count);

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
                            afterSelectPermanentCoroutine: SelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select which Digimon(s) to delete.", "The opponent is selecting which Digimon(s) to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: permanents, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                            IEnumerator SuccessProcess()
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                            }
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Digimon from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] You may play 1 [Lucemon] from your trash without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                if (cardSource.EqualsCardName("Lucemon"))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource)))
                    {
                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 card to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Trash, activateETB: true));
                    }
                }
            }
            #endregion

            return cardEffects;
        } 
    }
}