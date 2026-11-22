using System.Collections;
using System;

public partial class CardEffectFactory
{
    #region Static effect that can't be deleted
    public static CanNotBeDestroyedClass CanNotBeDestroyedStaticEffect(Hashtable _hashtable, Func<Permanent, bool> permanentCondition, bool isInheritedEffect, CardSource card, Func<bool> condition, string effectName, bool byBattle = false)
    {
        CanNotBeDestroyedClass canNotBeDestroyedClass = new CanNotBeDestroyedClass();
        canNotBeDestroyedClass.SetUpICardEffect(effectName, CanUseCondition, card);
        canNotBeDestroyedClass.SetUpCanNotBeDestroyedClass(permanentCondition: PermanentCondition);

        if (isInheritedEffect)
        {
            canNotBeDestroyedClass.SetIsInheritedEffect(true);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            if (byBattle && CardEffectCommons.IsByBattle(_hashtable))
            {
                return false;
            }

            return condition == null || condition();
        }

        bool PermanentCondition(Permanent permanent)
        {
            return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent)
                && !permanent.TopCard.CanNotBeAffected(canNotBeDestroyedClass)
                && (permanentCondition == null
                    || permanentCondition(permanent));
        }

        return canNotBeDestroyedClass;
    }
    #endregion
}