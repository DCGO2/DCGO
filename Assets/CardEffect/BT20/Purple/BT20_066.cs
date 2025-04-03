using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT20
{
    public class BT20_066 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete digimon and digivolve into ImperialDramon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Delete 1 of your opponent's level 3 Digimon. Then, if it's your turn, 2 of your Digimon may DNA digivolve into a Digimon card with [Imperialdramon] in its name or the [Free] trait in the hand.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.Level == 3)
                    {
                        if (permanent.TopCard.HasLevel)
                        {
                            if (permanent.CanSelectBySkill(activateClass))
                            {
                                return true;
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
                if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                {
                    return true;
                }
                return false;
                }
                
                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if(cardSource.CardTraits.Contains("Free") || cardSource.ContainsCardName("Imperialdramon"))
                            {                                                        
                                if (cardSource.CanPlayJogress(true))
                                {   
                                    return true;                                                                        
                                }
                            }                        
                        }
                    }
                    }

                    return false;                
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {                   
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = 1;

                        selectPermanentEffect.SetUp(
                            
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    
                    }

                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(card.PermanentOfThisCard(), card))
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
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
                                        if (selectedCard.CanPlayJogress(true))
                                        {
                                            JogressEvoRootsFrameIDs = new int[0];

                                            yield return GManager.instance.photonWaitController.StartWait("StingmonBT20_066");

                                            if (card.Owner.isYou || GManager.instance.IsAI)
                                            {
                                                GManager.instance.selectJogressEffect.SetUp_SelectDigivolutionRoots
                                                    (card: selectedCard,
                                                    isLocal: true,
                                                    isPayCost: true,
                                                    canNoSelect: true,
                                                    endSelectCoroutine_SelectDigivolutionRoots: EndSelectCoroutine_SelectDigivolutionRoots,
                                                    noSelectCoroutine: null);

                                                GManager.instance.selectJogressEffect.SetUpCustomPermanentConditions(new Func<Permanent, bool>[] { (permanent) => permanent.TopCard.Owner == card.Owner });

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
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete digimon and digivolve into ImperialDramon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Delete 1 of your opponent's level 3 Digimon. Then, if it's your turn, 2 of your Digimon may DNA digivolve into a Digimon card with [Imperialdramon] in its name or the [Free] trait in the hand.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.Level == 3)
                    {
                        if (permanent.TopCard.HasLevel)
                        {
                            if (permanent.CanSelectBySkill(activateClass))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);                    
                }

                
                bool CanActivateCondition(Hashtable hashtable)
                {
                if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                {
                    return true;
                }
                return false;
                }
                
                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if(cardSource.CardTraits.Contains("Free") || cardSource.ContainsCardName("Imperialdramon"))
                            {        
                                                
                                if (cardSource.CanPlayJogress(true))
                                {       
                                    return true;
                                }
                            }
                        
                        }
                    }
                    }

                    return false;                
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {                   
                        

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = 1;

                        selectPermanentEffect.SetUp(
                            
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    
                    }

                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(card.PermanentOfThisCard(), card))
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
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
                                        if (selectedCard.CanPlayJogress(true))
                                        {
                                            JogressEvoRootsFrameIDs = new int[0];

                                            yield return GManager.instance.photonWaitController.StartWait("StingmonBT20_066");

                                            if (card.Owner.isYou || GManager.instance.IsAI)
                                            {
                                                GManager.instance.selectJogressEffect.SetUp_SelectDigivolutionRoots
                                                    (card: selectedCard,
                                                    isLocal: true,
                                                    isPayCost: true,
                                                    canNoSelect: true,
                                                    endSelectCoroutine_SelectDigivolutionRoots: EndSelectCoroutine_SelectDigivolutionRoots,
                                                    noSelectCoroutine: null);

                                                GManager.instance.selectJogressEffect.SetUpCustomPermanentConditions(new Func<Permanent, bool>[] { (permanent) => permanent.TopCard.Owner == card.Owner });

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
            #endregion

            #region InheritedEffects
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

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