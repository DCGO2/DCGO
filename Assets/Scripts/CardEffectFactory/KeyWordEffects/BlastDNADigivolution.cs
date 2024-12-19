using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public class BlastDNACondition
{
    public string Name;
    public List<Permanent> Permanents;
    public List<CardSource> CardSources;

    public BlastDNACondition(string name)
    {
        Name = name;
        Permanents = new List<Permanent>();
        CardSources = new List<CardSource>();
    }
}

public partial class CardEffectFactory
{
    #region Trigger effect of [Blast DNA Digivolve]
    public static ActivateClass BlastDNADigivolveEffect(CardSource card, List<BlastDNACondition> blastDNAConditions, Func<bool> condition)
    {
        if (card == null) return null;
        if (!CardEffectCommons.IsExistOnHand(card)) return null;
        if (card.Owner.GetBattleAreaPermanents().Count == 0) return null;
        if (card.Owner.HandCards.Count < 2) return null;

        List<Permanent> fieldPermanents = new List<Permanent>();
        List<Permanent> permanentSources = new List<Permanent>();
        List<CardSource> handSources = new List<CardSource>();

        void FilterDNAPermanents()
        {
            if (blastDNAConditions[0].Permanents.Count >= 1 && blastDNAConditions[0].CardSources.Count == 0)
                blastDNAConditions[1].Permanents.Clear();

            if (blastDNAConditions[1].Permanents.Count >= 1 && blastDNAConditions[1].CardSources.Count == 0)
                blastDNAConditions[0].Permanents.Clear();
        }

        void FilterDNAHandSources()
        {
            if (blastDNAConditions[0].CardSources.Count >= 1 && blastDNAConditions[0].CardSources.Count == 0)
                blastDNAConditions[1].CardSources.Clear();

            if (blastDNAConditions[1].CardSources.Count >= 1 && blastDNAConditions[1].CardSources.Count == 0)
                blastDNAConditions[0].CardSources.Clear();
        }

        bool HasValidDNATargets()
        {
            fieldPermanents = card.Owner.GetBattleAreaDigimons();

            foreach (BlastDNACondition DNACondition in blastDNAConditions)
            {
                DNACondition.Permanents = fieldPermanents.Filter(permanent => permanent.TopCard.EqualsCardName(DNACondition.Name));
                DNACondition.CardSources = card.Owner.HandCards.Filter(cardSource => cardSource.EqualsCardName(DNACondition.Name));

                permanentSources.AddRange(DNACondition.Permanents);
                handSources.AddRange(DNACondition.CardSources);
            }

            FilterDNAPermanents();
            FilterDNAHandSources();

            if (blastDNAConditions[0].Permanents.Count(permanent => !permanent.TopCard.CanNotEvolve(permanent)) > 0 && blastDNAConditions[1].CardSources.Count > 0)
                return true;

            if (blastDNAConditions[0].CardSources.Count > 0 && blastDNAConditions[1].Permanents.Count(permanent => !permanent.TopCard.CanNotEvolve(permanent)) > 0)
                return true;

            return false;
        }

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Blast DNA Digivolve", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, DataBase.BlastDNADigivolveEffectDiscription());
        activateClass.SetIsCounterEffect(true);

        bool CanSelectPermanent(Permanent permanent)
        {
            if(CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
            {
                foreach (BlastDNACondition DNACondition in blastDNAConditions)
                {
                    if (DNACondition.Permanents.Contains(permanent))
                        return true;
                }
            }

            return false;
        }

        bool CanSelectHandSource(CardSource cardSource)
        {
            return handSources.Contains(cardSource);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, permanent => CardEffectCommons.IsOpponentPermanent(permanent, card)))
            {
                if (card.Owner.HandCards.Contains(card))
                {
                    if (condition == null || condition())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            if (card.Owner.HandCards.Contains(card))
            {
                if (HasValidDNATargets())
                {
                    if (condition == null || condition())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            Permanent selectedPermanent = null;
            CardSource selectedCardSource = null;

            int maxCount = Math.Min(1, permanentSources.Count);

            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

            selectPermanentEffect.SetUp(
                selectPlayer: card.Owner,
                canTargetCondition: CanSelectPermanent,
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                maxCount: maxCount,
                canNoSelect: false,
                canEndNotMax: false,
                selectPermanentCoroutine: SelectPermanentCoroutine,
                afterSelectPermanentCoroutine: null,
                mode: SelectPermanentEffect.Mode.Custom,
                cardEffect: activateClass);

            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to DNA digivolve.", "The opponent is selecting 1 Digimon to DNA digivolve.");

            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

            IEnumerator SelectPermanentCoroutine(Permanent permanent)
            {
                selectedPermanent = permanent;

                foreach(string name in selectedPermanent.TopCard.CardNames)
                {
                    handSources = handSources.Filter(source => !source.ContainsCardName(name));
                }

                maxCount = Math.Min(1, handSources.Count);

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectHandSource,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: false,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: null,
                    mode: SelectHandEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectHandEffect.SetUpCustomMessage("Select 1 Digimon to DNA digivolve.", "The opponent is selecting 1 Digimon to DNA digivolve.");

                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
            }

            IEnumerator SelectCardCoroutine(CardSource cardSource)
            {
                selectedCardSource = cardSource;

                Permanent playedPermanent;
                int frameID = -1;

                FieldCardFrame preferredFrame = selectedCardSource.PreferredFrame();

                if (preferredFrame != null)
                {
                    frameID = preferredFrame.FrameID;
                }

                if (0 <= frameID && frameID < card.Owner.fieldCardFrames.Count)
                {
                    playedPermanent = new Permanent(new List<CardSource>() { selectedCardSource }) { IsSuspended = false };

                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.CreateNewPermanent(playedPermanent, frameID));
                }

                int[] JogressEvoRootsFrameIDs = { 0, 0 };

                if (selectedPermanent.TopCard.EqualsCardName(blastDNAConditions[0].Name))
                {
                    JogressEvoRootsFrameIDs[0] = selectedPermanent.PermanentFrame.FrameID;
                    JogressEvoRootsFrameIDs[1] = selectedCardSource.PermanentOfThisCard().PermanentFrame.FrameID;
                }
                else
                {
                    JogressEvoRootsFrameIDs[0] = selectedCardSource.PermanentOfThisCard().PermanentFrame.FrameID;
                    JogressEvoRootsFrameIDs[1] = selectedPermanent.PermanentFrame.FrameID;
                }

                if (card.CanPlayJogress(true))
                {
                    PlayCardClass playCard = new PlayCardClass(
                        cardSources: new List<CardSource>() { card },
                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                        payCost: true,
                        targetPermanent: null,
                        isTapped: false,
                        root: SelectCardEffect.Root.Hand,
                        activateETB: true);

                    playCard.SetJogress(JogressEvoRootsFrameIDs);

                    yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());

                    foreach (BlastDNACondition DNACondition in blastDNAConditions)
                    {
                        DNACondition.Permanents = new List<Permanent>();
                        DNACondition.CardSources = new List<CardSource>();
                    }
                }
                else
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCard(selectedCardSource, false));
                }
                
            }
        }

        return activateClass;
    }
    #endregion

}