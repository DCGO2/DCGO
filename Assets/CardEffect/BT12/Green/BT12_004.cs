using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT12
{
    public class BT12_004 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DP +2000", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("DP+2000_BT12_004");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn][Once Per Turn] If you play a green Digimon, this Digimon gets +2000 DP for the turn.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardColors.Contains(CardColor.Green))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition))
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