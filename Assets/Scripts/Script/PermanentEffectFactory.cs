using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class for given effects for Permanents, applying to the entire stack even if any specific card is removed from it
/// </summary>
public partial class PermanentEffectFactory
{

    #region Effect of a Permanent to Delete Itself
    public static ActivateClass DeleteSelfEffect(Permanent permanent, bool deleteOnOwnturn = true, bool deleteOnOpponentsTurn = true)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Delete this Digimon", CanUseCondition, permanent.TopCard);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, "");
        activateClass.SetEffectSourcePermanent(permanent);
        return activateClass;

        bool CanUseCondition(Hashtable hashtable)
        {
            if (permanent.TopCard != null && CardEffectCommons.IsExistOnBattleArea(permanent.TopCard))
            {
                if (CardEffectCommons.IsOwnerTurn(permanent.TopCard))
                {
                    return deleteOnOwnturn;
                }
                else
                {
                    return deleteOnOpponentsTurn;
                }
            }
            return false;
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return permanent.TopCard != null
                && CardEffectCommons.IsExistOnBattleArea(permanent.TopCard);
        }

        IEnumerator ActivateCoroutine(Hashtable hashtable)
        {
            yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(new List<Permanent>() { permanent }, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
        }
    }
    #endregion

    #region Digimon Effect Immunity
    public static CanNotAffectedClass DigimonEffectImmunity(Permanent permanent)
    {
        bool CanUseCondition1(Hashtable hashtable)
        {
            return CardEffectCommons.IsExistOnBattleAreaDigimon(permanent.TopCard);
        }

        bool CardCondition(CardSource cardSource)
        {
            return cardSource == permanent.TopCard
                && CardEffectCommons.IsExistOnBattleAreaDigimon(permanent.TopCard);
        }

        bool SkillCondition(ICardEffect cardEffect)
        {
            return CardEffectCommons.IsOpponentEffect(cardEffect, permanent.TopCard)
                && cardEffect.IsDigimonEffect;
        }

        CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
        canNotAffectedClass.SetUpICardEffect("Not affected by opponent's Digimon's effects", CanUseCondition1, permanent.TopCard);
        canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
        canNotAffectedClass.SetEffectSourcePermanent(permanent);
        return canNotAffectedClass;

    }
    #endregion

    #region Cannot change Attack Target Effect
    public static CanNotSwitchAttackTargetClass CanNotSwitchAttackTargetEffect(Permanent targetPermanent)
    {
        CanNotSwitchAttackTargetClass canNotSwitchAttackTargetClass = new CanNotSwitchAttackTargetClass();
        canNotSwitchAttackTargetClass.SetUpICardEffect("This Digimon's attack target can't be switched.", CanUseCondition, targetPermanent.TopCard);
        canNotSwitchAttackTargetClass.SetUpCanNotSwitchAttackTargetClass(PermanentCondition: PermanentCondition);
        return canNotSwitchAttackTargetClass;

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsPermanentExistsOnBattleArea(targetPermanent) &&
                    CardEffectCommons.IsOwnerTurn(targetPermanent.TopCard);
        }

        bool PermanentCondition(Permanent permanent)
        {
            return permanent != null && permanent.TopCard && permanent == targetPermanent;
        }
    }
    #endregion

    #region At end of turn lose 3 memory
    public static ActivateClass EoTLose3Memory(CardSource card)
    {
        ActivateClass activateClass1 = new ActivateClass();
        activateClass1.SetUpICardEffect("Memory -3", CanUseCondition1, card);
        activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
        return activateClass1;

        string EffectDiscription1()
        {
            return "Lose 3 memory.";
        }

        bool CanUseCondition1(Hashtable hashtable)
        {
            return true;
        }

        bool CanActivateCondition1(Hashtable hashtable)
        {
            return true;
        }

        IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
        {
            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(-3, activateClass1));
        }
    }
    #endregion
}
