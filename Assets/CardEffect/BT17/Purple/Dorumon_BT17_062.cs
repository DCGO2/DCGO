using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT17
{
    public class Dorumon_BT17_062 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Dorimon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon digivolves", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Attacking] If [Kosuke Kisakata] is in this Digimon's digivolution cards and your opponent has a level 6 or higher Digimon, this Digimon may digivolve into [Dorugoramon] in the hand for a digivolution cost of 4, ignoring its digivolution requirements.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool IsDorugoramonCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.EqualsCardName("Dorugoramon");
                }

                bool IsKosukeCardCondition(CardSource cardSource)
                {
                    return cardSource.IsTamer &&
                           (cardSource.EqualsCardName("Kosuke Kisakata") ||
                            cardSource.EqualsCardName("KosukeKisakata"));
                }

                bool OpponentPermanentCondition(Permanent permanent)
                {
                    return permanent.IsDigimon &&
                           permanent.TopCard.HasLevel &&
                           permanent.Level >= 6;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           card.PermanentOfThisCard().DigivolutionCards.Some(IsKosukeCardCondition) &&
                           CardEffectCommons.HasMatchConditionOpponentsPermanent(card, OpponentPermanentCondition) &&
                           CardEffectCommons.HasMatchConditionOwnersHand(card, IsDorugoramonCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, IsDorugoramonCardCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: card.PermanentOfThisCard(),
                                cardCondition: IsDorugoramonCardCondition,
                                payCost: true,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: 4,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null));
                    }
                }
            }

            #endregion

            #region Reboot - ESS

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}