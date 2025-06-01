using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT20
{
    public class BT20_014 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete a Digimon with 5000DP or less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Delete 1 of your opponent's Digimon with 5000DP or less.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanent);
                }

                bool CanSelectPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           permanent.DP <= card.Owner.MaxDP_DeleteEffect(5000, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanent,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass
                        );

                    selectPermanentEffect.SetUpCustomMessage("Select 1 card to delete.", "The opponent is selecting 1 card to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete a Digimon with 5000DP or less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Delete 1 of your opponent's Digimon with 5000DP or less.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanent);
                }

                bool CanSelectPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           permanent.DP <= card.Owner.MaxDP_DeleteEffect(5000, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanent,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass
                        );

                    selectPermanentEffect.SetUpCustomMessage("Select 1 card to delete.", "The opponent is selecting 1 card to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            #region End of Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending a Digimon, this Digimon may digivolve into a Digimon card with [Jesmon] in its name", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] By suspending 1 of your other Digimon, this Digiomon may digivolve into a Digimon card with [Jesmon] in its name in the hand without paying the cost.";
                }

                bool CanSelectPermanentToSuspend(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           CardEffectCommons.CanActivateSuspendCostEffect(permanent.TopCard) &&
                           (permanent != card.PermanentOfThisCard());
                }

                bool CanSelectCardToDigivolveInto(CardSource cardSource)
                {
                    return cardSource.ContainsCardName("Jesmon") &&
                           cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentToSuspend))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {

                    #region select card to suspend
                    List<Permanent> selectedPermanents = new List<Permanent>();

                    int maxCount = 1;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentToSuspend,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: selectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass
                        );
                    selectPermanentEffect.SetUpCustomMessage("Select 1 card to suspend.", "The opponent is selecting 1 card to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator selectPermanentCoroutine (Permanent permanent)
                    {
                        selectedPermanents.Add( permanent );
                        yield return null;
                    }
                    #endregion

                    if (selectedPermanents.Count > 0)
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardToDigivolveInto))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                               targetPermanent: card.PermanentOfThisCard(),
                               cardCondition: CanSelectCardToDigivolveInto,
                               payCost: false,
                               reduceCostTuple: null,
                               fixedCostTuple: null,
                               ignoreDigivolutionRequirementFixedCost: -1,
                               isHand: true,
                               activateClass: activateClass,
                               successProcess: null));
                        }
                    }
                }
            }
            #endregion

            #region Your Turn - ESS
            if (timing == EffectTiming.OnAllyAttack)
            {
                bool CanUseCondition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && 
                           CardEffectCommons.IsOwnerTurn(card) &&
                           card.PermanentOfThisCard().TopCard.HasRoyalKnightTraits;
                }

                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: true, card: card, condition: CanUseCondition));
            }
            #endregion

            return cardEffects;
        }
    }
}