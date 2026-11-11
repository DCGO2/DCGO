using System.Collections;

public partial class CardEffectCommons
{
    public static IEnumerator Taunt(Permanent permanent)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("[Start of Your Main Phase] This Digimon attacks.", CanUseGrant, permanent.TopCard);
        activateClass.SetUpActivateClass(CanActivateGrant, ActivateGrant, -1, false, "[Start of Your Main Phase] This Digimon attacks.");
        activateClass.SetEffectSourcePermanent(permanent);
        permanent.UntilOwnerTurnEndEffects.Add(GetGrantEffect);

        if (!permanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(
                GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));
        }

        bool CanUseGrant(Hashtable hashtable)
        {
            return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent)
                && permanent.TopCard.Owner.GetBattleAreaDigimons().Contains(permanent)
                && GManager.instance.turnStateMachine.gameContext.TurnPlayer == permanent.TopCard.Owner;
        }

        bool CanActivateGrant(Hashtable hashtable)
        {
            return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent)
                && !permanent.TopCard.CanNotBeAffected(activateClass)
                && permanent.CanAttack(activateClass);
        }

        ICardEffect GetGrantEffect(EffectTiming _timing)
        {
            return _timing == EffectTiming.OnStartMainPhase ? activateClass : null;
        }

        IEnumerator ActivateGrant(Hashtable hashtable)
        {
            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

            selectAttackEffect.SetUp(
                attacker: permanent,
                canAttackPlayerCondition: () => true,
                defenderCondition: (permanent) => true,
                cardEffect: activateClass);

            selectAttackEffect.SetCanNotSelectNotAttack();

            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
        }
    }
}