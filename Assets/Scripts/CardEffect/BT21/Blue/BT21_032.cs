using System.Collections.Generic;

namespace DCGO.CardEffects.BT21
{
    public class BT21_032 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("DemiVeemon") ||
                           (targetPermanent.TopCard.IsLevel2 &&
                            targetPermanent.TopCard.EqualsTraits("Hero"));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && CardEffectCommons.IsOwnerTurn(card);
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent == card.PermanentOfThisCard();
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           (cardSource.EqualsTraits("Armor Form") ||
                            cardSource.EqualsTraits("Hero"));
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                cardEffects.Add(CardEffectFactory.ChangeDigivolutionCostStaticEffect(
                    changeValue: -1,
                    permanentCondition: PermanentCondition,
                    cardCondition: CardSourceCondition,
                    rootCondition: RootCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    setFixedCost: false));
            }

            #endregion

            #region Your Turn - ESS

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 2000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition));
            }

            #endregion

            return cardEffects;
        }
    }
}