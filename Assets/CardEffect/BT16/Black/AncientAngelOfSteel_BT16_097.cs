using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.BT16
{
    public class AncientAngelOfSteel_BT16_097 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main - Option Skill
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Ankylomon] or [Angemon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] You may play 1 [Ankylomon] or [Angemon] from your hand without paying the cost. Then, 2 of your Digimon may DNA digivolve into a Digimon card in your hand. If DNA digivolved by this effect, <Recovery +1(Deck)>";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardNames.Contains("Ankylomon") || cardSource.CardNames.Contains("Angemon"))
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectDNACardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.CanPlayJogress(true))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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

                        selectHandEffect.SetUpCustomMessage("Select 1 Digimon to play.", "The opponent is selecting 1 Digimon to play.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));

                        // DNA digivolve
                        if (card.Owner.GetBattleAreaDigimons().Count >= 2)
                        {
                            if (card.Owner.HandCards.Count(CanSelectDNACardCondition) >= 1)
                            {
                                List<CardSource> selectedDNACards = new List<CardSource>();

                                SelectHandEffect selectDNAEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectDNAEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectDNACardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectDNACoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectDNAEffect.SetUpCustomMessage("Select 1 card to DNA digivolve.", "The opponent is selecting 1 card to DNA digivolve.");
                                selectDNAEffect.SetNotShowCard();

                                yield return StartCoroutine(selectDNAEffect.Activate());

                                IEnumerator SelectDNACoroutine(CardSource cardSource)
                                {
                                    selectedDNACards.Add(cardSource);

                                    yield return null;
                                }

                                if (selectedDNACards.Count >= 1)
                                {
                                    foreach (CardSource selectedCard in selectedDNACards)
                                    {
                                        if (selectedCard.CanPlayJogress(true))
                                        {
                                            _jogressEvoRootsFrameIDs = new int[0];

                                            yield return GManager.instance.photonWaitController.StartWait("AncientAngelOfSteel_BT16_097");

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

                                                if (CardEffectCommons.IsExistOnBattleArea(card))
                                                {
                                                    if (card.Owner.LibraryCards.Count >= 1)
                                                    {
                                                        if (card.Owner.CanAddSecurity(activateClass))
                                                        {
                                                            yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery()); ;
                                                        }
                                                    }
                                                }
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

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Ankylomon] or [Angemon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] You may play 1 [Ankylomon] or [Angemon] from your hand without paying the cost. Then, 2 of your Digimon may DNA digivolve into a Digimon card in your hand. If DNA digivolved by this effect, <Recovery +1(Deck)>";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardNames.Contains("Ankylomon") || cardSource.CardNames.Contains("Angemon"))
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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

                        selectHandEffect.SetUpCustomMessage("Select 1 Digimon to play.", "The opponent is selecting 1 Digimon to play.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
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