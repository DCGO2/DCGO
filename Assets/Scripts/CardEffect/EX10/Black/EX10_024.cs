using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Kabemon
namespace DCGO.CardEffects.EX10
{
    public class EX10_024 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel2 && targetPermanent.TopCard.HasAppmonTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Link

            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
            }

            #endregion

            #region Link Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasAppmonTraits;
                }
                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 1, card: card));
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfDigimonAfterBattleSecurityEffect(card: card));
            }

            #endregion

            #endregion

            #region Link Effect

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 1 linked card, De-Digivolve 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] By trashing 1 of this Digimon's link cards, <De-Digivolve 1> 1 of your opponent's Digimon. (Trash the top card. You can't trash past level 3 cards.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && HasLinkedCards(card.PermanentOfThisCard().TopCard.PermanentOfThisCard());
                }

                bool PermamentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool HasLinkedCards(Permanent permanent)
                {
                    return !permanent.HasNoLinkCards;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent thisCardPermament = card.PermanentOfThisCard().TopCard.PermanentOfThisCard();

                    if (HasLinkedCards(thisCardPermament))
                    {
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
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashLinkCardsAndProcessAccordingToResult(
                            targetPermanent: thisCardPermament,
                            targetLinkCards: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                        IEnumerator SuccessProcess(List<CardSource> trashedLinkCards)
                        {
                            if (trashedLinkCards.Any() && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, PermamentCondition))
                            {
                                Permanent selectedPermament = null;

                                #region Select Opponent Digimon

                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, PermamentCondition));
                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: PermamentCondition,
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
                                    selectedPermament = permanent;
                                    yield return null;
                                }

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve", "The opponent is selecting 1 Digimon to De-Digivolve");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                #endregion

                                if (selectedPermament != null) yield return ContinuousController.instance.StartCoroutine(new IDegeneration(
                                    permanent: selectedPermament,
                                    DegenerationCount: 1,
                                    cardEffect: activateClass).Degeneration());
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