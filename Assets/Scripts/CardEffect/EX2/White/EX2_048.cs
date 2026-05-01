using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.EX2
{
    public class EX2_048 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [ADR-02 Searcher] under your [Mother D-Reaper]'s digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] You may place 1 of your [ADR-02 Searcher]s from in play or from your hand under 1 of your [Mother D-Reaper]s as its bottom digivolution card.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (!permanent.IsToken)
                        {
                            if (permanent.TopCard.CardNames.Contains("ADR-02 Searcher"))
                            {
                                return true;
                            }

                            if (permanent.TopCard.CardNames.Contains("ADR-02Searcher"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (!cardSource.IsToken)
                    {
                        if (cardSource.CardNames.Contains("ADR-02 Searcher"))
                        {
                            return true;
                        }

                        if (cardSource.CardNames.Contains("ADR-02Searcher"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (!permanent.IsToken)
                        {
                            if (permanent.TopCard.CardNames.Contains("Mother D-Reaper"))
                            {
                                return true;
                            }

                            if (permanent.TopCard.CardNames.Contains("MotherD-Reaper"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (card.Owner.ExecutingCards.Contains(card))
                    {
                        CardSource cardSource = CardEffectCommons.GetCardFromHashtable(hashtable);

                        if (cardSource == card)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (card.Owner.ExecutingCards.Contains(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition) || card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition) && card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                            {
                                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                                    {
                                        new SelectionElement<bool>(message: $"From Field", value : false, spriteIndex: 0),
                                        new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 1),
                                    };

                                string selectPlayerMessage = "From which area do you play a card?";
                                string notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

                                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                            }

                            else if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition) && card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                            {
                                GManager.instance.userSelectionManager.SetBool(true);
                            }

                            else if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition) && card.Owner.HandCards.Count(CanSelectCardCondition) == 0)
                            {
                                GManager.instance.userSelectionManager.SetBool(false);
                            }

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                            List<CardSource> selectedCards = new List<CardSource>();
                            Permanent selectedPermanent = null;

                            if (fromHand)
                            {
                                if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                                {
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

                                    selectHandEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                    //selectHandEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                    yield return StartCoroutine(selectHandEffect.Activate());

                                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                                    {
                                        selectedCards.Add(cardSource);

                                        yield return null;
                                    }


                                }
                            }

                            else
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                                {
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

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place in digivolution cards.", "The opponent is selecting 1 Digimon to place in digivolution cards.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        selectedPermanent = permanent;

                                        yield return null;
                                    }
                                }
                            }

                            Permanent getDigivolutiuonDigimon = null;

                            if ((fromHand && selectedCards.Count >= 1) || (!fromHand && selectedPermanent != null))
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                                {
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

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get a digivolution card.", "The opponent is selecting 1 Digimon that will get a digivolution card.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        getDigivolutiuonDigimon = permanent;

                                        yield return null;
                                    }
                                }
                            }

                            if (getDigivolutiuonDigimon != null)
                            {
                                if (!getDigivolutiuonDigimon.IsToken)
                                {
                                    if (fromHand)
                                    {
                                        if (selectedCards.Count >= 1)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(getDigivolutiuonDigimon.AddDigivolutionCardsBottom(selectedCards, activateClass));
                                        }
                                    }

                                    else
                                    {
                                        if (selectedPermanent != null)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { selectedPermanent, getDigivolutiuonDigimon } }, false, activateClass).PlacePermanentToDigivolutionCards());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [ADR-02 Searcher] under your [Mother D-Reaper]'s digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may place 1 of your [ADR-02 Searcher]s from in play or from your hand under 1 of your [Mother D-Reaper]s as its bottom digivolution card.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (permanent != null)
                    {
                        if (permanent.TopCard != null)
                        {
                            if (permanent.IsDigimon)
                            {
                                if (permanent.TopCard.Owner == card.Owner)
                                {
                                    if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                    {
                                        if (!permanent.IsToken)
                                        {
                                            if (permanent.TopCard.CardNames.Contains("ADR-02 Searcher"))
                                            {
                                                return true;
                                            }

                                            if (permanent.TopCard.CardNames.Contains("ADR-02Searcher"))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (!cardSource.IsToken)
                            {
                                if (cardSource.CardNames.Contains("ADR-02 Searcher"))
                                {
                                    return true;
                                }

                                if (cardSource.CardNames.Contains("ADR-02Searcher"))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (permanent != null)
                    {
                        if (permanent.TopCard != null)
                        {
                            if (permanent.IsDigimon)
                            {
                                if (permanent.TopCard.Owner == card.Owner)
                                {
                                    if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                    {
                                        if (!permanent.IsToken)
                                        {
                                            if (permanent.TopCard.CardNames.Contains("Mother D-Reaper"))
                                            {
                                                return true;
                                            }

                                            if (permanent.TopCard.CardNames.Contains("MotherD-Reaper"))
                                            {
                                                return true;
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
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition) || card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                            {
                                return true;
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
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition) || card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                                {
                                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition) && card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                                    {
                                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                                    {
                                        new SelectionElement<bool>(message: $"From Field", value : false, spriteIndex: 0),
                                        new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 1),
                                    };

                                        string selectPlayerMessage = "From which area do you play a card?";
                                        string notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

                                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                                    }

                                    else if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition) && card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                                    {
                                        GManager.instance.userSelectionManager.SetBool(true);
                                    }

                                    else if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition) && card.Owner.HandCards.Count(CanSelectCardCondition) == 0)
                                    {
                                        GManager.instance.userSelectionManager.SetBool(false);
                                    }

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                                    bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                                    List<CardSource> selectedCards = new List<CardSource>();
                                    Permanent selectedPermanent = null;

                                    if (fromHand)
                                    {
                                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                                        {


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

                                            selectHandEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                                            //selectHandEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                                            yield return StartCoroutine(selectHandEffect.Activate());

                                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                                            {
                                                selectedCards.Add(cardSource);

                                                yield return null;
                                            }
                                        }
                                    }

                                    else
                                    {
                                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                                        {
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

                                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place in digivolution cards.", "The opponent is selecting 1 Digimon to place in digivolution cards.");

                                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                            {
                                                selectedPermanent = permanent;

                                                yield return null;
                                            }
                                        }
                                    }

                                    Permanent getDigivolutiuonDigimon = null;

                                    if ((fromHand && selectedCards.Count >= 1) || (!fromHand && selectedPermanent != null))
                                    {
                                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                                        {
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

                                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get a digivolution card.", "The opponent is selecting 1 Digimon that will get a digivolution card.");

                                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                            {
                                                getDigivolutiuonDigimon = permanent;

                                                yield return null;
                                            }
                                        }
                                    }

                                    if (getDigivolutiuonDigimon != null)
                                    {
                                        if (!getDigivolutiuonDigimon.IsToken)
                                        {
                                            if (fromHand)
                                            {
                                                if (selectedCards.Count >= 1)
                                                {
                                                    yield return ContinuousController.instance.StartCoroutine(getDigivolutiuonDigimon.AddDigivolutionCardsBottom(selectedCards, activateClass));
                                                }
                                            }

                                            else
                                            {
                                                if (selectedPermanent != null)
                                                {
                                                    yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { selectedPermanent, getDigivolutiuonDigimon } }, false, activateClass).PlacePermanentToDigivolutionCards());
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

            return cardEffects;
        }
    }
}