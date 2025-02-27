using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT20
{
    public class BT20_016 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            #region DNA Digivolve
            if (timing == EffectTiming.None)
            {
            AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
            addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
            addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
            addJogressConditionClass.SetNotShowUI(true);
            cardEffects.Add(addJogressConditionClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return true;
            }



            JogressCondition GetJogress(CardSource cardSource)
            {
                if (cardSource == card)
                {
                    bool PermanentCondition1(Permanent permanent)
                    {
                        if (permanent != null)
                        {
                            if (permanent.TopCard != null)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))

                                {
                                            if (permanent.TopCard.CardColors.Contains(CardColor.Purple))
                                            {
                                                if (permanent.Levels_ForJogress(card).Contains(4))
                                                {
                                                    return true;
                                                }
                                            }
                                }
                            }
                        }

                        return false;
                    }

                    bool PermanentCondition2(Permanent permanent)
                    {
                        if (permanent != null)
                        {
                            if (permanent.TopCard != null)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                                {                                
                                            if (permanent.TopCard.CardColors.Contains(CardColor.Red))
                                            {
                                                if (permanent.Levels_ForJogress(card).Contains(4))
                                                {
                                                    return true;
                                                }
                                            }
                                }
                            }
                        }

                        return false;
                    }

                    JogressConditionElement[] elements = new JogressConditionElement[]
                    {
                        new JogressConditionElement(PermanentCondition1, "a level 4 red Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 4 purple Digimon"),
                    };

                    JogressCondition jogressCondition = new JogressCondition(elements, 0);

                    return jogressCondition;
                }

                return null;
            }
            }
            
            
            #endregion


            #region On Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 Digimon gains Piercing and 4000DP, then this digimon may attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] For the turn, 1 of your Digimon gains ＜Piercing＞ (When this Digimon attacks and deletes an opponent's Digimon and survives the battle, it performs any security checks it normally would) and gets +4000 DP. Then, this Digimon may attack. [All Turns] When any of your [Paildramon]/[Dinobeemon] would be deleted, 2 of your Digimon may DNA digivolve into [Imperialdramon: Dragon Mode] in the hand.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable,card);
                }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
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

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Digimon that will gain Pierce and 4000DP.",
                        "The opponent is selecting 1 Digimon that will gain Pierce and 4000DP.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(
                            targetPermanent: permanent,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: permanent,
                                changeValue: 4000,
                                effectDuration: EffectDuration.UntilEachTurnEnd, 
                                activateClass: activateClass));

                    }
                }
            }
            ActivateClass activateAttackClass = new ActivateClass();
                activateAttackClass.SetUpICardEffect("This Digimon may attack", CanUseConditionA, card);
                activateAttackClass.SetUpActivateClass(CanActivateConditionA, ActivateCoroutineA, 1, true, EffectDiscriptionA());
                cardEffects.Add(activateAttackClass);

                string EffectDiscriptionA()
                {
                    return "This Digimon may attack";
                }

                bool CanUseConditionA(Hashtable hashtable)
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

                bool CanActivateConditionA(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().CanAttack(activateAttackClass);
                }

                IEnumerator ActivateCoroutineA(Hashtable hashtable)
                {
                    SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                    selectAttackEffect.SetUp(
                        attacker: card.PermanentOfThisCard(),
                        canAttackPlayerCondition: () => true,
                        defenderCondition: (permanent) => false,
                        cardEffect: activateAttackClass);

                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                }
            #endregion
            
            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass1 = new ActivateClass();
                activateClass1.SetUpICardEffect("Piercing and Digivolve into Imperialdramon Dragon Mode", CanUseCondition1, card);
                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, true, EffectDiscription1());
                cardEffects.Add(activateClass1);

                string EffectDiscription1()
                {
                    return "[On Play]For the turn, 1 of your Digimon gains ＜Piercing＞ (When this Digimon attacks and deletes an opponent's Digimon and survives the battle, it performs any security checks it normally would) and gets +4000 DP. Then, this Digimon may attack. [All Turns] When any of your [Paildramon]/[Dinobeemon] would be deleted, 2 of your Digimon may DNA digivolve into [Imperialdramon: Dragon Mode] in the hand.";
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanActivateCondition1(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanUseCondition1(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

            IEnumerator ActivateCoroutine1(Hashtable _hashtable)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
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
                        cardEffect: activateClass1);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Digimon that will gain Pierce and 4000DP.",
                        "The opponent is selecting 1 Digimon that will gain Pierce and 4000DP.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(
                            targetPermanent: permanent,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: permanent,
                                changeValue: 4000,
                                effectDuration: EffectDuration.UntilEachTurnEnd, 
                                activateClass: activateClass));
                                

                    }
                }
            }
            ActivateClass activateAttackClass1 = new ActivateClass();
                activateAttackClass1.SetUpICardEffect("This Digimon may attack", CanUseConditionA1, card);
                activateAttackClass1.SetUpActivateClass(CanActivateConditionA1, ActivateCoroutineA1, 1, true, EffectDiscriptionA1());
                cardEffects.Add(activateAttackClass1);

                string EffectDiscriptionA1()
                {
                    return "This Digimon may attack";
                }

                bool CanUseConditionA1(Hashtable hashtable)
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

                bool CanActivateConditionA1(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().CanAttack(activateAttackClass1);
                }

                IEnumerator ActivateCoroutineA1(Hashtable hashtable)
                {
                    SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                    selectAttackEffect.SetUp(
                        attacker: card.PermanentOfThisCard(),
                        canAttackPlayerCondition: () => true,
                        defenderCondition: (permanent) => false,
                        cardEffect: activateAttackClass1);

                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                }
            }            
            #endregion

             #region All Turns
            if(timing == EffectTiming.None){
                ActivateClass activateClass1 = new ActivateClass();
                activateClass1.SetUpICardEffect("If would be deleted, DNA Digivolve", CanUseCondition1, card);
                activateClass1.SetUpActivateClass(CanActivateCondition2, ActivateCoroutine1, -1, true, EffectDiscription1());
                activateClass1.SetHashString("Dna Digivolve into Imperialdramon Dragon Mode");
                cardEffects.Add(activateClass);

                string EffectDiscription1(){
                    return "[All Turns] When any of your [Paildramon]/[Dinobeemon] would be deleted, 2 of your Digimon may DNA digivolve into [Imperialdramon: Dragon Mode] in the hand.";
                }

                bool isPaildramonOrDinoBeemon(Permanent permanent){
                    
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if(permanent.TopCard.EqualsCardName("Dinobeemon") || permanent.TopCard.EqualsCardName("Paildramon"))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                
                bool CanActivateCondition2(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, isPaildramonOrDinoBeemon);
                }

                bool CanSelectDNACardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.CanPlayJogress(true))
                        {
                            if (cardSource.EqualsCardName("Imperialdramon: Dragon Mode"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
                bool CanUseCondition1(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateCondition1(Hashtable hashtable)
                {                    
                    if (CardEffectCommons.MatchConditionPermanentCount(isPaildramonOrDinoBeemon)>=2){
                        return true;
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine1(Hashtable hashtable)
                {
                    
                    List<CardSource> selectedCards = new List<CardSource>();

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
                                _jogressEvoRootsFrameIDs = new int[0];

                                yield return GManager.instance.photonWaitController.StartWait("Paildramon_BT_016");

                                if (card.Owner.isYou || GManager.instance.IsAI)
                                {
                                    GManager.instance.selectJogressEffect.SetUp_SelectDigivolutionRoots
                                                                (card: selectedCard,
                                                                isLocal: true,
                                                                isPayCost: true,
                                                                canNoSelect: true,
                                                                endSelectCoroutine_SelectDigivolutionRoots: EndSelectCoroutine_SelectDigivolutionRoots,
                                                                noSelectCoroutine: null);

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.selectJogressEffect.SelectDigivolutionRoots());

                                    IEnumerator EndSelectCoroutine_SelectDigivolutionRoots(List<Permanent> permanents)
                                    {
                                        if (permanents.Count == 2)
                                        {
                                            _jogressEvoRootsFrameIDs = permanents.Distinct().ToArray().Map(permanent => permanent.PermanentFrame.FrameID);
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
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { selectedCard }, "Played Card", true, true));

                                    PlayCardClass playCard = new PlayCardClass(
                                        cardSources: new List<CardSource>() { selectedCard },
                                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                        payCost: true,
                                        targetPermanent: null,
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Hand,
                                        activateETB: true);

                                    playCard.SetJogress(_jogressEvoRootsFrameIDs);

                                    yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            
            #region InheritedEffect
            if (timing == EffectTiming.None)
            {               
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: true, card: card, condition: null));
            } 
            #endregion
            
            }
            return cardEffects;
        }
        
    private bool _endSelect = false;
    private int[] _jogressEvoRootsFrameIDs = new int[0];

    [PunRPC]
    public void SetJogressEvoRootsFrameIDs(int[] JogressEvoRootsFrameIDs)
    {
        this._jogressEvoRootsFrameIDs = JogressEvoRootsFrameIDs;
        _endSelect = true;
    }
    }
}


