using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX7
{
    public class WindSlicer_EX7_069 : CEntity_Effect
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
                        "[Main] You may suspend 1 level 6 or lower Digimon. If this effect suspended your Digimon, unsuspend 1 of your Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent) &&
                           permanent.IsDigimon &&
                           permanent.TopCard.HasLevel &&
                           permanent.Level <= 6;
                }

                bool IsOwnDigimonPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        bool ownDigimon = false;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            ownDigimon = permanents.Some(IsOwnDigimonPermanentCondition);

                            yield return null;
                        }

                        if (ownDigimon)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(IsOwnDigimonPermanentCondition))
                            {
                                selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: IsOwnDigimonPermanentCondition,
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
                }
            }

            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects,
                    effectName:
                    $"You may suspend 1 level 6 or lower Digimon. If this effect suspended your Digimon, unsuspend 1 of your Digimon.");
            }

            #endregion

            return cardEffects;
        }
    }
}