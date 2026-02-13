using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class BT16_098 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main - Option Skill

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete all opponent's digimon with lowest play cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] If you have a Digimon with [Dorugoramon] in its name, delete 1 of your opponent's Digimon or Tamers with a play cost of 4 or less. Then, delete all of your opponent's Digimon with the lowest play cost.";
                }

                bool HasDorugoramonInPlay(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.ContainsCardName("Dorugoramon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectDeleteTarget(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsDigimon || permanent.IsTamer)
                        {
                            if (permanent.TopCard.HasPlayCost && permanent.TopCard.GetCostItself <= 4)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool LowestPlayCostCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, true))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasDorugoramonInPlay))
                    {
                        if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectDeleteTarget))
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectDeleteTarget,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }

                    List<Permanent> destroyTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(LowestPlayCostCondition);
                    yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                }
            }

            #endregion

            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Delete all of your opponent's Digimon with the lowest play cost");
            }

            return cardEffects;
        }
    }
}