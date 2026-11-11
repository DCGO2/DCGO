using System.Collections;

public partial class CardEffectCommons
{
    public static IEnumerator StartOfMainAttack(Permanent targetPermanent, ICardEffect cardEffect)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Attack with this Digimon", CanUseCondition, targetPermanent.TopCard);
        activateClass.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDescription());
        activateClass.SetEffectSourcePermanent(targetPermanent);
        targetPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

        string EffectDescription()
        {
            return "[Start of Your Main Phase] Attack with this Digimon.";
        }

        bool CanUseCondition(Hashtable hashtable1)
        {
            return IsPermanentExistsOnBattleArea(targetPermanent)
                && targetPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(targetPermanent)
                && GManager.instance.turnStateMachine.gameContext.TurnPlayer == targetPermanent.TopCard.Owner;
        }

        bool CanActivateCondition1(Hashtable hashtable1)
        {
            return IsPermanentExistsOnBattleArea(targetPermanent)
                && !targetPermanent.TopCard.CanNotBeAffected(cardEffect)
                && targetPermanent.CanAttack(activateClass);
        }

        IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
        {
            if (IsPermanentExistsOnBattleArea(targetPermanent) && targetPermanent.CanAttack(activateClass))
            {
                SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                selectAttackEffect.SetUp(
                    attacker: targetPermanent,
                    canAttackPlayerCondition: () => true,
                    defenderCondition: (permanent) => true,
                    cardEffect: activateClass);

                selectAttackEffect.SetCanNotSelectNotAttack();

                yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
            }
        }

        ICardEffect GetCardEffect(EffectTiming _timing)
        {
            return _timing == EffectTiming.OnStartMainPhase ? activateClass : null;
        }

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(targetPermanent));
    }
}