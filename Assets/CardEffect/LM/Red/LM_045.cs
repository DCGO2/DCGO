using System.Collections;
using System.Collections.Generic;

//  Vermilion Memory Boost!
namespace DCGO.CardEffects.LM
{
    public class LM_045 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Color Requirements Condition

            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Yellow also meets this card's color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);

                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionPermanent((permanent) =>
                        permanent.TopCard.Owner == card.Owner &&
                        permanent.TopCard.CardColors.Contains(CardColor.Yellow) &&
                        (permanent.IsDigimon || permanent.IsTamer), true);
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        return true;
                    }

                    return false;
                }
            }

            #endregion

            #region Main

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Reveal the top 3 cards of your deck. Add 1 red or yellow Digimon card among them to the hand. Return the rest to the bottom of deck. Then, place this card in the battle area.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.CardColors.Contains(CardColor.Red) || cardSource.CardColors.Contains(CardColor.Yellow))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    selectCardConditions:
                    new SelectCardConditionClass[]
                    {
                        new SelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList:null,
                            canEndSelectCondition:null,
                            canNoSelect:false,
                            selectCardCoroutine: null,
                            message: "Select 1 red or yellow Digimon card.",
                            maxCount: 1,
                            canEndNotMax:false,
                            mode: SelectCardEffect.Mode.AddHand
                            )
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                    activateClass: activateClass,
                    canNoAction: false));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }

            #endregion

            #region Main Delay

            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.Gain2MemoryOptionDelayEffect(card));
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaceSelfDelayOptionSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}