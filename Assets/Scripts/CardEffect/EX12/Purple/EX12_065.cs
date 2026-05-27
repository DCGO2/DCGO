using System.Collections;
using System.Collections.Generic;

// Kaguyamon
namespace DCGO.CardEffects.EX12
{
    public class EX12_065 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Cost
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Puppet")
                            || targetPermanent.TopCard.EqualsTraits("Shambala");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 5)
                );
            }
            #endregion

            #region Fortitude
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.FortitudeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD/WA
            string SharedEffectName = "May play 1 5 cost or lower [Puppet]/[Shambala] from trash for free";
            string SharedHashString = "EX12_065_OP_WD_WA";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    hashValue: SharedHashString,
                    maxCountPerTurn: 1,
                    optional: false,
                    isSkippable: true,
                    onPlay: true,
                    whenDigivolving: true,
                    whenAttacking: true);

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] You may play 1 play cost 5 or lower [Puppet] or [Shambala] trait card from your trash without paying the cost.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool Used = false;

                bool CanPlayCardCondition(CardSource cardSource)
                {
                    return cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 5
                        && (cardSource.EqualsTraits("Puppet")
                            || cardSource.EqualsTraits("Shambala"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                selectCardEffect.SetUp(
                    canTargetCondition: CanPlayCardCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    canNoSelect: () => true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                    message: "Select 1 card to play.",
                    maxCount: 1,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    mode: SelectCardEffect.Mode.PlayForFree,
                    root: SelectCardEffect.Root.Trash,
                    customRootCardList: null,
                    canLookReverseCard: true,
                    selectPlayer: card.Owner,
                    cardEffect: activateClass);

                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                {
                    if (cardSources != null)
                    {
                        Used = true;

                        yield return null;
                    }
                }

                yield return StartCoroutine(selectCardEffect.Activate());

                if (!Used)
                {
                    activateClass.RemoveUse();
                }
            }
            #endregion

            #region All Turns Give Blocker & Retaliation
            bool SharedPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && (permanent.TopCard.EqualsTraits("Puppet")
                        || permanent.TopCard.EqualsTraits("TB"));
            }

            if (timing == EffectTiming.None)
            {
                bool CanUseAllTurnsCondition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                cardEffects.Add(CardEffectFactory.BlockerStaticEffect(
                    permanentCondition: SharedPermanentCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseAllTurnsCondition));

                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("Your [Puppet]/[TB] Digimon get Retaliation", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects, limitTiming: EffectTiming.OnDestroyedAnyone);
                cardEffects.Add(addSkillClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return PermanentCondition(cardSource.PermanentOfThisCard())
                        && cardSource == cardSource.PermanentOfThisCard().TopCard
                        && (cardSource.EqualsTraits("Puppet")
                            || cardSource.EqualsTraits("TB"));
                }

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnDestroyedAnyone)
                    {
                        bool Condition()
                        {
                            return CardSourceCondition(cardSource)
                                && CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => card.PermanentOfThisCard() == permanent);
                        }

                        cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(isInheritedEffect: false, card: cardSource, condition: Condition));
                    }

                    return cardEffects;
                }
            }
            #endregion

            #region On Delete
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Bottom deck 1 enemy lowest level Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Deletion] Return 1 of your opponent's lowest level Digimon to the bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(hashtable, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
