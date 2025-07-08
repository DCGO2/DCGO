using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

//Hydramon
namespace DCGO.CardEffects.EX9
{
    public class EX9_044 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasWGTraits && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region When Would be Played

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 of your [WG] trait Digimon to get Play Cost -4", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetHashString("PlayCost-4_EX9_044");
                cardEffects.Add(activateClass);

                activateClass.SetIsDigimonEffect(true);

                string EffectDescription()
                {
                    return
                        "When this card would be played, by suspending 1 of your [WG] trait Digimon, reduce the play cost by 4.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card && CardEffectCommons.IsExistOnHand(cardSource);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent != null &&
                           permanent.TopCard &&
                           !permanent.TopCard.CanNotBeAffected(activateClass) &&
                           !permanent.IsSuspended && permanent.CanSuspend &&
                           permanent.TopCard.HasWGTraits;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canNoSelect = true;
                    CardSource cardFromHashtable = CardEffectCommons.GetCardFromHashtable(hashtable);

                    if (cardFromHashtable && cardFromHashtable.PayingCost(SelectCardEffect.Root.Hand, null, checkAvailability: false) >
                        cardFromHashtable.Owner.MaxMemoryCost)
                    {
                        canNoSelect = false;
                    }

                    Permanent selectedPermanent = null;
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: canNoSelect,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to suspend.",
                        "The opponent is selecting 1 Digimon to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { selectedPermanent }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                        if (card.Owner.CanReduceCost(null, card))
                        {
                            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                            ChangeCostClass changeCostClass = new ChangeCostClass();
                            changeCostClass.SetUpICardEffect("Play Cost -4", CanUseCondition1, card);
                            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition,
                                rootCondition: RootCondition, isUpDown: IsUpDown, isCheckAvailability: () => false,
                                isChangePayingCost: () => true);
                            card.Owner.UntilCalculateFixedCostEffect.Add(_ => changeCostClass);

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(hashtable));
                        }

                        bool CanUseCondition1(Hashtable hashtable1)
                        {
                            return true;
                        }

                        int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root,
                            List<Permanent> targetPermanents)
                        {
                            if (CardSourceCondition(cardSource) &&
                                RootCondition(root) &&
                                PermanentsCondition(targetPermanents))
                            {
                                cost -= 4;
                            }

                            return cost;
                        }

                        bool PermanentsCondition(List<Permanent> targetPermanents)
                        {
                            return targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0;
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            return cardSource == card;
                        }

                        bool RootCondition(SelectCardEffect.Root root)
                        {
                            return true;
                        }

                        bool IsUpDown()
                        {
                            return true;
                        }
                    }
                }
            }

            #endregion

            #region Reduce Play Cost - Not Shown

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -4", CanUseCondition1, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition,
                    rootCondition: RootCondition, isUpDown: IsUpDown, isCheckAvailability: () => true,
                    isChangePayingCost: () => true);

                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition1(Hashtable hashtable1)
                {
                    return CardEffectCommons.MatchConditionPermanentCount(PermanentCondition) >= 1;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           !permanent.IsSuspended && permanent.CanSuspend &&
                           permanent.TopCard.HasWGTraits;
                }

                int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root,
                    List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource) &&
                        RootCondition(root) &&
                        PermanentsCondition(targetPermanents))
                    {
                        cost -= 4;
                    }

                    return cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    return targetPermanents == null ||
                           targetPermanents.Count(targetPermanent => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(targetPermanent) &&
                                                                     targetPermanent != null &&
                                                                     targetPermanent.TopCard &&
                                                                     !targetPermanent.TopCard.CanNotBeAffected(changeCostClass) &&
                                                                     !targetPermanent.IsSuspended && targetPermanent.CanSuspend &&
                                                                     targetPermanent.TopCard.HasWGTraits) <= 1;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                bool IsUpDown()
                {
                    return true;
                }
            }

            #endregion

            #region On Play/When Digivolving Shared

            bool OpponentsDigimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card) &&
                       (permanent.IsDigimon || permanent.IsTamer);
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 Digimon/Tamer, 1 Digimon/Tamer Can't Unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Suspend 1 of your opponent's Digimon or Tamers. Then, 1 of their Digimon or Tamers can't unsuspend until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimon))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon/Tamers that will not unsuspend.", "The opponent is selecting 1 Digimon/Tamers that will not unsuspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspend(
                                targetPermanent: selectedPermanent,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                condition: null,
                                effectName: "Can't unsuspend"
                            ));
                        }
                    }
                    yield return null;
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 Digimon/Tamer, 1 Digimon/Tamer Can't Unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] [When Digivolving] Suspend 1 of your opponent's Digimon or Tamers. Then, 1 of their Digimon or Tamers can't unsuspend until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimon))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon/Tamers that will not unsuspend.", "The opponent is selecting 1 Digimon/Tamers that will not unsuspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspend(
                                targetPermanent: selectedPermanent,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                condition: null,
                                effectName: "Can't unsuspend"
                            ));
                        }
                    }
                    yield return null;
                }
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DNA Digivolve.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("DNADigivolve_EX9_044");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] [Once Per Turn] When your Digimon are played or digivolve, if any of them have the [WG] trait, 2 of your Digimon may DNA digivolve into a Digimon card with the [WG] trait in the hand.";
                }

                bool PermanentConditionWGEnterField(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasWGTraits)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentConditionWGEnterField))
                            {
                                return true;
                            }

                            if (CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentConditionWGEnterField))
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
                        return true;
                    }

                    return false;
                }

                bool CanSelectDNACardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.CanPlayJogress(true) &&
                           cardSource.HasWGTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    bool DNADigivolved = false;
                    int maxCount = 1;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDNACardCondition,
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

                    selectHandEffect.SetUpCustomMessage("Select 1 card to DNA digivolve.",
                        "The opponent is selecting 1 card to DNA digivolve.");
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
                                _jogressEvoRootsFrameIDs = Array.Empty<int>();

                                yield return GManager.instance.photonWaitController.StartWait("Hydramon_EX9_044");

                                if (card.Owner.isYou || GManager.instance.IsAI)
                                {
                                    GManager.instance.selectJogressEffect.SetUp_SelectDigivolutionRoots
                                    (card: selectedCard,
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
                                            _jogressEvoRootsFrameIDs = permanents.Distinct().ToArray()
                                                .Map(permanent => permanent.PermanentFrame.FrameID);
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
                                        .ShowCardEffect(new List<CardSource>() { selectedCard }, "Played Card", true, true));

                                    PlayCardClass playCard = new PlayCardClass(
                                        cardSources: new List<CardSource>() { selectedCard },
                                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                        payCost: true,
                                        targetPermanent: null,
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Hand,
                                        activateETB: true);

                                    playCard.SetJogress(_jogressEvoRootsFrameIDs);
                                    DNADigivolved = true;
                                    yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }

        #region Photon DNA

        bool _endSelect;
        int[] _jogressEvoRootsFrameIDs = Array.Empty<int>();

        [PunRPC]
        public void SetJogressEvoRootsFrameIDs(int[] jogressEvoRootsFrameIDs)
        {
            this._jogressEvoRootsFrameIDs = jogressEvoRootsFrameIDs;
            _endSelect = true;
        }

        #endregion
    }
}