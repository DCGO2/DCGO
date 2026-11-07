using System.Collections;
using System.Collections.Generic;

// Gaogamon
namespace DCGO.CardEffects.BT27
{
    public class BT27_024 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - [DATA SQUAD]
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.EqualsTraits("DATA SQUAD");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    level: 3,
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Jamming
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(false, card, null));
            }
            #endregion

            #region OP/WD Shared
            string SharedEffectName = "Trash 2 enemy digivolution cards per 4 cards in their hand, then stun 1 enemy sourceless Digimon until their turn ends";

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
                return $"[{tag}] For every 4 cards your opponent's hand has, trash any 2 digivolution cards from their Digimon. Then, 1 of their Digimon with no digivolution cards can't suspend until their turn ends.";
            }

            bool CanSelectTrashPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionCards.Count >= 1;
            }

            bool CanSelectStunPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionCards.Count == 0;
            }

            IEnumerator SharedActivateCoroutine(Hashtable _hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                    permanentCondition: CanSelectTrashPermanentCondition,
                    cardCondition: null,
                    maxCount: card.Owner.Enemy.HandCards.Count / 4 * 2,
                    canNoTrash: false,
                    isFromOnly1Permanent: false,
                    activateClass: activateClass
                ));

                if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectStunPermanentCondition)) yield break;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectStunPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will not be able to suspend.",
                    "The opponent is selecting 1 Digimon that will not be able to suspend.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    if (permanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotSuspend(
                            targetPermanent: permanent,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            condition: null,
                            effectName: "Can't suspend"));
                    }
                }
            }
            #endregion

            #region Inherited WA
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Both players <Draw 1>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT27_024_Inherited");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] [Once Per Turn] Both players <Draw 1>.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(player, 1, activateClass).Draw());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
