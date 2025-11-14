using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX6
{
    public class EX6_068 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Main Effect
            
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[Main] You may place 1 Digimon card with the [Angel]/[Archangel]/[Three Great Angels] trait from your hand at the bottom of your security stack. Then, place this card in your battle area.";
                }
                
                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.CardTraits.Contains("Angel") ||
                            cardSource.CardTraits.Contains("Archangel") ||
                            cardSource.CardTraits.Contains("Three Great Angels") ||
                            cardSource.CardTraits.Contains("ThreeGreatAngels"))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
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
                    
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            
            #endregion
            
            #region Delay Effect
            
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Play 1 card with the [Three Great Angels] trait from your security stack without paying the cost",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetHashString("PlaySecurityCard_EX6_068");
                cardEffects.Add(activateClass);
                
                string EffectDescription()
                {
                    return
                        "[All Turns] When one of your Digimon with the [Angel] or [Archangel] trait is deleted, <Delay>.\r\n• Search your security stack. You may play 1 Digimon card with the [Three Great Angels] trait among it without paying the cost. Shuffle your security stack.";
                }
                
                bool DeletedPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardTraits.Contains("Angel") ||
                            permanent.TopCard.CardTraits.Contains("Archangel"))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                bool IsThreeGreatAngelsDigimonCard(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.ContainsTraits("Three Great Angels") ||
                            cardSource.ContainsTraits("ThreeGreatAngels"))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanDeclareOptionDelayEffect(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, DeletedPermanentCondition))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                            activateClass: activateClass, successProcess: permanents => SuccessProcess(),
                            failureProcess: null));
                }
                
                IEnumerator SuccessProcess()
                {
                    if (card.Owner.SecurityCards.Count() >= 1)
                    {
                        int maxCount = Math.Min(1, card.Owner.SecurityCards.Count(IsThreeGreatAngelsDigimonCard));
                        
                        List<CardSource> selectedCards = new List<CardSource>();
                        
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        
                        selectCardEffect.SetUp(
                            canTargetCondition: IsThreeGreatAngelsDigimonCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Select 1 Digimon to play.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Security,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);
                        
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }
                        
                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                    player: card.Owner,
                                    refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
                            }
                            
                            yield return null;
                        }
                        
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayPermanentCards(cardSources: selectedCards,
                                activateClass: activateClass, payCost: false, isTapped: false,
                                root: SelectCardEffect.Root.Security, activateETB: true));
                        
                        if (card.Owner.SecurityCards.Count >= 1)
                        {
                            ContinuousController.instance.PlaySE(GManager.instance.ShuffleSE);
                            
                            card.Owner.SecurityCards = RandomUtility.ShuffledDeckCards(card.Owner.SecurityCards);
                        }
                    }
                }
            }
            
            #endregion
            
            #region Security Effect
            
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaceSelfDelayOptionSecurityEffect(card));
            }
            
            #endregion
            
            return cardEffects;
        }
    }
}