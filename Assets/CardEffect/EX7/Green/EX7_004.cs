using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX7
{
    public class EX7_004 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Your Turn - ESS

            if (timing == EffectTiming.OnEndBattle)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Memory+1_EX7_004");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] [Once Per Turn] When this Digimon deletes an opponent's Digimon in battle, gain 1 memory.";
                }

                bool WinnerCondition(Permanent permanent) => permanent.cardSources.Contains(card);
                bool LoserCondition(Permanent permanent) => CardEffectCommons.IsOpponentPermanent(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.CanTriggerWhenDeleteOpponentDigimonByBattle(hashtable: hashtable,
                               winnerCondition: WinnerCondition, loserCondition: LoserCondition, isOnlyWinnerSurvive: false);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}