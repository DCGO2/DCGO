using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Angewomon
namespace DCGO.CardEffects.EX12
{
    public class EX12_044 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("NSp")
                        || targetPermanent.TopCard.EqualsTraits("VB");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 4)
                );
            }
            #endregion

            #region DNA Digivolution
            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect("DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool PermanentCondition1(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && (permanent.TopCard.CardColors.Contains(CardColor.Yellow)
                                    || permanent.TopCard.CardColors.Contains(CardColor.Blue))
                                && permanent.Levels_ForJogress(card).Contains(4);
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && (permanent.TopCard.CardColors.Contains(CardColor.Green)
                                    || permanent.TopCard.CardColors.Contains(CardColor.Black))
                                && permanent.Levels_ForJogress(card).Contains(4);
                        }

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 4 Yellow or Blue Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 4 Green or Black Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Shared OP/WD/WA
            string SharedEffectName = "1 enemy Digimon gets -4K DP for turn";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] 1 of your opponent's Digimon gets -4000 DP for the turn.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanent,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                IEnumerator SelectPermanent(Permanent permanent)
                {
                    if (permanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            permanent,
                            -4000,
                            EffectDuration.UntilEachTurnEnd,
                            activateClass));
                    }
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May digivolve into [Garurumon] in name/[NSo]/[VB] trait in trash for 2 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                 => "[When Attacking] If this Digimon's stack has 2 or more same-level cards, it may digivolve into an [Angel], [Holy Dragon], [Three Great Angels], [NSp] or [VB] trait Digimon card in the hand with the cost reduced by 2.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition)
                        && SameLevelSourcesCondition(hashtable);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Angel")
                        || cardSource.EqualsTraits("Holy Dragon")
                        || cardSource.EqualsTraits("Three Great Angels")
                        || cardSource.EqualsTraits("NSp")
                        || cardSource.EqualsTraits("VB");
                }

                bool SameLevelSourcesCondition(Hashtable hashtable)
                {
                    return (card.PermanentOfThisCard().StackCards
                        .Filter(x => !x.IsFlipped)
                        .GroupBy(x => x.Level)
                        .Any(g => g.Count() >= 2));
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        card.PermanentOfThisCard(),
                        digivolvingCard => CanSelectCardCondition(digivolvingCard),
                        payCost: true,
                        reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: false,
                        activateClass: activateClass,
                        successProcess: null
                    ));
                }
            }
            #endregion

            #region Inherited
            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel
                        && cardSource.Level <= 4
                        && (cardSource.EqualsTraits("Holy Beast")
                            || cardSource.EqualsTraits("NSp")
                            || cardSource.EqualsTraits("VB"));
                }

                string[] decodeStrings = { "(Lv.4 or lower w/[Holy Beast]/[NSp]/[VB] trait)", "Level 4 or lower Digimon card with [Holy Beast]/[NSp]/[VB] trait" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: true, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
