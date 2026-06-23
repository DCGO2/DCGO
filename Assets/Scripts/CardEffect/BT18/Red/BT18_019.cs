using System;
using System.Collections;
using System.Collections.Generic;

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
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && permanent.TopCard.CardNames.Contains("Kimeramon");
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && permanent.TopCard.CardNames.Contains("Machinedramon");
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

            #region OP/WD Shared
            #region Different Levels Shared
            List<Func<CardSource, bool>> Levels = new List<Func<CardSource, bool>> { Level3Selection, Level4Selection, Level5Selection, Level6Selection, Level7Selection };
            bool Level3Selection(CardSource cardSource)
            {
                return cardSource.IsDigimon
                    && cardSource.IsLevel3;
            }

            bool Level4Selection(CardSource cardSource)
            {
                return cardSource.IsDigimon
                    && cardSource.IsLevel4;
            }

            bool Level5Selection(CardSource cardSource)
            {
                return cardSource.IsDigimon
                    && cardSource.IsLevel5;
            }

            bool Level6Selection(CardSource cardSource)
            {
                return cardSource.IsDigimon
                    && cardSource.IsLevel6;
            }

            bool Level7Selection(CardSource cardSource)
            {
                return cardSource.IsDigimon
                    && cardSource.HasLevel
                    && cardSource.Level == 7;
            }
            #endregion

            string SharedEffectName = "Delete 1 enemy Digimon, then if DNA top deck 1 of all trash levels to gain 1 mem per.";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] Delete 1 of your opponent's Digimon. Then, if DNA digivolving, by returning 1 of each Digimon card with different levels from your opponent's trash to the top of the deck, for each card returned, gain 1 memory.";

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable _hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
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
                        bool exitLoop = false;

                        if (CardEffectCommons.HasMatchConditionOpponentsCardInTrash(card, level))
                        {
                            bool canSelectNo = selectedCards == null;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: level,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => canSelectNo,
                                selectCardCoroutine: LevelSelected,
                                afterSelectCardCoroutine: null,
                                message: $"Select level {Levels.IndexOf(level) + 3} Digimon card to add to the top of opponent's deck",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: card.Owner.Enemy.TrashCards.Filter(level),
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectCardEffect.Activate());
                        }

                        IEnumerator LevelSelected(CardSource cardSource)
                        {
                            if (cardSource != null)
                                selectedCards.Add(cardSource);
                            else
                                exitLoop = true;

                            yield return null;
                        }

                        if (exitLoop)
                            break;
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
            #endregion

            #region DigiXros
            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect("DigiXros -2", CanUseCondition, card);
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
                            return conditionCardSource != null
                                && conditionCardSource.Owner == card.Owner
                                && conditionCardSource.IsDigimon
                                && conditionCardSource.CardNames_DigiXros.Contains("Kimeramon");
                        }

                        DigiXrosConditionElement elementMachinedramon =
                            new DigiXrosConditionElement(CanSelectCardCondition1, "Machinedramon");

                        bool CanSelectCardCondition1(CardSource conditionCardSource)
                        {
                            return conditionCardSource != null
                                && conditionCardSource.Owner == card.Owner
                                && conditionCardSource.IsDigimon
                                && conditionCardSource.CardNames_DigiXros.Contains("Machinedramon");
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
