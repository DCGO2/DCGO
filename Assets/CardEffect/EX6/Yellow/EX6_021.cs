using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX6
{
    public class EX6_021 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region On Play/ When Digivolving Shared

            bool CanSelectPermanentSharedCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }
            
            bool CanSelectCardSharedCondition(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.HasAngelTraitRestrictive)
                    {
                        return true;
                    }
                }
                
                return false;
            }
            
            bool CanActivateSharedCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        return true;
                    }
                    
                    if (card.Owner.HandCards.Count >= 1)
                    {
                        return true;
                    }
                }
                
                return false;
            }
            
            #endregion
            
            #region On Play
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Add 1 card from top or bottom security to hand to -4000 DP an opponent's Digimon, then place 1 card from hand at the bottom of security",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] By adding the top or bottom card of your security stack to the hand, 1 of your opponent's Digimon gets -4000 DP for the turn. Then, you may place 1 Digimon card with the [Angel]/[Archangel]/[Three Great Angels] trait from your hand at the bottom of your security stack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "Security Top", value: 0, spriteIndex: 0),
                            new(message: "Security Bottom", value: 1, spriteIndex: 0),
                            new(message: "No Selection", value: 2, spriteIndex: 1),
                        };
                        
                        string selectPlayerMessage = "Add the top or bottom card of the security stack to hand";
                        string notSelectPlayerMessage =
                            "The opponent is selecting the top or bottom card of security stack.";
                        
                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements,
                            selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                            notSelectPlayerMessage: notSelectPlayerMessage);
                        
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());
                        
                        int selectionValue = GManager.instance.userSelectionManager.SelectedIntValue;
                        
                        if (selectionValue != 2)
                        {
                            CardSource chosenSecurityCard = (selectionValue == 0)
                                ? card.Owner.SecurityCards[0]
                                : card.Owner.SecurityCards[^1];
                            string placementValue = (selectionValue == 0)
                                ? "From Top Of Security"
                                : "From Bottom Of Security";

                            #region Log
                            if (chosenSecurityCard != null)
                            {
                                string log = "";

                                log += $"\nAdded Card {placementValue} to Hand:";
                                log += $"\n{chosenSecurityCard.BaseENGCardNameFromEntity}({chosenSecurityCard.CardID})";

                                log += "\n";

                                PlayLog.OnAddLog?.Invoke(log);
                            }
                            #endregion

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { chosenSecurityCard }, $"Added Hand Card {placementValue}", true, true));
                            yield return ContinuousController.instance.StartCoroutine(
                                CardObjectController.AddHandCards(new List<CardSource>() { chosenSecurityCard }, false,
                                    activateClass));
                            
                            yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                player: card.Owner,
                                refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
                            
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentSharedCondition))
                            {
                                int maxCount = Math.Min(1,
                                    CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentSharedCondition));
                                
                                SelectPermanentEffect selectPermanentEffect =
                                    GManager.instance.GetComponent<SelectPermanentEffect>();
                                
                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentSharedCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);
                                
                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -4000.",
                                    "The opponent is selecting 1 Digimon that will get DP -4000.");
                                
                                yield return ContinuousController.instance.StartCoroutine(
                                    selectPermanentEffect.Activate());
                                
                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(
                                        CardEffectCommons.ChangeDigimonDP(
                                            targetPermanent: permanent,
                                            changeValue: -4000,
                                            effectDuration: EffectDuration.UntilEachTurnEnd,
                                            activateClass: activateClass));
                                }
                            }

                            if (card.Owner.HandCards.Count(CanSelectCardSharedCondition) >= 1)
                            {
                                int maxCount = Math.Min(1, card.Owner.HandCards.Count(CanSelectCardSharedCondition));

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardSharedCondition,
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

                                selectHandEffect.SetUpCustomMessage("Select 1 card to place at the bottom of security.",
                                    "The opponent is selecting 1 card to place at the bottom of security.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Security Bottom Card");

                                yield return StartCoroutine(selectHandEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(
                                        CardObjectController.AddSecurityCard(cardSource, toTop: false));
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
                activateClass.SetUpICardEffect(
                    "Add 1 card from top or bottom security to hand to -4000 DP an opponent's Digimon, then place 1 card from hand at the bottom of security",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateSharedCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] By adding the top or bottom card of your security stack to the hand, 1 of your opponent's Digimon gets -4000 DP for the turn. Then, you may place 1 Digimon card with the [Angel]/[Archangel]/[Three Great Angels] trait from your hand at the bottom of your security stack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "Security Top", value: 0, spriteIndex: 0),
                            new(message: "Security Bottom", value: 1, spriteIndex: 0),
                            new(message: "No Selection", value: 2, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Add the top or bottom card of the security stack to hand";
                        string notSelectPlayerMessage =
                            "The opponent is selecting the top or bottom card of security stack.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements,
                            selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                            notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());

                        int selectionValue = GManager.instance.userSelectionManager.SelectedIntValue;

                        if (selectionValue != 2)
                        {
                            CardSource chosenSecurityCard = (selectionValue == 0)
                                ? card.Owner.SecurityCards[0]
                                : card.Owner.SecurityCards[^1];
                            string placementValue = (selectionValue == 0)
                                ? "From Top Of Security"
                                : "From Bottom Of Security";

                            #region Log
                            if (chosenSecurityCard != null)
                            {
                                string log = "";

                                log += $"\nAdded Card {placementValue} to Hand:";
                                log += $"\n{chosenSecurityCard.BaseENGCardNameFromEntity}({chosenSecurityCard.CardID})";

                                log += "\n";

                                PlayLog.OnAddLog?.Invoke(log);
                            }
                            #endregion

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { chosenSecurityCard }, $"Added Hand Card {placementValue}", true, true));
                            yield return ContinuousController.instance.StartCoroutine(
                                CardObjectController.AddHandCards(new List<CardSource>() { chosenSecurityCard }, false,
                                    activateClass));

                            yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                player: card.Owner,
                                refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentSharedCondition))
                            {
                                int maxCount = Math.Min(1,
                                    CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentSharedCondition));

                                SelectPermanentEffect selectPermanentEffect =
                                    GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentSharedCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -4000.",
                                    "The opponent is selecting 1 Digimon that will get DP -4000.");

                                yield return ContinuousController.instance.StartCoroutine(
                                    selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(
                                        CardEffectCommons.ChangeDigimonDP(
                                            targetPermanent: permanent,
                                            changeValue: -4000,
                                            effectDuration: EffectDuration.UntilEachTurnEnd,
                                            activateClass: activateClass));
                                }
                            }

                            if (card.Owner.HandCards.Count(CanSelectCardSharedCondition) >= 1)
                            {
                                int maxCount = Math.Min(1, card.Owner.HandCards.Count(CanSelectCardSharedCondition));

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardSharedCondition,
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

                                selectHandEffect.SetUpCustomMessage("Select 1 card to place at the bottom of security.",
                                    "The opponent is selecting 1 card to place at the bottom of security.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Security Bottom Card");

                                yield return StartCoroutine(selectHandEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(
                                        CardObjectController.AddSecurityCard(cardSource, toTop: false));
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Opponent's Turn - ESS

            if (timing == EffectTiming.None)
            {
                bool CanUseCondition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.ContainsTraits("Angel") ||
                            permanent.TopCard.ContainsTraits("Archangel") ||
                            permanent.TopCard.ContainsTraits("Three Great Angels") ||
                            permanent.TopCard.ContainsTraits("ThreeGreatAngels"))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                cardEffects.Add(CardEffectFactory.BlockerStaticEffect(
                    permanentCondition: PermanentCondition,
                    isInheritedEffect: true,
                    card: card,
                    condition: CanUseCondition));
            }
            
            #endregion
            
            return cardEffects;
        }
    }
}