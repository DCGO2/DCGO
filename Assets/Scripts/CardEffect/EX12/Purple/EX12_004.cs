using System.Collections.Generic;

// Onibimon
namespace DCGO.CardEffects.EX12
{
    public class EX12_004 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherited Fortitude
            if (timing == EffectTiming.OnEndTurn)
            {
                bool PermanentCondition()
                {
                    return CardEffectCommons.IsOwnerTurn(card)
                        && card.PermanentOfThisCard().TopCard.EqualsTraits("TB");
                }

                cardEffects.Add(CardEffectFactory.ExecuteSelfEffect(isInheritedEffect: true, card: card, condition: PermanentCondition));
            }
            #endregion

            return cardEffects;
        }
    }
}
