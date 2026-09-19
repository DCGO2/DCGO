using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT18
{
    public class BT18_046 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel3 &&
                           targetPermanent.TopCard.EqualsTraits("Royal Base");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
            }

            #endregion

            #region Opponent's Turn Effect

            if (timing == EffectTiming.None)
            {
                bool AttackerCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           permanent.DP <= card.PermanentOfThisCard().DP;
                }

                bool DefenderCondition(Permanent permanent)
                {
                    return permanent == null;
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if(card.PermanentOfThisCard().TopCard == card)
                                return true;
                        }
                    }

                    return false;
                }

                string effectName = "Opponent's Digimon with as much or less DP as this Digimon can't attack players";

                cardEffects.Add(CardEffectFactory.CanNotAttackStaticEffect(
                    attackerCondition: AttackerCondition,
                    defenderCondition: DefenderCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    effectName: effectName
                ));
            }

            #endregion

            #region All Turns - ESS
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 1000, isInheritedEffect: true, card: card, condition: Condition));
            }
            #endregion

            #region All Turns - Security
            if (timing == EffectTiming.None)
            {
                string EffectDiscription()
                {
                    return "(Security) [All Turns] All of your [Royal Base] trait Digimon get +1000 DP.";
                }

                bool Condition()
                {
                    return CardEffectCommons.IsExistInSecurity(card, false);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.EqualsTraits("Royal Base"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                permanentCondition: PermanentCondition,
                changeValue: 1000,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                effectName: EffectDiscription));
            }
            #endregion

            return cardEffects;
        }
    }
}