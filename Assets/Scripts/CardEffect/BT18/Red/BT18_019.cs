using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT18
{
    public class BT18_019 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DNA Digivolution

            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool PermanentCondition1(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                            {
                                if (permanent.TopCard.CardNames.Contains("Kimeramon"))
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                            {
                                if (permanent.TopCard.CardNames.Contains("Machinedramon"))
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "Kimeramon"),

                        new JogressConditionElement(PermanentCondition2, "Machinedramon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region Different Levels Shared
            List <Func <CardSource, bool>> Levels = new List<Func<CardSource, bool>> { Level3Selection, Level4Selection, Level5Selection, Level6Selection, Level7Selection };
            bool Level3Selection(CardSource source)
            {
                return source.IsDigimon &&
                       source.HasLevel && source.Level == 3;
            }

            bool Level4Selection(CardSource source)
            {
                return source.IsDigimon &&
                       source.HasLevel && source.Level == 4;
            }

            bool Level5Selection(CardSource source)
            {
                return source.IsDigimon &&
                       source.HasLevel && source.Level == 5;
            }

            bool Level6Selection(CardSource source)
            {
                return source.IsDigimon &&
                       source.HasLevel && source.Level == 6;
            }

            bool Level7Selection(CardSource source)
            {
                return source.IsDigimon &&
                       source.HasLevel && source.Level == 7;
            }
            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Delete 1 of your opponent's Digimon. Then, if DNA digivolving, by returning 1 of each Digimon card with different levels from your opponent's trash to the top of the deck, for each card returned, gain 1 memory.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

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
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if (CardEffectCommons.IsJogress(_hashtable))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        foreach (Func<CardSource, bool> level in Levels)
                        {
                            if (CardEffectCommons.HasMatchConditionOpponentsCardInTrash(card, level))
                            {
                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: level,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: LevelSelected,
                                    afterSelectCardCoroutine: null,
                                    message: $"Select level {Levels.IndexOf(level) + 3} Digimon card to add to the top of opponent's deck",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: card.Owner.Enemy.TrashCards.Filter(level),
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                yield return StartCoroutine(selectCardEffect.Activate());
                            }
                        }

                        IEnumerator LevelSelected(CardSource source)
                        {
                            if (source != null)
                                selectedCards.Add(source);

                            yield return null;
                        }

                        if (selectedCards.Count >= 1)
                        {

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: (CardSource) => true,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelection,
                                message: "Select order of cards to add to the top your deck\n(cards will be placed back to the top of the deck so that cards with lower numbers are on top).",
                                maxCount: selectedCards.Count,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: selectedCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectCardEffect.Activate());

                            IEnumerator AfterSelection(List<CardSource> sources)
                            {
                                sources.Reverse();

                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(sources));

                                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1 * sources.Count, activateClass));
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

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return CardEffectCommons.CanTriggerOnPlay(hashtable, card);

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                    {
                        int maxCount = Math.Min(1, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

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
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play a [Milleniummon] from the trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] By returning 1 [Kimeramon] and 1 [Machinedramon] from your trash to the bottom of the deck, you may play 1 [Millenniummon] from your trash without paying the cost.";
                }

                bool CanSelectCardConditionKimeramon(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.EqualsCardName("Kimeramon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardConditionMachinedramon(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.EqualsCardName("Machinedramon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardConditionMilleniummon(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass, root: SelectCardEffect.Root.Trash))
                    {
                        if (cardSource.EqualsCardName("Millenniummon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletion(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardConditionKimeramon) || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardConditionMachinedramon))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool returned = false;

                    int maxCount = 2;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                    canTargetCondition: (cardSource) => CanSelectCardConditionKimeramon(cardSource) || CanSelectCardConditionMachinedramon(cardSource),
                    canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                    canEndSelectCondition: CanEndSelectCondition,
                    canNoSelect: () => true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                    message: "Select cards to place at the bottom of the deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).",
                    maxCount: maxCount,
                    canEndNotMax: false,
                    isShowOpponent: false,
                    mode: SelectCardEffect.Mode.Custom,
                    root: SelectCardEffect.Root.Trash,
                    customRootCardList: null,
                    canLookReverseCard: true,
                    selectPlayer: card.Owner,
                    cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                    {
                        if (cardSources.Count(CanSelectCardConditionKimeramon) >= 1)
                        {
                            if (CanSelectCardConditionKimeramon(cardSource))
                            {
                                if (!CanSelectCardConditionMachinedramon(cardSource))
                                {
                                    return false;
                                }
                            }
                        }

                        if (cardSources.Count(CanSelectCardConditionMachinedramon) >= 1)
                        {
                            if (CanSelectCardConditionMachinedramon(cardSource))
                            {
                                if (!CanSelectCardConditionKimeramon(cardSource))
                                {
                                    return false;
                                }
                            }
                        }

                        return true;
                    }

                    bool CanEndSelectCondition(List<CardSource> cardSources)
                    {
                        if (cardSources.Count(CanSelectCardConditionKimeramon) == 0)
                        {
                            return false;
                        }

                        if (cardSources.Count(CanSelectCardConditionMachinedramon) == 0)
                        {
                            return false;
                        }

                        return true;
                    }

                    IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                    {
                        if (cardSources.Count == 2)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(cardSources));

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Bottom Cards", true, true));

                            returned = true;
                        }
                    }

                    if (returned)
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardConditionMilleniummon))
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            SelectCardEffect selectPlayEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectPlayEffect.SetUp(
                                        canTargetCondition: CanSelectCardConditionMilleniummon,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 card to play.",
                                        maxCount: 1,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            selectPlayEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                            selectPlayEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return StartCoroutine(selectPlayEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Trash, activateETB: true));
                        }
                    }
                }
            }

            #endregion

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
                        DigiXrosConditionElement elementKimeramon =
                            new DigiXrosConditionElement(CanSelectCardCondition, "Kimeramon");

                        bool CanSelectCardCondition(CardSource conditionCardSource)
                        {
                            if (conditionCardSource != null)
                            {
                                if (conditionCardSource.Owner == card.Owner)
                                {
                                    if (conditionCardSource.IsDigimon)
                                    {
                                        if (conditionCardSource.CardNames_DigiXros.Contains("Kimeramon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        DigiXrosConditionElement elementMachinedramon =
                            new DigiXrosConditionElement(CanSelectCardCondition1, "Machinedramon");

                        bool CanSelectCardCondition1(CardSource conditionCardSource)
                        {
                            if (conditionCardSource != null)
                            {
                                if (conditionCardSource.Owner == card.Owner)
                                {
                                    if (conditionCardSource.IsDigimon)
                                    {
                                        if (conditionCardSource.CardNames_DigiXros.Contains("Machinedramon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>()
                            { elementKimeramon, elementMachinedramon };

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 2);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
