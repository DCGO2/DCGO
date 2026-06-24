using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Chaosdramon
namespace DCGO.CardEffects.EX12
{
    public class EX12_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DNA Digivolution
            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect("DNA Digivolution", CanUseCondition, card);
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
                                && (permanent.TopCard.CardColors.Contains(CardColor.Red)
                                    || permanent.TopCard.CardColors.Contains(CardColor.Black))
                                && permanent.Levels_ForJogress(card).Contains(6);
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && (permanent.TopCard.CardColors.Contains(CardColor.Purple)
                                    || permanent.TopCard.CardColors.Contains(CardColor.Yellow))
                                && permanent.Levels_ForJogress(card).Contains(6);
                        }

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 6 Red or Black Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 6 Purple or Yellow Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Security A. +1
            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            #endregion

            #region Fragment
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                string EffectDescription()
                {
                    return "<Fragment <2>> (When this Digimon would be deleted, by trashing any 2 of its digivolution cards, it isn’t deleted.)";
                }

                cardEffects.Add(CardEffectFactory.FragmentSelfEffect(isInheritedEffect: false, card: card, condition: null, trashValue: 2, effectName: "Fragment <2>", effectDiscription: EffectDescription()));
            }
            #endregion

            #region Engage
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.EngageSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD/WA
            string SharedEffectName = "<De-Digivolve 2> all enemy Digimon, then may place 2 lvl 5 or lower [Machine]/[Cyborg]/[ME] from hand/trash under to delete 2 enemy Digimon with play cost equal or lower to source count";

            string SharedEffectHash = "EX12_060_OP_WD_WA";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: SharedEffectHash,
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] [Once Per Turn] <De-Digivolve 2> all of your opponent's Digimon. Then, by placing 2 level 5 or lower [Machine], [Cyborg] or [ME] trait cards from your hand or trash as this Digimon's bottom digivolution cards, delete 2 of your opponent's Digimon as high or lower a play cost as the number of this Digimon's digivolution cards.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.HasLevel
                    && cardSource.Level <= 5
                    && (cardSource.EqualsTraits("Machine")
                        || cardSource.EqualsTraits("Cyborg")
                        || cardSource.EqualsTraits("ME"));
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.HasPlayCost
                    && permanent.TopCard.GetCostItself <= card.PermanentOfThisCard().DigivolutionCards.Count;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(new IMassDegeneration(card.Owner.Enemy.GetBattleAreaDigimons(), 2, activateClass).Degeneration());

                if (card.Owner.HandCards.Count(CanSelectCardCondition) + card.Owner.TrashCards.Count(CanSelectCardCondition) >= 2)
                {
                    int toSelect = 2;
                    List<CardSource> selectedCards = new List<CardSource>();

                    while (toSelect > 0)
                    {
                        bool CanSelectFilteredCardCondtion(CardSource cardSource)
                        {
                            return CanSelectCardCondition(cardSource)
                                && !selectedCards.Contains(cardSource);
                        }

                        int validHandCardCount = card.Owner.HandCards.Count(CanSelectFilteredCardCondtion);
                        int validTrashCardCount = card.Owner.TrashCards.Count(CanSelectFilteredCardCondtion);

                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                        if (validHandCardCount > 0)
                        {
                            selectionElements.Add(new(message: "from Hand", value: 1, spriteIndex: 0));
                        }
                        if (validTrashCardCount > 0)
                        {
                            selectionElements.Add(new(message: "from Trash", value: 2, spriteIndex: 0));
                        }
                        selectionElements.Add(new(message: "Do not place", value: 3, spriteIndex: 1));

                        GManager.instance.userSelectionManager.SetIntSelection(
                            selectionElements: selectionElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "From which area will you select a card?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to select card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 3)
                        {
                            break;
                        }
                        if (GManager.instance.userSelectionManager.SelectedIntValue == 1)
                        {
                            int maxCount = Math.Min(toSelect, card.Owner.HandCards.Count(CanSelectCardCondition));
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectFilteredCardCondtion,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: true,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            string messagePluralize = maxCount > 1 ? "Select one or more cards to place under this Digimon. You will be able to select a second card if you only choose 1." : "Select a card to place under this Digimon.";

                            selectHandEffect.SetUpCustomMessage(
                                messagePluralize,
                                $"The opponent is selecting cards to place.");

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }
                        else
                        {
                            int maxCount = Math.Min(toSelect, card.Owner.TrashCards.Count(CanSelectCardCondition));
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectFilteredCardCondtion,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select card(s) to place",
                                maxCount: maxCount,
                                canEndNotMax: true,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            string messagePluralize = maxCount > 1 ? "Select one or more cards to place under this Digimon. You will be able to select a second card if you only choose 1." : "Select a card to place under this Digimon.";

                            selectCardEffect.SetUpCustomMessage(
                                messagePluralize,
                                $"The opponent is selecting cards to place.");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                selectedCards.Add(cardSource);
                                toSelect--;
                            }

                            yield return null;
                        }
                    }

                    if (selectedCards.Count == 2)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

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
            }
            #endregion

            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect("Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition);

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.HasLevel
                                && cardSource.Level <= 6
                                && (cardSource.EqualsTraits("Machine")
                                    || cardSource.EqualsTraits("Cyborg")
                                    || cardSource.EqualsTraits("ME");
                        }

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            List<CardSource> AllCards = cardSources.Clone();

                            AllCards.Add(cardSource);

                            return AllCards.Count == Combinations.GetUniqueNameCardCount(AllCards);
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            selectMessage: "6 Lv.6 or lower [Machine]/[Cyborg]/[ME] trait cards w/different names",
                            elementCount: 6,
                            reduceCost: 8);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
