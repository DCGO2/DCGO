using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using System.Net.NetworkInformation;

public partial class CardEffectFactory
{
    #region Static effect that adds one's own card's link condition
    public static AddLinkConditionClass AddSelfLinkConditionStaticEffect(Func<Permanent, bool> permanentCondition, int linkCost, CardSource card, Func<bool> condition = null, Func<CardSource, bool> cardCondition = null, string effectName = null)
    {
        bool CanUseCondition()
        {
            return condition == null || condition();
        }

        return AddLinkConditionStaticEffect(
            permanentCondition: permanentCondition,
            cardCondition: cardCondition ?? ((cardSource) => cardSource == card),
            linkCost: linkCost,
            isInheritedEffect: false,
            card: card,
            condition: CanUseCondition,
            effectName: effectName ?? "Link");
    }
    #endregion

    #region Static effect that adds link condition
    public static AddLinkConditionClass AddLinkConditionStaticEffect(Func<Permanent, bool> permanentCondition, Func<CardSource, bool> cardCondition, int linkCost, bool isInheritedEffect, CardSource card, Func<bool> condition, string effectName)
    {
        AddLinkConditionClass addLinkConditionClass = new AddLinkConditionClass();
        addLinkConditionClass.SetUpICardEffect(effectName, CanUseCondition, card);
        addLinkConditionClass.SetUpAddLinkConditionClass(getLinkCondition: GetLink);

        if (isInheritedEffect)
            addLinkConditionClass.SetIsInheritedEffect(true);

        bool CanUseCondition(Hashtable hashtable)
        {
            return condition == null || condition();
        }

        bool CardCondition(CardSource cardSource)
        {
            if (cardSource != null)
            {
                if (cardCondition == null || cardCondition(cardSource))
                {
                    return true;
                }
            }

            return false;
        }

        bool PermanentCondition(Permanent permanent)
        {
            if (permanent != null)
            {
                if (permanent.TopCard != null)
                {
                    if (permanentCondition == null || permanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        LinkCondition GetLink(CardSource cardSource)
        {
            if (cardSource == card)
            {
                if (CardCondition(cardSource)){
                    LinkCondition LinkCondition = new LinkCondition(
                                        digimonCondition: PermanentCondition,
                                        cost: linkCost);

                    return LinkCondition;
                }
            }

            return null;
        }

        

        

        return addLinkConditionClass;
    }
    #endregion
}