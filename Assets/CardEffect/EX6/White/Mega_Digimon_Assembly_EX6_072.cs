using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.EX6
{
    public class Mega_Digimon_Assembly_EX6_072 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Color Condition
            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);

                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (card.Owner.Enemy.GetBattleAreaDigimons().Count((permanent) => permanent.TopCard.Level == 6) >= 1)
                    {
                        return true;
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        return true;
                    }

                    return false;
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                //COPY from blast DNA
                /*if (card == null) return null;
                if (!CardEffectCommons.IsExistOnHand(card)) return null;
                if (card.Owner.GetBattleAreaPermanents().Count == 0) return null;
                if (card.Owner.HandCards.Count < 2) return null;

                List<Permanent> fieldPermanents = card.Owner.GetBattleAreaDigimons();
                List<Permanent> permanentSources = new List<Permanent>();
                List<CardSource> handSources = new List<CardSource>();

                foreach (BlastDNACondition DNACondition in blastDNAConditions)
                {
                    DNACondition.Permanents = fieldPermanents.Filter(permanent => permanent.TopCard.ContainsCardName(DNACondition.Name));
                    DNACondition.CardSources = card.Owner.HandCards.Filter(cardSource => cardSource.ContainsCardName(DNACondition.Name));

                    permanentSources.AddRange(DNACondition.Permanents);
                    handSources.AddRange(DNACondition.CardSources);
                }

                FilterDNAPermanents();
                FilterDNAHandSources();

                void FilterDNAPermanents()
                {
                    if (blastDNAConditions[0].Permanents.Count == 1)
                        blastDNAConditions[1].Permanents = blastDNAConditions[0].Permanents.Except(blastDNAConditions[1].Permanents).ToList();

                    if (blastDNAConditions[1].Permanents.Count == 1)
                        blastDNAConditions[0].Permanents = blastDNAConditions[1].Permanents.Except(blastDNAConditions[0].Permanents).ToList();
                }

                void FilterDNAHandSources()
                {
                    if (blastDNAConditions[0].CardSources.Count == 1)
                        blastDNAConditions[1].CardSources = blastDNAConditions[0].CardSources.Except(blastDNAConditions[1].CardSources).ToList();

                    if (blastDNAConditions[1].CardSources.Count == 1)
                        blastDNAConditions[0].CardSources = blastDNAConditions[1].CardSources.Except(blastDNAConditions[0].CardSources).ToList();
                }

                bool HasValidDNATargets()
                {
                    if (blastDNAConditions[0].Permanents.Count > 0 && blastDNAConditions[1].CardSources.Count > 0)
                        return true;

                    if (blastDNAConditions[0].CardSources.Count > 0 && blastDNAConditions[1].Permanents.Count > 0)
                        return true;

                    return false;
                }*/

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DNA Digivolve", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, DataBase.BlastDNADigivolveEffectDiscription());

                bool HasLevel7WithDNA(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if(cardSource.HasLevel && cardSource.Level == 7)
                        {
                            if (cardSource.jogressCondition != null)
                                return true;
                        }
                    }
                    return false;
                }

                bool HasLevel6HandSource(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasLevel && cardSource.Level == 6)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool HasLevel6Permanent(Permanent permanent)
                {
                    if (permanent.IsDigimon)
                    {
                        if (permanent.TopCard.HasLevel && permanent.Level == 6)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if(CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card))
                    {
                        if(CardEffectCommons.HasMatchConditionOwnersHand(card, HasLevel7WithDNA))
                        {
                            if(CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasLevel6Permanent))
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersHand(card, HasLevel6HandSource))
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
                    CardSource selectedLevel7 = null;
                    Permanent selectedPermanent = null;
                    CardSource selectedCardSource = null;

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, HasLevel7WithDNA));

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: HasLevel7WithDNA,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: SelectDNACoroutine,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectHandEffect.SetUpCustomMessage("Select 1 Digimon to DNA digivolve.", "The opponent is selecting 1 Digimon to DNA digivolve.");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectDNACoroutine(CardSource cardSource)
                    {
                        selectedLevel7 = cardSource;

                        yield return null;
                    }

                    //COpy from Blast DNA
                    /*
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

                        foreach (string name in selectedPermanent.TopCard.CardNames)
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

                        foreach (FieldCardFrame fieldCardFrame in selectedCardSource.Owner.fieldCardFrames)
                        {
                            if (card.CanPlayCardTargetFrame(fieldCardFrame, false, null))
                            {
                                if (fieldCardFrame.IsEmptyFrame())
                                {
                                    frameID = fieldCardFrame.FrameID;
                                    break;
                                }
                            }
                        }

                        if (0 <= frameID && frameID < card.Owner.fieldCardFrames.Count)
                        {
                            playedPermanent = new Permanent(new List<CardSource>() { selectedCardSource }) { IsSuspended = false };

                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.CreateNewPermanent(playedPermanent, frameID));
                        }

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
                    }*/
                }
            }
            #endregion

            #region Security
            if(timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 Digimon from trash to hand then add this card to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Return 1 level 6 or higher Digimon card from your trash to the hand. Then, add this card to the hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasLevel && cardSource.Level == 6)
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

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
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
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanSelectCardCondition));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 Digimon to add to your hand.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.AddHand,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }        
            #endregion

            return cardEffects;
        }
    }
}