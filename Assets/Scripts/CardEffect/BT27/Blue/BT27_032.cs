using System;
using System.Collections;
using System.Collections.Generic;

// MirageGaogamon // Full Moon Blaster
namespace DCGO.CardEffects.BT27
{
    public class BT27_032 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Side
            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.ContainsCardName("Gaogamon")
                        || targetPermanent.TopCard.HasDataSquadTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Reboot/Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Evade
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.EvadeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Digivolving

            string SharedEffectName = "For every 4 opponent's hand cards, 1 opponent's Digimon/Tamer can't suspend";

            string SharedEffectDescription(string tag)
                => $"[{tag}] For every 4 cards your opponent's hand has, 1 of their Digimon or Tamers can't suspend until their turn ends.";

            bool CanSelectCantSuspendCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                int maxCount = Math.Min(card.Owner.Enemy.HandCards.Count / 4, CardEffectCommons.MatchConditionPermanentCount(CanSelectCantSuspendCondition));

                if (maxCount <= 0) yield break;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectCantSuspendCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select Digimon or Tamers that can't suspend.", "The opponent is selecting Digimon or Tamers that can't suspend.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotSuspend(
                        targetPermanent: permanent,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass,
                        condition: null,
                        effectName: "Can't suspend"));
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                whenDigivolving: true);

            #endregion

            #region All Turns - OPT
            if (timing == EffectTiming.OnAddHand || timing == EffectTiming.OnDigivolutionCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May activate 1 [Main] effect on the Option side", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT27_032_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When effects add to your opponent's hand, or effects trash cards under your Tamers, you may activate 1 [Main] effect on this card's Option side.";

                bool OpponentCondition(Player player)
                    => player == card.Owner.Enemy;

                bool OwnTamerCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && ((timing == EffectTiming.OnAddHand
                                && CardEffectCommons.CanTriggerWhenAddHand(hashtable, OpponentCondition, cardEffect => cardEffect != null))
                            || (timing == EffectTiming.OnDigivolutionCardDiscarded
                                && CardEffectCommons.CanTriggerOnTrashDigivolutionCard(hashtable, OwnTamerCondition, cardEffect => cardEffect != null, _ => true)));

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ActivateMainOfOptionSide(card, activateClass));
                }
            }
            #endregion
            #endregion

            #region Option Side
            #region Use Req.
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                    => cardSource.HasDataSquadTraits;
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 opponent's highest level Digimon to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Return 1 of your opponent's highest level Digimon to the hand.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && CardEffectCommons.IsMaxLevel(permanent, card.Owner.Enemy);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition)) yield break;

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
                        mode: SelectPermanentEffect.Mode.Bounce,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to return to hand.", "The opponent is selecting 1 Digimon to return to hand.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion
            #endregion

            return cardEffects;
        }
    }
}
