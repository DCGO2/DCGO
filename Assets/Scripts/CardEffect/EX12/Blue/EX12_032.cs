using System.Collections;
using System.Collections.Generic;
using System.Linq;

// WereGarurumon
namespace DCGO.CardEffects.EX12
{
    public class EX12_032 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Garurumon")
                        || targetPermanent.TopCard.EqualsTraits("NSo")
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
                                && (permanent.TopCard.CardColors.Contains(CardColor.Blue)
                                    || permanent.TopCard.CardColors.Contains(CardColor.Yellow))
                                && permanent.Levels_ForJogress(card).Contains(4);
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && (permanent.TopCard.CardColors.Contains(CardColor.Purple)
                                    || permanent.TopCard.CardColors.Contains(CardColor.Red))
                                && permanent.Levels_ForJogress(card).Contains(4);
                        }

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 4 Blue or Yellow Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 4 Purple or Red Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "Stun 1 enemy Digimon/Tamer";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] 1 of your opponent's Digimon or Tamers can't suspend until their turn ends.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon
                        || permanent.IsTamer);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
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
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer that will get unable to suspend.", "The opponent is selecting 1 Digimon or Tamer that will get unable to suspend.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    Permanent selectedPermanent = permanent;

                    if (selectedPermanent != null)
                    {
                        CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                        canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCondition1, card);
                        canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                        selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => canNotSuspendClass);

                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                        }

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return selectedPermanent.TopCard != null
                                && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                        }

                        bool PermanentCondition(Permanent permanent)
                        {
                            return permanent == selectedPermanent;
                        }
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
                 => "[When Attacking] If this Digimon's stack has 2 or more same-level cards, it may digivolve into a Digimon card with [Garurumon] in its name or the [NSo] or [VB] trait in the trash with the cost reduced by 2.";

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
                    return cardSource.ContainsCardName("Garurumon")
                        || cardSource.EqualsTraits("NSo")
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
                        isHand: true,
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
                        && (cardSource.ContainsCardName("Gabumon")
                            || cardSource.ContainsCardName("Garurumon")
                            || cardSource.EqualsTraits("NSo")
                            || cardSource.EqualsTraits("VB"));
                }

                string[] decodeStrings = { "(Lv.4 or lower w/[Gabumon]/[Garurumon] in name or w/[NSo]/[VB] trait)", "Level 4 or lower Digimon card with [Gabumon]/[Garurumon] in name or [NSo]/[VB] Trait" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: true, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
