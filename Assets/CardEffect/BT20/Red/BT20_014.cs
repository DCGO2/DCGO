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

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                {
                    #region On Play
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
                        if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        {
                            if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanent))
                            {
                                return true;
                            }
                        }
                        return false;
                    }

                    bool CanSelectPermanent(Permanent permanent)
                    {
                        return permanent.DP <= 5000;
                    }

                    IEnumerator ActivateCoroutine(Hashtable hashtable)
                    {
                        List<Permanent> selectedPermanents = new List<Permanent>();

                        int maxCount = 1;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner.Enemy,
                            canTargetCondition: CanSelectPermanent,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
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
                    #endregion
                }
                {
                    #region On Digivolve
                    ActivateClass activateClass = new ActivateClass();
                    activateClass.SetUpICardEffect("Delete a Digimon with 5000DP or less", CanUseCondition, card);
                    activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                    cardEffects.Add(activateClass);

                    string EffectDiscription()
                    {
                        return "[On Digivolve] Delete 1 of your opponent's Digimon with 5000DP or less.";
                    }

                    bool CanUseCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                    }

                    bool CanActivateCondition(Hashtable hashtable)
                    {
                        if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        {
                            if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanent))
                            {
                                return true;
                            }
                        }
                        return false;
                    }

                    bool CanSelectPermanent(Permanent permanent)
                    {
                        return permanent.DP <= 5000;
                    }

                    IEnumerator ActivateCoroutine(Hashtable hashtable)
                    {
                        List<Permanent> selectedPermanents = new List<Permanent>();

                        int maxCount = 1;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner.Enemy,
                            canTargetCondition: CanSelectPermanent,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
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

                    #endregion
                }
            }

            if (timing == EffectTiming.OnEndTurn)
            {
                #region End of Turn
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend a Digimon, then Digivolve this Digimon into a Digimon card with [Jesmon] in its name", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] By suspending 1 of your other Digimon, this Digiomon may digivolve into a Digimon card with [Jesmon] in its name in the hand without paying the cost.";
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
                            if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardToDigivolveInto))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanSelectCardToDigivolveInto(CardSource cardSource)
                {
                    if (cardSource.ContainsCardName("Jesmon"))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanSelectPermanentToSuspend (Permanent permanent)
                {
                    if (permanent.CanSuspend)
                    {
                        if (permanent != card.PermanentOfThisCard())
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
                #endregion
            }

            if (timing == EffectTiming.None)
            {
                #region inherited
                bool CanUseCondition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) && CardEffectCommons.IsOwnerTurn(card);
                }

                bool PermanentCondition (Permanent permanent)
                {
                    if (permanent == card.PermanentOfThisCard())
                    {
                        if (permanent.TopCard.HasRoyalKnightTraits)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.RebootStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: true, card: card, condition: CanUseCondition));
                #endregion
            }

            return cardEffects;
        }
    }
}