using System;
using System.Collections;
using System.Collections.Generic;

// Gaogamon
namespace DCGO.CardEffects.BT25
{
    public class BT25_023 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("DATA SQUAD");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 3, permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "May play 1 [Thomas H. Norstein] from hand for free";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    additionalActivateCondition: AdditionalActivateCondition,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] If you have 1 or fewer Tamers, you may play 1 [Thomas H. Norstein] from your hand without paying the cost.";

            bool AdditionalActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.MatchConditionOwnersPermanentCount(card, HasTamersInBattleArea) <= 1;
            }

            bool HasTamersInBattleArea(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                    && permanent.IsTamer;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool HasTamerInHand(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Thomas H. Norstein")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                List<CardSource> selectedCards = new List<CardSource>();
                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, HasTamerInHand));

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: HasTamerInHand,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: true,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: null,
                    mode: SelectHandEffect.Mode.PlayForFree,
                    cardEffect: activateClass);

                yield return StartCoroutine(selectHandEffect.Activate());
            }
            #endregion

            #region WA - Inherited
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Both players <Draw 1>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT25_023_Inherited");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking][Once Per Turn] Both players <Draw 1>.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
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
