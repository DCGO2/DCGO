using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT5_108 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] Delete 1 of your opponent's unsuspended level 4 Digimon and 1 of your opponent's unsuspended level 5 Digimon.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (!permanent.IsSuspended)
                    {
                        if (permanent.Level == 4)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanSelectPermanentCondition1(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (!permanent.IsSuspended)
                    {
                        if (permanent.Level == 5)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, (permanent) => permanent.IsDigimon && CanSelectPermanentCondition(permanent) || CanSelectPermanentCondition1(permanent)))
                {
                    int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount((permanent) => CanSelectPermanentCondition(permanent) || CanSelectPermanentCondition1(permanent)));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: (permanent) => CanSelectPermanentCondition(permanent) || CanSelectPermanentCondition1(permanent),
                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    bool CanTargetCondition_ByPreSelecetedList(List<Permanent> permanents, Permanent permanent)
                    {
                        if (permanents.Count(CanSelectPermanentCondition) >= 1)
                        {
                            if (CanSelectPermanentCondition(permanent))
                            {
                                return false;
                            }
                        }

                        if (permanents.Count(CanSelectPermanentCondition1) >= 1)
                        {
                            if (CanSelectPermanentCondition1(permanent))
                            {
                                return false;
                            }
                        }

                        return true;
                    }

                    bool CanEndSelectCondition(List<Permanent> permanents)
                    {
                        if (permanents.Count <= 0)
                        {
                            return false;
                        }

                        if (permanents.Count(CanSelectPermanentCondition) >= 2)
                        {
                            return false;
                        }

                        if (permanents.Count(CanSelectPermanentCondition1) >= 2)
                        {
                            return false;
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            if (permanents.Count(CanSelectPermanentCondition) == 0)
                            {
                                return false;
                            }
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                        {
                            if (permanents.Count(CanSelectPermanentCondition1) == 0)
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Delete unsuspended level 4 and 5 Digimons");
        }

        return cardEffects;
    }
}
