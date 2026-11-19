using System.Collections;
using System.Collections.Generic;

// Liskmon
namespace DCGO.CardEffects.SD01
{
    public class SD01_03 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            Permanent thisPermanent = card.PermanentOfThisCard();

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon gets +2000 DP for the turn", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] This Digimon gets +2000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                        thisPermanent,
                        2000,
                        EffectDuration.UntilEachTurnEnd,
                        activateClass
                    ));

                }
            }
            #endregion

            #region Inherited
            if (timing == EffectTiming.None)
            {
                ChangeDPDeleteEffectMaxDPClass changeDPDeleteEffectMaxDPClass = new ChangeDPDeleteEffectMaxDPClass();
                changeDPDeleteEffectMaxDPClass.SetUpICardEffect("Add 1000 to this [ALYSION] trait Digimon's DP deletion effect's maximum",CanUseCondition, card);
                changeDPDeleteEffectMaxDPClass.SetUpChangeDPDeleteEffectMaxDPClass(changeMaxDP: ChangeMaxDP);
                changeDPDeleteEffectMaxDPClass.SetIsInheritedEffect(true);
                cardEffects.Add(changeDPDeleteEffectMaxDPClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(thisPermanent, card)
                        && thisPermanent.TopCard.HasAlysianTraits;
                }

                int ChangeMaxDP(int maxDP, ICardEffect cardEffect)
                {
                    if (cardEffect != null
                    && cardEffect.EffectSourceCard != null
                    && cardEffect.EffectSourceCard.Owner == card.Owner
                    && cardEffect.EffectSourceCard.PermanentOfThisCard() == thisPermanent)
                    { 
                        maxDP += 1000;
                    }

                    return maxDP;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
