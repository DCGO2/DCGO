using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Static effect that gives the ability to appfuse by name
    public static AddAppFusionConditionClass AddAppfuseMethodByName(List<string> cardNames, CardSource card, int cost = 0, string effectName = "App Fusion")
    {
        return AddAppfuseMethodByCondition(cardNames.Select<string, Func<CardSource, bool>>(name => card => card.EqualsCardName(name)).ToList(), card, cost, effectName);
    }
    #endregion

    #region Static effect that gives the ability to appfuse by condition
    public static AddAppFusionConditionClass AddAppfuseMethodByCondition(List<Func<CardSource, bool>> cardConditions, CardSource card, int cost = 0, string effectName = "App Fusion")
    {
        AddAppFusionConditionClass addAppFusionConditionClass = new AddAppFusionConditionClass();
        addAppFusionConditionClass.SetUpICardEffect(effectName, (hashtable) => true, card);
        addAppFusionConditionClass.SetUpAddAppFusionConditionClass(getAppFusionCondition: GetAppFusion);
        addAppFusionConditionClass.SetNotShowUI(true);
        return addAppFusionConditionClass;

        AppFusionCondition GetAppFusion(CardSource cardSource)
        {
            bool linkCondition(Permanent permanent, CardSource source)
            {
                if (source != null)
                {
                    if (source != card)
                    {
                        for (int i = 0; i < cardConditions.Count; i++)
                        {
                            if (cardConditions[i](source)) 
                            {
                                for (int j = 0; j < cardConditions.Count; j++)
                                {
                                    if(i != j)
                                    {
                                        if (cardConditions[j](permanent.TopCard))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            
                        }
                    }
                }

                return false;
            }
            bool digimonCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    for (int i = 0; i < cardConditions.Count; i++)
                    {
                        if (cardConditions[i](permanent.TopCard))
                        {
                            for (int j = 0; j < cardConditions.Count; j++)
                            {
                                if (i != j)
                                {
                                    if (permanent.LinkedCards.Find(x => cardConditions[j](x)))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }

                    }
                }

                return false;
            }

            if (cardSource == card)
            {
                AppFusionCondition AppFusionCondition = new AppFusionCondition(
                    linkedCondition: linkCondition,
                    digimonCondition: digimonCondition,
                    cost: cost);

                return AppFusionCondition;
            }

            return null;
        }
    }
    #endregion
}