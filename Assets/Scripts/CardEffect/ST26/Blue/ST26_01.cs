using System.Collections.Generic;

// Viximon
namespace DCGO.CardEffects.ST26
{
    public class ST26_01 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Your Turn - Inherited
            if (timing == EffectTiming.None)
            {
                bool OpponentDigimonWithSourcesCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.DigivolutionCards.Count >= 1;

                bool Condition()
                    => CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.PermanentOfThisCard().TopCard.EqualsTraits("Ice-Snow")
                        && !CardEffectCommons.HasMatchConditionPermanent(OpponentDigimonWithSourcesCondition);

                bool PermanentCondition(Permanent permanent)
                    => permanent == card.PermanentOfThisCard();

                bool CanNotBeDestroyedByBattleCondition(Permanent permanent, Permanent attackingPermanent, Permanent defendingPermanent, CardSource defendingCard)
                    => permanent == attackingPermanent || permanent == defendingPermanent;

                cardEffects.Add(CardEffectFactory.CanNotBeDestroyedByBattleStaticEffect(
                    canNotBeDestroyedByBattleCondition: CanNotBeDestroyedByBattleCondition,
                    permanentCondition: PermanentCondition,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition,
                    effectName: "Can't be deleted in battle"));
            }
            #endregion

            return cardEffects;
        }
    }
}
