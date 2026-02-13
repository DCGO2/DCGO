using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Yuu Amano & Nene Amano
namespace DCGO.CardEffects.EX10
{
    public class EX10_064 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Name Change

            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as [Yuu Amano]", CanUseCondition, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);

                cardEffects.Add(changeCardNamesClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
                {
                    if (cardSource == card)
                    {
                        CardNames.Add("Yuu Amano");
                    }

                    return CardNames;
                }
            }

            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as [Nene Amano]", CanUseCondition, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);

                cardEffects.Add(changeCardNamesClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
                {
                    if (cardSource == card)
                    {
                        CardNames.Add("Nene Amano");
                    }

                    return CardNames;
                }
            }

            #endregion

            #region Start of Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 card under this Tamer from hand or trash to Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By placing 1 [Bagra Army] or [Twilight] trait Digimon card from your hand or trash under this Tamer, <Draw 1>.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasBagraArmyTraits || cardSource.EqualsTraits("Twilight"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition) ||
                           CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition));
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool placed = false;

                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "From which area do you play a card?";
                        string notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                    }
                    else
                    {
                        GManager.instance.userSelectionManager.SetBool(canSelectHand);
                    }

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                    List<CardSource> selectedCards = new List<CardSource>();

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    int maxCount = 1;

                    if (fromHand)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 card to place in Digivolution cards.", "The opponent is selecting 1 card to place in Digivolution cards.");
                        selectHandEffect.SetNotShowCard();

                        yield return StartCoroutine(selectHandEffect.Activate());
                    }
                    else
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: CanEndSelectCondition,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to place in Digivolution cards.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card place in Digivolution cards.", "The opponent is selecting 1 card place in Digivolution cards.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Placed Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    bool CanEndSelectCondition(List<CardSource> cardSources)
                    {
                        if (CardEffectCommons.HasNoElement(cardSources))
                        {
                            return false;
                        }

                        return true;
                    }

                    if (selectedCards.Count >= 1)
                    {
                        if (!card.PermanentOfThisCard().IsToken)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));

                            placed = true;
                        }
                    }

                    if (placed)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Can select DigiXros cards from Tamer's digivolution cards from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("CanSelectDigiXrosFromTamer_EX10_064");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When any of your [Bagra Army] or [Twilight] trait Digimon cards with DigiXros requirements would be played, by suspending this Tamer, 1 card from under your Tamers and 1 card in your trash can also be placed for their DigiXros.";
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource.Owner == card.Owner)
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.HasDigiXros)
                            {
                                if (cardSource.EqualsTraits("Bagra Army") || cardSource.EqualsTraits("Twilight"))
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
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition))
                        {
                            if (CardEffectCommons.IsOnly1CardPlayed(hashtable))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (!card.PermanentOfThisCard().IsSuspended && card.PermanentOfThisCard().CanSuspend)
                        {
                            Hashtable hashtable = new Hashtable();
                            hashtable.Add("CardEffect", activateClass);

                            yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, hashtable).Tap());

                            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                            AddMaxUnderTamerCountDigiXrosClass addMaxTamerCountDigiXrosClass = new AddMaxUnderTamerCountDigiXrosClass();
                            addMaxTamerCountDigiXrosClass.SetUpICardEffect("Can select DigiXros cards from Tamer's digivolution cards", CanUseCondition1, card);
                            addMaxTamerCountDigiXrosClass.SetUpAddMaxUnderTamerCountDigiXrosClass(getMaxUnderTamerCount: GetCount);
                            card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => addMaxTamerCountDigiXrosClass);

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return true;
                            }

                            int GetCount(CardSource cardSource)
                            {
                                if (CardSourceCondition(cardSource))
                                {
                                    return 1;
                                }

                                return 0;
                            }

                            bool CardSourceCondition(CardSource cardSource)
                            {
                                if (cardSource != null)
                                {
                                    if (cardSource.Owner == card.Owner)
                                    {
                                        if (cardSource.HasDigiXros)
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            AddMaxTrashCountDigiXrosClass addMaxTrashCountDigiXrosClass = new AddMaxTrashCountDigiXrosClass();
                            addMaxTrashCountDigiXrosClass.SetUpICardEffect("Can select DigiXros cards from trash", CanUseCondition1, card);
                            addMaxTrashCountDigiXrosClass.SetUpAddMaxTrashCountDigiXrosClass(getMaxTrashCount: GetCount);
                            card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => addMaxTrashCountDigiXrosClass);
                        }
                    }
                }
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