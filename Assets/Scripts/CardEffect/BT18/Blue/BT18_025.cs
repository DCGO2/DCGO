using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT18
{
    public class BT18_025 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            // Any blue or red tamer
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.IsTamer && (targetPermanent.TopCard.CardColors.Contains(CardColor.Blue) ||
                                                       targetPermanent.TopCard.CardColors.Contains(CardColor.Red));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
            }

            // Tommy Himi
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return card.Owner.HandCards.Contains(card);
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Tommy Himi");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false,
                    card: card, condition: Condition));
            }

            // Kumamon
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Kumamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
            }

            #endregion

            #region Jamming

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region On Play/ When Digivolving Shared

            bool CanSelectPermanentConditionShared(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                       permanent.HasNoDigivolutionCards;
            }

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                       CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentConditionShared);
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent's Digimon can't suspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] 1 of your opponent's Digimon with no digivolution cards can't suspend until the end of their turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentConditionShared,
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
                        CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                        canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCanNotSuspendCondition, card);
                        canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCanNotSuspendCondition);
                        permanent.UntilOwnerTurnEndEffects.Add(_ => canNotSuspendClass);

                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                .CreateDebuffEffect(permanent));
                        }

                        bool CanUseCanNotSuspendCondition(Hashtable hashtableCanNotSuspend)
                        {
                            return permanent.TopCard != null && !permanent.TopCard.CanNotBeAffected(activateClass);
                        }

                        bool PermanentCanNotSuspendCondition(Permanent permanentCanNotSuspend)
                        {
                            return permanentCanNotSuspend == permanent;
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent's Digimon can't suspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] 1 of your opponent's Digimon with no digivolution cards can't suspend until the end of their turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentConditionShared,
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
                        CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                        canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCanNotSuspendCondition, card);
                        canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCanNotSuspendCondition);
                        permanent.UntilOwnerTurnEndEffects.Add(_ => canNotSuspendClass);

                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                .CreateDebuffEffect(permanent));
                        }

                        bool CanUseCanNotSuspendCondition(Hashtable hashtableCanNotSuspend)
                        {
                            return permanent.TopCard != null && !permanent.TopCard.CanNotBeAffected(activateClass);
                        }

                        bool PermanentCanNotSuspendCondition(Permanent permanentCanNotSuspend)
                        {
                            return permanentCanNotSuspend == permanent;
                        }
                    }
                }
            }

            #endregion

            #region Jamming - ESS

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}