using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Flickmon
namespace DCGO.CardEffects.EX11
{
    public class EX11_006 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve this Digimon into a Digimon with [Maquinamon] in text.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("Digivolve_EX11_006");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] [Once Per Turn] This Digimon linked with [Maquinamon] may digivolve into a Digimon card with [Maquinamon] in its text in the hand with the digivolution cost reduced by 2.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && card.Owner.HandCards.Count >= 1
                    && card.PermanentOfThisCard().LinkedCards.Any(cardSource => cardSource.EqualsCardName("Maquinamon"));

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                    && cardSource.HasText("Maquinamon")
                    && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass, root: SelectCardEffect.Root.Hand);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Exists(CanSelectCardCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: card.PermanentOfThisCard(),
                            cardCondition: CanSelectCardCondition,
                            payCost: true,
                            reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null));
                    }
                }
            }

            return cardEffects;
        }
    }
}
