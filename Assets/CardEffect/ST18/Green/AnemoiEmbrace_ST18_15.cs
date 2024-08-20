using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.ST18
{
    public class AnemoiEmbrace_ST18_15 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main Effect

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Main] Suspend 1 Digimon. If this effect suspended your Digimon, return 1 of your opponent's suspended Digimon to the bottom of the deck. Then, unsuspend 1 Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);
                }

                bool CanSelectSuspendedPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           permanent.IsSuspended;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        bool ownDigimon = false;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: permanent => CanSelectPermanentCondition(permanent) && permanent.CanSuspend,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            ownDigimon = permanents.Some(permanent =>
                                CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card));

                            yield return null;
                        }

                        if (ownDigimon && CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendedPermanentCondition))
                        {
                            selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition_ByPreSelecetedList: null,
                                canTargetCondition: CanSelectSuspendedPermanentCondition,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to bottom deck.",
                                "The opponent is selecting 1 Digimon to bottom deck.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: permanent => CanSelectPermanentCondition(permanent) && permanent.CanUnsuspend,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.UnTap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 of your opponent's suspended Digimon to the bottom of the deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Security] Return 1 of your opponent's suspended Digimon to the bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanSelectSuspendedPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           permanent.IsSuspended;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendedPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition_ByPreSelecetedList: null,
                            canTargetCondition: CanSelectSuspendedPermanentCondition,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to bottom deck.",
                            "The opponent is selecting 1 Digimon to bottom deck.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                }
            }

            #endregion

            return cardEffects;
        }
    }
}