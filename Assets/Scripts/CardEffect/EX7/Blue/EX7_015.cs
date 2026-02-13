using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX7
{
    public class EX7_015 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if(targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.IsLevel2)
                        return targetPermanent.TopCard.CardTraits.Contains("NSp");

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }
            #endregion 
        
            #region All Turns
            
            if (timing == EffectTiming.None)
            {
                //Use this instead of activateClass?
                CannotReduceCostClass cannotReduceCostClass = new CannotReduceCostClass();
                cannotReduceCostClass.SetUpICardEffect("Players can't reduce play costs", CanUseCondition, card);
                cannotReduceCostClass.SetUpCannotReduceCostClass(
                    playerCondition: PlayerCondition,
                    targetPermanentsCondition: TargetPermanentsCondition,
                    cardCondition: CardCondition);
                cardEffects.Add(cannotReduceCostClass);
                
                string EffectDiscription()
                {
                    return "[All Turns] Players can't reduce play costs.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool PlayerCondition(Player player)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool TargetPermanentsCondition(List<Permanent> targetPermanents)
                {
                    if (targetPermanents == null)
                    {
                        return true;
                    }

                    else
                    {
                        if (targetPermanents.Count((permanent) => permanent != null) == 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.HasPlayCost;
                }
            }
            
            #endregion

            return cardEffects;
        }
    }
}