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

            #region Pierce
                if(timing == EffectTiming.OnDetermineDoSecurityCheck)
                {
                    cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
                }
            #endregion

            #region Raid
                if(timing == EffectTiming.OnAllyAttack)
                {
                    cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));

                }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gains raid, piercing, trash security stack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Your opponent can't play Digimon or Tamers by effects until the end of their turn. Then, if [Imperialdramon: Dragon Mode] is in this Digimon's digivolution cards, trash your opponent's top security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    CanNotPutFieldClass canNotPutFieldClass = new CanNotPutFieldClass();
                    canNotPutFieldClass.SetUpICardEffect("Can't play Digimon or Tamers by effect", CanUseCondition1, card);
                    canNotPutFieldClass.SetUpCanNotPutFieldClass(cardCondition: CardCondition, cardEffectCondition: CardEffectCondition);
                    card.Owner.Enemy.UntilOwnerTurnEndEffects.Add((_timing) => canNotPutFieldClass);
                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().DebuffSE);

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    bool CardCondition(CardSource cardSource)
                    {
                        return cardSource.IsDigimon || cardSource.IsTamer || cardSource.IsDigiEgg;
                    }

                    bool CardEffectCondition(ICardEffect cardEffect)
                    {
                        return cardEffect != null &&
                               cardEffect.EffectSourceCard.Owner == card.Owner.Enemy;
                    }

                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.EqualsCardName("Imperialdramon: Dragon Mode")) >= 1)
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
            if(timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Digimon with as much or less DP as this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("ImperialDramon_BT20_020");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When your opponent's security stack is removed from, delete 1 of their Digimon with as much or less DP as this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
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
                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }
                
                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {   
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(card.PermanentOfThisCard().DP,activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
