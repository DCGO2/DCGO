using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.BT20
{
    public class BT20_020 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.TopCard.EqualsCardName("Imperialdramon: Dragon Mode");
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            if(timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            if(timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));

            }

            #region When Digievolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                
                IEnumerator ActivateCoroutine1(Hashtable hashtable)
                {
                    yield return null;
                    CanNotPutFieldClass canNotPutFieldClass = new CanNotPutFieldClass();
                    canNotPutFieldClass.SetUpICardEffect("Can't play Digimon or Tamers by effect", CanUseCondition1, card);
                    canNotPutFieldClass.SetUpCanNotPutFieldClass(cardCondition: CardCondition, cardEffectCondition: CardEffectCondition1);
                    card.Owner.Enemy.UntilOwnerTurnEndEffects.Add((_timing) => canNotPutFieldClass);
                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().DebuffSE);

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    bool CardCondition(CardSource cardSource)
                    {
                    if (cardSource.Owner == card.Owner.Enemy)
                    {
                        if (cardSource.IsDigimon || cardSource.IsTamer)
                        {
                            return true;
                        }
                    }
                        return false;
                    }

                bool CardEffectCondition1(ICardEffect cardEffect)
                {
                    return true;
                }

            }
                


                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gains raid, piercing, trash security stack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                bool CardEffectCondition(ICardEffect cardEffect)
                {
                    return cardEffect != null;
                }
                
                string EffectDiscription()
                {
                    return "＜Raid＞ (When this Digimon attacks, you may switch the target of attack to 1 of your opponent's unsuspended Digimon with the highest DP). ＜Piercing＞ (When this Digimon attacks and deletes an opponent's Digimon and survives the battle, it performs any security checks it normally would). [When Digivolving] Your opponent can't play Digimon or Tamers by effects until the end of their turn. Then, if [Imperialdramon: Dragon Mode] is in this Digimon's digivolution cards, trash your opponent's top security card. [All Turns] [Once Per Turn] When your opponent's security stack is removed from, delete 1 of their Digimon with as much or less DP as this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))                                                    
                            {
                                return true;
                            }
                        return false;                        
                    }                
                    return false;          
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return true;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if(card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardNames.Contains("Imperialdramon: Imperialdramon: Dragon Mode")) >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                player: card.Owner.Enemy,
                                destroySecurityCount: 1,
                                cardEffect: activateClass,
                                fromTop: true).DestroySecurity());
                    }
                }
            }
            #endregion


            #region All Turns
            if(timing == EffectTiming.None)
            {
                ActivateClass activateClass1 = new ActivateClass();
                activateClass1.SetUpICardEffect("Delete 1 Digimon with as much or less DP as this Digimon", CanUseCondition1, card);
                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, 1, true, EffectDiscription1());
                activateClass1.SetHashString("ImperialDramon_BT20_020");
                cardEffects.Add(activateClass1);

                string EffectDiscription1()
                {
                    return "[All Turns] [Once Per Turn] When your opponent's security stack is removed from, delete 1 of their Digimon with as much or less DP as this Digimon.";
                }

                bool CanUseCondition1(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {                        
                        if (CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => player == card.Owner.Enemy))
                        {
                            return true;
                        }                        
                    }

                    return false;
                }
                bool CanActivateCondition1(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }
                    return false;
                }
                
                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {   
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(13000,activateClass1))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine1(Hashtable hashtable)
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
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass1);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                }
            }
            #endregion
            return cardEffects;
        }
    }
}