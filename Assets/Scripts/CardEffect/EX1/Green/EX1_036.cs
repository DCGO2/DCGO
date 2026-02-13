using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX1
{
    public class EX1_036 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DP +2000", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("DP+2000_EX1_036");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn][Once Per Turn] When one of your opponent's Digimon becomes suspended, this Digimon gets +2000 DP for the turn.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, PermanentCondition))
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
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: card.PermanentOfThisCard(), changeValue: 2000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                }
            }

            return cardEffects;
        }
    }
}