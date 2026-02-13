using System.Collections;

public partial class CardEffectCommons
{
    public static IEnumerator StartOfMainAttack(Permanent targetPermanent)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Attack with this Digimon", CanUseCondition, targetPermanent.TopCard);
        activateClass.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription());
        activateClass.SetEffectSourcePermanent(targetPermanent);
        targetPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

        string EffectDiscription()
        {
            return "[Start of Your Main Phase] Attack with this Digimon.";
        }

        bool CanUseCondition(Hashtable hashtable1)
        {
            if (IsPermanentExistsOnBattleArea(targetPermanent))
            {
                if (targetPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(targetPermanent))
                {
                    if (GManager.instance.turnStateMachine.gameContext.TurnPlayer == targetPermanent.TopCard.Owner)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool CanActivateCondition1(Hashtable hashtable1)
        {
            if (IsPermanentExistsOnBattleArea(targetPermanent))
            {
                if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
                {
                    if (targetPermanent.CanAttack(activateClass))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
        {
            if (IsPermanentExistsOnBattleArea(targetPermanent))
            {
                if (targetPermanent.CanAttack(activateClass))
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
        }

        ICardEffect GetCardEffect(EffectTiming _timing)
        {
            if (_timing == EffectTiming.OnStartMainPhase) return activateClass;
            return null;
        }

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(targetPermanent));
    }
}