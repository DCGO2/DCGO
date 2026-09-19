using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT15
{
    public class BT15_002 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnAddHand)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DP +1000", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("DP+1000_BT15_002");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When one of your Digimon's effects adds cards to your hand, this Digimon gets +1000 DP until the end of your opponent's turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            bool CardEffectCondition(ICardEffect cardEffect)
                            {
                                if (CardEffectCommons.IsOwnerEffect(cardEffect, card))
                                {
                                    if (cardEffect.IsDigimonEffect)
                                    {
                                        return true;
                                    }
                                }
                                return false;
                            }

                            if (CardEffectCommons.CanTriggerWhenAddHand(
                                hashtable,
                                player => player == card.Owner,
                            CardEffectCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                        targetPermanent: card.PermanentOfThisCard(),
                        changeValue: 1000,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass));
                }
            }

            return cardEffects;
        }
    }
}