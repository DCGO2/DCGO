using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Richard Sampson
namespace DCGO.CardEffects.EX13
{
    public class EX13_071 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared OP/SOMP
            string SharedEffectName = "Place top card of deck face down under this tamer, then if enemy has Digimon gain 1 memory";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    onPlay: true,
                    startOfYourMainPhase: true);

            string SharedEffectDescription(string tag) =>
                $"[{tag}] You may place the top card of your deck face down under this tamer. Then, if your opponent has a Digimon, gain 1 memory.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.LibraryCards.Count >= 1)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>
                    {
                        new(message: $"Yes", value: 1, spriteIndex: 0),
                        new(message: $"No", value: 2, spriteIndex: 1)
                    };

                    string selectPlayerMessage = "Will you place the top card from your deck under this Tamer face down?";
                    string notSelectPlayerMessage = "The opponent is choosing whether to place the top card from their deck under their Tamer face down.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool Yes = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                    if (Yes)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                new List<CardSource> { card.Owner.LibraryCards[0] }, activateClass, isFacedown: true));
                    }
                }

                if (card.Owner.Enemy.GetBattleAreaDigimons().Count >= 1)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 3 face-downs from tamers and place lv4 & lv5 [Holy Beast] trait yellow Digimon cards from trash to under [Kudamon] to warp it to [Kentaurosmon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX13_071_Main");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] [Once Per Turn] By trashing 3 bottom face-down cards from under any of your Tamers and placing 1 each of level 4 and level 5 [Holy Beast] trait yellow Digimon cards from your trash as 1 of your [Kudamon]'s bottom digivolution cards, it may digivolve into [Kentaurosmon] in the hand or trash, ignoring level and with the cost reduced by 1.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                    && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                    && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectLv4CardCondition)
                    && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectLv5CardCondition)
                    && CardEffectCommons.HasMatchConditionPermanent(CanSelectKudamonPermanentCondition))
                    {
                        var faceDownSourceCount = card.Owner.GetBattleAreaPermanents()
                            .Filter(TamerWithOneOrMoreFaceDownSource)
                            .Sum(permanent => permanent.DigivolutionCards.Count(CanSelectTrashSourceCardCondition));

                        if (faceDownSourceCount >= 3)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool TamerWithOneOrMoreFaceDownSource(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Any(CanSelectTrashSourceCardCondition);
                }

                bool CanSelectTrashSourceCardCondition(CardSource cardSource)
                {
                    return cardSource.IsFaceDown;
                }

                bool CanSelectLv4CardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.EqualsTraits("Holy Beast")
                        && cardSource.IsLevel4;
                }

                bool CanSelectLv5CardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.EqualsTraits("Holy Beast")
                        && cardSource.IsLevel5;
                }

                bool CanSelectKudamonPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && permanent.TopCard.EqualsCardName("Kudamon");
                }

                bool CanSelectKentaurosmonCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Kentaurosmon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool trashed = false;

                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                    int maxCount1 = Math.Min(3, CardEffectCommons.MatchConditionPermanentCount(TamerWithOneOrMoreFaceDownSource));

                    selectPermanentEffect1.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TamerWithOneOrMoreFaceDownSource,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: maxCount1,
                        canNoSelect: true,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect1.SetUpCustomMessage("Select all Tamer(s) to trash bottom face-down cards from", "The opponent is selecting Tamer(s) to trash bottom face-down cards from");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    bool CanEndSelectCondition(List<Permanent> permanents)
                    {
                        if (permanents.Count > 0)
                        {
                            int faceDownCount = 0;

                            foreach(Permanent permanent in permanents)
                            {
                                faceDownCount += permanent.DigivolutionCards.Count(CanSelectTrashSourceCardCondition);
                            }

                            if (faceDownCount >= 3)
                            {
                                return true;
                            }
                        }
                        return false;
                    }

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents.Count == 1)
                        {
                            trashed = true;
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 3, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));
                        }
                        else if (permanents.Count == 2)
                        {
                            Permanent trash2Tamer = null;

                            SelectPermanentEffect selectPermanentEffect2 = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect2.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: permanent => permanents.Contains(permanent) && permanent.DigivolutionCards.Count(CanSelectTrashSourceCardCondition) >= 2,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: CanEndSelectCondition,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect2.SetUpCustomMessage("Select Tamer to trash 2 from", "The opponent is selecting Tamer(s) to trash bottom face-down cards from");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect2.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                trash2Tamer = permanent;

                                yield return null;
                            }

                            Permanent trash1Tamer = permanents.FirstOrDefault(permanent => permanent != trash2Tamer);

                            trashed = true;
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: trash2Tamer, trashCount: 2, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: trash1Tamer, trashCount: 1, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));
                        }
                        else if (permanents.Count == 3)
                        {
                            trashed = true;
                            foreach (Permanent selectedPermanent in permanents)
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: 1, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));
                        }

                        yield return null;
                    }

                    if (trashed)
                    {
                        List<CardSource> selectedSourceCards = new List<CardSource>() { card };
                        List<CardSource> digivolutionCardsOrder = new List<CardSource>();
                        Permanent selectedKudamonPermanent = null;

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedSourceCards.Add(cardSource);
                            yield return null;
                        }

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedKudamonPermanent = permanent;

                            yield return null;
                        }

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            digivolutionCardsOrder = cardSources.Clone();

                            yield return null;
                        }

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectLv4CardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 level 4 [Holy Beast] trait yellow Digimon card to place in Digivolution cards.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 level 4 [Holy Beast] trait yellow Digimon card to place in Digivolution cards.",
                            "The opponent is selecting 1 level 4 [Holy Beast] trait yellow Digimon card to place in Digivolution cards.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        SelectCardEffect selectCardEffect2 = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect2.SetUp(
                            canTargetCondition: CanSelectLv5CardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 level 5 [Holy Beast] trait yellow Digimon card to place in Digivolution cards.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect2.SetUpCustomMessage("Select 1 level 5 [Holy Beast] trait yellow Digimon card to place in Digivolution cards.",
                            "The opponent is selecting 1 level 5 [Holy Beast] trait yellow Digimon card to place in Digivolution cards.");
                        selectCardEffect2.SetUpCustomMessage_ShowCard("Digivolution Card");

                        yield return StartCoroutine(selectCardEffect2.Activate());

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectKudamonPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 [Kudamon] to place Digivolution cards under.", "The opponent is selecting 1 [Kudamon] to place Digivolution cards under.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        SelectCardEffect selectCardEffect3 = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect3.SetUp(
                            canTargetCondition: _ => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Specify the order to place the cards in the digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                            maxCount: selectedSourceCards.Count,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: selectedSourceCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect3.SetUpCustomMessage_ShowCard("Digivolution Cards");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect3.Activate());

                        yield return ContinuousController.instance.StartCoroutine(
                            selectedKudamonPermanent.AddDigivolutionCardsBottom(digivolutionCardsOrder, activateClass));

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            selectedKudamonPermanent,
                            CanSelectKentaurosmonCardCondition,
                            payCost: true,
                            reduceCostTuple: (reduceCost: 1, reduceCostCardCondition: null),
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null,
                            ignoreRequirements: CardEffectCommons.IgnoreRequirement.Level));
                    }

                    if (!trashed) activateClass.RemoveUse();
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
