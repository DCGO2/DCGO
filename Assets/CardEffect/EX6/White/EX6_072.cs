using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.EX6
{
    public class EX6_072 : CEntity_Effect
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
                    if (card.Owner.Enemy.GetBattleAreaDigimons().Count((permanent) => permanent.TopCard.HasLevel && permanent.TopCard.Level >= 6) >= 1)
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
                CardSource selectedLevel7 = null;
                JogressCondition selectedDNACondition = null; 
                List<Permanent> allowedPermanents = new List<Permanent>();
                List<CardSource> allowedCards = new List<CardSource>();

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DNA Digivolve", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, DataBase.BlastDNADigivolveEffectDiscription());
                cardEffects.Add(activateClass);

                bool HasLevel7WithDNA(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasLevel && cardSource.Level == 7)
                        {
                            if (cardSource.jogressCondition.Count > 0)
                                return true;
                        }
                    }
                    return false;
                }

                bool HasLevel6HandSource(CardSource cardSource)
                {
                    return true;
                }

                bool HasLevel6Permanent(Permanent permanent)
                {
                    if (permanent.IsDigimon)
                    {
                        if (permanent.TopCard.HasLevel && permanent.Level == 6)
                        {
                            if (!card.CanNotEvolve(permanent));
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                void FilterAllowableSelections(Permanent selectedPermanent = null) 
                {
                    JogressConditionElement[] elements = (JogressConditionElement[])selectedDNACondition.elements.Clone();

                    allowedPermanents = new List<Permanent>();
                    allowedCards = new List<CardSource>();

                    for (int i = 0; i < elements.Length; i++)
                    {
                        bool added = false;

                        foreach (Permanent permanent in card.Owner.GetBattleAreaPermanents())
                        {
                            if (selectedPermanent != null && permanent != selectedPermanent)
                                continue;

                            if (elements[i].EvoRootCondition(permanent))
                            {
                                allowedPermanents.Add(permanent);
                                added = true;
                            }
                        }

                        if (added)
                        {
                            int inverse = Mathf.Abs(i - 1);

                            foreach (CardSource source in card.Owner.HandCards.Filter(HasLevel6HandSource))
                            {
                                Permanent playedPermanent = new Permanent(new List<CardSource>() { source });

                                card.Owner.FieldPermanents[0] = playedPermanent;

                                if (elements[inverse].EvoRootCondition(playedPermanent))
                                    allowedCards.Add(source);

                                card.Owner.FieldPermanents[0] = null;
                            }
                        }
                    }
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
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

                    if(selectedLevel7 != null)
                    {
                        selectedDNACondition = selectedLevel7.jogressCondition[0];

                        if (selectedLevel7.jogressCondition.Count > 1)
                        {
                            #region select DNA condition
                            SelectDNACondition selectDNACondition = GManager.instance.GetComponent<SelectDNACondition>();
                            selectDNACondition.SetUp(selectedLevel7.Owner, selectedLevel7, SelectDNA);

                            yield return ContinuousController.instance.StartCoroutine(selectDNACondition.Activate());

                            IEnumerator SelectDNA(int dnaSelection)
                            {
                                selectedDNACondition = selectedLevel7.jogressCondition[dnaSelection];

                                yield return null;
                            }
                            #endregion
                        }

                        FilterAllowableSelections();

                        #region Selecting Field Permanent for DNA
                        bool PermanentDNASelection(Permanent permanent)
                        {
                            if (HasLevel6Permanent(permanent))
                            {
                                if(allowedPermanents.Contains(permanent))
                                    return true;
                            }

                            return false;
                        }

                        bool CardDNASelection(CardSource source)
                        {
                            if (HasLevel6HandSource(source))
                            {
                                if (allowedCards.Contains(source))
                                    return true;
                            }

                            return false;
                        }

                        maxCount = Math.Min(1, allowedPermanents.Count);

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: PermanentDNASelection,
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
                        #endregion

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if(selectedPermanent != null)
                        {
                            FilterAllowableSelections(selectedPermanent);

                            maxCount = Math.Min(1, allowedCards.Count);

                            SelectHandEffect selectHandDNAEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandDNAEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CardDNASelection,
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

                            selectHandDNAEffect.SetUpCustomMessage("Select 1 Digimon to DNA digivolve.", "The opponent is selecting 1 Digimon to DNA digivolve.");

                            yield return ContinuousController.instance.StartCoroutine(selectHandDNAEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCardSource = cardSource;

                                yield return null;
                            }
                        }
                    }

                    if(selectedLevel7 != null && selectedPermanent != null && selectedCardSource != null)
                    {
                        
                        Permanent playedPermanent;
                        int frameID = -1;

                        foreach (FieldCardFrame fieldCardFrame in selectedCardSource.Owner.fieldCardFrames)
                        {
                            if (selectedCardSource.CanPlayCardTargetFrame(fieldCardFrame, false, null))
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

                        if (selectedLevel7.CanJogressFromTargetPermanent(selectedPermanent, false))
                        {
                            int[] JogressEvoRootsFrameIDs = { selectedPermanent.PermanentFrame.FrameID, selectedCardSource.PermanentOfThisCard().PermanentFrame.FrameID };

                            PlayCardClass playCard = new PlayCardClass(
                                cardSources: new List<CardSource>() { selectedLevel7 },
                                hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                payCost: true,
                                targetPermanent: null,
                                isTapped: false,
                                root: SelectCardEffect.Root.Hand,
                                activateETB: true);

                            playCard.SetJogress(JogressEvoRootsFrameIDs);

                            yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());
                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCard(selectedCardSource, false));
                        }
                    }
                }
            }
            #endregion

            #region Security
            if(timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 Digimon from trash to hand then add this card to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Return 1 level 6 or higher Digimon card from your trash to the hand. Then, add this card to the hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasLevel && cardSource.Level >= 6)
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
