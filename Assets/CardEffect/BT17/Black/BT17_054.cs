using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DCGO.CardEffects.BT17
{
    public class BT17_054 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal 3, add 1 Tamer or 1 Digimon with [Machine] in traits", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Reveal the top 3 cards of your deck. Add 1 Tamer card or 1 Digimon card with the [Machine] trait among them to the hand. Trash the rest.";
                }

                bool IsTamerorMachine(CardSource cardSource)
                {
                    if (cardSource.IsTamer)
                        return true;

                    if (cardSource.IsDigimon && cardSource.ContainsTraits("Machine"))
                        return true;

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable,card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:IsTamerorMachine,
                            message: "Select 1 Tamer or Digimon with [Machine] in trait.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.Trash,
                        activateClass: activateClass,
                        canNoSelect: false
                    ));
                }
            }
            #endregion

            #region All Turns - ESS
            if (timing == EffectTiming.OnCounterTiming)
            {
                bool condition()
                {
                    if (card.PermanentOfThisCard().TopCard != card)
                        return card.PermanentOfThisCard().TopCard.ContainsTraits("Machine");

                    return false;
                }

                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: condition));
            }
            #endregion

            return cardEffects;
        }
    }
}