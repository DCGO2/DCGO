using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// Mother Eater
namespace DCGO.CardEffects.BT22
{
    public class BT22_007 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add [Mother Eater]s to top of stack. if 10 or more in digivolution source, play 3 [Mother Eater]s", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Breeding] [Start of Your Main Phase] Look at your Digi-Egg deck's top card. Among them, you may place [Mother Eater]s as this Digimon's top digivolution cards. Then, if this Digimon has 10 or more digivolution cards, you may play 3 [Mother Eater]s from its digivolution cards without paying the costs.";
                }

                bool CanSelectMother(CardSource source)
                {
                    return source.EqualsCardName("Mother Eater") &&
                           CardEffectCommons.CanPlayAsNewPermanent(source, false, activateClass, SelectCardEffect.Root.DigivolutionCards);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBreedingAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.DigitamaLibraryCards.Count >= 1)
                    {
                        CardSource topEggCard = card.Owner.DigitamaLibraryCards[0];
                        bool AddToBreedingArea = false;

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { topEggCard }, "Revealed Cards", true, true));
                        if (topEggCard.EqualsCardName("Mother Eater"))
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                                new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                            };

                            string selectPlayerMessage = "Place as top digivolution card?";
                            string notSelectPlayerMessage = "The opponent is choosing to place as top digivolution card";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            AddToBreedingArea = GManager.instance.userSelectionManager.SelectedBoolValue;
                        }

                        if (AddToBreedingArea)
                        {
                            Permanent thisPermanent = card.PermanentOfThisCard();
                            yield return ContinuousController.instance.StartCoroutine(thisPermanent.AddDigivolutionCardsTop(new List<CardSource>() { topEggCard }, activateClass));
                        }
                    }

                    if (card.PermanentOfThisCard().DigivolutionCards.Count >= 10)
                    {
                        Permanent thisPermanent = card.PermanentOfThisCard();

                        if (thisPermanent.DigivolutionCards.Exists(CanSelectMother))
                        {
                            int motherCount = thisPermanent.DigivolutionCards.Count(CanSelectMother);
                            if (card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame()) >= motherCount)
                            {
                                int maxCount = Math.Min(3, motherCount);
                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectMother,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: SelectCardCoroutine,
                                    message: "Select [Mother Eater]'s to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: thisPermanent.DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage(
                                    "Select [Mother Eater]'s to play.",
                                    "The opponent is selecting [Mother Eater]'s to play.");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                IEnumerator SelectCardCoroutine(List<CardSource> cardSources)
                                {
                                    if (cardSources.Count >= 1)
                                    {
                                        foreach (CardSource source in cardSources)
                                        {
                                            source.SetDP(16000);
                                        }

                                        yield return ContinuousController.instance.StartCoroutine(
                                            CardEffectCommons.PlayPermanentCards(
                                            cardSources: cardSources,
                                            activateClass: activateClass,
                                            payCost: false,
                                            isTapped: false,
                                            root: SelectCardEffect.Root.DigivolutionCards,
                                            activateETB: true));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Delete 1 of your opponent's Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectCondition);
                }

                bool CanSelectCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            #region All Turns - Remove Field

            if (timing == EffectTiming.WhenRemoveField)
            {
                List<Permanent> removedPermanents = new List<Permanent>();

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place card as source", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT22_007_Save");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Breeding] [All Turns] [Once Per Turn] When any of your [Eater] trait Digimon would leave the battle area other than by your effects, you may place them as this Digimon's bottom digivolution cards.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, IsEaterDigimon)
                        && !CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    removedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(IsEaterDigimon);

                    return CardEffectCommons.IsExistOnBreedingArea(card);
                }

                bool IsEaterDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasEaterTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    foreach (Permanent permanent in removedPermanents)
                    {
                        #region Remove Events from Permanent

                        permanent.HideDeleteEffect();
                        permanent.HideHandBounceEffect();
                        permanent.HideDeckBounceEffect();
                        permanent.HideWillRemoveFieldEffect();

                        permanent.DestroyingEffect = null;
                        permanent.IsDestroyedByBattle = false;
                        permanent.HandBounceEffect = null;
                        permanent.LibraryBounceEffect = null;
                        permanent.willBeRemoveField = false;

                        #endregion

                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(new List<CardSource>() { permanent.TopCard }, activateClass));
                    }
                }
            }

            #endregion

            #region Remove Field - Reset DP

            if (timing == EffectTiming.OnRemovedField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, "");
                activateClass.SetIsBackgroundProcess(true);
                cardEffects.Add(activateClass);

                bool IsEaterDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && permanent.TopCard.EqualsCardName("Mother Eater");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> permanets = new List<Permanent>();
                    if (CardEffectCommons.IsPermanentExistsOnBreedingArea(card.PermanentOfThisCard()))
                    {
                        permanets = card.Owner.GetBattleAreaDigimons()
                            .Filter(IsEaterDigimon)
                            .ToList();
                    }
                    else
                    {
                        permanets.Add(card.PermanentOfThisCard());
                    }

                    foreach (Permanent permanent in permanets)
                    {
                        #region Remove Events from Permanent

                        permanent.TopCard.SetDP(0);

                        #endregion
                    }

                    yield return null;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}