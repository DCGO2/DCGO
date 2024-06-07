using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX6
{
    public class Kakkinmon_EX6_005 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Main Phase - ESS
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 card, to gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By returning 1 card with the [Legend-Arms] trait from this Digimon’s digivolution cards to the hand, gain 1 memory.";
                }

                bool HasLegendArms(CardSource cardSource)
                {
                    return cardSource.ContainsTraits("Legend-Arms");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return card.PermanentOfThisCard().DigivolutionCards.Count(HasLegendArms) > 0;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedSource = null;

                    if (card.PermanentOfThisCard().DigivolutionCards.Count(HasLegendArms) > 0)
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: null,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card with [Legend-Arms] trait to hand",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.AddHand,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            customRootCardList: card.PermanentOfThisCard().DigivolutionCards.Where(HasLegendArms).ToList(),
                            canLookReverseCard: false,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        if (cardSource != null)
                            selectedSource = cardSource;

                        yield return null;
                    }

                    if(selectedSource != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                    }

                }
            }
            #endregion

            return cardEffects;
        }
    }
}