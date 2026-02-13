using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCGO.CardEffects.EX6
{
    public class EX6_003 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region When Attacking - ESS
            
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Add 1 card from top of security to hand to place 1 card from hand at the bottom of security",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Attacking_EX6_003");
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[When Attacking] [Once Per Turn] By adding the top card of your security stack to the hand, you may place 1 Digimon card with the [Angel]/[Archangel]/[Three Great Angels] trait from your hand at the bottom of your security stack.";
                }
                
                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if ((cardSource.ContainsTraits("Angel") && !cardSource.ContainsTraits("Fallen Angel")) ||
                            cardSource.ContainsTraits("Archangel") ||
                            cardSource.ContainsTraits("Three Great Angels") ||
                            cardSource.ContainsTraits("ThreeGreatAngels"))
                        {
                            return true;
                        }
                    }
                                        
                    return false;
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
                
                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.SecurityCards.Count >= 1)
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        CardSource topCard = card.Owner.SecurityCards[0];
                        
                        yield return ContinuousController.instance.StartCoroutine(
                            CardObjectController.AddHandCards(new List<CardSource>() { topCard }, false,
                                activateClass));
                        
                        yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                            player: card.Owner,
                            refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
                        
                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                        {
                            int maxCount = Math.Min(1, card.Owner.HandCards.Count(CanSelectCardCondition));
                            
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
            
            #endregion
            
            return cardEffects;
        }
    }
}