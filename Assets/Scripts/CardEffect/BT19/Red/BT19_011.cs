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

            #region Shared OP/WD
            string SharedEffectName = "Delete up to 3K DP of enemy Digimon + 2K DP per enemy Digimon, gain 1 memory per deleted Digimon";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] Delete any of your opponent's Digimon with DP adding up to 3000. For each of your opponent's Digimon, add 2000 to this DP-Based deletion effect's maximum. Then, for each Digimon deleted by this effect, gain 1 memory.";
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                int enemyCount = Math.Min(card.Owner.Enemy.GetBattleAreaDigimons().Count, card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame()));
                int maxDP = 3000 + 2000 * enemyCount;

                bool CanSelectOpponentsPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasDP
                        && permanent.DP <= card.Owner.MaxDP_DeleteEffect(maxDP, activateClass);
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

                int destroyCount = card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectOpponentsPermanent);
                int destroyedCount = 0;

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
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                {
                    if (permanents.Count > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: permanents, activateClass: activateClass, successProcess: SuccessProcess, failureProcess: null));
                    }
                }

                IEnumerator SuccessProcess(List<Permanent> permanents)
                {
                    if (card.Owner.CanAddMemory(activateClass))
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(permanents.Count, activateClass));
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
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
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