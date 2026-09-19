using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT14
{
    public class BT14_097 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as having [Sukamon] in its name", CanUseCondition, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);

                cardEffects.Add(changeCardNamesClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
                {
                    if (cardSource == card)
                    {
                        CardNames.Add("Sukamon_BT14_097");
                    }

                    return CardNames;
                }
            }

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] 1 of your non-white Digimon may digivolve into a Digimon card with [Sukamon] in its name in your hand without paying the cost, ignoring its digivolution requirements.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (!permanent.TopCard.CardColors.Contains(CardColor.White))
                        {
                            foreach (CardSource cardSource in card.Owner.HandCards)
                            {
                                if (CanSelectCardCondition(cardSource))
                                {
                                    if (!cardSource.CanNotEvolve(permanent))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.ContainsCardName("Sukamon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to digivolve.", "The opponent is selecting 1 Digimon to digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: selectedPermanent,
                                cardCondition: CanSelectCardCondition,
                                payCost: false,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: 0,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null));
                        }
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Opponent's 1 Digimon becomes [Sukamon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Until the end of your turn, change 1 of your opponent's Digimon into being white and having 3000 DP and an original name of [Sukamon].";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeBaseDigimonDP(
                                    targetPermanent: permanent,
                                    changeValue: 3000,
                                    effectDuration: EffectDuration.UntilOwnerTurnEnd,
                                    activateClass: activateClass));
                            }

                            if (selectedPermanent != null)
                            {
                                ChangeBaseCardNameClass changeBaseCardNameClass = new ChangeBaseCardNameClass();
                                changeBaseCardNameClass.SetUpICardEffect("Original card name is [Sukamon]", CanUseCondition1, card);
                                changeBaseCardNameClass.SetUpChangeBaseCardNamesClass(changeBaseCardNames: changeBaseCardNames);
                                CardEffectCommons.AddEffectToPermanent(
                                    targetPermanent: selectedPermanent,
                                    effectDuration: EffectDuration.UntilOwnerTurnEnd,
                                    card: card,
                                    cardEffect: changeBaseCardNameClass,
                                    timing: EffectTiming.None
                                );

                                bool CanUseCondition1(Hashtable hashtable)
                                {
                                    if (selectedPermanent.TopCard != null)
                                    {
                                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                List<string> changeBaseCardNames(CardSource cardSource, List<string> CardNames)
                                {
                                    if (cardSource == selectedPermanent.TopCard)
                                    {
                                        CardNames = new List<string>() { "Sukamon" };
                                    }

                                    return CardNames;
                                }
                            }

                            if (selectedPermanent != null)
                            {
                                ChangeBaseCardColorClass changeBaseCardColorClass = new ChangeBaseCardColorClass();
                                changeBaseCardColorClass.SetUpICardEffect("Original card color is white", CanUseCondition1, card);
                                changeBaseCardColorClass.SetUpChangeBaseCardColorClass(ChangeBaseCardColors: ChangeBaseCardColors);
                                CardEffectCommons.AddEffectToPermanent(
                                    targetPermanent: selectedPermanent,
                                    effectDuration: EffectDuration.UntilOwnerTurnEnd,
                                    card: card,
                                    cardEffect: changeBaseCardColorClass,
                                    timing: EffectTiming.None
                                );

                                bool CanUseCondition1(Hashtable hashtable)
                                {
                                    if (selectedPermanent.TopCard != null)
                                    {
                                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                List<CardColor> ChangeBaseCardColors(CardSource cardSource, List<CardColor> CardColors)
                                {
                                    if (cardSource == selectedPermanent.TopCard)
                                    {
                                        CardColors = new List<CardColor>() { CardColor.White };
                                    }

                                    return CardColors;
                                }
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}