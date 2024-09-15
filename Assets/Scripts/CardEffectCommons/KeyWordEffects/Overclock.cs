using System.Collections;
using System.Collections.Generic;

public partial class CardEffectCommons
{
    static bool CanSelectPermanentCondition(Permanent permanent, string trait, CardSource cardSource)
    {
        return IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, cardSource) &&
               permanent != cardSource.PermanentOfThisCard() &&
               (permanent.IsToken || permanent.TopCard.ContainsTraits(trait));
    }

    #region Can activate [Overclock]

    public static bool CanActivateOverclock(string trait, CardSource cardSource, ICardEffect activateClass)
    {
        return IsExistOnBattleArea(cardSource) &&
               HasMatchConditionPermanent(permanent => CanSelectPermanentCondition(permanent, trait, cardSource)) &&
               cardSource.PermanentOfThisCard().CanAttack(activateClass, withoutTap: true);
    }

    #endregion

    #region Effect process of [Overclock]

    public static IEnumerator OverclockProcess(string trait, CardSource cardSource, ICardEffect activateClass)
    {
        if (HasMatchConditionPermanent(permanent => CanSelectPermanentCondition(permanent, trait, cardSource)))
        {
            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

            selectPermanentEffect.SetUp(
                selectPlayer: cardSource.Owner,
                canTargetCondition: permanent => CanSelectPermanentCondition(permanent, trait, cardSource),
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                maxCount: 1,
                canNoSelect: true,
                canEndNotMax: false,
                selectPermanentCoroutine: SelectPermanentCoroutine,
                afterSelectPermanentCoroutine: null,
                mode: SelectPermanentEffect.Mode.Custom,
                cardEffect: activateClass);

            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.",
                "The opponent is selecting 1 Digimon to delete.");

            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

            IEnumerator SelectPermanentCoroutine(Permanent permanent)
            {
                yield return ContinuousController.instance.StartCoroutine(
                    DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { permanent },
                        activateClass: activateClass,
                        successProcess: _ => SuccessProcess(),
                        failureProcess: null));

                IEnumerator SuccessProcess()
                {
                    Permanent selectedPermanent = cardSource.PermanentOfThisCard();

                    if (selectedPermanent.CanAttack(activateClass, withoutTap: true))
                    {
                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: selectedPermanent,
                            canAttackPlayerCondition: () => true,
                            defenderCondition: _ => false,
                            cardEffect: activateClass);

                        selectAttackEffect.SetWithoutTap();
                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }
                }
            }
        }
    }

    #endregion

    #region Target 1 Digimon gains [Overclock]

    public static IEnumerator GainOverclock(string trait, Permanent targetPermanent, EffectDuration effectDuration,
        ICardEffect activateClass)
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

        ActivateClass overclock = CardEffectFactory.OverclockEffect(
            trait: trait,
            targetPermanent: targetPermanent,
            isInheritedEffect: false,
            condition: CanUseCondition,
            rootCardEffect: activateClass,
            targetPermanent.TopCard);

        AddEffectToPermanent(
            targetPermanent: targetPermanent,
            effectDuration: effectDuration,
            card: card,
            cardEffect: overclock,
            timing: EffectTiming.OnAllyAttack);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                .CreateBuffEffect(targetPermanent));
        }
    }

    #endregion
}