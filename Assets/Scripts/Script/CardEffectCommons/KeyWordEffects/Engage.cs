using System.Collections;
using System;

public partial class CardEffectCommons
{
    #region Can activate [Engage]
    public static bool CanActivateEngage(CardSource cardSource, ICardEffect activateClass)
    {
        return IsExistOnBattleArea(cardSource)
            && cardSource.PermanentOfThisCard().CanAttack(activateClass)
            && !GManager.instance.attackProcess.IsAttacking;
    }
    #endregion

    #region Effect process of [Engage]
    public static IEnumerator EngageProcess(CardSource cardSource, ICardEffect activateClass, Func<IEnumerator> beforeOnAttackCoroutine = null)
    {
        if (CanActivateEngage(cardSource, activateClass))
        {
            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

            selectAttackEffect.SetUp(
                attacker: cardSource.PermanentOfThisCard(),
                canAttackPlayerCondition: () => true,
                defenderCondition: (permanent) => true,
                cardEffect: activateClass);

            selectAttackEffect.SetBeforeOnAttackCoroutine(beforeOnAttackCoroutine);

            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
        }
    }
    #endregion

    #region Target 1 Digimon gains [Engage]
    public static IEnumerator GainEngage(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool CanUseCondition()
        {
            return IsPermanentExistsOnBattleArea(targetPermanent)
                && !targetPermanent.TopCard.CanNotBeAffected(activateClass);
        }

        ActivateClass engage = CardEffectCommons.EngageEffect(
            targetPermanent: targetPermanent,
            isInheritedEffect: false,
            condition: CanUseCondition,
            rootCardEffect: activateClass,
            card: targetPermanent.TopCard);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: engage,
            timing: EffectTiming.OnEndTurn);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion

    #region Trigger effect of [Engage]
    public static ActivateClass EngageEffect(
        Permanent targetPermanent,
        bool isInheritedEffect,
        Func<bool> condition,
        ICardEffect rootCardEffect,
        CardSource card,
        Func<IEnumerator> beforeOnAttackCoroutine = null)
    {
        if (targetPermanent == null) return null;
        if (targetPermanent.TopCard == null) return null;
        if (card == null) return null;

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Engage", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
        activateClass.SetIsInheritedEffect(isInheritedEffect);

        string EffectDescription()
        {
            return $"{DataBase.EngageEffectDiscription()}";
        }

        if (rootCardEffect != null)
        {
            activateClass.SetIsInheritedEffect(false);
            activateClass.SetEffectSourcePermanent(targetPermanent);
            activateClass.SetRootCardEffect(rootCardEffect);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsOwnerTurn(card);
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanActivateEngage(targetPermanent.TopCard, activateClass)
                && (condition == null
                    || condition());
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            return CardEffectCommons.EngageProcess(targetPermanent.TopCard, activateClass, beforeOnAttackCoroutine);
        }

        return activateClass;
    }
    #endregion

}