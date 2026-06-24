using System.Collections;
using System.Collections.Generic;

// Puroromon
namespace DCGO.CardEffects.EX11
{
    public class EX11_003 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherited
            if (timing == EffectTiming.OnAddSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX11_003_ESS");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] [Once Per Turn] When face-up [Royal Base] trait cards are placed in your security stack, <Draw 1>.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    List<CardSource> securityCards = CardEffectCommons.GetCardSourcesFromHashtable(hashtable).Filter(SecurityCondition);

                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerWhenAddSecurity(hashtable, player => player == card.Owner);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);
                }

                bool SecurityCondition(CardSource cardSource)
                {
                    return !cardSource.IsFlipped
                        && cardSource.EqualsTraits("Royal Base");
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(
                        player: card.Owner,
                        drawCount: 1,
                        cardEffect: activateClass).Draw()
                    );
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
