using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Eater Legion
namespace DCGO.CardEffects.BT23
{
    public class BT23_074 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Erika Mishima");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Alliance

            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                int maxCost = 6;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play up to 6 Play cost Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] If you have [Mother Eater] in the breeding area, you may play up to 6 play cost's total worth of Digimon cards with the [Eater] trait from your hand without paying the costs.";
                }

                bool HasMother(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card) &&
                           permanent.TopCard.EqualsCardName("Mother Eater");
                }

                bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource source)
                {
                    int sumCost = 0;

                    foreach (CardSource cardSource in cardSources)
                    {
                        sumCost += cardSource.GetCostItself;
                    }

                    sumCost += source.GetCostItself;

                    if (sumCost > maxCost)
                    {
                        return false;
                    }

                    return true;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasEaterTraits &&
                           cardSource.HasPlayCost &&
                           cardSource.GetCostItself <= maxCost;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(HasMother, true);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = card.Owner.HandCards.Count(CanSelectCardCondition);

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: true,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> selectedCards)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true));
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                int maxCost = 6;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play up to 6 Play cost Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If you have [Mother Eater] in the breeding area, you may play up to 6 play cost's total worth of Digimon cards with the [Eater] trait from your hand without paying the costs.";
                }

                bool HasMother(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card) &&
                           permanent.TopCard.EqualsCardName("Mother Eater");
                }

                bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource source)
                {
                    int sumCost = 0;

                    foreach (CardSource cardSource in cardSources)
                    {
                        sumCost += cardSource.GetCostItself;
                    }

                    sumCost += source.GetCostItself;

                    if (sumCost > maxCost)
                    {
                        return false;
                    }

                    return true;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasEaterTraits &&
                           cardSource.HasPlayCost &&
                           cardSource.GetCostItself <= maxCost;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(HasMother, true);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = card.Owner.HandCards.Count(CanSelectCardCondition);

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: true,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> selectedCards)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}