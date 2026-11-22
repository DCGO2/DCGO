using System.Collections;

public partial class CardEffectCommons
{
    #region Target 1 Digimon can't be deleted
    public static IEnumerator GainCanNotBeDeleted(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass, string effectName, bool byBattle = false, Hashtable _hashtable = null)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool PermanentCondition(Permanent attacker) => attacker == targetPermanent;

        bool CanUseCondition()
        {
            return IsPermanentExistsOnBattleArea(targetPermanent)
                && !targetPermanent.TopCard.CanNotBeAffected(activateClass);
        }

        CanNotBeDestroyedClass canNotBeDestroyedClass = CardEffectFactory.CanNotBeDestroyedStaticEffect(
            _hashtable: _hashtable,
            permanentCondition: PermanentCondition,
            isInheritedEffect: false,
            card: card,
            condition: CanUseCondition,
            effectName: effectName,
            byBattle: byBattle);

        canNotBeDestroyedClass.SetIsOptionEffect(activateClass.IsOptionEffect);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: canNotBeDestroyedClass,
            timing: EffectTiming.None);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion
}