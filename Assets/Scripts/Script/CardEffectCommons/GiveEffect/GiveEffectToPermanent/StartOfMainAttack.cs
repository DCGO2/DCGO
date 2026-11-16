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
        targetPermanent.UntilOwnerTurnEndEffects.Add(GetDetailEffect);

        string EffectDescription()
        {
            return "[Start of Your Main Phase] Attack with this Digimon.";
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return IsPermanentExistsOnBattleArea(targetPermanent)
                && targetPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(targetPermanent)
                && GManager.instance.turnStateMachine.gameContext.TurnPlayer == targetPermanent.TopCard.Owner;
        }

        bool CanActivateCondition1(Hashtable hashtable)
        {
            return IsPermanentExistsOnBattleArea(targetPermanent)
                && !targetPermanent.TopCard.CanNotBeAffected(cardEffect)
                && targetPermanent.CanAttack(activateClass);
        }

        bool CanShowDebuffCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(targetPermanent)
                && !targetPermanent.TopCard.CanNotBeAffected(cardEffect);
        }

        IEnumerator ActivateCoroutine1(Hashtable hashtable)
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

        ICardEffect GetCardEffect(EffectTiming timing)
        {
            return timing == EffectTiming.OnStartMainPhase ? activateClass : null;
        }

        ICardEffect GetDetailEffect(EffectTiming timing)
        {
            if (timing == EffectTiming.None)
            {
                return CardEffectFactory.AddDetailClass(CanShowDebuffCondition, permanent => permanent == targetPermanent, EffectDescription(), true, targetPermanent.TopCard);
            }

            return null;
        }

        if (!targetPermanent.TopCard.CanNotBeAffected(cardEffect))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(targetPermanent));
        }
    }
}