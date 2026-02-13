using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public partial class CardEffectCommons
{
    /// <summary>
    /// Creates a temporary permanent. Calling method can later ensure clear the frame
    /// </summary>
    /// <param name="card">Card to make a permanent of</param>
    /// <param name="finalCard">If true, CardObjectController will be used to more properly create the permanent so it works fully with the jogress, so this will not place the permanent into any frames</param>
    /// <returns>The created Permanent</returns>
    private static Permanent PlayTempPermanent(CardSource card, bool finalCard = false)
    {
        Permanent playedPermanent = null;
        if (card != null)
        {
            int frameID = card.PreferredFrame().FrameID;

            if (0 <= frameID && frameID < card.Owner.fieldCardFrames.Count)
            {
                playedPermanent = new Permanent(new List<CardSource>() { card }) { IsSuspended = false };

                if (!finalCard)
                {
                    card.Owner.FieldPermanents[frameID] = playedPermanent;
                }
                return playedPermanent;
            }
        }
        return null;
    }

    /// <summary>
    /// Check for if a cardSource can meet the jogress requirements of another given card
    /// </summary>
    /// <param name="cardSource">The Card to verify if it is a potential jogress root</param>
    /// <param name="jogressTarget">Card that will be DNA digivolved into</param>
    /// <param name="firstCondition">If the first root has already been found, it is passed here so we only check if this card can fulfill the second root. Otherwise we check if this card can fulfill the first root and some permanent can fulfill the second</param>
    /// <param name="permanentCondition">Condition to filter which permaments may be used for this effect</param>
    /// <param name="cardCondition">Condition to filter which cards may be used for this effect</param>
    /// <returns></returns>
    private static bool CardFulfillsRequirement(Player owner, CardSource cardSource, CardSource jogressTarget, Permanent firstCondition, Func<Permanent, bool> permanentCondition = null, Func<CardSource, bool> cardCondition = null)
    {
        if (jogressTarget.jogressCondition.Count <= 0)
            return false;
        bool isValid = false;
        if(cardCondition == null || cardCondition(cardSource))
        {
            Permanent tempPermanent = PlayTempPermanent(cardSource);
            if (tempPermanent == null)
                return false;
            if (firstCondition == null)
            {
                foreach (JogressCondition DNACondition in jogressTarget.jogressCondition)
                {
                    if(isValid)
                        break;
                    if (DNACondition.elements[0].EvoRootCondition(tempPermanent))
                    {
                        foreach(Permanent secondPermanent in owner.GetBattleAreaDigimons().Filter(permanent => permanentCondition == null || permanentCondition(permanent)))
                        {
                            if (secondPermanent == tempPermanent)
                                continue;
                            if(DNACondition.elements[1].EvoRootCondition(secondPermanent))
                            {
                                isValid = true;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (JogressCondition DNACondition in jogressTarget.jogressCondition)
                {
                    if (DNACondition.elements[0].EvoRootCondition(firstCondition) && DNACondition.elements[1].EvoRootCondition(tempPermanent))
                    {
                        isValid = true;
                        break;
                    }
                }
            }
            owner.FieldPermanents[tempPermanent.PermanentFrame.FrameID] = null;
        }
        return isValid;
    }

    private static IEnumerator SelectHandCard(Player owner, CardSource jogressTarget, Permanent firstCondition, bool isOptional, ICardEffect activateClass, Func<CardSource, IEnumerator> SelectCardCoroutine, Func<Permanent, bool> permanentCondition = null, Func<CardSource, bool> digivolutionCardCondition = null)
    {
        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

        selectHandEffect.SetUp(
            selectPlayer: owner,
            canTargetCondition: cardSource => CardFulfillsRequirement(owner, cardSource, jogressTarget, firstCondition, permanentCondition, digivolutionCardCondition),
            canTargetCondition_ByPreSelecetedList: null,
            canEndSelectCondition: null,
            maxCount: 1,
            canNoSelect: isOptional,
            canEndNotMax: false,
            isShowOpponent: true,
            selectCardCoroutine: SelectCardCoroutine,
            afterSelectCardCoroutine: null,
            mode: SelectHandEffect.Mode.Custom,
            cardEffect: activateClass);

        selectHandEffect.SetUpCustomMessage("Select 1 Digimon to DNA digivolve.", "The opponent is selecting DNA digivolution cards.");
        selectHandEffect.SetNotShowCard();

        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
    }

    private static IEnumerator SelectTrashCard(Player owner, CardSource jogressTarget, Permanent firstCondition, bool isOptional, ICardEffect activateClass, Func<CardSource, IEnumerator> SelectCardCoroutine, Func<Permanent, bool> permanentCondition = null, Func<CardSource, bool> digivolutionCardCondition = null)
    {
        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

        selectCardEffect.SetUp(
            canTargetCondition: cardSource => CardFulfillsRequirement(owner, cardSource, jogressTarget, firstCondition, permanentCondition, digivolutionCardCondition),
            canTargetCondition_ByPreSelecetedList: null,
            canEndSelectCondition: null,
            canNoSelect: () => isOptional,
            selectCardCoroutine: SelectCardCoroutine,
            afterSelectCardCoroutine: null,
            message: "Select 1 Digimon to DNA digivolve.",
            maxCount: 1,
            canEndNotMax: false,
            isShowOpponent: false,
            mode: SelectCardEffect.Mode.Custom,
            root: SelectCardEffect.Root.Trash,
            customRootCardList: null,
            canLookReverseCard: true,
            selectPlayer: owner,
            cardEffect: activateClass);

        selectCardEffect.SetNotShowCard();
        selectCardEffect.SetNotAddLog();

        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
    }

    /// <summary>
    /// Check for if a permanent can meet the jogress requirements of another given card. 
    /// </summary>
    /// <param name="permanent">The Permanent to verify if it is a potential jogress root</param>
    /// <param name="jogressTarget">Card that will be DNA digivolved into</param>
    /// <param name="firstCondition">If the first root has already been found, it is passed here so we only check if this permanent can fulfill the second root. Otherwise we check if this permanent can fulfill the first root and some card can fulfill the second</param>
    /// <param name="isWithHand">If the cardsource for the second condition is coming form hand, otherwise it will be taken from trash</param>
    /// <param name="permanentCondition">Condition to filter which permaments may be used for this effect</param>
    /// <param name="cardCondition">Condition to filter which cards may be used for this effect</param>
    /// <returns></returns>
    private static bool PermanentFulfillsRequirement(Player owner, Permanent permanent, CardSource jogressTarget, Permanent firstCondition, bool isWithHandCard, Func<Permanent, bool> permanentCondition = null, Func<CardSource, bool> cardCondition = null)
    {
        if (jogressTarget.jogressCondition.Count <= 0 || jogressTarget.CanNotEvolve(permanent))
            return false;
        if(permanentCondition == null || permanentCondition(permanent))
        {
            if (firstCondition == null)
            {
                foreach (JogressCondition DNACondition in jogressTarget.jogressCondition)
                {
                    if (DNACondition.elements[0].EvoRootCondition(permanent))
                    {
                        List<CardSource> sources = isWithHandCard ? owner.HandCards : owner.TrashCards;

                        foreach(CardSource cardSource in sources.Filter(cardSource => cardCondition == null || cardCondition(cardSource)))
                        {
                            Permanent tempPermanent = PlayTempPermanent(cardSource);
                            if (tempPermanent == null)
                                continue;
                            bool isValid = DNACondition.elements[1].EvoRootCondition(tempPermanent);
                            owner.FieldPermanents[tempPermanent.PermanentFrame.FrameID] = null;

                            if(isValid)
                                return true;

                        }
                    }
                }
            }
            else
            {
                if (permanent == firstCondition)
                {
                    return false;
                }
                foreach (JogressCondition DNACondition in jogressTarget.jogressCondition)
                {
                    if (DNACondition.elements[0].EvoRootCondition(firstCondition) && DNACondition.elements[1].EvoRootCondition(permanent))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static IEnumerator SelectPermanent(Player owner, CardSource jogressTarget, Permanent firstCondition, bool isOptional, ICardEffect activateClass, bool isWithHand, Func<Permanent, IEnumerator> SelectPermanentCoroutine, Func<Permanent, bool> permanentCondition = null, Func<CardSource, bool> digivolutionCardCondition = null)
    {
        Permanent selectedPermanent = null;

        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

        selectPermanentEffect.SetUp(
            selectPlayer: owner,
            canTargetCondition: permanent => PermanentFulfillsRequirement(owner, permanent, jogressTarget, firstCondition, isWithHand, permanentCondition, digivolutionCardCondition),
            canTargetCondition_ByPreSelecetedList: null,
            canEndSelectCondition: null,
            maxCount: 1,
            canNoSelect: isOptional,
            canEndNotMax: false,
            selectPermanentCoroutine: SelectPermanentCoroutine,
            afterSelectPermanentCoroutine: null,
            mode: SelectPermanentEffect.Mode.Custom,
            cardEffect: activateClass);

        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to DNA digivolve.", "The opponent is selecting 1 Digimon to DNA digivolve.");

        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
    }

    public static bool CanJogressWithHandOrTrash(CardSource source, Player owner, bool isWithHandCard, bool isIntoHandCard, Func<CardSource, bool> targetCardCondition = null, Func<Permanent, bool> permanentCondition = null, Func<CardSource, bool> digivolutionCardCondition = null)
    {
        return (isIntoHandCard ? IsExistOnHand(source) : IsExistOnTrash(source))
            && (targetCardCondition == null || targetCardCondition(source)) 
            && source.jogressCondition.Count > 0
            && (HasMatchConditionPermanent(permanent => PermanentFulfillsRequirement(owner, permanent, source, null, isWithHandCard, permanentCondition, digivolutionCardCondition)) 
                || (isWithHandCard ?
                    owner.HandCards.Some(cardSource => CardFulfillsRequirement(owner, cardSource, source, null, permanentCondition, digivolutionCardCondition)) : 
                    owner.TrashCards.Some(cardSource => CardFulfillsRequirement(owner, cardSource, source, null, permanentCondition, digivolutionCardCondition))));
    }

    /// <summary>
    /// Method that allows the user to DNA digivolve a Permanent on the field with a card in hand or trash as the other DNA root into a card in the hand or trash
    /// </summary>
    /// <param name="targetCardCondition">CardCondition for the digimon that will be DNA Digivolved into</param>
    /// <param name="permanentCondition">PermanentCondition for the permanent which will make one of the roots</param>
    /// <param name="digivolutionCardCondition">CardCondition for the card that will become one of the roots</param>
    /// <param name="payCost">If the pay cost for the digivolution must be payed</param>
    /// <param name="isWithHandCard">If the card that will a root is coming from the Hand, if false it comes from trash</param>
    /// <param name="isIntoHandCard">If the card to be DNA digivolved into is coming from hand, if false it comes from trash</param>
    /// <param name="activateClass">ActivateClass for the effect causing this DNA Digivolution</param>
    /// <param name="successProcess">IEnumerator to run on success</param>
    /// <param name="failedProcess">IEnumerator to run on failure</param>
    /// <param name="isOptional">If this effect is optional. If true, the user may no select at any time</param>
    /// <returns></returns>
    public static IEnumerator DNADigivolveWithHandOrTrashCardIntoHandOrTrash(
        Func<CardSource, bool> targetCardCondition, 
        Func<Permanent, bool> permanentCondition, 
        Func<CardSource, bool> digivolutionCardCondition,
        bool payCost,
        bool isWithHandCard, 
        bool isIntoHandCard,
        ICardEffect activateClass,
        IEnumerator successProcess,
        bool ignoreSelection = false,
        IEnumerator failedProcess = null,
        bool isOptional = true)
    {
        CardSource dnaTarget = null;
        Permanent selectedPermanent = null;
        CardSource selectedCardSource = null;
        Permanent playedPermanent = null;
        Player owner = activateClass.EffectSourceCard.Owner;

        IEnumerator SelectDNACardCoroutine(CardSource source)
        {
            dnaTarget = source;

            yield return null;
        }

        IEnumerator SelectPermanentCoroutine(Permanent permanent)
        {
            selectedPermanent = permanent;

            yield return null;
        }

        IEnumerator SelectCardCoroutine(CardSource cardSource)
        {
            selectedCardSource = cardSource;

            yield return null;
        }

        if(ignoreSelection)
        {
            dnaTarget = activateClass.EffectSourceCard;
        }
        else if (isIntoHandCard && owner.HandCards.Some(cardSource => CanJogressWithHandOrTrash(cardSource, owner, isWithHandCard, isIntoHandCard, targetCardCondition, permanentCondition, digivolutionCardCondition)))
        {
            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

            selectHandEffect.SetUp(
                selectPlayer: owner,
                canTargetCondition: cardSource => CanJogressWithHandOrTrash(cardSource, owner, isWithHandCard, isIntoHandCard, targetCardCondition, permanentCondition, digivolutionCardCondition),
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                maxCount: 1,
                canNoSelect: isOptional,
                canEndNotMax: false,
                isShowOpponent: true,
                selectCardCoroutine: SelectDNACardCoroutine,
                afterSelectCardCoroutine: null,
                mode: SelectHandEffect.Mode.Custom,
                cardEffect: activateClass);

            selectHandEffect.SetUpCustomMessage("Select 1 Digimon to DNA digivolve.", "The opponent is selecting 1 Digimon to DNA digivolve.");

            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
        }
        else if (!isIntoHandCard && owner.TrashCards.Some(cardSource => CanJogressWithHandOrTrash(cardSource, owner, isWithHandCard, isIntoHandCard, targetCardCondition, permanentCondition, digivolutionCardCondition)))
        {
            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

            selectCardEffect.SetUp(
            canTargetCondition: cardSource => CanJogressWithHandOrTrash(cardSource, owner, isWithHandCard, isIntoHandCard, targetCardCondition, permanentCondition, digivolutionCardCondition),
            canTargetCondition_ByPreSelecetedList: null,
            canEndSelectCondition: null,
            canNoSelect: () => isOptional,
            selectCardCoroutine: SelectDNACardCoroutine,
            afterSelectCardCoroutine: null,
            message: "Select 1 Digimon to DNA digivolve.",
            maxCount: 1,
            canEndNotMax: false,
            isShowOpponent: false,
            mode: SelectCardEffect.Mode.Custom,
            root: SelectCardEffect.Root.Trash,
            customRootCardList: null,
            canLookReverseCard: true,
            selectPlayer: owner,
            cardEffect: activateClass);

            selectCardEffect.SetNotShowCard();
            selectCardEffect.SetNotAddLog();

            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
        }

        if (dnaTarget == null)
            yield break;

        bool validPermanent = HasMatchConditionPermanent(permanent => PermanentFulfillsRequirement(owner, permanent, dnaTarget, null, isWithHandCard, permanentCondition, digivolutionCardCondition));
        bool validHandOrTrash = isWithHandCard ?
                    owner.HandCards.Some(cardSource => CardFulfillsRequirement(owner, cardSource, dnaTarget, null, permanentCondition, digivolutionCardCondition)) : 
                    owner.TrashCards.Some(cardSource => CardFulfillsRequirement(owner, cardSource, dnaTarget, null, permanentCondition, digivolutionCardCondition));

        if(validPermanent || validHandOrTrash)
        {
            #region select source cards
            if (validPermanent && validHandOrTrash)
            {
                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                {
                    new SelectionElement<bool>(message: $"From Battle Area", value : true, spriteIndex: 0),
                    new SelectionElement<bool>(message: isWithHandCard ? $"From Hand" : $"From Trash", value : false, spriteIndex: 1),
                };

                string selectPlayerMessage = "From where will you select the first digimon?";
                string notSelectPlayerMessage = "The opponent is selecting 1 Digimon to DNA digivolve.";

                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
            }
            else
            {
                GManager.instance.userSelectionManager.SetBool(validPermanent);
            }

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

            bool isPermanentFirst = GManager.instance.userSelectionManager.SelectedBoolValue;
            if (isPermanentFirst)
            {
                yield return ContinuousController.instance.StartCoroutine(SelectPermanent(owner, dnaTarget, null, isOptional, activateClass, isWithHandCard, SelectPermanentCoroutine, permanentCondition, digivolutionCardCondition));

                if (selectedPermanent != null)
                {
                    if (isWithHandCard)
                        yield return ContinuousController.instance.StartCoroutine(SelectHandCard(owner, dnaTarget, selectedPermanent, isOptional, activateClass, SelectCardCoroutine, permanentCondition, digivolutionCardCondition));
                    else 
                        yield return ContinuousController.instance.StartCoroutine(SelectTrashCard(owner, dnaTarget, selectedPermanent, isOptional, activateClass, SelectCardCoroutine, permanentCondition, digivolutionCardCondition));
                    if (selectedCardSource != null)
                    {
                        playedPermanent = PlayTempPermanent(selectedCardSource, true);
                        if (playedPermanent != null) yield return ContinuousController.instance.StartCoroutine(CardObjectController.CreateNewPermanent(playedPermanent, selectedCardSource.PreferredFrame().FrameID));
                    }
                            
                }
            }
            else
            {
                if (isWithHandCard)
                    yield return ContinuousController.instance.StartCoroutine(SelectHandCard(owner, dnaTarget, null, isOptional, activateClass, SelectCardCoroutine, permanentCondition, digivolutionCardCondition));
                else
                    yield return ContinuousController.instance.StartCoroutine(SelectTrashCard(owner, dnaTarget, null, isOptional, activateClass, SelectCardCoroutine, permanentCondition, digivolutionCardCondition));

                if(selectedCardSource != null)
                {
                    playedPermanent = PlayTempPermanent(selectedCardSource, true);
                    if (playedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.CreateNewPermanent(playedPermanent, selectedCardSource.PreferredFrame().FrameID));
                        
                        yield return ContinuousController.instance.StartCoroutine(SelectPermanent(owner, dnaTarget, playedPermanent, isOptional, activateClass, isWithHandCard, SelectPermanentCoroutine, permanentCondition, digivolutionCardCondition));
                    }                  
                }
            }
            #endregion

            if (selectedPermanent != null && playedPermanent != null)
            {
                List<Permanent> orderedRoots = isPermanentFirst ? new List<Permanent>() { selectedPermanent, playedPermanent } : new List<Permanent>() { playedPermanent, selectedPermanent };
                int[] JogressEvoRootsFrameIDs = orderedRoots.Map(permanent => permanent.PermanentFrame.FrameID).ToArray();

                if (dnaTarget.CanJogressFromTargetPermanents(orderedRoots, payCost))
                {
                    PlayCardClass playCard = new PlayCardClass(
                        cardSources: new List<CardSource>() { dnaTarget },
                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                        payCost: payCost,
                        targetPermanent: null,
                        isTapped: false,
                        root: isIntoHandCard ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash,
                        activateETB: true);

                    playCard.SetJogress(JogressEvoRootsFrameIDs);

                    yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());
                }
            }
        }
        if (dnaTarget == null || dnaTarget.PermanentOfThisCard() == null)
        {
            if (playedPermanent != null)
            {
                if (isWithHandCard)
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCard(selectedCardSource, false));
                else
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(selectedCardSource));
            }
        }
    }

    public static IEnumerator DNADigivolvePermanentsIntoHandOrTrashCard(
        Func<CardSource, bool> canSelectDNACardCondition,
        bool payCost,
        bool isHand,
        ICardEffect activateClass,
        Func<Permanent, bool>[] permanentConditions = null,
        IEnumerator successProcess = null,
        bool ignoreSelection = false,
        IEnumerator failedProcess = null,
        bool isOptional = true)
    {
        Player owner = activateClass.EffectSourceCard.Owner;
        CardSource dnaTarget = null;

        IEnumerator SelectCardCoroutine(CardSource cardSource)
        {
            dnaTarget = cardSource;

            yield return null;
        }

        bool CanJogressCondition(CardSource cardSource)
        {
            return (canSelectDNACardCondition == null || canSelectDNACardCondition(cardSource))
                && cardSource.CanPlayJogress(true);
        }

        int maxCount = 1;

        if (ignoreSelection)
        {
            dnaTarget = activateClass.EffectSourceCard;
        }
        else if (isHand && owner.HandCards.Some(canSelectDNACardCondition))
        {
            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

            selectHandEffect.SetUp(
                selectPlayer: owner,
                canTargetCondition: canSelectDNACardCondition,
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                maxCount: maxCount,
                canNoSelect: isOptional,
                canEndNotMax: false,
                isShowOpponent: true,
                selectCardCoroutine: SelectCardCoroutine,
                afterSelectCardCoroutine: null,
                mode: SelectHandEffect.Mode.Custom,
                cardEffect: activateClass);

            selectHandEffect.SetUpCustomMessage("Select 1 card to DNA digivolve.", "The opponent is selecting 1 card to DNA digivolve.");
            selectHandEffect.SetNotShowCard();

            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
        } 
        else if(!isHand && owner.TrashCards.Some(canSelectDNACardCondition))
        {
            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                selectCardEffect.SetUp(
                    canTargetCondition: canSelectDNACardCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    canNoSelect: () => isOptional,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: null,
                    message: "Select 1 card to digivolve.",
                    maxCount: maxCount,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    mode: SelectCardEffect.Mode.Custom,
                    root: SelectCardEffect.Root.Trash,
                    customRootCardList: null,
                    canLookReverseCard: true,
                    selectPlayer: owner,
                    cardEffect: activateClass);

            selectCardEffect.SetUpCustomMessage("Select 1 card to DNA digivolve.", "The opponent is selecting 1 card to DNA digivolve.");
            selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");

            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
        }

        if (dnaTarget != null)
        {
            Component component = activateClass.EffectSourceCard.cEntity_EffectController.gameObject.AddComponent(typeof (SetJogressEvoRootsController));
            SetJogressEvoRootsController controller = (SetJogressEvoRootsController)component;
            int[] _jogressEvoRootsFrameIDs = new int[0];

            yield return GManager.instance.photonWaitController.StartWait("DNA_Digivolve_by_Effect");

            if (owner.isYou || GManager.instance.IsAI)
            {
                GManager.instance.selectJogressEffect.SetUp_SelectDigivolutionRoots
                                            (card: dnaTarget,
                                            isLocal: true,
                                            isPayCost: true,
                                            canNoSelect: true,
                                            endSelectCoroutine_SelectDigivolutionRoots: EndSelectCoroutine_SelectDigivolutionRoots,
                                            noSelectCoroutine: null);

                if(permanentConditions != null)
                    GManager.instance.selectJogressEffect.SetUpCustomPermanentConditions(permanentConditions);

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.selectJogressEffect.SelectDigivolutionRoots());

                IEnumerator EndSelectCoroutine_SelectDigivolutionRoots(List<Permanent> permanents)
                {
                    if (permanents.Count == 2)
                    {
                        _jogressEvoRootsFrameIDs = permanents.Distinct().ToArray().Map(permanent => permanent.PermanentFrame.FrameID);
                    }

                    yield return null;
                }

                controller.photonView.RPC("SetJogressEvoRootsFrameIDs", RpcTarget.All, _jogressEvoRootsFrameIDs);
            }
            else
            {
                GManager.instance.commandText.OpenCommandText("The opponent is choosing a card to DNA digivolve.");
            }

            yield return new WaitWhile(() => !controller.EndSelect);
            controller.EndSelect = false;

            GManager.instance.commandText.CloseCommandText();
            yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

            if (controller.JogressEvoRootsFrameIDs.Length == 2)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { dnaTarget }, "Played Card", true, true));

                PlayCardClass playCard = new PlayCardClass(
                    cardSources: new List<CardSource>() { dnaTarget },
                    hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                    payCost: true,
                    targetPermanent: null,
                    isTapped: false,
                    root: isHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash,
                    activateETB: true);

                playCard.SetJogress(controller.JogressEvoRootsFrameIDs);

                yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());
            }
        }
    }

    //Private class used to register the callback so this doesn't need to be defined in every card that uses DNA by effect
    private class SetJogressEvoRootsController : MonoBehaviourPunCallbacks
    {
        public bool EndSelect = false;
        public int[] JogressEvoRootsFrameIDs = new int[0];

        [PunRPC]
        public void SetJogressEvoRootsFrameIDs(int[] JogressEvoRootsFrameIDs)
        {
            this.JogressEvoRootsFrameIDs = JogressEvoRootsFrameIDs;
            EndSelect = true;
        }
    } 
}