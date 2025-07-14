using System;
using System.Collections;
using System.Collections.Generic;

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
                        List<CardSource> motherEaterCards = card.PermanentOfThisCard().DigivolutionCards.Filter(cardSource => cardSource.EqualsCardName("Mother Eater"));
                        if (motherEaterCards.Count >= 3 && motherEaterCards.TrueForAll(cardSource => CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass)))
                        {
                            bool playDigimon = false;

                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                                new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                            };

                            string selectPlayerMessage = "Place as top digivolution card?";
                            string notSelectPlayerMessage = "The opponent is choosing to place as top digivolution card";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            playDigimon = GManager.instance.userSelectionManager.SelectedBoolValue;

                            if (playDigimon)
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    CardEffectCommons.PlayPermanentCards(
                                    cardSources: motherEaterCards,
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

            #endregion

            #region All Turns

            if (timing == EffectTiming.None)
            {
                string EffectDiscription()
                {
                    return "[Breeding] [All Turns] All of your [Mother Eater]s in the battle area are treated as having 16000 DP.";
                }

                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBreedingAreaDigimon(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.EqualsCardName("Mother Eater"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                permanentCondition: PermanentCondition,
                changeValue: 16000,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                effectName: EffectDiscription));
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

            return cardEffects;
        }
    }
}