using System.Collections;
using System.Collections.Generic;

public partial class CardEffectCommons
{
    #region Can activate [Execute]

    public static bool CanActivateExecute(CardSource cardSource, ICardEffect activateClass)
    {
        return IsExistOnBattleArea(cardSource) &&
               cardSource.PermanentOfThisCard().CanAttack(activateClass);
    }

    #endregion

    #region Effect process of [Execute]

    public static IEnumerator ExecuteProcess(CardSource cardSource, ICardEffect activateClass)
    {
        Permanent selectedPermanent = cardSource.PermanentOfThisCard();

        if (selectedPermanent.CanAttack(activateClass))
        {
            #region Attack unsuspended Digimon

            CanAttackTargetDefendingPermanentClass canAttackTargetDefendingPermanentClass = new CanAttackTargetDefendingPermanentClass();
            canAttackTargetDefendingPermanentClass.SetUpICardEffect("Can attack unsuspended Digimon", CanUseCondition, cardSource);
            canAttackTargetDefendingPermanentClass.SetUpCanAttackTargetDefendingPermanentClass(
                attackerCondition: AttackerCondition,
                defenderCondition: DefenderCondition,
                cardEffectCondition: CardEffectCondition);
            selectedPermanent.UntilEachTurnEndEffects.Add(_ => canAttackTargetDefendingPermanentClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return IsExistOnBattleArea(cardSource) &&
                       IsOwnerTurn(cardSource);
            }

            bool AttackerCondition(Permanent attacker)
            {
                return attacker == selectedPermanent;
            }

            bool DefenderCondition(Permanent defender)
            {
                return IsPermanentExistsOnOpponentBattleAreaDigimon(defender, cardSource) &&
                       !defender.IsSuspended;
            }

            bool CardEffectCondition(ICardEffect cardEffect)
            {
                return true;
            }

            #endregion

            #region Attack

            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

            selectAttackEffect.SetUp(
                attacker: selectedPermanent,
                canAttackPlayerCondition: () => true,
                defenderCondition: _ => true,
                cardEffect: activateClass);

            selectAttackEffect.SetAfterOnAttackCoroutine(AfterAttackCoroutine);

            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());

            #endregion

            #region Delete this Digimon

            IEnumerator AfterAttackCoroutine()
            {
                yield return ContinuousController.instance.StartCoroutine(
                    new DestroyPermanentsClass(new List<Permanent>() { selectedPermanent }, CardEffectHashtable(activateClass)).Destroy());
            }

            #endregion
        }
    }

    #endregion

    #region Target 1 Digimon gains [Execute]

    public static IEnumerator GainExecute(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass)
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

        ActivateClass execute = CardEffectFactory.ExecuteEffect(
            targetPermanent: targetPermanent,
            isInheritedEffect: false,
            condition: CanUseCondition,
            rootCardEffect: activateClass,
            targetPermanent.TopCard);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: execute,
            timing: EffectTiming.OnEndTurn);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                .CreateBuffEffect(targetPermanent));
        }
    }

    #endregion
}