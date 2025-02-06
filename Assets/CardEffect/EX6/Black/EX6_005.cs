using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DCGO.CardEffects.EX6
{
    public class EX6_005 : CEntity_Effect
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
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return card.PermanentOfThisCard().DigivolutionCards.Count(HasLegendArms) > 0;
                        }
                    }
                    

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: HasLegendArms,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                message: "Select 1 digivolution card to add to your hand.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.AddHand,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards.Where(HasLegendArms).ToList(),
                                canLookReverseCard: false,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count > 0)
                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));

                        yield return null;
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}