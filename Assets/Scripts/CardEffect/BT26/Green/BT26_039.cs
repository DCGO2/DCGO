using System;
using System.Collections;
using System.Collections.Generic;

// Sunflowmon
namespace DCGO.CardEffects.BT26
{
    public class BT26_039 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("DATA SQUAD");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName()
                => "If 1 or fewer Tamers, may play [Yoshino Fujieda] free";

            string SharedEffectDescription(string tag)
                => $"[{tag}] If you have 1 or fewer Tamers, you may play 1 [Yoshino Fujieda] from your hand without paying the cost.";

            bool CanSelectHandCardCondition(CardSource cardSource, ICardEffect activateClass)
                => cardSource.IsTamer
                    && cardSource.EqualsCardName("Yoshino Fujieda")
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.OwnerHas1OrLessTamers(card)
                    && CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanSelectHandCardCondition(cardSource, activateClass));

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanSelectHandCardConditionBound(CardSource cardSource) => CanSelectHandCardCondition(cardSource, activateClass);

                if (CardEffectCommons.OwnerHas1OrLessTamers(card) && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCardConditionBound))
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: CanSelectHandCardConditionBound,
                        root: SelectCardEffect.Root.Hand,
                        cardEffect: activateClass,
                        payCost: false));
                }
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                additionalActivateCondition: SharedAdditionalActivateCondition,
                onPlay: true,
                whenDigivolving: true);

            #region Inherit
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent's Digimon can't unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT26_039_Inherit");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] [Once Per Turn] 1 of your opponent's Digimon can't unsuspend until their turn ends.";

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that can't unsuspend.", "The opponent is selecting 1 Digimon that can't unsuspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspend(permanent, EffectDuration.UntilOpponentTurnEnd, activateClass, null, "Can't unsuspend"));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
