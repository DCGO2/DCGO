using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT17
{
    public class BT17_083 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            #region Start of Your Turn

            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnAddHand)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain 1 Memory and Jamming", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Gain1MemoryJamming_BT17_083");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return
                        "[Your Turn] [Once Per Turn] When an effect adds cards to your hand, gain 1 memory. Then, this Digimon gains [Jamming] for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return CardEffectCommons.CanTriggerWhenAddHand(hashtable,
                                player => player == card.Owner,
                                cardEffect => cardEffect != null);
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        card.Owner.AddMemory(1, activateClass));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainJamming(
                        targetPermanent: card.PermanentOfThisCard(), effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}