using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Target 1 Digimon gains [Progress]
    public static IEnumerator GainProgress(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool CanUseCondition()
        {
            if (IsPermanentExistsOnBattleArea(targetPermanent))
            {
                if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
                {
                    return true;
                }
            }

            return false;
        }

        CanNotAffectedClass progress = CardEffectFactory.ProgressStaticEffect(isInheritedEffect: false, card: card, condition: CanUseCondition);

        AddEffectToPermanent(targetPermanent: targetPermanent, effectDuration: effectDuration, card: card, cardEffect: progress, timing: EffectTiming.None);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion

    #region Can activate [Progress]
    public static bool CanActivateProgress(CardSource cardSource)
    {
        if (IsExistOnBattleAreaDigimon(cardSource))
        {
            if (GManager.instance.attackProcess.IsAttacking)
            {
                if (GManager.instance.attackProcess.AttackingPermanent == cardSource.PermanentOfThisCard())
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Progress]
    public static IEnumerator ProgressProcess(CardSource cardSource, ICardEffect activateClass, Func<IEnumerator> beforeOnAttackCoroutine = null)
    {
        Permanent selectedPermanent = cardSource.PermanentOfThisCard();

        if (CanActivateProgress(cardSource))
        {
            CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
            canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon's effect", CanUseCondition1, cardSource);
            canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
            selectedPermanent.UntilEndAttackEffects.Add((_timing) => canNotAffectedClass);

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));

            bool CanUseCondition1(Hashtable hashtable)
            {
                return CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent);
            }

            bool CardCondition(CardSource cardSource)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                {
                    if (cardSource == selectedPermanent.TopCard)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool SkillCondition(ICardEffect cardEffect)
            {
                if (cardEffect != null)
                {
                    if (cardEffect.EffectSourceCard != null)
                    {
                        if (IsOpponentEffect(cardEffect,cardSource))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
    }
    #endregion
}