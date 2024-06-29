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

        List<Permanent> permanentSources = card.Owner.GetBattleAreaDigimons()
                    .Clone()
                    .Filter(permanent => permanent.TopCard.ContainsCardName(blastDNAConditions[0].Name)
                                      || permanent.TopCard.ContainsCardName(blastDNAConditions[1].Name));

        List<CardSource> handSources = card.Owner.HandCards
                    .Clone()
                    .Filter(cardSource => cardSource.ContainsCardName(blastDNAConditions[0].Name)
                                       || cardSource.ContainsCardName(blastDNAConditions[1].Name));

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Blast DNA Digivolve", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, DataBase.BlastDNADigivolveEffectDiscription());
        activateClass.SetIsCounterEffect(true);

        bool CanSelectPermanent(Permanent permanent)
        {
            return permanentSources.Contains(permanent);
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
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanent))
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

            /*permanentSources.ForEach(permanent =>
            {
                if (permanent.TopCard.ContainsCardName(blastDNAConditions[0].Name))
                    blastDNAConditions[0].Permanents.Add(permanent);

                if (permanent.TopCard.ContainsCardName(blastDNAConditions[1].Name))
                    blastDNAConditions[1].Permanents.Add(permanent);
            });

            handSources.ForEach(cardSource =>
            {
                if (cardSource.ContainsCardName(blastDNAConditions[0].Name))
                    blastDNAConditions[0].CardSources.Add(cardSource);

                if (cardSource.ContainsCardName(blastDNAConditions[1].Name))
                    blastDNAConditions[1].CardSources.Add(cardSource);
            });*/

            yield return null;
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
                    handSources = handSources.Filter(source => source.ContainsCardName(name));

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

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to DNA digivolve.", "The opponent is selecting 1 Digimon to DNA digivolve.");

                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
            }

            IEnumerator SelectCardCoroutine(CardSource cardSource)
            {
                selectedCardSource = cardSource;

                PlayCardClass playCardClass = new PlayCardClass(
                        cardSources: new List<CardSource>() { selectedCardSource },
                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                        payCost: false,
                        targetPermanent: null,
                        isTapped: false,
                        root: SelectCardEffect.Root.Hand,
                        activateETB: false);

                yield return ContinuousController.instance.StartCoroutine(playCardClass.PlayCard());

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { card }, "Played Card", true, true));

                int[] JogressEvoRootsFrameIDs = { selectedPermanent.PermanentFrame.FrameID, selectedCardSource.PermanentOfThisCard().PermanentFrame.FrameID };

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
            }

            #region Blast Digivolve
            /*Permanent selectedPermanent = null;

            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

            selectPermanentEffect.SetUp(
                selectPlayer: card.Owner,
                canTargetCondition: CanSelectPermanentCondition,
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                maxCount: maxCount,
                canNoSelect: false,
                canEndNotMax: false,
                selectPermanentCoroutine: SelectPermanentCoroutine,
                afterSelectPermanentCoroutine: null,
                mode: SelectPermanentEffect.Mode.Custom,
                cardEffect: activateClass);

            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to digivolve.", "The opponent is selecting 1 Digimon to digivolve.");

            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

            IEnumerator SelectPermanentCoroutine(Permanent permanent)
            {
                selectedPermanent = permanent;

                yield return null;
            }

            if (selectedPermanent != null)
            {
                if (card.CanPlayCardTargetFrame(selectedPermanent.PermanentFrame, false, activateClass))
                {
                    PlayCardClass playCardClass = new PlayCardClass(
                        cardSources: new List<CardSource>() { card },
                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                        payCost: false,
                        targetPermanent: selectedPermanent,
                        isTapped: false,
                        root: SelectCardEffect.Root.Hand,
                        activateETB: true);

                    yield return ContinuousController.instance.StartCoroutine(playCardClass.PlayCard());
                }
            }*/
            #endregion
        }

        return activateClass;
    }
    #endregion

}