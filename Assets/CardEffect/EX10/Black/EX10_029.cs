using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//EX10 Warpmon
namespace DCGO.CardEffects.EX10
{
    public class EX10_029 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Secuity

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfDigimonAfterBattleSecurityEffect(card: card));
            }

            #endregion

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasStandardAppTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Link Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasAppmonTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 2, card: card));
            }

            #endregion

            #region Link

            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
            }

            #endregion

            #region Blocker

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region When Linked

            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("De-digivolve protection", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription() => "[When Linking] By trashing 1 of this Digimon's link cards, <De-Digivolve> effects don't affect 1 of your Digimon until your opponent's turn ends.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenLinking(hashtable, null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           !card.PermanentOfThisCard().HasNoLinkCards;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent thisCardPermament = card.PermanentOfThisCard();

                    CardSource selectedCard = null;

                    #region Select Link Card

                    int maxCount = Math.Min(1, thisCardPermament.LinkedCards.Count);
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: _ => true,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 linked card to trash.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: thisCardPermament.LinkedCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }
                    selectCardEffect.SetUpCustomMessage("Select 1 linked card to trash.", "The opponent is selecting 1 linked card to trash.");
                    yield return StartCoroutine(selectCardEffect.Activate());

                    #endregion

                    if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashLinkCardsAndProcessAccordingToResult(
                        targetPermanent: thisCardPermament,
                        targetLinkCards: new List<CardSource>() { selectedCard },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<CardSource> trashedLinkCards)
                    {
                        if (trashedLinkCards.Any())
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                                Permanent selectedPermanent = null;

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

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    selectedPermanent = permanent;
                                    yield return null;
                                }

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to gain De-digivolve immunity.", "The opponent is selecting 1 Digimon to gain De-digivolve immunity.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                if (selectedPermanent != null)
                                {
                                    #region De-digivolve immunity

                                    bool CanUseImmunityCondition(Hashtable hashtable1)
                                    {
                                        return selectedPermanent.TopCard != null;
                                    }

                                    bool PermanentImmunityCondition(Permanent permanent)
                                    {
                                        return permanent == selectedPermanent;
                                    }

                                    ImmuneFromDeDigivolveClass immuneFromDeDigivolveClass = new ImmuneFromDeDigivolveClass();
                                    immuneFromDeDigivolveClass.SetUpICardEffect("Isn't affected by <De-Digivolve>", CanUseImmunityCondition, selectedPermanent.TopCard);
                                    immuneFromDeDigivolveClass.SetUpImmuneFromDeDigivolveClass(PermanentCondition: PermanentImmunityCondition);
                                    selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => immuneFromDeDigivolveClass);

                                    #endregion
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}