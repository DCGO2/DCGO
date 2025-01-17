using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT13
{
    public class BT13_017 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Choose any number of your opponent's Digimon so that their DP total is up to 6000 and delete them. For each of your other Digimon, add 2000 to the maximum this DP-based deletion effect can delete.";
                }

                int maxDP()
                {
                    int maxDP = 6000;

                    maxDP += 2000 * card.Owner.GetBattleAreaDigimons().Count((permanent) => permanent != card.PermanentOfThisCard());

                    return maxDP;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(maxDP(), activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    int maxCount = Math.Min(card.Owner.Enemy.GetBattleAreaDigimons().Count(CanSelectPermanentCondition), CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    bool CanEndSelectCondition(List<Permanent> permanents)
                    {
                        if (permanents.Count <= 0)
                        {
                            return false;
                        }

                        int sumDP = 0;

                        foreach (Permanent permanent1 in permanents)
                        {
                            sumDP += permanent1.DP;
                        }

                        if (sumDP > card.Owner.MaxDP_DeleteEffect(maxDP(), activateClass))
                        {
                            return false;
                        }

                        return true;
                    }

                    bool CanTargetCondition_ByPreSelecetedList(List<Permanent> permanents, Permanent permanent)
                    {
                        int sumDP = 0;

                        foreach (Permanent permanent1 in permanents)
                        {
                            sumDP += permanent1.DP;
                        }

                        sumDP += permanent.DP;

                        if (sumDP > card.Owner.MaxDP_DeleteEffect(maxDP(), activateClass))
                        {
                            return false;
                        }

                        return true;
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Choose any number of your opponent's Digimon so that their DP total is up to 6000 and delete them. For each of your other Digimon, add 2000 to the maximum this DP-based deletion effect can delete.";
                }

                int maxDP()
                {
                    int maxDP = 6000;

                    maxDP += 2000 * card.Owner.GetBattleAreaDigimons().Count((permanent) => permanent != card.PermanentOfThisCard());

                    return maxDP;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(maxDP(), activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    int maxCount = Math.Min(card.Owner.Enemy.GetBattleAreaDigimons().Count(CanSelectPermanentCondition), CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    bool CanEndSelectCondition(List<Permanent> permanents)
                    {
                        if (permanents.Count <= 0)
                        {
                            return false;
                        }

                        int sumDP = 0;

                        foreach (Permanent permanent1 in permanents)
                        {
                            sumDP += permanent1.DP;
                        }

                        if (sumDP > card.Owner.MaxDP_DeleteEffect(maxDP(), activateClass))
                        {
                            return false;
                        }

                        return true;
                    }

                    bool CanTargetCondition_ByPreSelecetedList(List<Permanent> permanents, Permanent permanent)
                    {
                        int sumDP = 0;

                        foreach (Permanent permanent1 in permanents)
                        {
                            sumDP += permanent1.DP;
                        }

                        sumDP += permanent.DP;

                        if (sumDP > card.Owner.MaxDP_DeleteEffect(maxDP(), activateClass))
                        {
                            return false;
                        }

                        return true;
                    }
                }
            }

            if (timing == EffectTiming.None)
            {
                int count()
                {
                    return card.Owner.GetBattleAreaDigimons().Count((permanent) => permanent != card.PermanentOfThisCard() && (permanent.TopCard.ContainsCardName("Sistermon") || permanent.TopCard.HasRoyalKnightTraits));
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (count() >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                string EffectName() => $"Your Digimons gain DP+{1000 * count()}";

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect<Func<int>>(
                    permanentCondition: PermanentCondition,
                    changeValue: () => 1000 * count(),
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition,
                    effectName: EffectName));
            }

            return cardEffects;
        }
    }
}