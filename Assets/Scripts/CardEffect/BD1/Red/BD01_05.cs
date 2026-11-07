using System.Collections;
using System.Collections.Generic;

// Reptilimon
namespace DCGO.CardEffects.BD01
{
    public class BD01_05 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Your Turns - When Opponent Digimon Deleted
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain <Security +1> for the turn", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BD01_05_ESS");
                cardEffects.Add(activateClass);

                string EffectDescription() => "[Your Turn] [Once Per Turn] When any of your opponent's Digimon are deleted, this Digimon gains <Security A.+1> for the turn. (This Digimon checks 1 additional security card.)";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);

                bool PermanentCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                     yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.ChangeDigimonSAttack(card.PermanentOfThisCard(), 1, EffectDuration.UntilEachTurnEnd, activateClass));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
