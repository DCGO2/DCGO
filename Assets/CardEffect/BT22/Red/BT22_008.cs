using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Agumon
namespace DCGO.CardEffects.BT22
{
    public class BT22_008 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Koromon") || (targetPermanent.TopCard.IsLevel2 && targetPermanent.TopCard.HasCSTraits);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return a card from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Play] You may return 1 Digimon card with [Greymon], [Garurumon] or [Omnimon] in its name from your trash to the hand";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && (cardSource.ContainsCardName("Greymon") ||
                            cardSource.ContainsCardName("Garurumon") ||
                            cardSource.ContainsCardName("Omnimon"));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, CanSelectCardCondition));
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to add to your hand.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.AddHand,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to add to your hand.", "The opponent is selecting 1 card to add to their hand.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DNA digivolve this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] This Digimon and any of your other Digimon may DNA digivolve into a Digimon card in the hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.Owner == card.Owner)
                            {
                                if (cardSource.CanPlayJogress(true))
                                {
                                    if (isExistOnField(card))
                                    {
                                        if (cardSource.CanJogressFromTargetPermanent(card.PermanentOfThisCard(), true))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (card.Owner.HandCards.Count >= 1)
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent != card.PermanentOfThisCard()))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
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
                                        if (selectedCard.CanPlayJogress(true) && selectedCard.CanJogressFromTargetPermanent(card.PermanentOfThisCard(), true))
                                        {
                                            JogressEvoRootsFrameIDs = new int[0];

                                            yield return GManager.instance.photonWaitController.StartWait("Agumon _BT22_008");

                                            if (card.Owner.isYou || GManager.instance.IsAI)
                                            {
                                                GManager.instance.selectJogressEffect.SetUp_SelectDigivolutionRoots
                                                                           (card: selectedCard,
                                                                           isLocal: true,
                                                                           isPayCost: true,
                                                                           canNoSelect: true,
                                                                           endSelectCoroutine_SelectDigivolutionRoots: EndSelectCoroutine_SelectDigivolutionRoots,
                                                                           noSelectCoroutine: null);

                                                GManager.instance.selectJogressEffect.SetUpCustomPermanentConditions(new Func<Permanent, bool>[] { (permanent) => permanent == card.PermanentOfThisCard() });

                                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.selectJogressEffect.SelectDigivolutionRoots());

                                                IEnumerator EndSelectCoroutine_SelectDigivolutionRoots(List<Permanent> permanents)
                                                {
                                                    permanents = permanents.Distinct().ToList();

                                                    if (permanents.Count == 2)
                                                    {
                                                        JogressEvoRootsFrameIDs = new int[2];

                                                        for (int i = 0; i < permanents.Count; i++)
                                                        {
                                                            if (i < JogressEvoRootsFrameIDs.Length)
                                                            {
                                                                JogressEvoRootsFrameIDs[i] = permanents[i].PermanentFrame.FrameID;
                                                            }
                                                        }
                                                    }

                                                    if (!permanents.Contains(card.PermanentOfThisCard()))
                                                    {
                                                        JogressEvoRootsFrameIDs = new int[0];
                                                    }

                                                    yield return null;
                                                }

                                                photonView.RPC("SetJogressEvoRootsFrameIDs", RpcTarget.All, JogressEvoRootsFrameIDs);
                                            }
                                            else
                                            {
                                                GManager.instance.commandText.OpenCommandText("The opponent is choosing a card to DNA digivolve.");
                                            }

                                            yield return new WaitWhile(() => !endSelect);
                                            endSelect = false;

                                            GManager.instance.commandText.CloseCommandText();
                                            yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                                            if (JogressEvoRootsFrameIDs.Length == 2)
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

                                                playCard.SetJogress(JogressEvoRootsFrameIDs);

                                                yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());
                                            }
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

        #region DNA Required

        bool endSelect = false;
        int[] JogressEvoRootsFrameIDs = new int[0];

        [PunRPC]
        public void SetJogressEvoRootsFrameIDs(int[] JogressEvoRootsFrameIDs)
        {
            this.JogressEvoRootsFrameIDs = JogressEvoRootsFrameIDs;
            endSelect = true;
        }

        #endregion
    }
}