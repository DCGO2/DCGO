using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AddJogressConditionClass : ICardEffect, IAddJogressConditionEffect
{
    Func<CardSource, JogressCondition> _getJogressCondition { get; set; }
    public void SetUpAddJogressConditionClass(Func<CardSource, JogressCondition> getJogressCondition)
    {
        _getJogressCondition = getJogressCondition;
    }

    public JogressCondition GetJogressCondition(CardSource cardSource)
    {
        if (cardSource != null)
        {
            if (_getJogressCondition != null)
            {
                JogressCondition jogressCondition = _getJogressCondition(cardSource);

                if (jogressCondition != null)
                {
                    if (jogressCondition.elements != null)
                    {
                        JogressConditionElement[] newElements = jogressCondition.elements.Map((element) =>
                                                     {
                                                         bool EvoRootCondition(Permanent permanent)
                                                         {
                                                             return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, cardSource) && (element.EvoRootCondition == null || element.EvoRootCondition(permanent));
                                                         }

                                                         return new JogressConditionElement(evoRootCondition: EvoRootCondition, selectMessage: element.SelectMessage);
                                                     });

                        return new JogressCondition(newElements, jogressCondition.cost);
                    }
                }
            }
        }

        return null;
    }
}