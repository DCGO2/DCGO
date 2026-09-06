using System.Collections;
using System.Collections.Generic;

// Mervamon
namespace DCGO.CardEffects.BT26
{
    public class BT26_081 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement Minervamon
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Minervamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Alternate Digivolution Requirement TS
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Shared On Play / When Digivolving
            string SharedEffectName() => "Play up to 8 cost worth of [Iliad] cards, then -4000 DP per [Iliad]/[TS] Digimon or Tamer";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may play up to 8 play cost's total worth of [Iliad] trait cards from your hand or trash without paying the cost. Then, to 1 of your opponent's Digimon, give -4000 DP until their turn ends for each of your [Iliad] or [TS] trait Digimon or Tamers.";

            bool CanSelectDPTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool IliadOrTSPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer)
                    && (permanent.TopCard.EqualsTraits("Iliad") || permanent.TopCard.HasTSTraits);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                int totalCost = 8;

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("Iliad")
                        && cardSource.HasPlayCost
                        && cardSource.GetCostItself <= totalCost
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

                List<CardSource> selectedCards = new List<CardSource>();

                bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                {
                    int sumCost = cardSource.GetCostItself;
                    foreach (CardSource cardSource1 in cardSources) sumCost += cardSource1.GetCostItself;
                    return sumCost <= totalCost;
                }

                bool CanEndSelectCardCondition(List<CardSource> cards)
                {
                    int sumCost = 0;
                    foreach (CardSource source in cards) sumCost += source.GetCostItself;
                    return sumCost <= totalCost;
                }

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCards.Add(cardSource);

                    totalCost -= cardSource.GetCostItself;

                    yield return null;
                }

                bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                while (canSelectHand|| canSelectTrash)
                {
                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<int>> selectionElements1 = new List<SelectionElement<int>>()
                    {
                        new (message: $"From hand", value : 1, spriteIndex: 0),
                        new (message: $"From trash", value : 2, spriteIndex: 0),
                        new (message: $"Don't select", value: 3, spriteIndex: 1)
                    };

                        string selectPlayerMessage1 = "From which area will you select a card?";
                        string notSelectPlayerMessage1 = "The opponent is choosing from which area to select a card.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);
                    }
                    else
                    {
                        GManager.instance.userSelectionManager.SetInt(canSelectHand ? 1 : 2);
                    }
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    SelectCardEffect.Root root = GManager.instance.userSelectionManager.SelectedIntValue == 1 ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                    bool doSelect = GManager.instance.userSelectionManager.SelectedIntValue != 3;

                    if (doSelect)
                    {
                        if (GManager.instance.userSelectionManager.SelectedIntValue == 1)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                canEndSelectCondition: CanEndSelectCardCondition,
                                maxCount: -1,
                                canNoSelect: true,
                                canEndNotMax: true,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage($"Select up to {totalCost} play cost worth of [Iliad] trait cards to play from your hand.", $"The opponent is selecting up to {totalCost} play cost worth of [Iliad] trait cards to play from their hand.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Selected cards");
                            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                        }
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                canEndSelectCondition: CanEndSelectCardCondition,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: $"Select up to {totalCost} play cost worth of [Iliad] trait cards to play from your trash.",
                                maxCount: -1,
                                canEndNotMax: true,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage($"Select up to {totalCost} play cost worth of [Iliad] trait cards to play from your trash.", $"The opponent is selecting up to {totalCost} play cost worth of [Iliad] trait cards to play from their trash.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Selected cards");
                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }
                    }
                }

                if (selectedCards.Count >= 1)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: selectedCards,
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Hand,
                        activateETB: true));
                }

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDPTargetCondition))
                {
                    int countIliadOrTS = card.Owner.GetFieldPermanents().Filter(IliadOrTSPermanentCondition).Count;
                    int dpChange = 4000 * countIliadOrTS;

                    if (dpChange != 0)
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDPTargetCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage($"Select 1 Digimon to give -{dpChange} DP.", $"The opponent is selecting 1 Digimon to give -{dpChange} DP to.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: selectedPermanent,
                                changeValue: -dpChange,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));
                        }
                    }
                }
            }
            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            #region All Turns - Grant to Iliad Digimon
            if (timing == EffectTiming.None)
            {
                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("All of your [Iliad] trait Digimon gain <Alliance>, <Reboot>, <Blocker> and +2K DP", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                cardEffects.Add(addSkillClass);

                bool GrantCondition() => CardEffectCommons.IsExistOnBattleArea(card);

                bool GrantPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.EqualsTraits("Iliad");

                bool CanUseCondition(Hashtable hashtable) => GrantCondition();
              
                bool CardSourceCondition(CardSource cardSource)
                    => cardSource.PermanentOfThisCard() != null
                        && GrantPermanentCondition(cardSource.PermanentOfThisCard())
                        && cardSource == cardSource.PermanentOfThisCard().TopCard;

                bool IsOwnIliadDigimon(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.EqualsTraits("Iliad");

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    #region Alliance
                    if (timing == EffectTiming.OnAllyAttack)
                    {
                        cardEffects.Add(CardEffectFactory.AllianceStaticEffect(IsOwnIliadDigimon, false, card, GrantCondition));
                    }
                    #endregion

                    #region Reboot
                    if (timing == EffectTiming.None)
                    {
                        cardEffects.Add(CardEffectFactory.RebootStaticEffect(IsOwnIliadDigimon, false, card, GrantCondition));
                    }
                    #endregion

                    #region Blocker
                    if (timing == EffectTiming.None)
                    {
                        cardEffects.Add(CardEffectFactory.BlockerStaticEffect(IsOwnIliadDigimon, false, card, GrantCondition));
                    }
                    #endregion

                    #region DP +2000
                    if (timing == EffectTiming.None)
                    {
                        cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(IsOwnIliadDigimon, 2000, false, card, GrantCondition, effectName: () => "All of your [Iliad] trait Digimon get +2000 DP."));
                    }
                    #endregion

                    return cardEffects;
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

                bool CanUseCondition(Hashtable hashtable) => true;

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition);

                        bool CanSelectCardCondition(CardSource cs)
                            => cs.EqualsCardName("Minervamon");

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: null,
                            selectMessage: "[Minervamon]",
                            elementCount: 1,
                            reduceCost: 5);

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
