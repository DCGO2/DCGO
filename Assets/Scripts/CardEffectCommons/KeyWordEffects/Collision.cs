using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can activate [Collision]
    public static bool CanActivateCollision(CardSource cardSource, ICardEffect activateClass)
    {
        if (IsExistOnBattleArea(cardSource))
        {
            if (cardSource.PermanentOfThisCard().CanAttack(activateClass))
            {
                if (cardSource.Owner.Enemy.GetBattleAreaDigimons().Count >= 1)
                {
                    if (!GManager.instance.attackProcess.IsAttacking)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Blitz]
    public static IEnumerator CollisionProcess(CardSource cardSource, ICardEffect activateClass, Func<IEnumerator> beforeOnAttackCoroutine = null)
    {
        if (CanActivateCollision(cardSource, activateClass))
        {
            foreach(Permanent enemyDigimon in cardSource.Owner.Enemy.GetBattleAreaDigimons())
            {
                if (!enemyDigimon.TopCard.CanNotBeAffected(activateClass))
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(
                        targetPermanent: enemyDigimon,
                        effectDuration: EffectDuration.UntilEndAttack,
                        activateClass: activateClass));

                    int maxCount = 1;

                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: cardSource.Owner.Enemy,
                        canTargetCondition: CanSelectBlockerCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: null);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will block.", "The opponent is selecting 1 Digimon that will block.");
                    selectPermanentEffect.SetUpCustomBackButtonMessage("Not Block");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    bool CanSelectBlockerCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                        {
                            if (permanent.TopCard.Owner.isYou)
                            {
                                if (permanent.HasBlocker && permanent.CanBlock(cardSource.PermanentOfThisCard()))
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(null, true, selectedPermanent));
                    }
                }
            }
        }
    }
    #endregion
}