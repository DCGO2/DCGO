using System.Collections;

public partial class CardEffectCommons
{
    #region Can activate [Vortex]

    public static bool CanActivateVortex(CardSource cardSource, ICardEffect activateClass)
    {
        return IsExistOnBattleArea(cardSource) &&
               cardSource.PermanentOfThisCard().CanAttack(activateClass, isVortex: true) &&
               HasMatchConditionOpponentsPermanent(cardSource, permanent =>
                   permanent.IsDigimon &&
                   cardSource.PermanentOfThisCard().CanAttackTargetDigimon(permanent, activateClass, isVortex: true));
    }

    #endregion

    #region Effect process of [Vortex]

    public static IEnumerator VortexProcess(CardSource cardSource, ICardEffect activateClass)
    {
        Permanent selectedPermanent = cardSource.PermanentOfThisCard();

        if (selectedPermanent.CanAttack(activateClass, isVortex: true))
        {
            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

            selectAttackEffect.SetUp(
                attacker: selectedPermanent,
                canAttackPlayerCondition: () => false,
                defenderCondition: _ => true,
                cardEffect: activateClass);
            
            selectAttackEffect.SetIsVortex();
            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
        }
    }

    #endregion

    #region Target 1 Digimon gains [Vortex]

    public static IEnumerator GainVortex(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool CanUseCondition()
        {
            return IsPermanentExistsOnBattleArea(targetPermanent) &&
                   !targetPermanent.TopCard.CanNotBeAffected(activateClass);
        }

        ActivateClass vortex = CardEffectFactory.VortexEffect(
            targetPermanent: targetPermanent,
            isInheritedEffect: false,
            condition: CanUseCondition,
            rootCardEffect: activateClass,
            targetPermanent.TopCard);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: vortex,
            timing: EffectTiming.OnEndTurn);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                .CreateBuffEffect(targetPermanent));
        }
    }

    #endregion
}