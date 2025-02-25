using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT20
{
    public class BT20_003 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            #region On Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [Free] digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When any of your purple Digimon are played, this Digimon may digivolve into a Digimon card with the [Free] trait in the hand with the digivolution cost reduced by 1.";
                }

                

                bool isPurpleDigimon(Permanent permanent)
                {                 
                    return permanent.TopCard.CardColors.Contains(CardColor.Purple);                    
                }

                bool isOwnerDigimon(CardSource cardSource)
                {                          
                    return (cardSource.Owner == card.Owner && cardSource.IsDigimon && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass));
                }

                

                bool canEvolveIntoFree(CardSource cardSource){
                    if (cardSource.CardTraits.Contains("Free")){
                        return true;
                    }
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if(CardEffectCommons.HasMatchConditionOwnersHand(card, isOwnerDigimon))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, isPurpleDigimon))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return true;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                { 
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent != card.PermanentOfThisCard())
                        {
                            foreach (CardSource cardSource in card.Owner.HandCards)
                            {
                                if (canEvolveIntoFree(cardSource))
                                {
                                    if (cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass))
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
                        Permanent selectedPermanent = null;

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will digivolve.", "The opponent is selecting 1 Digimon that will digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: selectedPermanent,
                                cardCondition: canEvolveIntoFree,
                                payCost: true,
                                reduceCostTuple: (reduceCost: 1, reduceCostCardCondition: null),
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null));
                        }
                    }
                }
            }
            #endregion

            #region InheritedEffect
            if (timing == EffectTiming.None)
            {
                bool InheritedEffectCondition()
                {
                    return CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 2000,
                    isInheritedEffect: true,
                    card: card,
                    condition: InheritedEffectCondition));
            }
            #endregion

            return cardEffects;
        }
    }
}