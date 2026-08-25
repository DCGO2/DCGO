using System.Collections;
using System.Collections.Generic;

// RizeGreymon
namespace DCGO.CardEffects.BT21
{
    public class BT21_044 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("GeoGreymon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "[Marcus Damon] becomes a Digimon with <Rush> and <Alliance> for turn, then 1 Digimon may attack";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    isSkippableFunction: IsSkippable,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] For the turn, 1 of your [Marcus Damon]s is also treated as a 3000 DP Digimon, can't digivolve, and gains <Rush> and <Alliance>. Then, 1 of your Digimon may attack.";

            bool IsSkippable(Hashtable hashtable)
            {
                return !CardEffectCommons.HasMatchConditionPermanent(SharedCanSelectPermanentCondition);
            }

            bool SharedCanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                    && permanent.TopCard.EqualsCardName("Marcus Damon");
            }

            bool SharedCanSelectOwnerPermanentCondition(Permanent permanent, ActivateClass activateClass)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.CanAttack(activateClass);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(SharedCanSelectPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect1.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SharedCanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine1,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect1.SetUpCustomMessage("Select 1 [Marcus Damon].", "The opponent is selecting 1 [Marcus Damon].");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    IEnumerator SelectPermanentCoroutine1(Permanent selectedPermanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.BecomeDigimonThatCantDigivolve(targetPermanent: selectedPermanent, DP: 3000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainAlliance(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                    }
                }

                if (CardEffectCommons.HasMatchConditionPermanent(_ => SharedCanSelectOwnerPermanentCondition(_, activateClass)))
                {
                    SelectPermanentEffect selectPermanentEffect2 = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect2.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: _ => SharedCanSelectOwnerPermanentCondition(_, activateClass),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Attack,
                        cardEffect: activateClass);

                    selectPermanentEffect2.SetUpCustomMessage("Select 1 Digimon that will attack.",
                        "The opponent is selecting 1 Digimon that will attack.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect2.Activate());
                }
            }
            #endregion

            #region Shared AT/Inherit
            string SharedEffectDescription1()
            {
                return "[All Turns][Once Per Turn] When any of your yellow or red Tamers are deleted, you may place 1 [Marcus Damon] from your trash as the top security card.";
            }

            bool SharedCanActivateCondition1(ActivateClass activateClass)
            {
                return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
            }

            bool SharedCanSelectCardCondition1(CardSource cardSource)
            {
                return cardSource.EqualsCardName("Marcus Damon");
            }

            bool SharedPermanentCondition1(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card);
            }

            IEnumerator SharedActivateCoroutine1(ActivateClass activateClass)
            {
                bool isUsed = false;

                if (card.Owner.CanAddSecurity(activateClass)
                && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, SharedCanSelectCardCondition1))
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: SharedCanSelectCardCondition1,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 [Marcus Damon] to add to security.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 card to add to security.", "The opponent is selecting 1 card to add to security.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Security Card");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (selectedCards.Count >= 1)
                    {
                        isUsed = true;

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(selectedCards[0]));
                    }
                }

                if (!isUsed) activateClass.RemoveUse();
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Marcus Damon] on the top of security from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(_ => SharedCanActivateCondition1(activateClass), _ => SharedActivateCoroutine1(activateClass), 1, false, SharedEffectDescription1());
                activateClass.SetHashString("BT21_044_AT");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, SharedPermanentCondition1, activateClass);
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Marcus Damon] on the top of security from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(_ => SharedCanActivateCondition1(activateClass), _ => SharedActivateCoroutine1(activateClass), 1, false, SharedEffectDescription1());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT21_044_Inherited_AT");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, SharedPermanentCondition1, activateClass);
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
