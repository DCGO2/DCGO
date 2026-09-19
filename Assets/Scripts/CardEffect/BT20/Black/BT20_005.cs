using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT20
{
    public class BT20_005 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Your Turn - ESS
            if (timing == EffectTiming.OnSecurityCheck)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain <Jamming> for the turn", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When this Digimon checks face-up security cards, it gains <Jamming> for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerOnAttack(hashtable, card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (!CardEffectCommons.GetCardFromHashtable(hashtable).IsFlipped)
                                return true;
                        }
                    }

                    return false;
                }


                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainJamming(targetPermanent: card.PermanentOfThisCard(), effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}