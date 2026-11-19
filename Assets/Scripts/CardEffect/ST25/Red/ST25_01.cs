using System.Collections.Generic;

// Tokomon
namespace DCGO.CardEffects.ST25
{
    public class ST25_01 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Your Turn - Inherited
            if (timing == EffectTiming.None)
            {
                bool Condition()
                    => CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.Owner.Enemy.MemoryForPlayer >= 3
                        && card.PermanentOfThisCard().TopCard.HasText("Gemmon");

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 3000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}
