using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// King Drasil
namespace DCGO.CardEffects.BT27
{
    public class BT27_104 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 1 hand card under this Tamer, <Draw 1> and gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Start of Your Main Phase] By placing 1 card from your hand under this Tamer, <Draw 1> and gain 1 memory.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.Owner.HandCards.Count >= 1;

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceFromHandOrTrashToDrawAndGainMemory(
                        card: card,
                        cardEffect: activateClass,
                        customMessage: "Select 1 card to place under this Tamer."));
                }
            }
            #endregion

            #region End of Your Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending this Tamer, digivolve into or use a [Royal Knight] card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[End of Your Turn] By suspending this Tamer, activate 1 of the effects below:\n・1 of your Digimon may digivolve into a [Royal Knight] trait card under this Tamer with the digivolution cost reduced by 2.\n・You may use 1 use cost 5 or higher [Royal Knight] trait Option card from your hand or under this Tamer with the use cost reduced by 3.";

                const int DigivolveReduceCost = 2;
                const int UseReduceCost = 3;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);

                bool IsUnderThisTamer(CardSource cardSource)
                    => card.PermanentOfThisCard() != null
                        && card.PermanentOfThisCard().DigivolutionCards.Contains(cardSource);

                bool CanSelectOptionCondition(CardSource cardSource)
                    => cardSource.IsOption
                        && cardSource.HasRoyalKnightTraits
                        && cardSource.BasePlayCostFromEntity >= 5
                        && !cardSource.CanNotPlayThisOption
                        && cardSource.Owner.MaxMemoryCost >= cardSource.GetCostItself - UseReduceCost;

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                    {
                        new(message: "Digivolve into a [Royal Knight] card under this Tamer", value: 1, spriteIndex: 0),
                        new(message: "Use a [Royal Knight] Option card", value: 2, spriteIndex: 0),
                        new(message: "Don't suspend", value: 0, spriteIndex: 1),
                    };

                    GManager.instance.userSelectionManager.SetIntSelection(
                        selectionElements: selectionElements,
                        selectPlayer: card.Owner,
                        selectPlayerMessage: "Which effect will you activate by suspending this Tamer?",
                        notSelectPlayerMessage: "The opponent is choosing which effect to activate.");

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    int selectedEffect = GManager.instance.userSelectionManager.SelectedIntValue;

                    if (selectedEffect == 0) yield break;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<Permanent> _permanents)
                    {
                        if (selectedEffect == 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(DigivolveCoroutine());
                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(UseOptionCoroutine());
                        }
                    }
                }

                #region 1 of your Digimon may digivolve into a [Royal Knight] card under this Tamer for 2 less
                IEnumerator DigivolveCoroutine()
                {
                    Permanent thisTamer = card.PermanentOfThisCard();

                    if (thisTamer == null) yield break;

                    bool OwnDigimonCondition(Permanent permanent)
                        => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                    bool RoyalKnightSourceCondition(CardSource cardSource)
                        => IsUnderThisTamer(cardSource)
                            && cardSource.IsDigimon
                            && cardSource.HasRoyalKnightTraits;

                    // Registered before the selections so their availability checks already see the reduced cost
                    Func<EffectTiming, ICardEffect> getChangeCostEffect = CardEffectCommons.ChangeDigivolutionCostPlayerEffect(
                        permanentCondition: OwnDigimonCondition,
                        cardCondition: RoyalKnightSourceCondition,
                        rootCondition: null,
                        changeValue: -DigivolveReduceCost,
                        setFixedCost: false,
                        activateClass: activateClass);

                    if (getChangeCostEffect != null)
                    {
                        CardEffectCommons.AddEffectToPlayer(
                            effectDuration: EffectDuration.UntilCalculateFixedCost,
                            card: card,
                            cardEffect: null,
                            timing: EffectTiming.None,
                            getCardEffect: getChangeCostEffect);
                    }

                    bool CanDigivolveInto(Permanent permanent, CardSource cardSource)
                        => RoyalKnightSourceCondition(cardSource)
                            && !cardSource.CanNotEvolve(permanent)
                            && cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, true, activateClass, SelectCardEffect.Root.DigivolutionCards);

                    bool CanSelectDigimonCondition(Permanent permanent)
                        => OwnDigimonCondition(permanent)
                            && thisTamer.DigivolutionCards.Any(cardSource => CanDigivolveInto(permanent, cardSource));

                    Permanent selectedDigimon = null;
                    CardSource selectedCard = null;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDigimonCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDigimonCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectDigimonCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to digivolve.", "The opponent is selecting 1 Digimon to digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectDigimonCoroutine(Permanent permanent)
                        {
                            selectedDigimon = permanent;
                            yield return null;
                        }
                    }

                    if (selectedDigimon != null)
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: cardSource => CanDigivolveInto(selectedDigimon, cardSource),
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 [Royal Knight] card to digivolve into.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            customRootCardList: thisTamer.DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 [Royal Knight] card to digivolve into.", "The opponent is selecting 1 card to digivolve into.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }
                    }

                    if (selectedDigimon != null && selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new PlayCardClass(
                            cardSources: new List<CardSource>() { selectedCard },
                            hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                            payCost: true,
                            targetPermanent: selectedDigimon,
                            isTapped: false,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            activateETB: true).PlayCard());
                    }

                    if (getChangeCostEffect != null) card.Owner.UntilCalculateFixedCostEffect.Remove(getChangeCostEffect);
                }
                #endregion

                #region May use 1 use cost 5 or higher [Royal Knight] Option from hand or under this Tamer for 3 less
                IEnumerator UseOptionCoroutine()
                {
                    Permanent thisTamer = card.PermanentOfThisCard();

                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectOptionCondition);
                    bool canSelectSources = thisTamer != null && thisTamer.DigivolutionCards.Any(CanSelectOptionCondition);

                    if (!canSelectHand && !canSelectSources) yield break;

                    bool fromHand = canSelectHand;

                    if (canSelectHand && canSelectSources)
                    {
                        List<SelectionElement<int>> zoneElements = new List<SelectionElement<int>>()
                        {
                            new(message: "From hand", value: 1, spriteIndex: 0),
                            new(message: "From under this Tamer", value: 2, spriteIndex: 0),
                            new(message: "Don't use", value: 3, spriteIndex: 1),
                        };

                        GManager.instance.userSelectionManager.SetIntSelection(
                            selectionElements: zoneElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "From which area will you use an Option card?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to use an Option card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;

                        fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                    }

                    CardSource selectedCard = null;

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (fromHand)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOptionCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 Option card to use.", "The opponent is selecting 1 Option card to use.");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }
                    else
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectOptionCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 Option card to use.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            customRootCardList: thisTamer.DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 Option card to use.", "The opponent is selecting 1 Option card to use.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Used Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    if (selectedCard == null) yield break;

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect($"Use Cost -{UseReduceCost}", _ => true, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: UseCondition, rootCondition: _ => true, isUpDown: () => true, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                    card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                    ICardEffect GetCardEffect(EffectTiming _timing)
                        => _timing == EffectTiming.None ? changeCostClass : null;

                    bool UseCondition(CardSource cardSource) => cardSource == selectedCard;

                    int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    {
                        if (UseCondition(cardSource))
                        {
                            cost -= UseReduceCost;
                        }

                        return cost;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                        cardSources: new List<CardSource>() { selectedCard },
                        activateClass: activateClass,
                        payCost: true,
                        root: fromHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.DigivolutionCards));

                    card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                }
                #endregion
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
