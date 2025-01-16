using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT9_080 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Play 1 Digimon from trash", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] Play 1 purple or yellow Digimon card with 6000 DP or less from your trash without paying its memory cost. If you have 1 or fewer security cards, you may play 1 level 6 or lower Digimon card with [Angel] or [Fallen Angel] in its traits from your trash without paying its memory cost instead.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource != null)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.Owner == card.Owner)
                            {
                                if (cardSource.CardColors.Contains(CardColor.Purple) || cardSource.CardColors.Contains(CardColor.Yellow))
                                {
                                    if (cardSource.CardDP <= 6000)
                                    {
                                        return true;
                                    }
                                }

                                if (card.Owner.SecurityCards.Count <= 1)
                                {
                                    if (cardSource.Level <= 6 && cardSource.HasLevel)
                                    {
                                        if (cardSource.CardTraits.Contains("Angel"))
                                        {
                                            return true;
                                        }

                                        if (cardSource.CardTraits.Contains("Fallen Angel"))
                                        {
                                            return true;
                                        }

                                        if (cardSource.CardTraits.Contains("FallenAngel"))
                                        {
                                            return true;
                                        }
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
                return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
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
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = 1;

                            if (card.Owner.TrashCards.Count(CanSelectCardCondition) <= maxCount)
                            {
                                maxCount = card.Owner.TrashCards.Count(CanSelectCardCondition);
                            }

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: CanEndSelectCondition,
                                        canNoSelect: () => false,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 card to play.",
                                        maxCount: maxCount,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return StartCoroutine(selectCardEffect.Activate());

                            bool CanEndSelectCondition(List<CardSource> cardSources)
                            {
                                if (maxCount >= 1)
                                {
                                    if (cardSources.Count <= 0)
                                    {
                                        return false;
                                    }
                                }

                                return true;
                            }

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Trash, activateETB: true));
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.OnEndTurn)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("DNA digivolve this Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[End of Your Turn] You may DNA digivolve this Digimon and one of your other Digimon in play into a Digimon card in your hand for its DNA digivolve cost. ";
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

                                        yield return GManager.instance.photonWaitController.StartWait("Patamon_BT8_020");

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

        return cardEffects;
    }

    bool endSelect = false;
    int[] JogressEvoRootsFrameIDs = new int[0];

    [PunRPC]
    public void SetJogressEvoRootsFrameIDs(int[] JogressEvoRootsFrameIDs)
    {
        this.JogressEvoRootsFrameIDs = JogressEvoRootsFrameIDs;
        endSelect = true;
    }
}
