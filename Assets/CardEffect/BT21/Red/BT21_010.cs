using System.Collections.Generic;

//Gammamon
namespace DCGO.CardEffects.BT21
{
    public class BT21_010 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Cost

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.IsDigimon)
                    {
                        if (targetPermanent.TopCard.EqualsCardName("Gurimon") || (targetPermanent.Level == 2 && targetPermanent.TopCard.EqualsTraits("Hero")))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Warp

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsOwnerTurn(card) && CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.SecurityCards.Count <= 2 || TamerCondition())
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool TamerCondition()
                {
                    var tamers = card.Owner
                        .GetBattleAreaPermanents()
                        .Filter(x => x.IsTamer && x.TopCard.EqualsTraits("Hero"))
                        .Map(x => x.TopCard);

                    return Combinations.GetUniqueNameCardCount(tamers) >= 3;
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent == card.PermanentOfThisCard();
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource.Owner.HandCards.Contains(cardSource))
                    {
                        if (cardSource.EqualsCardName("Siriusmon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                string effectName = $"Can digivolve to [Siriusmon]";
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 4, true, card, Condition, effectName: effectName, CardCondition));
            }

            #endregion

            #region +2k DP

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 2000, isInheritedEffect: true, card: card, Condition));
            }

            #endregion

            return cardEffects;
        }
    }
}