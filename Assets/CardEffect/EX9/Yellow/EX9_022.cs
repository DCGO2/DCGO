using System.Collections.Generic;

// Elecmon
namespace DCGO.CardEffects.EX9
{
    public class EX9_022 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution condition

            if (timing == EffectTiming.None)
            {
                bool Condition(Permanent permanent)
                {
                    return permanent.TopCard.IsLevel2 && permanent.TopCard.EqualsTraits("DM");
                }
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(Condition, 0, false, card, null));
            }

            #endregion

            #region Training

            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.TrainingEffect(card: card));
            }

            #endregion

            #region Your Turn - ESS

            if (timing == EffectTiming.None)
            {
                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.Owner == card.Owner.Enemy;
                }

                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSecurityDigimonCardDPStaticEffect(
                    cardCondition: CardCondition,
                    changeValue: -3000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition,
                    effectName: "Opponent's Security Digimon gains DP -3000"));
            }

            #endregion

            return cardEffects;
        }
    }
}