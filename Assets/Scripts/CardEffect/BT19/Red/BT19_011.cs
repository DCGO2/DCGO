using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT19
{
    public class BT19_011 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Blast Digivolve
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                int enemyCount = Math.Min(card.Owner.Enemy.GetBattleAreaDigimons().Count, card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame()));
                int maxDP = 3000 + 2000 * enemyCount;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete up to 3000 DP of your opponents Digimon, this DP-based deletion increases by 2000 DP for each of your opponent's Digimon and gain memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Delete any of your opponent's Digimon with DP adding up to 3000. For each of your opponent's Digimon, add 2000 to this DP-Based deletion effect's maximum. Then, for each Digimon deleted by this effect, gain 1 memory.";
                }

                bool CanSelectOpponentsPermanent(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(maxDP, activateClass))
                        {
                            if (permanent.TopCard.HasDP)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanEndSelectCondition(List<Permanent> permanents)
                {
                    if (permanents.Count <= 0)
                        return false;

                    int sumDP = 0;

                    foreach (Permanent permanent1 in permanents)
                    {
                        sumDP += permanent1.DP;
                    }

                    if (sumDP > card.Owner.MaxDP_DeleteEffect(maxDP, activateClass))
                        return false;

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

                    if (sumDP > card.Owner.MaxDP_DeleteEffect(maxDP, activateClass))
                        return false;

                    return true;
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
                    int destroyCount = card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectOpponentsPermanent);

                    List<Permanent> destroyTargetPermanents = new List<Permanent>();

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentsPermanent,
                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: destroyCount,
                        canNoSelect: false,
                        canEndNotMax: true,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        destroyTargetPermanents.Add(permanent);

                        yield return null;
                    }

                    if (card.Owner.CanAddMemory(activateClass))
                    {
                        var memoryGain = destroyTargetPermanents.Count - destroyTargetPermanents.Filter(x => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(x, card)).Count;
                        if (memoryGain > 0) yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(memoryGain, activateClass));
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                int enemyCount = Math.Min(card.Owner.Enemy.GetBattleAreaDigimons().Count, card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame()));
                int maxDP = 3000 + 2000 * enemyCount;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete up to 3000 DP of your opponents Digimon, this DP-based deletion increases by 2000 DP for each of your opponent's Digimon and gain memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Delete any of your opponent's Digimon with DP adding up to 3000. For each of your opponent's Digimon, add 2000 to this DP-Based deletion effect's maximum. Then, for each Digimon deleted by this effect, gain 1 memory.";
                }

                bool CanSelectOpponentsPermanent(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(maxDP, activateClass))
                        {
                            if (permanent.TopCard.HasDP)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanEndSelectCondition(List<Permanent> permanents)
                {
                    if (permanents.Count <= 0)
                        return false;

                    int sumDP = 0;

                    foreach (Permanent permanent1 in permanents)
                    {
                        sumDP += permanent1.DP;
                    }

                    if (sumDP > card.Owner.MaxDP_DeleteEffect(maxDP, activateClass))
                        return false;

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

                    if (sumDP > card.Owner.MaxDP_DeleteEffect(maxDP, activateClass))
                        return false;

                    return true;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int destroyCount = card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectOpponentsPermanent);

                    List<Permanent> destroyTargetPermanents = new List<Permanent>();

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentsPermanent,
                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: destroyCount,
                        canNoSelect: false,
                        canEndNotMax: true,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        destroyTargetPermanents.Add(permanent);

                        yield return null;
                    }

                    if (card.Owner.CanAddMemory(activateClass))
                    {
                        var memoryGain = destroyTargetPermanents.Count - destroyTargetPermanents.Filter(x => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(x, card)).Count;
                        if (memoryGain > 0) yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(memoryGain, activateClass));
                    }
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.None)
            {
                ChangeDPDeleteEffectMaxDPClass changeDPDeleteEffectMaxDPClass = new ChangeDPDeleteEffectMaxDPClass();
                changeDPDeleteEffectMaxDPClass.SetUpICardEffect("Maximum DP of DP-based deletion effects gets +3000 DP", CanUseCondition, card);
                changeDPDeleteEffectMaxDPClass.SetUpChangeDPDeleteEffectMaxDPClass(changeMaxDP: ChangeMaxDP);
                changeDPDeleteEffectMaxDPClass.SetIsInheritedEffect(true);
                cardEffects.Add(changeDPDeleteEffectMaxDPClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            return true;
                        }                       
                    }

                    return false;
                }

                int ChangeMaxDP(int maxDP, ICardEffect cardEffect)
                {
                    if (cardEffect != null)
                    {
                        if (cardEffect.EffectSourceCard != null)
                        {
                            if (cardEffect.EffectSourceCard.Owner == card.Owner)
                            {
                                if (cardEffect.EffectSourceCard.PermanentOfThisCard() == card.PermanentOfThisCard()) maxDP += 3000;
                            }
                        }
                    }

                    return maxDP;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}