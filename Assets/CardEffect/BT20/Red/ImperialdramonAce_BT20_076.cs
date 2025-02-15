using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.BT20
{
    public class ImperialdramonAce_BT20_076 : CEntity_Effect{
    
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
        List<ICardEffect> cardEffects = new List<ICardEffect>();


        #region Ace - Blast DNA Digivolve
            
            if (timing == EffectTiming.OnCounterTiming)
            {
                List<BlastDNACondition> blastDNAConditions = new List<BlastDNACondition>
                {
                    new("DinoBeemon"),
                    new("Paildramon")
                };
                cardEffects.Add(CardEffectFactory.BlastDNADigivolveEffect(card: card,
                    blastDNAConditions: blastDNAConditions, condition: null));
            }
            
        #endregion

        #region DNA Digivolution
            
            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);
                
                //NoCanUseConditionHere
                
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
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent,
                                                    card))
                                            {
                                                if (permanent.TopCard.CardColors.Contains(CardColor.Purple))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(5))
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
                        
                        bool PermanentCondition2(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent,
                                                    card))
                                            {
                                                if (permanent.TopCard.CardColors.Contains(CardColor.Red))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(5))
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
                        
                        JogressConditionElement[] elements =
                        {
                            new(PermanentCondition1, "a level 5 Purple Digimon"),
                            new(PermanentCondition2, "a level 5 Red Digimon")
                        };
                        
                        JogressCondition jogressCondition = new JogressCondition(elements, 0);
                        
                        return jogressCondition;
                    }
                    
                    return null;
                }
            }
            
            #endregion
        #region Shared On Play / When Digivolving
                        
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Delete 1 Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);
            string EffectDiscription()
            {
                return "[When Digivolving] Delete 1 of your opponent's Digimon with 11000 DP or less. Then, if DNA digivolving, this Digimon may digivolve into [Imperialdramon: Fighter Mode] in the hand or trash without paying the cost.";
            }
            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(11000, activateClass))
                    {
                        return true;
                    }
                }

                return false;
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                    }
                return false;
            }
            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        return true;
                    }
                return false;
            }
            IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                    {
                        int maxCount = Math.Min(1, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

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
                    activateClass.SetUpICardEffect("Digivolve into Imperialdramon: Fightermode", CanUseCondition, card);
                    activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                    cardEffects.Add(activateClass);      
                
                    bool CanSelectCardCondition (CardSource cardSource){
                    return cardSource.EqualsCardName("Imperialdramon: Fighter Mode");
                    }
            
                }
            
                        
            if (timing == EffectTiming.OnEnterFieldAnyone){                
                

            }
            #endregion
            //TODO: Evolve
        return cardEffects;
                
    }
    }
}
