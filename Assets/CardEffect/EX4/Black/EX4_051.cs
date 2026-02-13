using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.EX4
{
    public class EX4_051 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("MetalGreymon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Choose 1 effect", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Activate 1 of the effects below. - <De-Digivolve 1> 3 of your opponent's Digimon. - 1 of your other Digimon digivolves into a level 6 or lower Digimon card with [Garurumon] in its name in your hand without paying the cost. -This Digimon and one of your other Digimon may DNA digivolve into a Digimon card in your hand for the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            yield return GManager.instance.photonWaitController.StartWait("Kuresgarurumon_Select_ETB");

                            if (card.Owner.isYou)
                            {
                                GManager.instance.commandText.OpenCommandText("Which effect will you activate?");

                                List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>()
                                {
                                    new Command_SelectCommand($"De-Digivolve", () => photonView.RPC("SetActionID", RpcTarget.All, 0), 0),
                                    new Command_SelectCommand($"Your other 1 Digimon digivolves", () => photonView.RPC("SetActionID", RpcTarget.All, 1), 0),
                                    new Command_SelectCommand($"DNA Digivolution", () => photonView.RPC("SetActionID", RpcTarget.All, 2), 0),
                                };

                                GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);
                            }

                            else
                            {
                                GManager.instance.commandText.OpenCommandText("The opponent is choosing which effect to activate.");

                                #region AI
                                if (GManager.instance.IsAI)
                                {
                                    SetActionID(UnityEngine.Random.Range(0, 3));
                                }
                                #endregion
                            }

                            yield return new WaitWhile(() => !endSelect);
                            endSelect = false;

                            GManager.instance.commandText.CloseCommandText();
                            yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                            switch (actionID)
                            {
                                case 0:
                                    bool CanSelectPermanentCondition(Permanent permanent)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                                        {
                                            if (permanent.CanSelectBySkill(activateClass))
                                            {
                                                return true;
                                            }
                                        }

                                        return false;
                                    }

                                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                                    {
                                        int maxCount = Math.Min(3, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                        selectPermanentEffect.SetUp(
                                            selectPlayer: card.Owner,
                                            canTargetCondition: CanSelectPermanentCondition,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: CanEndSelectCondition,
                                            maxCount: maxCount,
                                            canNoSelect: false,
                                            canEndNotMax: false,
                                            selectPermanentCoroutine: SelectPermanentCoroutine,
                                            afterSelectPermanentCoroutine: null,
                                            mode: SelectPermanentEffect.Mode.Custom,
                                            cardEffect: activateClass);

                                        selectPermanentEffect.SetUpCustomMessage("Select Digimons to De-Digivolve.", "The opponent is selecting Digimons to De-Digivolve.");

                                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                        bool CanEndSelectCondition(List<Permanent> permanents)
                                        {
                                            if (permanents.Count <= 0)
                                            {
                                                return false;
                                            }

                                            return true;

                                        }

                                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                        {
                                            Permanent selectedPermanent = permanent;

                                            if (selectedPermanent != null)
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(new IDegeneration(selectedPermanent, 1, activateClass).Degeneration());
                                            }

                                            yield return null;
                                        }
                                    }
                                    break;

                                case 1:
                                    bool CanSelectPermanentCondition1(Permanent permanent)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                                        {
                                            if (permanent != card.PermanentOfThisCard())
                                            {
                                                foreach (CardSource cardSource in card.Owner.HandCards)
                                                {
                                                    if (CanSelectCardCondition(cardSource))
                                                    {
                                                        if (cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass))
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    bool CanSelectCardCondition(CardSource cardSource)
                                    {
                                        return cardSource.IsDigimon && cardSource.HasGarurumonName && cardSource.Level <= 6 && cardSource.HasLevel;
                                    }

                                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                                    {
                                        Permanent selectedPermanent = null;

                                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                        selectPermanentEffect.SetUp(
                                            selectPlayer: card.Owner,
                                            canTargetCondition: CanSelectPermanentCondition1,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: null,
                                            maxCount: maxCount,
                                            canNoSelect: true,
                                            canEndNotMax: false,
                                            selectPermanentCoroutine: SelectPermanentCoroutine,
                                            afterSelectPermanentCoroutine: null,
                                            mode: SelectPermanentEffect.Mode.Custom,
                                            cardEffect: activateClass);

                                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will digivolve.", "The opponent is selecting 1 Digimon that will digivolve.");

                                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                        {
                                            selectedPermanent = permanent;

                                            yield return null;
                                        }

                                        if (selectedPermanent != null)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                                targetPermanent: selectedPermanent,
                                                cardCondition: CanSelectCardCondition,
                                                payCost: false,
                                                reduceCostTuple: null,
                                                fixedCostTuple: null,
                                                ignoreDigivolutionRequirementFixedCost: -1,
                                                isHand: true,
                                                activateClass: activateClass,
                                                successProcess: null));
                                        }
                                    }
                                    break;

                                case 2:
                                    bool CanSelectCardCondition1(CardSource cardSource)
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

                                    if (card.Owner.HandCards.Count(CanSelectCardCondition1) >= 1)
                                    {
                                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent != card.PermanentOfThisCard()))
                                        {
                                            List<CardSource> selectedCards = new List<CardSource>();

                                            int maxCount = 1;

                                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                            selectHandEffect.SetUp(
                                                selectPlayer: card.Owner,
                                                canTargetCondition: CanSelectCardCondition1,
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

                                                        yield return GManager.instance.photonWaitController.StartWait("Jogress_Kuresgarurumon_EX4_049");

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
                                    break;
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash the top card of opponent's security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("TrashSecurity_EX4_051");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking][Once Per Turn] If this Digimon has [Omnimon] in its name, trash the top card of your opponent's security stack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.ContainsCardName("Omnimon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }

            return cardEffects;
        }

        bool endSelect = false;
        int actionID = -1;

        [PunRPC]
        public void SetActionID(int actionID)
        {
            this.actionID = actionID;
            endSelect = true;
        }

        int[] JogressEvoRootsFrameIDs = new int[0];

        [PunRPC]
        public void SetJogressEvoRootsFrameIDs(int[] JogressEvoRootsFrameIDs)
        {
            this.JogressEvoRootsFrameIDs = JogressEvoRootsFrameIDs;
            endSelect = true;
        }
    }
}