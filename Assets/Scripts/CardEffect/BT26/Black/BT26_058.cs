using System;
using System.Collections;
using System.Collections.Generic;

// HiAndromon
namespace DCGO.CardEffects.BT26
{
    public class BT26_058 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("CS");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared When Digivolving / When Attacking

            string SharedEffectName()
                => "1 of your [CS] Digimon isn't affected by opponent's Digimon effects";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] Your opponent's Digimon effects don't affect 1 of your Digimon with the [CS] trait until their turn ends.";

            bool CanSelectPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.EqualsTraits("CS");

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
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

                selectPermanentEffect.SetUpCustomMessage("Select 1 [CS] Digimon that isn't affected by opponent's Digimon effects.", "The opponent is selecting 1 [CS] Digimon that isn't affected by opponent's Digimon effects.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                    canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon effects", CanUseCondition1, card);
                    canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition1, SkillCondition: SkillCondition1);
                    permanent.UntilOpponentTurnEndEffects.Add((_timing) => canNotAffectedClass);

                    bool CanUseCondition1(Hashtable _hashtable) => CardEffectCommons.IsPermanentExistsOnBattleArea(permanent);

                    bool CardCondition1(CardSource cardSource) => CardEffectCommons.IsPermanentExistsOnBattleArea(permanent) && cardSource == permanent.TopCard;

                    bool SkillCondition1(ICardEffect cardEffect)
                        => cardEffect != null
                            && cardEffect.EffectSourceCard != null
                            && cardEffect.EffectSourceCard.Owner == card.Owner.Enemy
                            && cardEffect.EffectSourceCard.IsDigimon;

                    yield return null;
                }
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: "BT26_058_WD_WA",
                additionalActivateCondition: SharedAdditionalActivateCondition,
                whenDigivolving: true,
                whenAttacking: true);

            #region All Turns - Protect CS Digimon
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing this Digimon's top stacked card under the leaving Digimon, it doesn't leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] When any of your Digimon with the [CS] trait would leave the battle area, by placing this Digimon's top stacked card as its bottom digivolution card, they don't leave.";

                bool ProtectedPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("CS");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, ProtectedPermanentCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.PermanentOfThisCard().DigivolutionCards.Count >= 1;

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> removedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(ProtectedPermanentCondition);

                    bool isUsed = false;

                    if (card.PermanentOfThisCard() != null && card.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                    {
                        CardSource topStackedCard = card.PermanentOfThisCard().DigivolutionCards[0];

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: _ => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "By placing this Digimon's top stacked card under the leaving Digimon, it doesn't leave.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            customRootCardList: new List<CardSource>() { topStackedCard },
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("By placing this Digimon's top stacked card under the leaving Digimon, it doesn't leave.", "The opponent is deciding whether to place this Digimon's top stacked card under the leaving Digimon.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1 && removedPermanents.Count >= 1)
                            {
                                Permanent targetPermanent = removedPermanents[0];

                                yield return ContinuousController.instance.StartCoroutine(targetPermanent.AddDigivolutionCardsBottom(cardSources, activateClass));

                                isUsed = true;
                            }
                        }
                    }

                    if (isUsed)
                    {
                        foreach (Permanent permanent in removedPermanents)
                        {
                            permanent.willBeRemoveField = false;
                            permanent.HideDeleteEffect();
                            permanent.HideHandBounceEffect();
                            permanent.HideDeckBounceEffect();
                            permanent.HideWillRemoveFieldEffect();
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
