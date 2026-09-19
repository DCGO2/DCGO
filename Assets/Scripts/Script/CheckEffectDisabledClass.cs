using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public static class CheckEffectDisabledClass
{
    public static bool isDisabled(ICardEffect targetCardEffect)
    {
        if (targetCardEffect != null)
        {
            List<ValidateCardEffectElement> Tree = CreateTree(targetCardEffect);

            List<ValidateCardEffectElement> selectedElements = new List<ValidateCardEffectElement>();

            foreach (ValidateCardEffectElement element in Tree)
            {
                if (element.childrenElements.Count == 0)
                {
                    CheckActive(element, selectedElements);
                }
            }

            while (Tree.Count((element) => !selectedElements.Contains(element)) > 0)
            {
                foreach (ValidateCardEffectElement element in Tree)
                {
                    if (element.childrenElements.Count((childElement) => selectedElements.Contains(childElement)) >= 1)
                    {
                        CheckActive(element, selectedElements);
                    }
                }
            }

            foreach (ValidateCardEffectElement element in Tree)
            {
                if (element.cardEffect == targetCardEffect)
                {
                    return !element.isActive;
                }
            }
        }

        return false;
    }

    static List<ValidateCardEffectElement> CreateTree(ICardEffect cardEffect)
    {
        List<ValidateCardEffectElement> tree = new List<ValidateCardEffectElement>();

        CreateElement(null, cardEffect, tree);

        return tree;
    }

    static void CreateElement(ValidateCardEffectElement parentElement, ICardEffect cardEffect, List<ValidateCardEffectElement> tree)
    {
        ValidateCardEffectElement element = new ValidateCardEffectElement(parentElement, cardEffect, new List<ValidateCardEffectElement>());

        if (parentElement != null)
        {
            parentElement.childrenElements.Add(element);
        }

        tree.Add(element);

        if (PotentiallyDisablingEffects(element.cardEffect, tree).Count >= 1)
        {
            foreach (ICardEffect childCardEffect in PotentiallyDisablingEffects(element.cardEffect, tree))
            {
                CreateElement(element, childCardEffect, tree);
            }
        }
    }

    static void CheckActive(ValidateCardEffectElement element, List<ValidateCardEffectElement> selectedElements)
    {
        if (!selectedElements.Contains(element))
        {
            element.isActive = element.childrenElements.Count((childeElement) => childeElement.isActive) == 0;

            selectedElements.Add(element);
        }
    }

    #region 
    public static List<ICardEffect> PotentiallyDisablingEffects(ICardEffect targetCardEffect, List<ValidateCardEffectElement> tree)
    {
        List<ICardEffect> PotentiallyDisablingEffects = new List<ICardEffect>();

        if (targetCardEffect != null)
        {
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
            {
                foreach (Permanent permanent in player.GetBattleAreaPermanents())
                {
                    #region Permanent Effects

                    foreach (ICardEffect cardEffect in permanent.EffectList_Added(EffectTiming.None))
                    {
                        if (cardEffect != targetCardEffect && tree.Count((element) => element.cardEffect == cardEffect) == 0)
                        {
                            if (cardEffect is IDisableCardEffect)
                            {
                                if (((IDisableCardEffect)cardEffect).IsDisabled(targetCardEffect))
                                {
                                    PotentiallyDisablingEffects.Add(cardEffect);
                                }
                            }
                        }
                    }

                    foreach (CardSource cardSource in permanent.cardSources)
                    {
                        if (cardSource.cEntity_EffectController.cEntity_Effect != null)
                        {
                            foreach (ICardEffect cardEffect in cardSource.cEntity_EffectController.cEntity_Effect.GetCardEffects(EffectTiming.None, cardSource))
                            {
                                if (((cardSource == permanent.TopCard) == (cardEffect.IsInheritedEffect))  || (!cardSource.IsLinked == cardEffect.IsLinkedEffect) || (cardSource.IsFlipped))
                                {
                                    continue;
                                }

                                if (cardEffect != targetCardEffect && tree.Count((element) => element.cardEffect == cardEffect) == 0)
                                {
                                    if (cardEffect is IDisableCardEffect)
                                    {
                                        if (((IDisableCardEffect)cardEffect).IsDisabled(targetCardEffect))
                                        {
                                            PotentiallyDisablingEffects.Add(cardEffect);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (CardSource cardSource in permanent.LinkedCards)
                    {
                        if (cardSource.cEntity_EffectController.cEntity_Effect != null)
                        {
                            foreach (ICardEffect cardEffect in cardSource.cEntity_EffectController.cEntity_Effect.GetCardEffects(EffectTiming.None, cardSource))
                            {
                                if ((!cardEffect.IsLinkedEffect) || (cardSource.IsFlipped))
                                {
                                    continue;
                                }

                                if (cardEffect != targetCardEffect && tree.Count((element) => element.cardEffect == cardEffect) == 0)
                                {
                                    if (cardEffect is IDisableCardEffect)
                                    {
                                        if (((IDisableCardEffect)cardEffect).IsDisabled(targetCardEffect))
                                        {
                                            PotentiallyDisablingEffects.Add(cardEffect);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }

                #region Player Effects
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is IDisableCardEffect)
                    {
                        if (cardEffect != targetCardEffect && tree.Count((element) => element.cardEffect == cardEffect) == 0)
                        {
                            if (cardEffect is IDisableCardEffect)
                            {
                                if (((IDisableCardEffect)cardEffect).IsDisabled(targetCardEffect))
                                {
                                    PotentiallyDisablingEffects.Add(cardEffect);
                                }
                            }
                        }
                    }
                }
                #endregion
            }
        }

        return PotentiallyDisablingEffects;
    }
    #endregion
}

public class ValidateCardEffectElement
{
    public ValidateCardEffectElement parentElement { get; set; }
    public ICardEffect cardEffect { get; set; }
    public List<ValidateCardEffectElement> childrenElements { get; set; }
    public bool isActive { get; set; } = false;

    public ValidateCardEffectElement(ValidateCardEffectElement parentElement, ICardEffect cardEffect, List<ValidateCardEffectElement> childrenElements)
    {
        this.parentElement = parentElement;
        this.cardEffect = cardEffect;
        this.childrenElements = childrenElements;
    }
}
