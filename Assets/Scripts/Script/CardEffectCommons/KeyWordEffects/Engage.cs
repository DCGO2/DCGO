using System;
using System.Collections;

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

        ActivateClass engage = CardEffectFactory.EngageEffect(
            targetPermanent: targetPermanent,
            isInheritedEffect: false,
            condition: CanUseCondition,
            rootCardEffect: activateClass,
            card: card);

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
}
