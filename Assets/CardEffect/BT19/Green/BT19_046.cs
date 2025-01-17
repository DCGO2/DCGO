using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT19
{
    public class BT19_046 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play/When Digivolving Shared
            bool OpponentsDigimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool OpponentsDataTraitDigimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                       permanent.TopCard.EqualsTraits("Data");
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 digimon, then 1 Digimon with [Data] trait can't unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Suspend 1 of your opponent's Digimon. Then, 1 of their Digimon with the [Data] trait can't unsuspend until the end of their turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimon))
                    {
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDataTraitDigimon))
                    {
                        Permanent selectedPermanent = null;

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsDataTraitDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectCantUnsuspendTarget,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectCantUnsuspendTarget(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if(selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspend(
                                        targetPermanent: selectedPermanent,
                                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                        activateClass: activateClass,
                                        condition: null,
                                        effectName: "Can't unsuspend"
                                    ));
                        }
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 digimon, then 1 Digimon with [Data] trait can't unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Suspend 1 of your opponent's Digimon. Then, 1 of their Digimon with the [Data] trait can't unsuspend until the end of their turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimon))
                    {
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDataTraitDigimon))
                    {
                        Permanent selectedPermanent = null;

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsDataTraitDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectCantUnsuspendTarget,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectCantUnsuspendTarget(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspend(
                                        targetPermanent: selectedPermanent,
                                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                        activateClass: activateClass,
                                        condition: null,
                                        effectName: "Can't unsuspend"
                                    ));
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}