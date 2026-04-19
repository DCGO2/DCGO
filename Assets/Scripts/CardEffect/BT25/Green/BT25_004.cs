using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Tapmon
namespace DCGO.CardEffects.BT25
{
    public class BT25_004 : CEntity_Effect
    {
        
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared Conditions

            bool CardCondition(CardSource cardSource)
            {
                return cardSource.EqualsTraits("Social")
                    || cardSource.EqualsTraits("Tool")
                    || cardSource.EqualsTraits("Game");
            }

            bool PermanentCondition(Permanent permanent) => permanent == card.PermanentOfThisCard();

            bool RootCondition(SelectCardEffect.Root root) => true;

            #endregion

            #region Reduce Link Cost
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ();
                activateClass.SetUpICardEffect("May reduce Link cost by 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT25_004_YT");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When a [Social], [Tool] or [Game] trait card would link to this Digimon, you may reduce the cost by 1.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenWouldLink(hashtable, CardCondition, PermanentCondition)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleArea(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    card.Owner.UntilCalculateFixedCostEffect.Add((timing) => CardEffectFactory.GrantedReduceLinkCostClass(
                        card: card, 
                        reducedCost: 1,
                        cardSourceCondition: CardCondition,
                        permanentCondition: PermanentCondition,
                        rootCondition: RootCondition
                    ));

                    yield return null;
                }
            }
            #endregion

            #region Reduce Link cost - Not Shown
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ReduceLinkCostClass(
                    card: card, 
                    canUseCondition: CanUseCondition,
                    isInheritedEffect: true,
                    isOptional: true,
                    reducedCost: 1,
                    cardSourceCondition: CardCondition,
                    permanentCondition: PermanentCondition,
                    rootCondition: RootCondition
                ));

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
