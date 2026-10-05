using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Snatchmon
namespace DCGO.CardEffects.BT18
{
    public class BT18_065 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place up to 2 [Vemmon] from trash to digivolution cards.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] You may place up to 2 [Vemmon] from your trash under this Digimon as its bottom digivolution cards.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Vemmon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    int maxCount = Math.Min(2, card.Owner.TrashCards.Count(CanSelectCardCondition));

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select [Vemmon] to place on bottom of digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                        maxCount: maxCount,
                        canEndNotMax: true,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");
                    selectCardEffect.SetUpCustomMessage("Select [Vemmon] to place on bottom of digivolution cards.", "The opponent is selecting [Vemmon] to place on bottom of digivolution cards.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (selectedCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));
                    }
                }
            }
            #endregion

            #region End of Your Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into a Digimon with [Vemmon] in text.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[End of Your Turn] If this Digimon has 4 or more digivolution cards, this Digimon may Digivolve into a Digimon card with [Vemmon] in its text in your hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.PermanentOfThisCard().DigivolutionCards.Count >= 4
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanTriggerCardCondition);
                }

                bool CanTriggerCardCondition(CardSource cardSource)
                {
                    return cardSource.HasText("Vemmon")
                        && cardSource.CanPlayCardTargetFrame(
                            frame: card.PermanentOfThisCard().PermanentFrame,
                            PayCost: true,
                            cardEffect: activateClass,
                            root: SelectCardEffect.Root.Hand);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasText("Vemmon");
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: CanSelectCardCondition,
                        payCost: true,
                        reduceCostTuple: (reduceCost: 0, reduceCostCardCondition: null),
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true,
                        activateClass: activateClass,
                        successProcess: null));
                }
            }
            #endregion

            #region DigiXros from trash
            if (timing == EffectTiming.None)
            {
                AddMaxTrashCountDigiXrosClass addMaxTrashCountDigiXrosClass = new AddMaxTrashCountDigiXrosClass();
                addMaxTrashCountDigiXrosClass.SetUpICardEffect("Trash cards can be selected for DigiXros", CanUseCondition, card);
                addMaxTrashCountDigiXrosClass.SetUpAddMaxTrashCountDigiXrosClass(getMaxTrashCount: GetCount);
                addMaxTrashCountDigiXrosClass.SetNotShowUI(true);
                cardEffects.Add(addMaxTrashCountDigiXrosClass);

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && !permanent.TopCard.EqualsCardName("Vemmon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1) == 0;
                }

                int GetCount(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        return 4;
                    }

                    return 0;
                }
            }
            #endregion

            #region DigiXros
            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect("DigiXros", CanUseCondition, card);
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
                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>();

                        DigiXrosConditionElement element1 = new DigiXrosConditionElement(CanSelectCardCondition1, "Vemmon");

                        bool CanSelectCardCondition1(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.IsDigimon
                                && cardSource.CardNames_DigiXros.Contains("Vemmon");
                        }

                        for (int i = 0; i < 4; i++)
                        {
                            elements.Add(element1);
                        }

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 1);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region ESS - All Turns
            if (timing == EffectTiming.OnDigivolutionCardReturnToDeckBottom)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon and it gain <Blocker>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT18_065_Inherited");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When any [Vemmon] return to the bottom of the deck from this Digimon's digivolution cards, this Digimon unsuspends, and gains <Blocker> until the end of your opponent's turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnReturnToLibraryBottomDigivolutionCard(hashtable, cardSource => cardSource.EqualsCardName("Vemmon"), card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(targetPermanent: card.PermanentOfThisCard(), effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
