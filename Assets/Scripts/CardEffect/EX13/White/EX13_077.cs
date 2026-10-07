using System;
using System.Collections;
using System.Collections.Generic;

// Omnimon: Merciful Mode
namespace DCGO.CardEffects.EX13
{
    public class EX13_077 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            bool IsOwnerDigimonOrTamer(Permanent permanent)
                => permanent.IsDigimon || permanent.IsTamer;

            int OwnerColorCount()
                => CardEffectCommons.GetUniqueColourCountOnOwnerBattleArea(card, IsOwnerDigimonOrTamer);

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Omnimon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName = "1 Digimon may attack without suspending, then per 2 colors, may battle or return 5 from opponent's trash to <Recovery +1>";

            string SharedEffectDescription(string tag)
                => $"[{tag}] 1 of your Digimon may attack without suspending. Then, for every 2 of your Digimon and Tamers' colors, activate 1 effect below:\r\n・This Digimon may battle 1 of your opponent's Digimon.\r\n・By returning 5 cards from your opponent's trash to the bottom of the deck, <Recovery +1>";

            bool CanSelectBattleTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.HasDP;

            bool IsOpponentTrashCard(CardSource cardSource)
                => cardSource != null && cardSource.Owner == card.Owner.Enemy;

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region 1 of your Digimon may attack without suspending
                bool CanSelectAttackerCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.CanAttack(activateClass, true);

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectAttackerCondition))
                {
                    Permanent selectedAttacker = null;

                    SelectPermanentEffect selectAttackerEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectAttackerEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectAttackerCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectAttackerCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectAttackerEffect.SetUpCustomMessage("Select 1 Digimon that will attack without suspending.", "The opponent is selecting 1 Digimon that will attack without suspending.");

                    yield return ContinuousController.instance.StartCoroutine(selectAttackerEffect.Activate());

                    IEnumerator SelectAttackerCoroutine(Permanent permanent)
                    {
                        selectedAttacker = permanent;

                        yield return null;
                    }

                    if (selectedAttacker != null && selectedAttacker.CanAttack(activateClass, true))
                    {
                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: selectedAttacker,
                            canAttackPlayerCondition: () => true,
                            defenderCondition: (permanent) => true,
                            cardEffect: activateClass);

                        selectAttackEffect.SetWithoutTap();

                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }
                }
                #endregion

                #region For every 2 colors, activate 1 effect
                int effectLoops = OwnerColorCount() / 2;

                while (effectLoops > 0)
                {
                    bool canBattle = CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectBattleTargetCondition);
                    bool canRecover = CardEffectCommons.MatchConditionOpponentsCardCountInTrash(card, IsOpponentTrashCard) >= 5;

                    if (!canBattle && !canRecover) break;

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                    if (canBattle)
                    {
                        selectionElements.Add(new(message: "Battle 1 of your opponent's Digimon", value: 1, spriteIndex: 0));
                    }

                    if (canRecover)
                    {
                        selectionElements.Add(new(message: "Return 5 cards from opponent's trash, Recovery +1", value: 2, spriteIndex: 0));
                    }

                    selectionElements.Add(new(message: "Don't activate", value: 3, spriteIndex: 1));

                    GManager.instance.userSelectionManager.SetIntSelection(
                        selectionElements: selectionElements,
                        selectPlayer: card.Owner,
                        selectPlayerMessage: $"Which effect will you activate? ({effectLoops} remaining)",
                        notSelectPlayerMessage: "The opponent is choosing which effect to activate.");

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    int selectedValue = GManager.instance.userSelectionManager.SelectedIntValue;

                    if (selectedValue == 3) break;

                    if (selectedValue == 1)
                    {
                        SelectPermanentEffect selectBattleTargetEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectBattleTargetEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectBattleTargetCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectBattleTargetCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectBattleTargetEffect.SetUpCustomMessage("Select 1 Digimon to battle.", "The opponent is selecting 1 Digimon to battle.");

                        yield return ContinuousController.instance.StartCoroutine(selectBattleTargetEffect.Activate());

                        IEnumerator SelectBattleTargetCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IBattle(card.PermanentOfThisCard(), permanent, null, true).Battle());
                        }
                    }
                    else
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: IsOpponentTrashCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Select 5 cards in opponent's trash to place at the bottom of the deck.",
                            maxCount: 5,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: card.Owner.Enemy.TrashCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetNotShowCard();
                        selectCardEffect.SetNotAddLog();

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count == 5)
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    CardObjectController.AddLibraryBottomCards(cardSources, cardEffect: activateClass));

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                    .ShowCardEffect(cardSources, "Deck Bottom Card", true, true));

                                yield return ContinuousController.instance.StartCoroutine(new IRecovery(
                                    player: card.Owner,
                                    AddLifeCount: 1,
                                    cardEffect: activateClass).Recovery());
                            }
                        }
                    }

                    effectLoops--;
                }
                #endregion
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            #endregion

            #region All Turns - Gains all colors in its digivolution cards
            if (timing == EffectTiming.None)
            {
                ChangeCardColorClass changeCardColorClass = new ChangeCardColorClass();
                changeCardColorClass.SetUpICardEffect("Also treated as digivolution cards' colors", CanUseCondition, card);
                changeCardColorClass.SetUpChangeCardColorClass(ChangeCardColors: ChangeCardColors);
                cardEffects.Add(changeCardColorClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.PermanentOfThisCard().TopCard == card
                        && card.PermanentOfThisCard().DigivolutionCards.Count >= 1;

                List<CardColor> ChangeCardColors(CardSource cardSource, List<CardColor> cardColors)
                {
                    if (cardSource == card && CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        foreach (CardSource digivolutionCard in card.PermanentOfThisCard().DigivolutionCards)
                        {
                            if (digivolutionCard.IsFlipped) continue;

                            foreach (CardColor cardColor in digivolutionCard.CardColors)
                            {
                                if (!cardColors.Contains(cardColor))
                                {
                                    cardColors.Add(cardColor);
                                }
                            }
                        }
                    }

                    return cardColors;
                }
            }
            #endregion

            #region All Turns - +1000 DP per color
            if (timing == EffectTiming.None)
            {
                Func<int> changeValue = () => 1000 * OwnerColorCount();

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: changeValue,
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
