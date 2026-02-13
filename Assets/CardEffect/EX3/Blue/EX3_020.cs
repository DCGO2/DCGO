using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.EX3
{
    public class EX3_020 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Coredramon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.EvadeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                AddJogressLevelsClass addJogressLevelsClass = new AddJogressLevelsClass();
                addJogressLevelsClass.SetUpICardEffect("Also treated as level 6 for DNA Digivolution", CanUseCondition, card);
                addJogressLevelsClass.SetUpAddJogressLevelsClass(AddJogressLevels);

                cardEffects.Add(addJogressLevelsClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }



                List<int> AddJogressLevels(CardSource cardSource, Permanent permanent)
                {
                    List<int> levels = new List<int>();

                    if (permanent == card.PermanentOfThisCard())
                    {
                        if (cardSource != null)
                        {
                            if (cardSource.Owner == card.Owner)
                            {
                                if (cardSource.Owner.HandCards.Contains(cardSource))
                                {
                                    if (cardSource.CardNames.Contains("Examon"))
                                    {
                                        levels.Add(6);
                                    }
                                }
                            }
                        }
                    }

                    return levels;
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
                    return "[End of Your Turn] This Digimon and 1 of your other Digimon with [Dramon] in its name may DNA digivolve into a Digimon card in your hand by paying its DNA digivolve cost.";
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
                                            foreach (Permanent permanent in card.Owner.GetBattleAreaDigimons())
                                            {
                                                if (permanent.TopCard.HasDramonName)
                                                {
                                                    if (permanent != card.PermanentOfThisCard())
                                                    {
                                                        if (cardSource.CanJogressFromTargetPermanent(permanent, true))
                                                        {
                                                            if (cardSource.CanJogressFromTargetPermanents(new List<Permanent>() { card.PermanentOfThisCard(), permanent }, true))
                                                            {
                                                                return true;
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
                                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent != card.PermanentOfThisCard() && permanent.TopCard.HasDramonName))
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

                                            yield return GManager.instance.photonWaitController.StartWait("Wingdramon_EX3_020");

                                            if (card.Owner.isYou || GManager.instance.IsAI)
                                            {
                                                GManager.instance.selectJogressEffect.SetUp_SelectDigivolutionRoots
                                                                           (card: selectedCard,
                                                                           isLocal: true,
                                                                           isPayCost: true,
                                                                           canNoSelect: true,
                                                                           endSelectCoroutine_SelectDigivolutionRoots: EndSelectCoroutine_SelectDigivolutionRoots,
                                                                           noSelectCoroutine: null);

                                                GManager.instance.selectJogressEffect.SetUpCustomPermanentConditions(new Func<Permanent, bool>[] { (permanent) => permanent == card.PermanentOfThisCard(), (permanent) => permanent != card.PermanentOfThisCard() && permanent.TopCard.HasDramonName });

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

                                                    if (!permanents.Contains(card.PermanentOfThisCard()) || (permanents.Count((permanent) => permanent != card.PermanentOfThisCard() && permanent.TopCard.HasDramonName) == 0))
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

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.HasDramonName)
                        {
                            return true;
                        }

                        if (card.PermanentOfThisCard().TopCard.ContainsCardName("Examon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.EvadeSelfEffect(isInheritedEffect: true, card: card, condition: Condition));
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
}