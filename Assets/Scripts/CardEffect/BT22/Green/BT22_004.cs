using System.Collections;
using System.Collections.Generic;

// Wanyamon
namespace DCGO.CardEffects.BT22
{
    public class BT22_004 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region ESS

            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into a [CS] digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT22_004_Digivolve");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When effects place [CS] trait Digimon cards in this Digimon's digivolution cards, it may digivolve into a [CS] trait Digimon card in the hand with the digivolution cost reduced by 1.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.CanTriggerOnAddDigivolutionCard(hashtable, IsThisPermanent, null, IsCsDigimon);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectEvoCondition);
                }

                bool IsThisPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent) && permanent == card.PermanentOfThisCard().TopCard.PermanentOfThisCard();
                }

                bool IsCsDigimon(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.HasCSTraits;
                }

                bool CanSelectEvoCondition(CardSource cardSource)
                {
                    return IsCsDigimon(cardSource) && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().TopCard.PermanentOfThisCard().PermanentFrame, true, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                        targetPermanent: card.PermanentOfThisCard().TopCard.PermanentOfThisCard(),
                                        cardCondition: CanSelectEvoCondition,
                                        payCost: true,
                                        reduceCostTuple: (reduceCost: 1, reduceCostCardCondition: null),
                                        fixedCostTuple: null,
                                        ignoreDigivolutionRequirementFixedCost: -1,
                                        isHand: true,
                                        activateClass: activateClass,
                                        successProcess: null));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}