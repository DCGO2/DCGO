using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects
{
    public class Puroromon_BT18_004 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Royal Base] trait Digimon face up as bottom security, add top security to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By placing 1 Digimon card with the [Royal Base] trait in your hand face up as your bottom security card, add your top security card to the hand.";
                }

                bool IsRoyalBaseDigimon(CardSource card)
                {
                    return card.EqualsTraits("Royal Base");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().DigivolutionCards.Contains(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return CardEffectCommons.HasMatchConditionOwnersHand(card,IsRoyalBaseDigimon);

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsRoyalBaseDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.PutSecurityBottom,
                        cardEffect: activateClass);

                    selectHandEffect.SetIsFaceup();

                    yield return StartCoroutine(selectHandEffect.Activate());
                }
            }

            return cardEffects;
        }
    }
}