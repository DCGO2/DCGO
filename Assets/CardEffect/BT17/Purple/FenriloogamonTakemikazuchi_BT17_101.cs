using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Photon.Pun;

namespace DCGO.CardEffects.BT17
{
    public class FenriloogamonTakemikazuchi_BT17_101 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DNA Digivolution

            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool PermanentCondition1(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent,
                                                    card))
                                            {
                                                if (permanent.TopCard.EqualsCardName("Fenriloogamon"))
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

                        bool PermanentCondition2(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent,
                                                    card))
                                            {
                                                if (permanent.TopCard.EqualsCardName("Kazuchimon"))
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

                        JogressConditionElement[] elements =
                        {
                            new(PermanentCondition1, "Fenriloogamon"),
                            new(PermanentCondition2, "Kazuchimon")
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region Trash - Your Turn

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DNA Digivolve", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] When one of your level 6 Digimon with [Pulsemon] in its text is played, 2 of your Digimon may DNA digivolve into this card.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.IsDigimon &&
                           permanent.TopCard.HasLevel &&
                           permanent.Level == 6 &&
                           permanent.TopCard.HasText("Pulsemon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.CanPlayJogress(true))
                    {
                        _jogressEvoRootsFrameIDs = Array.Empty<int>();

                        yield return GManager.instance.photonWaitController.StartWait("FenriloogamonTakemikazuchi_BT17_101");

                        if (card.Owner.isYou || GManager.instance.IsAI)
                        {
                            GManager.instance.selectJogressEffect.SetUp_SelectDigivolutionRoots
                            (card: card,
                                isLocal: true,
                                isPayCost: true,
                                canNoSelect: true,
                                endSelectCoroutine_SelectDigivolutionRoots: EndSelectCoroutineSelectDigivolutionRoots,
                                noSelectCoroutine: null);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.selectJogressEffect
                                .SelectDigivolutionRoots());

                            IEnumerator EndSelectCoroutineSelectDigivolutionRoots(List<Permanent> permanents)
                            {
                                if (permanents.Count == 2)
                                {
                                    _jogressEvoRootsFrameIDs =
                                        permanents.Distinct().ToArray().Map(permanent => permanent.PermanentFrame.FrameID);
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
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                .ShowCardEffect(new List<CardSource>() { card }, "Played Card", true, true));

                            PlayCardClass playCard = new PlayCardClass(
                                cardSources: new List<CardSource>() { card },
                                hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                payCost: true,
                                targetPermanent: null,
                                isTapped: false,
                                root: SelectCardEffect.Root.Trash,
                                activateETB: true);

                            playCard.SetJogress(_jogressEvoRootsFrameIDs);

                            yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "1 of your opponent's Digimon gets -16000 DP for the turn. If DNA Digivolving set opponent's memory to 3. Then, gain 1 memory and <Recovery +1>",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] 1 of your opponent's Digimon gets -16000 DP for the turn. If DNA digivolving, you may set the memory to 3 on your opponent's side. Then, if this Digimon has a Tamer in its digivolution cards, gain 1 memory and  ";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanSelectOpponentPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOpponentPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -16000.",
                            "The opponent is selecting 1 Digimon that will get DP -16000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: permanent, changeValue: -16000, effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                        }
                    }

                    if (CardEffectCommons.IsJogress(hashtable))
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                        {
                            new(message: "Yes", value: true, spriteIndex: 0),
                            new(message: "No", value: false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Set opponent's memory to 3?";
                        string notSelectPlayerMessage = "The opponent is choosing whether to set your memory to 3 or not";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements,
                            selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                            notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());

                        bool setMemory = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (setMemory)
                            yield return ContinuousController.instance.StartCoroutine(card.Owner.Enemy.SetFixedMemory(3, activateClass));
                    }

                    if (card.PermanentOfThisCard().DigivolutionCards.Some(cardSource => cardSource.IsTamer))
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                    }
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 of your security to trash opponent's security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Attacking] By trashing the top card of your security stack, trash the top card of your opponent's security stack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if(CardEffectCommons.CanTriggerOnAttack(hashtable, card))
                        return card.Owner.SecurityCards.Count >= 1;

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);                           
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());

                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }

            #endregion

            return cardEffects;
        }

        #region DNA

        private bool _endSelect;
        private int[] _jogressEvoRootsFrameIDs = Array.Empty<int>();

        [PunRPC]
        public void SetJogressEvoRootsFrameIDs(int[] jogressEvoRootsFrameIDs)
        {
            _jogressEvoRootsFrameIDs = jogressEvoRootsFrameIDs;
            _endSelect = true;
        }

        #endregion
    }
}