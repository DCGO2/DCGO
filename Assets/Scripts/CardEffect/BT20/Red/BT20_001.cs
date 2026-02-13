using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT20
{
    public class BT20_001 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {                
                bool CanActivateCondition()
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        {
                            if (card.Owner.LibraryCards.Count >= 1)
                            {
                                if (card.PermanentOfThisCard().DigivolutionCards.Count >= 4)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    
                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 2000, isInheritedEffect: true, card: card, condition: CanActivateCondition));

                
            }

            return cardEffects;
        }
    }
}