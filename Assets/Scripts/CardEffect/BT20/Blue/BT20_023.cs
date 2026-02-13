using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT20
{
    public class BT20_023 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel3 && targetPermanent.TopCard.ContainsCardName("Dracomon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition,
                    digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Jamming

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve for reduced cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Your Turn] When any of your green Digimon with [Dracomon]/[Examon] in their texts are played, this Digimon may digivolve into [Wingdramon] in the hand with the digivolution cost reduced by 2.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PlayedPermanentCondition) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool PlayedPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.CardColors.Contains(CardColor.Green) &&
                           (permanent.TopCard.HasText("Dracomon") || permanent.TopCard.HasText("Examon"));
                }

                bool DigivolveCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.EqualsCardName("Wingdramon") &&
                           cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, true, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionOwnersHand(card, DigivolveCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: DigivolveCardCondition,
                        payCost: true,
                        reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true,
                        activateClass: activateClass,
                        successProcess: null));
                }
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