using System;
using System.Collections;
using System.Collections.Generic;

// Wolvermon
namespace DCGO.CardEffects.BT26
{
    public class BT26_053 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Glowing Dawn");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnAttackTargetChanged)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing a Tamer's bottom face-down card, use 1 [Glowing Dawn] Option cost 4 or less free", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT26_053_AllTurns");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When attack targets change, by trashing the bottom face-down card from under any of your Tamers, you may use 1 Option card with the [Glowing Dawn] trait and a use cost of 4 or less from your hand without paying the cost.";

                bool IsTamerWithFaceDownCard(ICardEffect activateClass, Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.HasFaceDownDigivolutionCards
                        && !permanent.ImmuneFromStackTrashing(activateClass);

                bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

                bool CanSelectOptionCardCondition(CardSource cardSource)
                    => cardSource.IsOption
                        && cardSource.EqualsTraits("Glowing Dawn")
                        && cardSource.HasPlayCost && cardSource.GetCostItself <= 4
                        && !cardSource.CanNotPlayThisOption;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentAttackTargetSwitch(hashtable, permanent => true);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(permanent => IsTamerWithFaceDownCard(activateClass, permanent));

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool IsTamerWithFaceDownCardBound(Permanent permanent) => IsTamerWithFaceDownCard(activateClass, permanent);

                    if (CardEffectCommons.HasMatchConditionPermanent(IsTamerWithFaceDownCardBound))
                    {
                        Permanent selectedTamer = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsTamerWithFaceDownCardBound,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedTamer = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to trash its bottom face-down card.", "The opponent is selecting 1 Tamer to trash its bottom face-down card.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedTamer != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedTamer, trashCount: 1, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));

                            if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectOptionCardCondition))
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                    canTargetCondition: CanSelectOptionCardCondition,
                                    root: SelectCardEffect.Root.Hand,
                                    cardEffect: activateClass,
                                    payCost: false));
                            }
                        }
                    }
                }
            }
            #endregion

            #region Inherit - Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
