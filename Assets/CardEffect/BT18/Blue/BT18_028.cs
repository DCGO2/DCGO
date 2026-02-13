using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT18
{
    public class BT18_028 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DigiXros

            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros -2", CanUseCondition, card);
                addDigiXrosConditionClass.SetUpAddDigiXrosConditionClass(getDigiXrosCondition: GetDigiXros);
                addDigiXrosConditionClass.SetNotShowUI(true);
                cardEffects.Add(addDigiXrosConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                DigiXrosCondition GetDigiXros(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        DigiXrosConditionElement elementHuman =
                            new DigiXrosConditionElement(CanSelectCardCondition, "Kumamon");

                        bool CanSelectCardCondition(CardSource conditionCardSource)
                        {
                            if (conditionCardSource != null)
                            {
                                if (conditionCardSource.Owner == card.Owner)
                                {
                                    if (conditionCardSource.IsDigimon)
                                    {
                                        if (conditionCardSource.CardNames_DigiXros.Contains("Kumamon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        DigiXrosConditionElement elementBeast =
                            new DigiXrosConditionElement(CanSelectCardCondition1, "Korikakumon");

                        bool CanSelectCardCondition1(CardSource conditionCardSource)
                        {
                            if (conditionCardSource != null)
                            {
                                if (conditionCardSource.Owner == card.Owner)
                                {
                                    if (conditionCardSource.IsDigimon)
                                    {
                                        if (conditionCardSource.CardNames_DigiXros.Contains("Korikakumon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>()
                            { elementHuman, elementBeast };

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 2);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region On Play/ When Digivolving Shared

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 2 bottom sources from all opponent's Digimon. Their sourceless digimon can't suspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] Trash the bottom 2 digivolution cards of all of your opponent's Digimon. None of their Digimon with no digivolution cards can suspend until the end of their turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    foreach (Permanent selectedPermanent in card.Owner.Enemy.GetBattleAreaDigimons())
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                            targetPermanent: selectedPermanent,
                            trashCount: 2,
                            isFromTop: false,
                            activateClass: activateClass));    
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotSuspendPlayerEffect(
                        permanentCondition: PermanentCanNotSuspendCondition,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass,
                        isOnlyActivePhase: false,
                        effectName: "Can't Suspend"));

                    bool PermanentCanNotSuspendCondition(Permanent permanentCanNotSuspend)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanentCanNotSuspend,card) &&
                               permanentCanNotSuspend.HasNoDigivolutionCards;
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 2 bottom digivolution cards of all your opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] Trash the bottom 2 digivolution cards of all of your opponent's Digimon. None of their Digimon with no digivolution cards can suspend until the end of their turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    foreach (Permanent selectedPermanent in card.Owner.Enemy.GetBattleAreaDigimons())
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                            targetPermanent: selectedPermanent,
                            trashCount: 2,
                            isFromTop: false,
                            activateClass: activateClass));
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotSuspendPlayerEffect(
                        permanentCondition: PermanentCanNotSuspendCondition,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass,
                        isOnlyActivePhase: false,
                        effectName: "Can't Suspend"));

                    bool PermanentCanNotSuspendCondition(Permanent permanentCanNotSuspend)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanentCanNotSuspend, card) &&
                               !permanentCanNotSuspend.HasNoDigivolutionCards;
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play a Digimon from digivolution cards.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] When this Digimon would leave the battle area, you may play 1 level 4 or lower Digimon card with the [Mammal]/[Ice-Snow]/[Hybrid] trait from this Digimon's digivolution cards without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.HasLevel && cardSource.Level <= 4 &&
                           (cardSource.ContainsTraits("Mammal") || cardSource.ContainsTraits("Ice-Snow") ||
                            cardSource.ContainsTraits("Hybrid")) &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().DigivolutionCards.Some(CanSelectCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 digivolution card to play.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Custom,
                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage(
                        "Select 1 digivolution card to play.",
                        "The opponent is selecting 1 digivolution card to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            activateETB: true));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}