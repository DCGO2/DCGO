using System;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT18
{
    public class BT18_085 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared

            

            #endregion

            #region Digivolution Cost Reduction

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return !CardEffectCommons.IsExistOnField(card);
                }

                int OpponentTrashColorCount()
                {
                    List<CardColor> cardColors = new List<CardColor>();

                    foreach (CardSource cardSource in card.Owner.Enemy.TrashCards)
                    {
                        cardColors.AddRange(cardSource.CardColors);
                    }

                    cardColors = cardColors.Distinct().ToList();

                    return cardColors.Count;
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card);
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                cardEffects.Add(CardEffectFactory.ChangeDigivolutionCostStaticEffect<Func<int>>(
                    changeValue: () => -OpponentTrashColorCount(),
                    permanentCondition: PermanentCondition,
                    cardCondition: CardSourceCondition,
                    rootCondition: RootCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    setFixedCost: false));
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                int OpponentTrashColorCount()
                {
                    List<CardColor> cardColors = new List<CardColor>();

                    foreach (CardSource cardSource in card.Owner.Enemy.TrashCards)
                    {
                        cardColors.AddRange(cardSource.BaseCardColorsFromEntity);
                    }

                    cardColors = cardColors.Distinct().ToList();

                    return cardColors.Count / 2;
                }


                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: OpponentTrashColorCount(),
                    isInheritedEffect: false, card: card, condition: Condition));

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 2000 * (OpponentTrashColorCount()),
                    isInheritedEffect: false, card: card, condition: Condition));
            }

            #endregion

            return cardEffects;
        }
    }
}