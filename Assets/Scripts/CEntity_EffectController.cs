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

    #region 効果リストを取得
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
            // 他のカードの効果によって追加された効果
            if (timing != EffectTiming.None)
            {
                #region 他のカードの効果によって追加された効果
                foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                {
                    if (player != null)
                    {
                        #region 場のパーマネントの効果
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
                                            if (cardEffect.IsInheritedEffect == (cardSource == permanent.TopCard))
                                            {
                                                continue;
                                            }

                                            if (cardEffect.CanUse(null))
                                            {
                                                GetCardEffects = ((IAddSkillEffect)cardEffect).GetCardEffect(card, GetCardEffects, timing);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        #region プレイヤーによって追加された効果
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

            // 自分によって追加される場合のみEffectTiming.Noneについて探索
            else
            {
                if (thisPermanent != null)
                {
                    if (thisPermanent.TopCard.cEntity_EffectController.cEntity_Effect != null)
                    {
                        foreach (CardSource cardSource in thisPermanent.cardSources)
                        {
                            /*
                            if (cardSource != thisPermanent.TopCard)
                            {
                                if (!thisPermanent.IsDigimon)
                                {
                                    continue;
                                }
                            }
                            */

                            foreach (ICardEffect cardEffect in cardSource.cEntity_EffectController.cEntity_Effect.GetCardEffects(EffectTiming.None, thisPermanent.TopCard))
                            {
                                if (cardEffect is IAddSkillEffect)
                                {
                                    if (cardEffect.IsInheritedEffect == (cardSource == thisPermanent.TopCard))
                                    {
                                        continue;
                                    }

                                    if (cardEffect.CanUse(null))
                                    {
                                        GetCardEffects = ((IAddSkillEffect)cardEffect).GetCardEffect(card, GetCardEffects, timing);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return GetCardEffects.Filter(cardEfect => cardEfect != null);
    }
    #endregion

    #region そのターン中の使用回数をリセット
    public void InitUseCountThisTurn()
    {
        UseEffectsThisTurn = new List<ICardEffect>();
    }
    #endregion

    #region カード効果をセット
    public void AddCardEffect(string ID, string ClassName)
    {
        ID = ID.Split("-")[0];
        #region カード効果クラスのインスタンスを生成してセット
        bool CanAttachEffectComponent()
        {
            if (string.IsNullOrEmpty(ClassName)) return false;
            if (Type.GetType(ClassName) == null)
            {
                if (Type.GetType($"DCGO.CardEffects.{ID}.{ClassName}") == null) return false;
            }

            return true;
        }

        CEntity_Effect cEntity_Effect = null;

        if (CanAttachEffectComponent())
        {
            Type t = Type.GetType(ClassName);

            if (t == null)
                t = Type.GetType($"DCGO.CardEffects.{ID}.{ClassName}");

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

    #region その効果をこのターンに使用した回数を取得
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

    #region その効果がこのターン中の使用上限回数に達しているかどうか
    public bool isOverMaxCountPerTurn(ICardEffect cardEffect, int MaxCountPerTurn)
    {
        return GetUseCountThisTurn(cardEffect) >= MaxCountPerTurn;
    }
    #endregion

    #region このターンに使った効果に登録
    public void RegisterUseEfffectThisTurn(ICardEffect cardEffect)
    {
        UseEffectsThisTurn.Add(cardEffect);
    }
    #endregion
}

public class EmptyEffectClass : CEntity_Effect
{

}
