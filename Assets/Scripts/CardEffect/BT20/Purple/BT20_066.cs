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
                            yield return ContinuousController.instance.StartCoroutine(
                                                        CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                                                            CanSelectCardCondition,
                                                            payCost: true,
                                                            isHand: true,
                                                            activateClass
                                                        ));
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
                            yield return ContinuousController.instance.StartCoroutine(
                                                        CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                                                            CanSelectCardCondition,
                                                            payCost: true,
                                                            isHand: true,
                                                            activateClass
                                                        ));
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
    }
}