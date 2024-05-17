using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace DCGO.CardEffects.BT16
{
    public class Darkdramon_BT16_065 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Before Play - reduce cost
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce Play Cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("PlayCost-_BT16_065");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When you would play this card, if there is a Digimon with the [Boss] trait, reduce the play cost by 6. By returning 6 cards with the [D-Brigade] trait from your trash to the top of the deck, reduce the play cost by 6.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.CardTraits.Contains("D-Brigade");
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card);
                }

                bool HasBossTraitInPlay(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            if (permanent.TopCard.CardTraits.Contains("Boss"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    int minusCount = 0;

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasBossTraitInPlay))
                    {
                        minusCount = 6;
                    }

                    if (CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectCardCondition) >= 6)
                    {
                        int maxCount = Math.Min(6, card.Owner.TrashCards.Count(CanSelectCardCondition));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: CanEndSelectCondition,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Select cards to return to the top of deck\n(cards will be placed back to the top of the deck so that cards with lower numbers are on top).",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetNotAddLog();
                        selectCardEffect.SetUpCustomMessage_ShowCard("Deck Top Cards");
                        selectCardEffect.SetUpCustomMessage(
                            "Select cards to put on top of the deck.",
                            "The opponent is selecting cards to put on top of the deck.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        bool CanEndSelectCondition(List<CardSource> cardSources)
                        {
                            if (CardEffectCommons.HasNoElement(cardSources))
                            {
                                return false;
                            }

                            return true;
                        }

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count == 6)
                            {
                                cardSources.Reverse();

                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(cardSources));

                                minusCount += cardSources.Count;

                                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                                ChangeCostClass changeCostClass = new ChangeCostClass();
                                changeCostClass.SetUpICardEffect($"Digivolution Cost -{minusCount}", CanUseCondition1, card);
                                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                                card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                                bool CanUseCondition1(Hashtable hashtable)
                                {
                                    return true;
                                }

                                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                {
                                    if (CardSourceCondition(cardSource))
                                    {
                                        if (RootCondition(root))
                                        {
                                            if (PermanentsCondition(targetPermanents))
                                            {
                                                Cost -= minusCount;
                                            }
                                        }
                                    }

                                    return Cost;
                                }

                                bool PermanentsCondition(List<Permanent> targetPermanents)
                                {
                                    if (targetPermanents != null)
                                    {
                                        if (targetPermanents.Count(PermanentCondition) >= 1)
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                bool PermanentCondition(Permanent targetPermanent)
                                {
                                    if (targetPermanent.TopCard != null)
                                    {
                                        if (targetPermanent.TopCard.Owner == card.Owner)
                                        {
                                            if (targetPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(targetPermanent))
                                            {
                                                return true;
                                            }
                                        }
                                    }

                                    return false;
                                }

                                bool CardSourceCondition(CardSource cardSource)
                                {
                                    if (cardSource != null)
                                    {
                                        if (cardSource == card)
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                bool RootCondition(SelectCardEffect.Root root)
                                {
                                    return true;
                                }

                                bool isUpDown()
                                {
                                    return true;
                                }
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
                activateClass.SetUpICardEffect("Reveal top 3, then delete 1 digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Reveal the top 3 cards of your deck. Delete 1 of your opponent's Digimon with play cost less than or equal to the play cost of 1 of the cards among them. Trash the revealed cards.";
                }

                bool RevealSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if(cardSource.HasPlayCost)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if(card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int costDeletion = 0;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                        revealCount: 3,
                        simplifiedSelectCardCondition:
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: RevealSelectCardCondition,
                            message: "Select play cost to use",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: -1,
                            selectCardCoroutine: SelectCardCoroutine),
                        remainingCardsPlace: RemainingCardsPlace.Trash,
                        activateClass: activateClass,
                        revealedCardsCoroutine: null
                    ));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        costDeletion = cardSource.GetCostItself;

                        if(CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentCondition))
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
                        


                        yield return null;
                    }

                    bool CanSelectPermanentCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                        {
                            if (permanent.TopCard.HasPlayCost)
                            {
                                if(permanent.TopCard.GetCostItself <= costDeletion)
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }

                    yield return null;
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal top 3, then delete 1 digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Reveal the top 3 cards of your deck. Delete 1 of your opponent's Digimon with play cost less than or equal to the play cost of 1 of the cards among them. Trash the revealed cards.";
                }

                bool RevealSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasPlayCost)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int costDeletion = 0;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                        revealCount: 3,
                        simplifiedSelectCardCondition:
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: RevealSelectCardCondition,
                            message: "Select play cost to use",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: -1,
                            selectCardCoroutine: SelectCardCoroutine),
                        remainingCardsPlace: RemainingCardsPlace.Trash,
                        activateClass: activateClass,
                        revealedCardsCoroutine: null
                    ));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        costDeletion = cardSource.GetCostItself;

                        if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentCondition))
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



                        yield return null;
                    }

                    bool CanSelectPermanentCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                        {
                            if (permanent.TopCard.HasPlayCost)
                            {
                                if (permanent.TopCard.GetCostItself <= costDeletion)
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }

                    yield return null;
                }
            }
            #endregion

            #region End of Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DNA digivolve into [Chaosmon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] 2 of your Digimon may DNA digivolve into a [Chaosmon] in your hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.CanPlayJogress(true))
                        {
                            if (cardSource.CardNames.Contains("Chaosmon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return true;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    // DNA digivolve
                    if (card.Owner.GetBattleAreaDigimons().Count >= 2)
                    {
                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = 1;

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select 1 card to DNA digivolve.", "The opponent is selecting 1 card to DNA digivolve.");
                            selectHandEffect.SetNotShowCard();

                            yield return StartCoroutine(selectHandEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                foreach (CardSource selectedCard in selectedCards)
                                {
                                    if (selectedCard.CanPlayJogress(true))
                                    {
                                        _jogressEvoRootsFrameIDs = new int[0];

                                        yield return GManager.instance.photonWaitController.StartWait("Darkdramon_BT16_065");

                                        if (card.Owner.isYou || GManager.instance.IsAI)
                                        {
                                            GManager.instance.selectJogressEffect.SetUp_SelectDigivolutionRoots
                                                                        (card: selectedCard,
                                                                        isLocal: true,
                                                                        isPayCost: true,
                                                                        canNoSelect: true,
                                                                        endSelectCoroutine_SelectDigivolutionRoots: EndSelectCoroutine_SelectDigivolutionRoots,
                                                                        noSelectCoroutine: null);

                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.selectJogressEffect.SelectDigivolutionRoots());

                                            IEnumerator EndSelectCoroutine_SelectDigivolutionRoots(List<Permanent> permanents)
                                            {
                                                if (permanents.Count == 2)
                                                {
                                                    _jogressEvoRootsFrameIDs = permanents.Distinct().ToArray().Map(permanent => permanent.PermanentFrame.FrameID);
                                                }

                                                yield return null;
                                            }

                                            photonView.RPC("SetJogressEvoRootsFrameIDs", RpcTarget.All, _jogressEvoRootsFrameIDs);
                                        }

                                        else
                                        {
                                            GManager.instance.commandText.OpenCommandText("The opponent is choosing a card to DNA digivolve.");
                                        }

                                        yield return new WaitWhile(() => !_endSelect);
                                        _endSelect = false;

                                        GManager.instance.commandText.CloseCommandText();
                                        yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                                        if (_jogressEvoRootsFrameIDs.Length == 2)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { selectedCard }, "Played Card", true, true));

                                            PlayCardClass playCard = new PlayCardClass(
                                                cardSources: new List<CardSource>() { selectedCard },
                                                hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                                payCost: true,
                                                targetPermanent: null,
                                                isTapped: false,
                                                root: SelectCardEffect.Root.Hand,
                                                activateETB: true);

                                            playCard.SetJogress(_jogressEvoRootsFrameIDs);

                                            yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion


            return cardEffects;
        }

        private bool _endSelect = false;
        private int[] _jogressEvoRootsFrameIDs = new int[0];

        [PunRPC]
        public void SetJogressEvoRootsFrameIDs(int[] JogressEvoRootsFrameIDs)
        {
            this._jogressEvoRootsFrameIDs = JogressEvoRootsFrameIDs;
            _endSelect = true;
        }
    }
}