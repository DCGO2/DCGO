using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class CEntity_EffectController : MonoBehaviour
{
    //Number of skills used this turn (referenced by use limit)
    List<ICardEffect> UseEffectsThisTurn = new List<ICardEffect>();

    #region CEntity_Effect
    public CEntity_Effect cEntity_Effect { get; set; }
    #endregion

    #region Get effect list
    public List<ICardEffect> GetCardEffects_ExceptAddedEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> GetCardEffects = new List<ICardEffect>();

        if (cEntity_Effect != null)
        {
            foreach (ICardEffect cardEffect in cEntity_Effect.GetCardEffects(timing, card))
            {
                GetCardEffects.Add(cardEffect);
            }
        }

        return GetCardEffects;
    }
    public List<ICardEffect> GetCardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> GetCardEffects = new List<ICardEffect>();

        foreach (ICardEffect cardEffect in GetCardEffects_ExceptAddedEffects(timing, card))
        {
            GetCardEffects.Add(cardEffect);
        }

        Permanent thisPermanent = card.PermanentOfThisCard();
        bool isDigivolutionCard = thisPermanent != null && thisPermanent.DigivolutionCards.Contains(card);

        if (!isDigivolutionCard)
        {
            // Effects added by other card effects
            if (timing != EffectTiming.None)
            {
                #region Effects added by other card effects
                foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                {
                    if (player != null)
                    {
                        #region Effects of permanents in play
                        foreach (Permanent permanent in player.GetFieldPermanents())
                        {
                            if (permanent.TopCard.cEntity_EffectController.cEntity_Effect != null)
                            {
                                foreach (CardSource cardSource in permanent.cardSources)
                                {
                                    if (cardSource != permanent.TopCard)
                                    {
                                        if (!permanent.IsDigimon)
                                        {
                                            continue;
                                        }
                                    }

                                    foreach (ICardEffect cardEffect in cardSource.cEntity_EffectController.cEntity_Effect.GetCardEffects(EffectTiming.None, permanent.TopCard))
                                    {
                                        if (cardEffect is IAddSkillEffect)
                                        {
                                            if (cardEffect.IsInheritedEffect == (cardSource == permanent.TopCard) || cardSource.IsFlipped)
                                            {
                                                continue;
                                            }

                                            if (cardEffect.CanUse(null))
                                            {
                                                if (!card.CanNotBeAffected(cardEffect))
                                                    GetCardEffects = ((IAddSkillEffect)cardEffect).GetCardEffect(card, GetCardEffects, timing);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Effects from security
                        foreach (CardSource source in player.SecurityCards)
                        {
                            if (source.IsFlipped)
                                continue;

                            foreach (ICardEffect cardEffect in source.EffectList(EffectTiming.None))
                            {
                                if (cardEffect is IAddSkillEffect)
                                {
                                    if (cardEffect.CanUse(null))
                                    {
                                        GetCardEffects = ((IAddSkillEffect)cardEffect).GetCardEffect(card, GetCardEffects, timing);
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Effects added by players
                        foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                        {
                            if (cardEffect is IAddSkillEffect)
                            {
                                if (cardEffect.CanUse(null))
                                {
                                    GetCardEffects = ((IAddSkillEffect)cardEffect).GetCardEffect(card, GetCardEffects, timing);
                                }
                            }
                        }
                        #endregion
                    }
                }
                #endregion
            }

            // Explore about EffectTiming.None only if added by me
            else
            {
                if (thisPermanent != null)
                {
                    if (thisPermanent.TopCard.cEntity_EffectController.cEntity_Effect != null)
                    {
                        foreach (CardSource cardSource in thisPermanent.cardSources)
                        {
                            foreach (ICardEffect cardEffect in cardSource.cEntity_EffectController.cEntity_Effect.GetCardEffects(EffectTiming.None, thisPermanent.TopCard))
                            {
                                if (cardEffect is IAddSkillEffect)
                                {
                                    if (cardEffect.IsInheritedEffect == (cardSource == thisPermanent.TopCard) || cardSource.IsFlipped)
                                    {
                                        continue;
                                    }

                                    if (cardEffect.CanUse(null))
                                    {
                                        //GetCardEffects = ((IAddSkillEffect)cardEffect).GetCardEffect(card, GetCardEffects, timing);
                                    }
                                }
                            }
                        }
                    }
                }

                if (CardEffectCommons.IsExistInSecurity(card))
                {
                    foreach (ICardEffect cardEffect in card.cEntity_EffectController.cEntity_Effect.GetCardEffects(EffectTiming.None, card))
                    {
                        if (cardEffect is IAddSkillEffect)
                        {
                            if (cardEffect.CanUse(null))
                            {
                                GetCardEffects = ((IAddSkillEffect)cardEffect).GetCardEffect(card, GetCardEffects, timing);
                            }
                        }
                    }
                }
            }
        }

        return GetCardEffects.Filter(cardEfect => cardEfect != null);
    }
    #endregion

    #region Reset the number of uses during that turn
    public void InitUseCountThisTurn()
    {
        UseEffectsThisTurn = new List<ICardEffect>();
    }
    #endregion

    #region set card effects
    public void AddCardEffect(string ID, string ClassName)
    {
        ID = ID.Split("-")[0];
        #region Generate and set an instance of the card effect class
        bool CanAttachEffectComponent()
        {
            if (string.IsNullOrEmpty(ClassName)) 
                return false;

            if (Type.GetType(ClassName) == null)
            {
                if (!ClassName.Contains("token"))
                {
                    if (Type.GetType($"DCGO.CardEffects.{ID}.{ClassName}") == null)
                        return false;
                }
                else
                {
                    if (Type.GetType($"DCGO.CardEffects.Tokens.{ClassName}") == null)
                        return false;
                }
                
            }

            return true;
        }

        CEntity_Effect cEntity_Effect = null;

        if (CanAttachEffectComponent())
        {
            Type t = Type.GetType(ClassName);

            if (t == null)
            {
                if (!ClassName.Contains("token"))
                    t = Type.GetType($"DCGO.CardEffects.{ID}.{ClassName}");
                else
                    t = Type.GetType($"DCGO.CardEffects.Tokens.{ClassName}");
            }
                

            Component component = this.gameObject.AddComponent(t);

            if (component is CEntity_Effect)
            {
                cEntity_Effect = (CEntity_Effect)(component);
            }

            else
            {
                Debug.Log($"{ClassName} has error");
            }
        }

        else
        {
            cEntity_Effect = this.gameObject.AddComponent<EmptyEffectClass>();
        }

        this.cEntity_Effect = cEntity_Effect;
        #endregion
    }
    #endregion

    #region Gets the number of times the effect was used this turn
    public int GetUseCountThisTurn(ICardEffect cardEffect)
    {
        int useCount = 0;

        foreach (ICardEffect cardEffect1 in UseEffectsThisTurn)
        {
            if (cardEffect.IsSameEffect(cardEffect1))
            {
                useCount++;
            }
        }

        return useCount;
    }
    #endregion

    #region Whether the effect has reached the maximum number of times it can be used this turn.
    public bool isOverMaxCountPerTurn(ICardEffect cardEffect, int MaxCountPerTurn)
    {
        return GetUseCountThisTurn(cardEffect) >= MaxCountPerTurn;
    }
    #endregion

    #region Register as effects used this turn
    public void RegisterUseEfffectThisTurn(ICardEffect cardEffect)
    {
        UseEffectsThisTurn.Add(cardEffect);
    }
    #endregion
}

public class EmptyEffectClass : CEntity_Effect
{

}
