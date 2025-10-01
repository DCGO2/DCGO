using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Permanent
{
    public Permanent(List<CardSource> cardSources)
    {
        SetCardSources(cardSources);
    }

    public void SetCardSources(List<CardSource> cardSources)
    {
        List<CardSource> newCardSources = cardSources.Clone();

        //TODO: Attempted fix for random face up secuirty/flipped sources
        //newCardSources.Reverse();

        foreach (CardSource cardSource in newCardSources)
        {
            AddCardSource(cardSource);
        }
    }

    public FieldCardFrame PermanentFrame
    {
        get
        {
            if (TopCard != null)
            {
                int index = Array.IndexOf(TopCard.Owner.FieldPermanents, this);

                if (0 <= index && index <= TopCard.Owner.fieldCardFrames.Count - 1)
                {
                    return TopCard.Owner.fieldCardFrames[index];
                }
            }

            return null;
        }
    }

    public bool oldIsTapped_playCard { get; set; }

    #region Level
    public int Level
    {
        get
        {
            int Level = 0;

            if (TopCard != null)
            {
                Level = TopCard.Level;

                if (!TopCard.HasLevel)
                {
                    Level = 1145140;
                }

                #region レベルを変更する効果

                foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                {
                    foreach (Permanent permanent in player.GetFieldPermanents())
                    {
                        #region 場のパーマネントの効果
                        foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                        {
                            if (cardEffect is IChangePermanentLevelEffect)
                            {
                                if (cardEffect.CanUse(null))
                                {
                                    Level = ((IChangePermanentLevelEffect)cardEffect).GetPermanentLevel(Level, this);
                                }
                            }
                        }
                        #endregion
                    }

                    #region プレイヤーの効果
                    foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IChangePermanentLevelEffect)
                        {
                            if (cardEffect.CanUse(null))
                            {
                                Level = ((IChangePermanentLevelEffect)cardEffect).GetPermanentLevel(Level, this);
                            }
                        }
                    }
                    #endregion
                }

                #endregion
            }

            return Level;
        }
    }
    #endregion

    #region Place all evolution sources in the trash
    public IEnumerator DiscardEvoRoots(bool ignoreOverflow = false, bool putToTrash = true)
    {
        List<CardSource> evoRoots = DigivolutionCards.Clone();
        List<CardSource> linkRoots = LinkedCards.Clone();

        if (!ignoreOverflow)
        {
            yield return ContinuousController.instance.StartCoroutine(new AceOverflowClass(evoRoots).Overflow());
            yield return ContinuousController.instance.StartCoroutine(new AceOverflowClass(linkRoots).Overflow());
        }

        foreach (CardSource cardSource1 in evoRoots)
        {
            if (putToTrash)
            {
                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource1));
            }

            else
            {
                yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(cardSource1));
            }
        }

        foreach (CardSource cardSource2 in linkRoots)
        {
            if (putToTrash)
            {
                yield return ContinuousController.instance.StartCoroutine(RemoveLinkedCard(cardSource2));
            }

            else
            {
                yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(cardSource2));
            }
        }
    }
    #endregion

    #region Whether this permanent has DP
    public bool HasDP
    {
        get
        {
            if (TopCard == null)
            {
                return false;
            }

            if (!IsDigimon)
            {
                return false;
            }

            if (!TopCard.HasDP && TopCard.IsDigiEgg)
            {
                return false;
            }

            #region Effect of not having DP
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
            {
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    foreach (ICardEffect cardEffect1 in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect1 is IDontHaveDPEffect)
                        {
                            if (cardEffect1.CanUse(null))
                            {
                                if (((IDontHaveDPEffect)cardEffect1).DontHaveDP(this))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            return true;
        }
    }
    #endregion

    #region Base DP
    public int BaseDP
    {
        get
        {
            int BaseDP = 0;

            if (HasDP)
            {
                BaseDP = TopCard.BaseCardDP;
                BaseDP += TopCard.BaseDP;

                #region 基礎DPを変更する効果

                List<ICardEffect> cardEffects_ChangeDP = new List<ICardEffect>();

                foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                {
                    foreach (Permanent permanent in player.GetFieldPermanents())
                    {
                        #region 場のパーマネントの効果
                        foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                        {
                            if (cardEffect is IChangeBaseDPEffect)
                            {
                                if (cardEffect.CanUse(null))
                                {
                                    if (((IChangeBaseDPEffect)cardEffect).PermanentCondition(this))
                                    {
                                        if (((IChangeBaseDPEffect)cardEffect).IsMinusDP())
                                        {
                                            if (this.ImmuneFromDPMinus(cardEffect))
                                            {
                                                continue;
                                            }
                                        }

                                        if (!TopCard.CanNotBeAffected(cardEffect))
                                        {
                                            cardEffects_ChangeDP.Add(cardEffect);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }

                    #region プレイヤーの効果
                    foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IChangeBaseDPEffect)
                        {
                            if (cardEffect.CanUse(null))
                            {
                                if (((IChangeBaseDPEffect)cardEffect).PermanentCondition(this))
                                {
                                    if (((IChangeBaseDPEffect)cardEffect).IsMinusDP())
                                    {
                                        if (this.ImmuneFromDPMinus(cardEffect))
                                        {
                                            continue;
                                        }
                                    }

                                    if (!TopCard.CanNotBeAffected(cardEffect))
                                    {
                                        cardEffects_ChangeDP.Add(cardEffect);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }

                List<ICardEffect> cardEffects_ChangeDP_isUpDown = new List<ICardEffect>();
                List<ICardEffect> cardEffects_ChangeDP_NotIsUpDown = new List<ICardEffect>();

                foreach (ICardEffect cardEffect in cardEffects_ChangeDP)
                {
                    if (cardEffect is IChangeBaseDPEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (((IChangeBaseDPEffect)cardEffect).IsUpDown())
                            {
                                cardEffects_ChangeDP_isUpDown.Add(cardEffect);
                            }

                            else
                            {
                                cardEffects_ChangeDP_NotIsUpDown.Add(cardEffect);
                            }
                        }
                    }
                }

                foreach (ICardEffect cardEffect in cardEffects_ChangeDP_isUpDown)
                {
                    if (cardEffect is IChangeBaseDPEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            BaseDP = ((IChangeBaseDPEffect)cardEffect).GetDP(BaseDP, this);
                        }
                    }
                }

                foreach (ICardEffect cardEffect in cardEffects_ChangeDP_NotIsUpDown)
                {
                    if (cardEffect is IChangeBaseDPEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            BaseDP = ((IChangeBaseDPEffect)cardEffect).GetDP(BaseDP, this);
                        }
                    }
                }

                #endregion

                if (BaseDP < 0)
                {
                    BaseDP = 0;
                }
            }

            return BaseDP;
        }
    }
    #endregion

    #region DP

    public int GetDP(Permanent ignorePermanent = null)
    {
        int DP = -1;

        if (HasDP)
        {
            DP = BaseDP;

            #region DP By Effect

            List<ICardEffect> cardEffects_ChangeDP = new List<ICardEffect>();

            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                #region Effects of permanents in play
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    if (ignorePermanent != null)
                    {
                        if (permanent == ignorePermanent)
                            continue;
                    }
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IChangeDPEffect)
                        {
                            if (cardEffect.CanUse(null))
                            {
                                if (((IChangeDPEffect)cardEffect).PermanentCondition(this))
                                {
                                    if (((IChangeDPEffect)cardEffect).IsMinusDP())
                                    {
                                        if (this.ImmuneFromDPMinus(cardEffect))
                                        {
                                            continue;
                                        }
                                    }

                                    if (!TopCard.CanNotBeAffected(cardEffect))
                                    {
                                        cardEffects_ChangeDP.Add(cardEffect);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Effects of face up security
                foreach (CardSource cardSource in player.SecurityCards)
                {
                    if (cardSource.IsFlipped)
                        continue;

                    foreach (ICardEffect cardEffect in cardSource.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IChangeDPEffect)
                        {
                            if (cardEffect.CanUse(null))
                            {
                                if (((IChangeDPEffect)cardEffect).PermanentCondition(this))
                                {
                                    if (((IChangeDPEffect)cardEffect).IsMinusDP())
                                    {
                                        if (this.ImmuneFromDPMinus(cardEffect))
                                        {
                                            continue;
                                        }
                                    }

                                    if (!TopCard.CanNotBeAffected(cardEffect))
                                    {
                                        cardEffects_ChangeDP.Add(cardEffect);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Player effect
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is IChangeDPEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (((IChangeDPEffect)cardEffect).PermanentCondition(this))
                            {
                                if (((IChangeDPEffect)cardEffect).IsMinusDP())
                                {
                                    if (this.ImmuneFromDPMinus(cardEffect))
                                    {
                                        continue;
                                    }
                                }

                                if (!TopCard.CanNotBeAffected(cardEffect))
                                {
                                    cardEffects_ChangeDP.Add(cardEffect);
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            List<ICardEffect> cardEffects_ChangeDP_isUpDown = new List<ICardEffect>();
            List<ICardEffect> cardEffects_ChangeDP_NotIsUpDown = new List<ICardEffect>();

            foreach (ICardEffect cardEffect in cardEffects_ChangeDP)
            {
                if (cardEffect is IChangeDPEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        if (((IChangeDPEffect)cardEffect).IsUpDown())
                        {
                            cardEffects_ChangeDP_isUpDown.Add(cardEffect);
                        }

                        else
                        {
                            cardEffects_ChangeDP_NotIsUpDown.Add(cardEffect);
                        }
                    }
                }
            }

            foreach (ICardEffect cardEffect in cardEffects_ChangeDP_isUpDown)
            {
                if (cardEffect is IChangeDPEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        DP = ((IChangeDPEffect)cardEffect).GetDP(DP, this);
                    }
                }
            }

            foreach (ICardEffect cardEffect in cardEffects_ChangeDP_NotIsUpDown)
            {
                if (cardEffect is IChangeDPEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        DP = ((IChangeDPEffect)cardEffect).GetDP(DP, this);
                    }
                }
            }
            #endregion

            #region DP Boosts
            foreach (DPBoost boost in Boosts)
            {
                DP += boost.DP;
            }
            #endregion

            DP += LinkedDP;

            if (DP < 0)
            {
                DP = 0;
            }
        }

        return DP;
    }
    public int DP
    {
        get
        {
            int DP = -1;

            if (HasDP)
            {
                DP = BaseDP;

                #region DP By Effect

                List<ICardEffect> cardEffects_ChangeDP = new List<ICardEffect>();

                foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                {
                    #region Effects of permanents in play
                    foreach (Permanent permanent in player.GetFieldPermanents())
                    {
                        foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                        {
                            if (cardEffect is IChangeDPEffect)
                            {
                                if (cardEffect.CanUse(null))
                                {
                                    if (((IChangeDPEffect)cardEffect).PermanentCondition(this))
                                    {
                                        if (((IChangeDPEffect)cardEffect).IsMinusDP())
                                        {
                                            if (this.ImmuneFromDPMinus(cardEffect))
                                            {
                                                continue;
                                            }
                                        }

                                        if (!TopCard.CanNotBeAffected(cardEffect))
                                        {
                                            cardEffects_ChangeDP.Add(cardEffect);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region Effects of face up security
                    foreach (CardSource cardSource in player.SecurityCards)
                    {
                        if (cardSource.IsFlipped)
                            continue;

                        foreach (ICardEffect cardEffect in cardSource.EffectList(EffectTiming.None))
                        {
                            if (cardEffect is IChangeDPEffect)
                            {
                                if (cardEffect.CanUse(null))
                                {
                                    if (((IChangeDPEffect)cardEffect).PermanentCondition(this))
                                    {
                                        if (((IChangeDPEffect)cardEffect).IsMinusDP())
                                        {
                                            if (this.ImmuneFromDPMinus(cardEffect))
                                            {
                                                continue;
                                            }
                                        }

                                        if (!TopCard.CanNotBeAffected(cardEffect))
                                        {
                                            cardEffects_ChangeDP.Add(cardEffect);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region Player effect
                    foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IChangeDPEffect)
                        {
                            if (cardEffect.CanUse(null))
                            {
                                if (((IChangeDPEffect)cardEffect).PermanentCondition(this))
                                {
                                    if (((IChangeDPEffect)cardEffect).IsMinusDP())
                                    {
                                        if (this.ImmuneFromDPMinus(cardEffect))
                                        {
                                            continue;
                                        }
                                    }

                                    if (!TopCard.CanNotBeAffected(cardEffect))
                                    {
                                        cardEffects_ChangeDP.Add(cardEffect);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }

                List<ICardEffect> cardEffects_ChangeDP_isUpDown = new List<ICardEffect>();
                List<ICardEffect> cardEffects_ChangeDP_NotIsUpDown = new List<ICardEffect>();

                foreach (ICardEffect cardEffect in cardEffects_ChangeDP)
                {
                    if (cardEffect is IChangeDPEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (((IChangeDPEffect)cardEffect).IsUpDown())
                            {
                                cardEffects_ChangeDP_isUpDown.Add(cardEffect);
                            }

                            else
                            {
                                cardEffects_ChangeDP_NotIsUpDown.Add(cardEffect);
                            }
                        }
                    }
                }

                foreach (ICardEffect cardEffect in cardEffects_ChangeDP_isUpDown)
                {
                    if (cardEffect is IChangeDPEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            DP = ((IChangeDPEffect)cardEffect).GetDP(DP, this);
                        }
                    }
                }

                foreach (ICardEffect cardEffect in cardEffects_ChangeDP_NotIsUpDown)
                {
                    if (cardEffect is IChangeDPEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            DP = ((IChangeDPEffect)cardEffect).GetDP(DP, this);
                        }
                    }
                }
                #endregion

                #region DP Boosts
                foreach(DPBoost boost in Boosts)
                {
                    DP += boost.DP;
                }
                #endregion

                DP += LinkedDP;

                if (DP < 0)
                {
                    DP = 0;
                }
            }

            return DP;
        }
    }

    public int LinkedDP { get; set; }

    public List<DPBoost> Boosts = new List<DPBoost>();

    public void AddBoost(DPBoost boost)
    {
        if (Boosts.Any(x => x.ID == boost.ID))
            Boosts.First(x => x.ID == boost.ID).DP = boost.DP;
        else
            Boosts.Add(boost);            
    }

    public void RemoveBoost(string ID)
    {
        if (Boosts.Any(x => x.ID == ID))
            Boosts.Remove(Boosts.First(x => x.ID == ID));
    }
    public class DPBoost
    {
        public DPBoost(string id, int dp, Func<bool> cond)
        {
            ID = id;
            DP = dp;
            Condition = cond;
        }

        public string ID = "";
        public int DP = 0;
        public Func<bool> Condition = null;
    }
    #endregion

    #region Will it not receive negative DP effect?
    public bool ImmuneFromDPMinus(ICardEffect cardEffect)
    {
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                foreach (ICardEffect cardEffect1 in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect1 is IImmuneFromDPMinusEffect)
                    {
                        if (cardEffect1.CanUse(null))
                        {
                            if (((IImmuneFromDPMinusEffect)cardEffect1).ImmuneFromDPMinus(this, cardEffect))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            foreach (ICardEffect cardEffect1 in player.EffectList(EffectTiming.None))
            {
                if (cardEffect1 is IImmuneFromDPMinusEffect)
                {
                    if (cardEffect1.CanUse(null))
                    {
                        if (((IImmuneFromDPMinusEffect)cardEffect1).ImmuneFromDPMinus(this, cardEffect))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region Can be returned to your hand?
    public bool CannotReturnToHand(ICardEffect cardEffect)
    {
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                foreach (ICardEffect cardEffect1 in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect1 is ICannotReturnToHandEffect)
                    {
                        if (cardEffect1.CanUse(null))
                        {
                            if (((ICannotReturnToHandEffect)cardEffect1).CannotReturnToHand(this, cardEffect))
                            {
                                return true;
                            }
                        }
                    }
                }

                foreach (ICardEffect cardEffect1 in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect1 is ICannotReturnToHandEffect)
                    {
                        if (cardEffect1.CanUse(null))
                        {
                            if (((ICannotReturnToHandEffect)cardEffect1).CannotReturnToHand(this, cardEffect))
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
    #endregion

    #region Can be returned to deck?
    public bool CannotReturnToLibrary(ICardEffect cardEffect)
    {
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                foreach (ICardEffect cardEffect1 in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect1 is ICannotReturnToLibraryEffect)
                    {
                        if (cardEffect1.CanUse(null))
                        {
                            if (((ICannotReturnToLibraryEffect)cardEffect1).CannotReturnToLibrary(this, cardEffect))
                            {
                                return true;
                            }
                        }
                    }
                }

                foreach (ICardEffect cardEffect1 in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect1 is ICannotReturnToLibraryEffect)
                    {
                        if (cardEffect1.CanUse(null))
                        {
                            if (((ICannotReturnToLibraryEffect)cardEffect1).CannotReturnToLibrary(this, cardEffect))
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
    #endregion

    #region Immune From De-Digivolve
    public bool ImmuneFromDeDigivolve()
    {
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                foreach (ICardEffect cardEffect1 in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect1 is IImmuneFromDeDigivolveEffect)
                    {
                        if (cardEffect1.CanUse(null))
                        {
                            if (((IImmuneFromDeDigivolveEffect)cardEffect1).ImmuneDeDigivolve(this))
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
    #endregion

    #region Immune From Trashing Stack
    public bool ImmuneFromStackTrashing(ICardEffect effect)
    {
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                foreach (ICardEffect cardEffect1 in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect1 is IImmuneFromStackTrashingEffect)
                    {
                        if (cardEffect1.CanUse(null))
                        {
                            if (((IImmuneFromStackTrashingEffect)cardEffect1).ImmuneStackTrashing(this, effect))
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
    #endregion

    #region Card Sources
    public List<CardSource> cardSources = new List<CardSource>();
    #endregion

    #region Stacked Cards
    public List<CardSource> StackCards => cardSources.Filter(cardSource => !LinkedCards.Contains(cardSource));
    #endregion

    #region Digivolution Cards
    public List<CardSource> DigivolutionCards => cardSources.Filter(cardSource => cardSource != TopCard && !LinkedCards.Contains(cardSource));
    #endregion

    #region Linked Cards
    public int LinkedMax
    {
        get
        {
            int Max = 1;

            #region Effect of changing the number of sheets to undergo security check

            List<ICardEffect> cardEffects_ChangeLinkedMax = new List<ICardEffect>();

            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    #region Effects of permanents in play
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IChangeLinkMaxEffect)
                        {
                            if (((IChangeLinkMaxEffect)cardEffect).PermanentCondition(this))
                            {
                                if (cardEffect.CanUse(null))
                                {
                                    if (!TopCard.CanNotBeAffected(cardEffect))
                                    {
                                        cardEffects_ChangeLinkedMax.Add(cardEffect);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }

                #region player effect
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is IChangeLinkMaxEffect)
                    {
                        if (((IChangeLinkMaxEffect)cardEffect).PermanentCondition(this))
                        {
                            if (cardEffect.CanUse(null))
                            {
                                if (!TopCard.CanNotBeAffected(cardEffect))
                                {
                                    cardEffects_ChangeLinkedMax.Add(cardEffect);
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            List<ICardEffect> cardEffects_ChangeLinkedMax_UpToConstant = new List<ICardEffect>();
            List<ICardEffect> cardEffects_ChangeLinkedMax_UpDownValue = new List<ICardEffect>();
            List<ICardEffect> cardEffects_ChangeLinkedMax_DownToConstant = new List<ICardEffect>();

            foreach (ICardEffect cardEffect in cardEffects_ChangeLinkedMax)
            {
                if (cardEffect is IChangeLinkMaxEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        switch (((IChangeLinkMaxEffect)cardEffect).isUpDown())
                        {
                            case CalculateOrder.UpToConstant:
                                cardEffects_ChangeLinkedMax_UpToConstant.Add(cardEffect);
                                break;

                            case CalculateOrder.UpDownValue:
                                cardEffects_ChangeLinkedMax_UpDownValue.Add(cardEffect);
                                break;

                            case CalculateOrder.DownToConstant:
                                cardEffects_ChangeLinkedMax_DownToConstant.Add(cardEffect);
                                break;
                        }
                    }
                }
            }

            foreach (ICardEffect cardEffect in cardEffects_ChangeLinkedMax_UpToConstant)
            {
                if (cardEffect is IChangeLinkMaxEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        Max = ((IChangeLinkMaxEffect)cardEffect).GetLinkMax(Max, this, InvertSecutiryValue);
                    }
                }
            }

            foreach (ICardEffect cardEffect in cardEffects_ChangeLinkedMax_UpDownValue)
            {
                if (cardEffect is IChangeLinkMaxEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        Max = ((IChangeLinkMaxEffect)cardEffect).GetLinkMax(Max, this, InvertSecutiryValue);
                    }
                }
            }

            foreach (ICardEffect cardEffect in cardEffects_ChangeLinkedMax_DownToConstant)
            {
                if (cardEffect is IChangeLinkMaxEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        Max = ((IChangeLinkMaxEffect)cardEffect).GetLinkMax(Max, this, InvertSecutiryValue);
                    }
                }
            }
            #endregion

            return Max;
        }
    }

    public List<CardSource> LinkedCards = new List<CardSource>();
    #endregion

    #region Add Card Source
    public void AddCardSource(CardSource cardSource)
    {
        cardSources.Insert(0, cardSource);

        if (!cardSource.IsFlipped)
            cardSource.SetFace();
        else
            cardSource.SetReverse();
    }
    #endregion

    #region Add digivolution cards to top of sources
    /// <summary>
    /// IEnumerator to add a list of CardSource to a permanents top sources, CAN NOT be used to put a field permanent under must use IPlacePermanentToDigivolutionCards
    /// </summary>
    /// <param name="addedDigivolutionCards"></param>
    /// <param name="cardEffect"></param>
    /// <param name="skipEffectAndActivateSkill"></param>
    /// <returns></returns>
    public IEnumerator AddDigivolutionCardsTop(List<CardSource> addedDigivolutionCards, ICardEffect cardEffect)
    {
        List<CardSource> addedCards = new List<CardSource>();

        bool isFromSameDigimon = false;
        bool isFromDigimon = false;

        foreach (CardSource addedDigivolutionCard in addedDigivolutionCards)
        {
            if (cardSources.Contains(addedDigivolutionCard))
            {
                isFromSameDigimon = true;
            }

            if (CardEffectCommons.IsExistOnBattleArea(addedDigivolutionCard))
            {
                if (addedDigivolutionCard.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                {
                    isFromDigimon = true;
                }
            }

            yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(addedDigivolutionCard));

            if (!this.IsToken && !addedDigivolutionCard.IsToken)
            {
                this.cardSources.Insert(1, addedDigivolutionCard);

                if (!addedDigivolutionCard.IsFlipped || addedDigivolutionCard.IsBeingRevealed || GManager.instance.turnStateMachine.gameContext.IsSecurityLooking)
                    addedDigivolutionCard.SetFace();

                addedCards.Add(addedDigivolutionCard);
            }
        }

        if (addedCards.Count >= 1)
        {
            if (ShowingPermanentCard != null)
            {
                yield return ContinuousController.instance.StartCoroutine(ShowingPermanentCard.ShowAddDigivolutionCardEffect());
            }

            #region "進化元が増えた時"の効果

            #region Hashtable Setting
            Hashtable hashtable = new Hashtable()
                {
                    {"Permanent", this},
                    {"CardEffect", cardEffect},
                    {"CardSources", addedCards},
                    {"isFromSameDigimon", isFromSameDigimon},
                    {"isFromDigimon", isFromDigimon},
                };
            #endregion

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable, EffectTiming.OnAddDigivolutionCards));
            #endregion
        }
    }
    #endregion

    #region Add digivolution cards to bottom of sources
    /// <summary>
    /// IEnumerator to add a list of CardSource to a permanents bottom sources, CAN NOT be used to put a field permanent under must use IPlacePermanentToDigivolutionCards
    /// </summary>
    /// <param name="addedDigivolutionCards"></param>
    /// <param name="cardEffect"></param>
    /// <param name="skipEffectAndActivateSkill"></param>
    /// <returns></returns>
    public IEnumerator AddDigivolutionCardsBottom(List<CardSource> addedDigivolutionCards, ICardEffect cardEffect, bool skipEffectAndActivateSkill = false, bool isFacedown = false)
    {
        List<CardSource> addedCards = new List<CardSource>();

        bool isFromSameDigimon = false;
        bool isFromDigimon = false;

        foreach (CardSource addedDigivolutionCard in addedDigivolutionCards)
        {
            if (addedDigivolutionCard.PermanentOfThisCard() != null)
            {
                if (addedDigivolutionCard.PermanentOfThisCard() != this)
                {
                    if (addedDigivolutionCard.PermanentOfThisCard().TopCard == addedDigivolutionCard)
                    {
                        IPlacePermanentToDigivolutionCards placePermanent = new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { addedDigivolutionCard.PermanentOfThisCard(), this } }, false, cardEffect, skipEffectAndActivateSkill);

                        yield return ContinuousController.instance.StartCoroutine(placePermanent.PlacePermanentToDigivolutionCards());

                        if (!placePermanent.Placed)
                        {
                            continue;
                        }
                    }
                }
            }

            if (CardEffectCommons.IsExistOnBattleArea(addedDigivolutionCard))
            {
                if (addedDigivolutionCard.PermanentOfThisCard().DigivolutionCards.Count >= 1)
                {
                    isFromDigimon = true;
                }
            }

            if (cardSources.Contains(addedDigivolutionCard))
            {
                isFromSameDigimon = true;

                if (addedDigivolutionCard == TopCard)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(addedDigivolutionCard));

                    ShowingPermanentCard.ShowPermanentData(true);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(
                        addedDigivolutionCard,
                        this));
                }
            }

            yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(addedDigivolutionCard));

            if (!IsToken && !addedDigivolutionCard.IsToken)
            {
                cardSources.Add(addedDigivolutionCard);

                if (isFacedown)
                    addedDigivolutionCard.SetReverse();
                else
                    addedDigivolutionCard.SetFace();

                addedCards.Add(addedDigivolutionCard);
            }
        }

        if (addedCards.Count >= 1 && !skipEffectAndActivateSkill)
        {
            if (ShowingPermanentCard != null)
            {
                yield return ContinuousController.instance.StartCoroutine(ShowingPermanentCard.ShowAddDigivolutionCardEffect());
            }

            #region Effect of "when the number of evolution sources increases"

            #region Hashtable Setting
            Hashtable hashtable = new Hashtable()
                {
                    {"Permanent", this},
                    {"CardEffect", cardEffect},
                    {"CardSources", addedCards},
                    {"isFromSameDigimon", isFromSameDigimon},
                    {"isFromDigimon", isFromDigimon},
                };
            #endregion

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable, EffectTiming.OnAddDigivolutionCards));
            #endregion
        }
    }
    #endregion

    #region Add Link cards
    /// <summary>
    /// IEnumerator to add a list of CardSource to a permanents top sources, CAN NOT be used to put a field permanent under must use IPlacePermanentToLinkCards
    /// </summary>
    /// <param name="addedLinkCards"></param>
    /// <param name="cardEffect"></param>
    /// <param name="skipEffectAndActivateSkill"></param>
    /// <returns></returns>
    public IEnumerator AddLinkCard(CardSource addedLinkCard, ICardEffect cardEffect)
    {
        bool addedCard = false;

        bool isFromDigimon = false;

        if (CardEffectCommons.IsExistOnBattleArea(addedLinkCard))
        {
            if (addedLinkCard.PermanentOfThisCard().DigivolutionCards.Count >= 1)
            {
                isFromDigimon = true;
            }
        }

        if (LinkedCards.Count >= LinkedMax)
        {
            if(LinkedMax > 1)
                yield return ContinuousController.instance.StartCoroutine(RemoveLinkedCard(null,((LinkedCards.Count + 1) - LinkedMax)));
            else
                yield return ContinuousController.instance.StartCoroutine(RemoveLinkedCard(LinkedCards[0]));
        }

        yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(addedLinkCard));

        if (!this.IsToken && !addedLinkCard.IsToken)
        {
            LinkedCards.Insert(0, addedLinkCard);
            LinkedDP += addedLinkCard.LinkDP;

            this.cardSources.Insert(1, addedLinkCard);
            addedLinkCard.SetFace();
            addedCard = true;
        }

        if (addedCard)
        {
            if (ShowingPermanentCard != null)
            {
                yield return ContinuousController.instance.StartCoroutine(ShowingPermanentCard.ShowAddDigivolutionCardEffect());
            }

            #region Add Linked Card

            #region Hashtable Setting
            Hashtable hashtable = new Hashtable()
                {
                    {"Permanent", this},
                    {"CardEffect", cardEffect},
                    {"Card", addedLinkCard},
                    {"isFromDigimon", isFromDigimon},
                };
            #endregion

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable, EffectTiming.WhenLinked));
            #endregion
        }
    }
    #endregion

    #region RemoveCardSource
    public IEnumerator RemoveCardSource(CardSource cardSource)
    {
        yield return null;

        cardSources.Remove(cardSource);
    }
    #endregion

    #region Remove Linked Card
    public IEnumerator RemoveLinkedCard(CardSource cardSource, int removeCount = 0, bool trashCard = true)
    {
        if (LinkedCards.Contains(cardSource))
        {
            LinkedDP -= cardSource.LinkDP;
            LinkedCards.Remove(cardSource);

            yield return ContinuousController.instance.StartCoroutine(RemoveCardSource(cardSource));

            if (trashCard)
            {
                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource));
            }        
        }

        if(removeCount > 0)
        {
            int maxCount = Mathf.Min(removeCount, LinkedCards.Count);
            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

            selectCardEffect.SetUp(
                        canTargetCondition: (CardSource) => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => false,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        message: $"Select {maxCount} card to trash.",
                        maxCount: removeCount,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Discard,
                        root: SelectCardEffect.Root.Custom,
                        customRootCardList: LinkedCards,
                        canLookReverseCard: true,
                        selectPlayer: TopCard.Owner,
                        cardEffect: null);

            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
        }

        //TODO: Add event call if something was removed
    }
    #endregion

    #region Top Card
    public CardSource TopCard
    {
        get
        {
            if (cardSources.Count >= 1)
            {
                if (LinkedCards.Count > 0)
                    return cardSources.First(source => !LinkedCards.Contains(source));

                else
                    return cardSources[0];
            }

            return null;
        }
    }
    #endregion

    #region Permanent effect list

    #region このパーマネントの効果リスト
    public List<ICardEffect> EffectList(EffectTiming timing)
    {
        return EffectList_ForCard(timing, TopCard);
    }
    #endregion

    #region このポケモンの追加効果リスト
    public List<ICardEffect> EffectList_Added(EffectTiming timing)
    {
        List<ICardEffect> _EffectList = new List<ICardEffect>();

        if (TopCard != null)
        {
            foreach (Func<EffectTiming, ICardEffect> GetCardEffect in UntilOwnerDrawPhaseEffects)
            {
                ICardEffect cardEffect = GetCardEffect(timing);

                if (cardEffect != null)
                {
                    _EffectList.Add(cardEffect);
                }
            }

            foreach (Func<EffectTiming, ICardEffect> GetCardEffect in UntilOwnerTurnEndEffects)
            {
                ICardEffect cardEffect = GetCardEffect(timing);

                if (cardEffect != null)
                {
                    _EffectList.Add(cardEffect);
                }
            }

            foreach (Func<EffectTiming, ICardEffect> GetCardEffect in UntilEachTurnEndEffects)
            {
                ICardEffect cardEffect = GetCardEffect(timing);

                if (cardEffect != null)
                {
                    _EffectList.Add(cardEffect);
                }
            }

            foreach (Func<EffectTiming, ICardEffect> GetCardEffect in UntilOpponentTurnEndEffects)
            {
                ICardEffect cardEffect = GetCardEffect(timing);

                if (cardEffect != null)
                {
                    _EffectList.Add(cardEffect);
                }
            }

            foreach (Func<EffectTiming, ICardEffect> GetCardEffect in UntilEndBattleEffects)
            {
                ICardEffect cardEffect = GetCardEffect(timing);

                if (cardEffect != null)
                {
                    _EffectList.Add(cardEffect);
                }
            }

            foreach (Func<EffectTiming, ICardEffect> GetCardEffect in UntilOwnerTurnStartEffects)
            {
                ICardEffect cardEffect = GetCardEffect(timing);

                if (cardEffect != null)
                {
                    _EffectList.Add(cardEffect);
                }
            }

            foreach (Func<EffectTiming, ICardEffect> GetCardEffect in UntilNextUntapEffects)
            {
                ICardEffect cardEffect = GetCardEffect(timing);

                if (cardEffect != null)
                {
                    _EffectList.Add(cardEffect);
                }
            }

            foreach (Func<EffectTiming, ICardEffect> GetCardEffect in PermanentEffects)
            {
                ICardEffect cardEffect = GetCardEffect(timing);

                if (cardEffect != null)
                {
                    _EffectList.Add(cardEffect);
                }
            }

            foreach (Func<EffectTiming, ICardEffect> GetCardEffect in UntilEndAttackEffects)
            {
                ICardEffect cardEffect = GetCardEffect(timing);

                if (cardEffect != null)
                {
                    _EffectList.Add(cardEffect);
                }
            }

            _EffectList = _EffectList.Filter(cardEffect => cardEffect != null);

            foreach (ICardEffect cardEffect in _EffectList)
            {
                if (cardEffect != null)
                {
                    if (cardEffect.EffectSourceCard == null)
                    {
                        cardEffect.SetEffectSourceCard(TopCard);
                    }

                    cardEffect.SetIsInheritedEffect(false);
                }
            }
        }

        return _EffectList;
    }
    #endregion

    #region このパーマネントの効果リスト(引数:CardSource)
    public List<ICardEffect> EffectList_ForCard(EffectTiming timing, CardSource _cardSource)
    {
        List<ICardEffect> _EffectList = new List<ICardEffect>();

        if (TopCard != null && _cardSource != null)
        {
            foreach (CardSource cardSource in cardSources)
            {
                
                if (cardSource != null)
                {
                    if (!cardSource.IsFlipped)
                    {
                        bool isTopCard = cardSource == TopCard;

                        if (!isTopCard)
                        {
                            if (!IsDigimon)
                            {
                                continue;
                            }
                        }

                        foreach (ICardEffect cardEffect in cardSource.cEntity_EffectController.GetCardEffects(timing, cardSource))
                        {
                            if (cardEffect != null)
                            {
                                if (cardEffect.IsInheritedEffect && !isTopCard)
                                {
                                    _EffectList.Add(cardEffect);
                                    continue;
                                }

                                if (cardEffect.IsLinkedEffect && cardSource.IsLinked)
                                {
                                    _EffectList.Add(cardEffect);
                                    continue;
                                }

                                if(isTopCard && !cardEffect.IsInheritedEffect && !cardEffect.IsLinkedEffect)
                                {
                                    _EffectList.Add(cardEffect);
                                }
                            }
                        }
                    }
                }
            }

            foreach (ICardEffect cardEffect in EffectList_Added(timing))
            {
                if (cardEffect != null)
                {
                    _EffectList.Add(cardEffect);
                }
            }

            foreach (ICardEffect cardEffect in _EffectList)
            {
                if (cardEffect != null)
                {
                    if (cardEffect.EffectSourceCard == null)
                    {
                        cardEffect.SetEffectSourceCard(_cardSource);
                    }
                }
            }
        }

        return _EffectList;
    }
    #endregion

    #region 自分のターン終了時に消える効果
    public List<Func<EffectTiming, ICardEffect>> UntilOwnerTurnEndEffects = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #region 自分のドローフェイズ終了時に消える効果
    public List<Func<EffectTiming, ICardEffect>> UntilOwnerDrawPhaseEffects = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #region お互いのターン終了時に消える効果
    public List<Func<EffectTiming, ICardEffect>> UntilEachTurnEndEffects = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #region 相手のターン終了時に消える効果
    public List<Func<EffectTiming, ICardEffect>> UntilOpponentTurnEndEffects = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #region バトル終了時に消える効果
    public List<Func<EffectTiming, ICardEffect>> UntilEndBattleEffects = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #region 攻撃終了時に消える効果
    public List<Func<EffectTiming, ICardEffect>> UntilEndAttackEffects = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #region 自分のターン開始時に消える効果
    public List<Func<EffectTiming, ICardEffect>> UntilOwnerTurnStartEffects = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #region 次にアンタップする時に消える効果
    public List<Func<EffectTiming, ICardEffect>> UntilNextUntapEffects = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #region 場を離れるまで残る効果
    public List<Func<EffectTiming, ICardEffect>> PermanentEffects = new List<Func<EffectTiming, ICardEffect>>();
    #endregion

    #endregion

    #region 起動型効果を宣言できるか
    public bool CanDeclareSkill() => CanDeclareSkillList().Count > 0;
    #endregion

    #region 宣言可能な起動型効果リスト
    public List<ICardEffect> CanDeclareSkillList()
    {
        List<ICardEffect> CanDeclareSkillList = new List<ICardEffect>();

        if (TopCard != null)
        {
            foreach (ICardEffect _cardEffect in EffectList(EffectTiming.OnDeclaration))
            {
                if (_cardEffect is ActivateICardEffect)
                {
                    if (_cardEffect.CanUse(null))
                    {
                        CanDeclareSkillList.Add(_cardEffect);
                    }
                }
            }
        }

        return CanDeclareSkillList;
    }
    #endregion

    #region このパーマネントが場に出たターン
    public int EnterFieldTurnCount { get; set; } = -1;
    #endregion

    #region このパーマネントを表す場のオブジェクト
    public FieldPermanentCard ShowingPermanentCard { get; set; }
    #endregion

    #region Whether this permanent can be selected by the effect
    public bool CanSelectBySkill(ICardEffect skill)
    {
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
        {
            #region Effects of field permanents
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect is ICanNotSelectBySkillEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (((ICanNotSelectBySkillEffect)cardEffect).CanNotSelectBySkill(this, skill))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            #endregion
        }

        return true;
    }
    #endregion

    #region Number of sheets to undergo security check
    public int InvertSecutiryValue
    {
        get
        {
            int Invert = 0;

            List<ICardEffect> cardEffects_InvertStrike = new List<ICardEffect>();

            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    #region Effects of permanents in play
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IInvertSAttackEffect)
                        {
                            if (cardEffect.CanUse(null))
                            {
                                if (!TopCard.CanNotBeAffected(cardEffect))
                                {
                                    cardEffects_InvertStrike.Add(cardEffect);
                                }
                            }
                        }
                    }
                    #endregion
                }

                #region player effect
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is IInvertSAttackEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (!TopCard.CanNotBeAffected(cardEffect))
                            {
                                cardEffects_InvertStrike.Add(cardEffect);
                            }
                        }
                    }
                }
                #endregion
            }

            foreach (ICardEffect cardEffect in cardEffects_InvertStrike)
            {
                Invert = ((IInvertSAttackEffect)cardEffect).InversionValue(this, Invert);
            }

            return Mathf.Clamp(Invert,-1,1);
        }
    }
    public List<int> SecurityAttackChanges
    {
        get
        {
            List<int> SecurityAttackChanges = new List<int>();

            int Strike = 1;

            List<ICardEffect> cardEffects_ChangeDirectStrike = new List<ICardEffect>();

            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    #region 場のパーマネントの効果
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IChangeSAttackEffect)
                        {
                            if (cardEffect.CanUse(null))
                            {
                                if (!TopCard.CanNotBeAffected(cardEffect))
                                {
                                    if (((IChangeSAttackEffect)cardEffect).isUpDown() == CalculateOrder.UpDownValue)
                                    {
                                        cardEffects_ChangeDirectStrike.Add(cardEffect);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }

                #region プレイヤーの効果
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is IChangeSAttackEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (!TopCard.CanNotBeAffected(cardEffect))
                            {
                                if (((IChangeSAttackEffect)cardEffect).isUpDown() == CalculateOrder.UpDownValue)
                                {
                                    cardEffects_ChangeDirectStrike.Add(cardEffect);
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            foreach (ICardEffect cardEffect in cardEffects_ChangeDirectStrike)
            {
                if (cardEffect is IChangeSAttackEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        int Strike1 = ((IChangeSAttackEffect)cardEffect).GetSAttack(Strike, this, 0);

                        if (Strike1 != Strike)
                        {
                            SecurityAttackChanges.Add(Strike1 - Strike);
                        }
                    }
                }
            }

            return SecurityAttackChanges;
        }
    }

    public bool HasSecurityAttackChanges
    {
        get
        {
            if (!IsDigimon)
            {
                return false;
            }

            return SecurityAttackChanges.Count >= 1;
        }
    }

    public int Strike_AllowMinus
    {
        get
        {
            int Strike = 1;

            #region Effect of changing the number of sheets to undergo security check

            List<ICardEffect> cardEffects_ChangeDirectStrike = new List<ICardEffect>();

            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    #region Effects of permanents in play
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IChangeSAttackEffect)
                        {
                            if (((IChangeSAttackEffect)cardEffect).PermanentCondition(this))
                            {
                                if (cardEffect.CanUse(null))
                                {
                                    if (!TopCard.CanNotBeAffected(cardEffect))
                                    {
                                        cardEffects_ChangeDirectStrike.Add(cardEffect);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }

                #region player effect
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is IChangeSAttackEffect)
                    {
                        if (((IChangeSAttackEffect)cardEffect).PermanentCondition(this))
                        {
                            if (cardEffect.CanUse(null))
                            {
                                if (!TopCard.CanNotBeAffected(cardEffect))
                                {
                                    cardEffects_ChangeDirectStrike.Add(cardEffect);
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            List<ICardEffect> cardEffects_ChangeDirectStrike_UpToConstant = new List<ICardEffect>();
            List<ICardEffect> cardEffects_ChangeDirectStrike_UpDownValue = new List<ICardEffect>();
            List<ICardEffect> cardEffects_ChangeDirectStrike_DownToConstant = new List<ICardEffect>();

            foreach (ICardEffect cardEffect in cardEffects_ChangeDirectStrike)
            {
                if (cardEffect is IChangeSAttackEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        switch (((IChangeSAttackEffect)cardEffect).isUpDown())
                        {
                            case CalculateOrder.UpToConstant:
                                cardEffects_ChangeDirectStrike_UpToConstant.Add(cardEffect);
                                break;

                            case CalculateOrder.UpDownValue:
                                cardEffects_ChangeDirectStrike_UpDownValue.Add(cardEffect);
                                break;

                            case CalculateOrder.DownToConstant:
                                cardEffects_ChangeDirectStrike_DownToConstant.Add(cardEffect);
                                break;
                        }
                    }
                }
            }

            foreach (ICardEffect cardEffect in cardEffects_ChangeDirectStrike_UpToConstant)
            {
                if (cardEffect is IChangeSAttackEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        Strike = ((IChangeSAttackEffect)cardEffect).GetSAttack(Strike, this, InvertSecutiryValue);
                    }
                }
            }

            foreach (ICardEffect cardEffect in cardEffects_ChangeDirectStrike_UpDownValue)
            {
                if (cardEffect is IChangeSAttackEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        Strike = ((IChangeSAttackEffect)cardEffect).GetSAttack(Strike, this, InvertSecutiryValue);
                    }
                }
            }

            foreach (ICardEffect cardEffect in cardEffects_ChangeDirectStrike_DownToConstant)
            {
                if (cardEffect is IChangeSAttackEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        Strike = ((IChangeSAttackEffect)cardEffect).GetSAttack(Strike, this, InvertSecutiryValue);
                    }
                }
            }
            #endregion

            return Strike;
        }
    }

    public int Strike
    {
        get
        {
            int Strike = Strike_AllowMinus;

            if (Strike < 0)
            {
                Strike = 0;
            }

            return Strike;
        }
    }
    #endregion

    #region このパーマネントがタップされているかどうか
    public bool OldIsSuspended = false;
    public bool IsSuspended = false;

    public int DPWhenSuspended = 114514;
    #endregion

    #region Whether this permanent can unsuspend
    public bool CanUnsuspend
    {
        get
        {
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
            {
                #region Effects of field permanents
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is ICanNotUnsuspendEffect)
                        {
                            if (cardEffect.CanUse(null))
                            {
                                if (((ICanNotUnsuspendEffect)cardEffect).CanNotUnsuspend(this))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Effects of players
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is ICanNotUnsuspendEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (((ICanNotUnsuspendEffect)cardEffect).CanNotUnsuspend(this))
                            {
                                return false;
                            }
                        }
                    }
                }
                #endregion
            }

            return true;
        }
    }
    #endregion

    #region このパーマネントが移動できるかどうか
    public bool CanMove
    {
        get
        {
            if (TopCard != null)
            {
                #region the effects of permanents
                if (GManager.instance.turnStateMachine.gameContext.Players
                    .Map(player => player.GetFieldPermanents())
                    .Flat()
                    .Map(permanent => permanent.EffectList(EffectTiming.None))
                    .Flat()
                    .Some(cardEffect => cardEffect is ICanNotMoveEffect
                        && cardEffect.CanUse(null)
                        && ((ICanNotMoveEffect)cardEffect).CanNotMove(TopCard, null)))
                {
                    return false;
                }
                #endregion

                #region the effects of players
                if (GManager.instance.turnStateMachine.gameContext.Players
                        .Map(player => player.EffectList(EffectTiming.None))
                        .Flat()
                        .Some(cardEffect => cardEffect is ICanNotMoveEffect
                            && cardEffect.CanUse(null)
                            && ((ICanNotMoveEffect)cardEffect).CanNotMove(TopCard, null)))
                {
                    return false;
                }
                #endregion

                #region the effects of itself
                if (this == null)
                {
                    if (EffectList(EffectTiming.None)
                            .Some(cardEffect => cardEffect is ICanNotMoveEffect
                                && cardEffect.CanUse(null)
                                && ((ICanNotMoveEffect)cardEffect).CanNotMove(TopCard, null)))
                    {
                        return false;
                    }
                }
                #endregion

                if (TopCard.PermanentOfThisCard().PermanentFrame.isBreedingAreaFrame())
                {
                    if (!TopCard.Owner.GetBreedingAreaPermanents().Contains(this))
                    {
                        return false;
                    }
                }
                else
                {
                    if (TopCard.Owner.GetBreedingAreaPermanents().Count > 0)
                        return false;
                }

                if (!IsDigimon)
                    return false;

                if (TopCard.IsDigiEgg && DP <= 0)
                    return false;

                if (GManager.instance.turnStateMachine.gameContext.TurnPlayer.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame()) == 0)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }
    }
    #endregion

    #region このパーマネントが攻撃できるかどうか
    public bool CanAttack(ICardEffect cardEffect, bool withoutTap = false, bool isVortex = false)
    {
        // can not attack with empty cards
        if (TopCard == null)
        {
            return false;
        }

        // can not attack during opponent's turn
        if (TopCard.Owner != GManager.instance.turnStateMachine.gameContext.TurnPlayer)
        {
            return false;
        }

        //Can not attack during another attack
        if (GManager.instance.attackProcess.IsAttacking)
            return false;

        // can not attack to player
        if (!CanAttackTargetDigimon(null, cardEffect, withoutTap, isVortex))
        {
            // can not attack to opponent's Digimon
            if (TopCard.Owner.Enemy.GetFieldPermanents().Count((permanent) => CanAttackTargetDigimon(permanent, cardEffect, withoutTap, isVortex)) == 0)
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    #region このパーマネントが対象の攻撃パーマネントをブロックできるかどうか
    public bool CanBlock(Permanent AttackingPermanent)
    {
        if (TopCard == null)
        {
            return false;
        }

        if (!IsDigimon)
        {
            return false;
        }

        if (IsSuspended)
        {
            return false;
        }

        if (!CanSuspend)
        {
            return false;
        }

        if (PermanentFrame != null)
        {
            if (!PermanentFrame.IsBattleAreaFrame())
            {
                return false;
            }
        }

        if (!AttackingPermanent.IsDigimon)
            return false;

        if (!AttackingPermanent.CanSwitchAttackTarget)
            return false;

        #region "Unblockable" effect

        #region Effects of permanents in play
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect is ICannotBlockEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (((ICannotBlockEffect)cardEffect).CannotBlock(AttackingPermanent, this))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region player effect
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
        {
            foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
            {
                if (cardEffect is ICannotBlockEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        if (((ICannotBlockEffect)cardEffect).CannotBlock(AttackingPermanent, this))
                        {
                            if (!TopCard.CanNotBeAffected(cardEffect))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #endregion



        return true;
    }
    #endregion

    #region 対象のパーマネントを攻撃できるか
    public bool CanAttackTargetDigimon(Permanent Defender, ICardEffect cardEffect, bool withoutTap = false, bool isVortex = false)
    {
        if (TopCard != null)
        {
            if (!IsDigimon)
            {
                return false;
            }

            if (!withoutTap)
            {
                if (IsSuspended)
                {
                    return false;
                }

                if (!CanSuspend)
                {
                    return false;
                }
            }

            if (PermanentFrame != null)
            {
                if (!PermanentFrame.IsBattleAreaFrame())
                {
                    return false;
                }
            }

            if (EnterFieldTurnCount == GManager.instance.turnStateMachine.TurnCount)
            {
                if (!HasRush && !isVortex)
                {
                    return false;
                }
            }

            #region 「対象の防御パーマネントを攻撃できない」効果

            #region 場のパーマネントの効果
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
            {
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    foreach (ICardEffect cardEffect1 in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect1 is ICanNotAttackTargetDefendingPermanentEffect)
                        {
                            if (cardEffect1.CanUse(null))
                            {
                                if (((ICanNotAttackTargetDefendingPermanentEffect)cardEffect1).CanNotAttackTargetDefendingPermanent(this, Defender))
                                {
                                    if (!TopCard.CanNotBeAffected(cardEffect1))
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region プレイヤーの効果
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
            {
                foreach (ICardEffect cardEffect1 in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect1 is ICanNotAttackTargetDefendingPermanentEffect)
                    {
                        if (cardEffect1.CanUse(null))
                        {
                            if (((ICanNotAttackTargetDefendingPermanentEffect)cardEffect1).CanNotAttackTargetDefendingPermanent(this, Defender))
                            {
                                if (!TopCard.CanNotBeAffected(cardEffect1))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #endregion

            if (Defender != null)
            {
                if (Defender.TopCard != null)
                {
                    if (Defender.TopCard.Owner == TopCard.Owner.Enemy)
                    {
                        if (Defender.IsDigimon && Defender.TopCard.Owner.GetBattleAreaPermanents().Contains(Defender))
                        {
                            if (Defender.IsSuspended)
                            {
                                return true;
                            }

                            #region 「対象の防御パーマネントを攻撃できる」効果

                            #region 場のパーマネントの効果
                            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
                            {
                                foreach (Permanent permanent in player.GetFieldPermanents())
                                {
                                    foreach (ICardEffect cardEffect1 in permanent.EffectList(EffectTiming.None))
                                    {
                                        if (cardEffect1 is ICanAttackTargetDefendingPermanentEffect)
                                        {
                                            if (cardEffect1.CanUse(null))
                                            {
                                                if (((ICanAttackTargetDefendingPermanentEffect)cardEffect1).CanAttackTargetDefendingPermanent(this, Defender, cardEffect))
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region プレイヤーの効果
                            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
                            {
                                foreach (ICardEffect cardEffect1 in player.EffectList(EffectTiming.None))
                                {
                                    if (cardEffect1 is ICanAttackTargetDefendingPermanentEffect)
                                    {
                                        if (cardEffect1.CanUse(null))
                                        {
                                            if (((ICanAttackTargetDefendingPermanentEffect)cardEffect1).CanAttackTargetDefendingPermanent(this, Defender, cardEffect))
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #endregion                           
                        }
                    }
                }
            }

            else
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Is Unblockable
    public bool IsUnblockable
    {
        get
        {
            foreach (ICardEffect cardEffect in this.EffectList(EffectTiming.None))
            {
                if (cardEffect is CannotBlockClass)
                {
                    if (cardEffect.EffectName == "Unblockable")
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
    #endregion

    #region Has Blocker
    public bool HasBlocker
    {
        get
        {
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                #region Effects of permanents in play
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IBlockerEffect)
                        {
                            if (cardEffect.CanTrigger(null))
                            {
                                if (((IBlockerEffect)cardEffect).IsBlocker(this))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Effects of faceup security
                foreach (CardSource source in player.SecurityCards)
                {
                    if (source.IsFlipped)
                        continue;

                    foreach (ICardEffect cardEffect in source.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IBlockerEffect)
                        {
                            if (cardEffect.CanTrigger(null))
                            {
                                if (((IBlockerEffect)cardEffect).IsBlocker(this))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region プレイヤーの効果
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is IBlockerEffect)
                    {
                        if (cardEffect.CanTrigger(null))
                        {
                            if (((IBlockerEffect)cardEffect).IsBlocker(this))
                            {
                                return true;
                            }
                        }
                    }
                }
                #endregion
            }

            return false;
        }
    }
    #endregion

    #region Has Jamming
    public bool HasJamming
    {
        get
        {
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    #region 場のパーマネントの効果
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is ICanNotBeDestroyedByBattleEffect)
                        {
                            if (cardEffect.CanTrigger(null))
                            {
                                if (cardEffect.EffectName == "Jamming")
                                {
                                    if (((ICanNotBeDestroyedByBattleEffect)cardEffect).PermanentCondition(this))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }

                #region プレイヤーの効果
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is ICanNotBeDestroyedByBattleEffect)
                    {
                        if (cardEffect.CanTrigger(null))
                        {
                            if (cardEffect.EffectName == "Jamming")
                            {
                                if (((ICanNotBeDestroyedByBattleEffect)cardEffect).PermanentCondition(this))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            return false;
        }
    }
    #endregion

    #region Has Ice Clad
    public bool HasIceclad
    {
        get
        {
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                #region Ice clad permanent effects
                foreach (ICardEffect cardEffect in EffectList(EffectTiming.None))
                {
                    if (cardEffect is IIcecladEffect)
                    {
                        if (cardEffect.CanTrigger(null))
                        {
                            if (((IIcecladEffect)cardEffect).HasIceclad(this))
                            {
                                return true;
                            }
                        }
                    }
                }
                #endregion

                #region Ice clad Player Effects
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is IIcecladEffect)
                    {
                        if (cardEffect.CanTrigger(null))
                        {
                            if (((IIcecladEffect)cardEffect).HasIceclad(this))
                            {
                                return true;
                            }
                        }
                    }
                }
                #endregion
            }

            return false;
        }
    }
    #endregion

    #region Whether this permanent has Pierce
    public bool HasPierce
    {
        get
        {
            if (IsDigimon)
            {
                foreach (ICardEffect cardEffect in EffectList(EffectTiming.OnDetermineDoSecurityCheck))
                {
                    if (cardEffect is ActivateICardEffect)
                    {
                        if (cardEffect.CanTrigger(CardEffectCommons.PierceCheckHashtableOfPermanent(this)))
                        {
                            if (cardEffect.EffectName == "Pierce")
                            {
                                return true;
                            }

                            if (cardEffect.EffectName == "Piercing")
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
    #endregion

    #region Has Reboot
    public bool HasReboot
    {
        get
        {
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    #region 場のパーマネントの効果
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IRebootEffect)
                        {
                            if (cardEffect.CanTrigger(null))
                            {
                                if (((IRebootEffect)cardEffect).HasReboot(this))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    #endregion
                }

                #region プレイヤーの効果
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is IRebootEffect)
                    {
                        if (cardEffect.CanTrigger(null))
                        {
                            if (((IRebootEffect)cardEffect).HasReboot(this))
                            {
                                return true;
                            }
                        }
                    }
                }
                #endregion
            }

            return false;
        }
    }
    #endregion

    #region Has Raid
    public bool HasRaid
    {
        get
        {
            foreach (ICardEffect cardEffect in this.EffectList(EffectTiming.OnAllyAttack))
            {
                if (cardEffect is ActivateICardEffect)
                {
                    if (cardEffect.EffectName == "Raid")
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
    #endregion

    #region Has Rush
    public bool HasRush
    {
        get
        {
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    #region 場のパーマネントの効果
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IRushEffect)
                        {
                            if (cardEffect.CanTrigger(null))
                            {
                                if (((IRushEffect)cardEffect).HasRush(this))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    #endregion
                }

                #region プレイヤーの効果
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is IRushEffect)
                    {
                        if (cardEffect.CanTrigger(null))
                        {
                            if (((IRushEffect)cardEffect).HasRush(this))
                            {
                                return true;
                            }
                        }
                    }
                }
                #endregion
            }

            return false;
        }
    }
    #endregion

    #region Has Retaliation
    public bool HasRetaliation
    {
        get
        {
            if (RetaliationCount >= 1)
            {
                return true;
            }

            return false;
        }
    }
    #endregion

    #region Retaliation Count
    public int RetaliationCount
    {
        get
        {
            int count = 0;

            foreach (ICardEffect cardEffect in EffectList(EffectTiming.OnDestroyedAnyone))
            {
                if (cardEffect is ActivateICardEffect)
                {
                    if (cardEffect.EffectName == "Retaliation")
                    {
                        if (cardEffect.CanTrigger(CardEffectCommons.OnDeletionCheckHashtableOfPermanent(this)))
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }
    }
    #endregion

    #region Has Fortitude
    public bool HasFortitude
    {
        get
        {
            foreach (ICardEffect cardEffect in EffectList(EffectTiming.OnDestroyedAnyone))
            {
                if (cardEffect is ActivateICardEffect)
                {
                    if (cardEffect.EffectName == "Fortitude")
                    {
                        if (cardEffect.CanTrigger(CardEffectCommons.OnDeletionCheckHashtableOfPermanent(this)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
    #endregion

    #region Has Blitz
    public bool HasBlitz
    {
        get
        {
            foreach (ICardEffect cardEffect in EffectList(EffectTiming.OnEnterFieldAnyone))
            {
                if (cardEffect is ActivateICardEffect)
                {
                    if (cardEffect.CanTrigger(CardEffectCommons.WhenDigivolutionCheckHashtableOfPermanent(this)) || cardEffect.CanTrigger(CardEffectCommons.OnPlayCheckHashtableOfPermanent(this)))
                    {
                        if (!string.IsNullOrEmpty(cardEffect.EffectDiscription))
                        {
                            if (cardEffect.EffectDiscription.Contains("Blitz"))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
    #endregion

    #region Has Evade
    public bool HasEvade
    {
        get
        {
            foreach (ICardEffect cardEffect in this.EffectList(EffectTiming.WhenPermanentWouldBeDeleted))
            {
                if (cardEffect is ActivateICardEffect)
                {
                    Hashtable hashtable = new Hashtable()
                    {
                        {"Permanents", new List<Permanent>() { this }}
                    };

                    if (cardEffect.CanTrigger(hashtable))
                    {
                        if (cardEffect.EffectName == "Evade")
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
    #endregion

    #region Has Mind Link
    public bool HasMindLink
    {
        get
        {
            foreach (ICardEffect cardEffect in EffectList(EffectTiming.OnDeclaration))
            {
                if (cardEffect is ActivateICardEffect)
                {
                    if (cardEffect.CanTrigger(null))
                    {
                        if (!String.IsNullOrEmpty(cardEffect.EffectDiscription))
                        {
                            if (cardEffect.EffectDiscription.Contains("Mind Link"))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
    #endregion

    #region Has Barrier
    public bool HasBarrier
    {
        get
        {
            foreach (ICardEffect cardEffect in this.EffectList(EffectTiming.WhenPermanentWouldBeDeleted))
            {
                if (cardEffect is ActivateICardEffect)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable.Add("Permanents", new List<Permanent>() { this });
                    hashtable.Add("battle", new IBattle(null, null, null));

                    if (cardEffect.CanTrigger(hashtable))
                    {
                        if (cardEffect.EffectName == "Barrier")
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
    #endregion

    #region Has Alliance
    public bool HasAlliance
    {
        get
        {
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    #region Permanent Effects
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.OnAllyAttack))
                    {
                        if (cardEffect is IAllianceEffect)
                        {
                            if (cardEffect.CanTrigger(null))
                            {
                                if (((IAllianceEffect)cardEffect).HasAlliance(this))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    #endregion
                }

                #region Effects of faceup security
                foreach (CardSource source in player.SecurityCards)
                {
                    if (source.IsFlipped)
                        continue;

                    foreach (ICardEffect cardEffect in source.EffectList(EffectTiming.OnAllyAttack))
                    {
                        if (cardEffect is IAllianceEffect)
                        {
                            if (cardEffect.CanTrigger(null))
                            {
                                if (((IAllianceEffect)cardEffect).HasAlliance(this))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Player Effects
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.OnAllyAttack))
                {
                    if (cardEffect is IAllianceEffect)
                    {
                        if (cardEffect.CanTrigger(null))
                        {
                            if (((IAllianceEffect)cardEffect).HasAlliance(this))
                            {
                                return true;
                            }
                        }
                    }
                }
                #endregion
            }

            return false;
        }
    }
    #endregion

    #region Has Collision
    public bool HasCollision
    {
        get
        {
            foreach (ICardEffect cardEffect in this.EffectList(EffectTiming.OnAllyAttack))
            {
                if (cardEffect is ActivateICardEffect)
                {
                    if (cardEffect.EffectName == "Collision")
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
    #endregion

    #region Has Partition
    public bool HasPartition
    {
        get
        {
            foreach (ICardEffect cardEffect in this.EffectList(EffectTiming.WhenPermanentWouldBeDeleted))
            {
                if (cardEffect is ActivateICardEffect)
                {
                    if (cardEffect.EffectName == "Partition")
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
    #endregion

    #region Has Scapegoat
    public bool HasScapegoat
    {
        get
        {
            foreach (ICardEffect cardEffect in this.EffectList(EffectTiming.WhenPermanentWouldBeDeleted))
            {
                if (cardEffect is ActivateICardEffect)
                {
                    if (cardEffect.EffectName == "<Scapegoat>")
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
    #endregion

    #region 消滅時効化を持つか
    public bool HasOnDeletionEffect
    {
        get
        {
            foreach (ICardEffect cardEffect in EffectList(EffectTiming.OnDestroyedAnyone))
            {
                if (cardEffect is ActivateICardEffect)
                {
                    if (cardEffect.CanTrigger(CardEffectCommons.OnDeletionCheckHashtableOfPermanent(this)))
                    {
                        if (!string.IsNullOrEmpty(cardEffect.EffectDiscription))
                        {
                            if (cardEffect.IsOnDeletion)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
    #endregion

    #region バトルしているか
    public IBattle battle { get; set; } = null;
    #endregion

    #region Can Be Destroyed
    public bool CanBeDestroyed()
    {
        #region 消滅しない効果
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                #region 場のパーマネントの効果
                foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect is ICanNotBeDestroyedEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (((ICanNotBeDestroyedEffect)cardEffect).CanNotBeDestroyed(this))
                            {
                                return false;
                            }
                        }
                    }
                }
                #endregion
            }

            #region プレイヤーの効果
            foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
            {
                if (cardEffect is ICanNotBeDestroyedEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        if (((ICanNotBeDestroyedEffect)cardEffect).CanNotBeDestroyed(this))
                        {
                            return false;
                        }
                    }
                }
            }
            #endregion
        }
        #endregion

        return true;
    }
    #endregion

    #region Can Be Destroyed By Battle
    public bool CanBeDestroyedByBattle(Permanent AttackingPermanent, Permanent DefendingPermanent, CardSource DefendingCard)
    {
        if (!CanBeDestroyed())
        {
            return false;
        }

        #region Given Effects
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                #region Permanents
                foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect is ICanNotBeDestroyedByBattleEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (((ICanNotBeDestroyedByBattleEffect)cardEffect).CanNotBeDestroyedByBattle(this, AttackingPermanent, DefendingPermanent, DefendingCard))
                            {
                                return false;
                            }
                        }
                    }
                }
                #endregion
            }

            #region Effects of faceup security
            foreach (CardSource source in player.SecurityCards)
            {
                if (source.IsFlipped)
                    continue;



                foreach (ICardEffect cardEffect in source.EffectList(EffectTiming.None))
                {
                    if (cardEffect is ICanNotBeDestroyedByBattleEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (((ICanNotBeDestroyedByBattleEffect)cardEffect).CanNotBeDestroyedByBattle(this, AttackingPermanent, DefendingPermanent, DefendingCard))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            #endregion

            #region Player Effects
            foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
            {
                if (cardEffect is ICanNotBeDestroyedByBattleEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        if (((ICanNotBeDestroyedByBattleEffect)cardEffect).CanNotBeDestroyedByBattle(this, AttackingPermanent, DefendingPermanent, DefendingCard))
                        {
                            return false;
                        }
                    }
                }
            }
            #endregion
        }
        #endregion

        return true;
    }
    #endregion

    #region このパーマネントが効果で消滅するか
    public bool CanBeDestroyedBySkill(ICardEffect cardEffect)
    {
        if (this.TopCard != null)
        {
            if (this.TopCard.CanNotBeAffected(cardEffect))
            {
                return false;
            }

            if (!CanBeDestroyed())
            {
                return false;
            }

            #region 効果で消滅しない効果
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    #region 場のパーマネントの効果
                    foreach (ICardEffect cardEffect1 in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect1 is ICanNotBeDestroyedBySkillEffect)
                        {
                            if (cardEffect1.CanUse(null))
                            {
                                if (((ICanNotBeDestroyedBySkillEffect)cardEffect1).CanNotBeDestroyedBySkill(this, cardEffect))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    #endregion
                }

                #region プレイヤーの効果
                foreach (ICardEffect cardEffect1 in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect1 is ICanNotBeDestroyedBySkillEffect)
                    {
                        if (cardEffect1.CanUse(null))
                        {
                            if (((ICanNotBeDestroyedBySkillEffect)cardEffect1).CanNotBeDestroyedBySkill(this, cardEffect))
                            {
                                return false;
                            }
                        }
                    }
                }
                #endregion
            }
            #endregion
        }

        return true;
    }
    #endregion

    #region Can Leave Field
    public bool CanBeRemoved()
    {
        #region Effect that never disappears
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                #region Effects of permanents in play
                foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect is ICanNotBeRemovedEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (((ICanNotBeRemovedEffect)cardEffect).CanNotBeRemoved(this))
                            {
                                return false;
                            }
                        }
                    }
                }
                #endregion
            }

            #region player effect
            foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
            {
                if (cardEffect is ICanNotBeRemovedEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        if (((ICanNotBeRemovedEffect)cardEffect).CanNotBeRemoved(this))
                        {
                            return false;
                        }
                    }
                }
            }
            #endregion
        }
        #endregion

        return true;
    }
    #endregion

    #region トークンかどうか
    public bool IsToken
    {
        get
        {
            if (TopCard != null)
            {
                if (TopCard.IsToken)
                {
                    return true;
                }
            }

            return false;
        }
    }
    #endregion

    #region そのパーマネントが消滅・バウンス待機状態か
    public bool willBeRemoveField { get; set; } = false;
    #endregion

    #region Is a Digimon card
    public bool IsDigimon
    {
        get
        {
            if (TopCard != null)
            {
                if (TopCard.IsFlipped)
                    return false;

                if (TopCard.IsDigimon || TopCard.IsDigiEgg)
                {
                    return true;
                }

                #region Effect of treating it as a Digimon
                foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                {
                    foreach (Permanent permanent in player.GetFieldPermanents())
                    {
                        #region Effects of permanents in play
                        foreach (ICardEffect cardEffect in permanent.EffectList_Added(EffectTiming.None))
                        {
                            if (cardEffect is ITreatAsDigimonEffect)
                            {
                                if (cardEffect.CanTrigger(null))
                                {
                                    if (((ITreatAsDigimonEffect)cardEffect).IsDigimon(this))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                        #endregion
                    }

                    #region player effect
                    foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is ITreatAsDigimonEffect)
                        {
                            if (cardEffect.CanTrigger(null))
                            {
                                if (((ITreatAsDigimonEffect)cardEffect).IsDigimon(this))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    #endregion
                }
                #endregion
            }

            return false;
        }
    }
    #endregion

    #region Is a Tamer card
    public bool IsTamer
    {
        get
        {
            if (TopCard != null)
            {
                if (TopCard.IsFlipped)
                    return false;

                if (TopCard.IsTamer)
                {
                    return true;
                }
            }

            return false;
        }
    }
    #endregion
    
    #region Is an Option card
    public bool IsOption
    {
        get
        {
            if (TopCard != null)
            {
                if (TopCard.IsOption)
                {
                    return true;
                }
            }

            return false;
        }
    }
    #endregion

    #region Levels handled by Jogress
    public List<int> Levels_ForJogress(CardSource cardSource)
    {
        List<int> levels = new List<int>();

        if (cardSource != null)
        {
            if (cardSource.HasLevel)
            {
                levels.Add(this.Level);
            }

            #region Effect of "adding levels handled by jogless evolution"
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    #region Effects of permanents in play
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IAddJogressLevelsEffect)
                        {
                            if (cardEffect.CanUse(null))
                            {
                                foreach (int level in ((IAddJogressLevelsEffect)cardEffect).GetJogressLevels(cardSource, this))
                                {
                                    levels.Add(level);
                                }
                            }
                        }
                    }
                    #endregion
                }

                #region player effect
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is IAddJogressLevelsEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            foreach (int level in ((IAddJogressLevelsEffect)cardEffect).GetJogressLevels(cardSource, this))
                            {
                                levels.Add(level);
                            }
                        }
                    }
                }
                #endregion
            }
            #endregion
        }

        return levels;
    }
    #endregion

    #region Names handled by Jogress
    public List<string> Names_ForDNA(CardSource cardSource)
    {
        List<string> names = new List<string>();

        if (cardSource != null)
        {
            foreach(string name in cardSource.CardNames)
                names.Add(name);

            #region Effect of "adding names handled by DNA evolution"
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    #region Effects of permanents in play
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is IAddDNANamesEffect)
                        {
                            if (cardEffect.CanUse(null))
                            {
                                foreach (string name in ((IAddDNANamesEffect)cardEffect).GetDNANames(cardSource, this))
                                {
                                    names.Add(name);
                                }
                            }
                        }
                    }
                    #endregion
                }

                #region player effect
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is IAddDNANamesEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            foreach (string name in ((IAddDNANamesEffect)cardEffect).GetDNANames(cardSource, this))
                            {
                                names.Add(name);
                            }
                        }
                    }
                }
                #endregion
            }
            #endregion
        }

        return names;
    }
    #endregion

    #region バトルで消滅したか
    public bool IsDestroyedByBattle { get; set; } = false;
    #endregion

    #region 消滅に使用された効果
    public ICardEffect DestroyingEffect { get; set; } = null;
    #endregion

    #region 他のパーマネントの下に置くのに使用された効果
    public ICardEffect PlaceOtherPermanentEffect { get; set; } = null;
    #endregion

    #region 手札バウンス に使用された効果
    public ICardEffect HandBounceEffect { get; set; } = null;
    #endregion

    #region 山札バウンス に使用された効果
    public ICardEffect LibraryBounceEffect { get; set; } = null;
    #endregion

    #region 登場に使用された効果
    public ICardEffect PlayingEffect { get; set; } = null;
    #endregion

    #region 進化に使用された効果
    public ICardEffect DigivolvingEffect { get; set; } = null;
    #endregion

    #region Whether this digimon is placed in the trash by not having a DP
    public bool IsPlaceToTrashDueToNotHavingDP { get; set; } = true;
    #endregion

    #region Whether this permanent can suspend
    public bool CanSuspend
    {
        get
        {
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
            {
                #region Effects of field permanents
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is ICanNotSuspendEffect)
                        {
                            if (cardEffect.CanUse(null))
                            {
                                if (((ICanNotSuspendEffect)cardEffect).CanNotSuspend(this))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Effects of players
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is ICanNotSuspendEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (((ICanNotSuspendEffect)cardEffect).CanNotSuspend(this))
                            {
                                return false;
                            }
                        }
                    }
                }
                #endregion
            }

            return true;
        }
    }
    #endregion

    #region このデジモンのアタックの対象が変更できるか
    public bool CanSwitchAttackTarget
    {
        get
        {
            #region アタックの対象が変更できない効果
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    #region 場のパーマネントの効果
                    foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                    {
                        if (cardEffect is ICanNotSwitchAttackTargetEffect)
                        {
                            if (cardEffect.CanUse(null))
                            {
                                if (((ICanNotSwitchAttackTargetEffect)cardEffect).CanNotBeSwitchAttackTarget(this))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    #endregion
                }

                #region プレイヤーの効果
                foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                {
                    if (cardEffect is ICanNotSwitchAttackTargetEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (((ICanNotSwitchAttackTargetEffect)cardEffect).CanNotBeSwitchAttackTarget(this))
                            {
                                return false;
                            }
                        }
                    }
                }
                #endregion
            }
            #endregion

            return true;
        }
    }
    #endregion

    #region このデジモンを場からデジクロス条件の代わりにできるか
    public bool CanSubstituteForDigiXrosCondition(CardSource cardSource)
    {
        #region デジクロス条件の代わりにできる効果
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                #region 場のパーマネントの効果
                foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect is ICanSelectDigiXrosEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (((ICanSelectDigiXrosEffect)cardEffect).CanSelect(cardSource, this))
                            {
                                return true;
                            }
                        }
                    }
                }
                #endregion
            }

            #region プレイヤーの効果
            foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
            {
                if (cardEffect is ICanSelectDigiXrosEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        if (((ICanSelectDigiXrosEffect)cardEffect).CanSelect(cardSource, this))
                        {
                            return true;
                        }
                    }
                }
            }
            #endregion
        }
        #endregion

        return false;
    }
    #endregion

    #region Can Substitue for Assembly
    public bool CanSubstituteForAssemblyCondition(CardSource cardSource)
    {
        #region Effects that can be used in place of Assembly conditions
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                #region Effects of permanents in play
                foreach (ICardEffect cardEffect in permanent.EffectList(EffectTiming.None))
                {
                    if (cardEffect is ICanSelectAssemblyEffect)
                    {
                        if (cardEffect.CanUse(null))
                        {
                            if (((ICanSelectAssemblyEffect)cardEffect).CanSelect(cardSource, this))
                            {
                                return true;
                            }
                        }
                    }
                }
                #endregion
            }

            #region player effect
            foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
            {
                if (cardEffect is ICanSelectAssemblyEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        if (((ICanSelectAssemblyEffect)cardEffect).CanSelect(cardSource, this))
                        {
                            return true;
                        }
                    }
                }
            }
            #endregion
        }
        #endregion

        return false;
    }
    #endregion

    #region 登場した直後のLevel
    public int LevelJustAfterPlayed { get; set; } = -1;
    #endregion

    #region 登場した直後のPlay Cost
    public int PlayCostJustAfterPlayed { get; set; } = -1;
    #endregion

    #region 登場した直後のCardNames
    public List<string> CardNamesJustAfterPlayed { get; set; } = new List<string>();
    #endregion

    #region 進化した直後のCardNames
    public List<string> CardNamesJustAfterDigivolved { get; set; } = new List<string>();
    #endregion

    #region 登場した直後のTraits
    public List<string> TraitsJustAfterPlayed { get; set; } = new List<string>();
    #endregion

    #region 場を離れる直前のDP
    public int DPJustBeforeRemoveField { get; set; } = -1;
    #endregion

    #region 場を離れる直前のLevel
    public int LevelJustBeforeRemoveField { get; set; } = -1;
    #endregion

    #region 場を離れる直前のCost
    public int CostJustBeforeRemoveField { get; set; } = -1;
    #endregion

    #region 場を離れる直前のカード名
    public List<string> CardNamesJustBeforeRemoveField { get; set; } = new List<string>();
    #endregion

    #region Card Traits Just Before Removed Field
    public List<string> CardTraitsJustBeforeRemoveField { get; set; } = new List<string>();
    #endregion

    #region バースト進化で手札に戻ったか
    public bool IsReturnedToHandByBurstDigivolution { get; set; } = false;
    #endregion

    #region Is Linked Card Added as Source By App Fusion
    public bool IsAddedAsSourceByAppFusion { get; set; } = false;
    #endregion

    #region バースト進化したか
    public bool IsBurstDigivolved { get; set; } = false;
    #endregion

    #region Did it App Fusion?
    public bool IsAppFusion { get; set; } = false;
    #endregion

    #region 効果で場に出たオプションか
    public bool IsPlayedOptionPermanent { get; set; } = false;
    #endregion

    #region 進化元を持たないか
    public bool HasNoDigivolutionCards => DigivolutionCards.Count == 0;
    #endregion

    #region Has face down Digivolution Cards
    public bool HasFaceDownDigivolutionCards => DigivolutionCards.Any(x => x.IsFlipped);
    #endregion

    #region Has No Link Cards
    public bool HasNoLinkCards => LinkedCards.Count == 0;
    #endregion

    #region Digivolution cards' colors
    public List<CardColor> DigivolutionCardsColors
    {
        get
        {
            List<CardColor> cardColors = new List<CardColor>();

            foreach (CardSource cardSource in DigivolutionCards)
            {
                if (cardSource.IsFlipped)
                    continue;

                foreach (CardColor cardColor in cardSource.CardColors)
                {
                    if (!cardColors.Contains(cardColor))
                    {
                        cardColors.Add(cardColor);
                    }
                }
            }

            return cardColors.Distinct().ToList();
        }
    }
    #endregion

    #region Show "Unsuspend" effect object
    public void ShowUnsuspendEffect()
    {
        if (TopCard != null)
        {
            if (ShowingPermanentCard != null)
            {
                if (ShowingPermanentCard.WillUntapObject != null)
                {
                    ShowingPermanentCard.WillUntapObject.transform.parent.gameObject.SetActive(true);
                    ShowingPermanentCard.WillUntapObject.SetActive(true);
                }
            }
        }
    }
    #endregion

    #region Show "Return to deck" effect object
    public void ShowDeckBounceEffect()
    {
        if (TopCard != null)
        {
            if (willBeRemoveField)
            {
                if (ShowingPermanentCard != null)
                {
                    if (ShowingPermanentCard.WillBeDeckBounceObject != null)
                    {
                        ShowingPermanentCard.WillBeDeckBounceObject.transform.parent.gameObject.SetActive(true);
                        ShowingPermanentCard.WillBeDeckBounceObject.SetActive(true);
                    }
                }
            }
        }
    }
    #endregion

    #region Hide "Return to deck" effect object
    public void HideDeckBounceEffect()
    {
        if (TopCard != null)
        {
            if (ShowingPermanentCard != null)
            {
                if (ShowingPermanentCard.WillBeDeckBounceObject != null)
                {
                    ShowingPermanentCard.WillBeDeckBounceObject.SetActive(false);
                }
            }
        }
    }
    #endregion

    #region Show "Return to hand" effect object
    public void ShowHandBounceEffect()
    {
        if (TopCard != null)
        {
            if (willBeRemoveField)
            {
                if (ShowingPermanentCard != null)
                {
                    if (ShowingPermanentCard.WillBeHandBounceObject != null)
                    {
                        ShowingPermanentCard.WillBeHandBounceObject.transform.parent.gameObject.SetActive(true);
                        ShowingPermanentCard.WillBeHandBounceObject.SetActive(true);
                    }
                }
            }
        }
    }
    #endregion

    #region Hide "Return to hand" effect object
    public void HideHandBounceEffect()
    {
        if (TopCard != null)
        {
            if (ShowingPermanentCard != null)
            {
                if (ShowingPermanentCard.WillBeHandBounceObject != null)
                {
                    ShowingPermanentCard.WillBeHandBounceObject.SetActive(false);
                }
            }
        }
    }
    #endregion

    #region Show "Delete" effect object
    public void ShowDeleteEffect()
    {
        if (TopCard != null)
        {
            if (willBeRemoveField)
            {
                if (ShowingPermanentCard != null)
                {
                    if (ShowingPermanentCard.WillBeDeletedObject != null)
                    {
                        ShowingPermanentCard.WillBeDeletedObject.transform.parent.gameObject.SetActive(true);
                        ShowingPermanentCard.WillBeDeletedObject.SetActive(true);
                    }
                }
            }
        }
    }
    #endregion

    #region Hide "Delete" effect object
    public void HideDeleteEffect()
    {
        if (TopCard != null)
        {
            if (ShowingPermanentCard != null)
            {
                if (ShowingPermanentCard.WillBeDeletedObject != null)
                {
                    ShowingPermanentCard.WillBeDeletedObject.SetActive(false);
                }
            }
        }
    }
    #endregion

    #region Show "Remove Field" effect object
    public void ShowWillRemoveFieldEffect()
    {
        if (TopCard != null)
        {
            if (willBeRemoveField)
            {
                if (ShowingPermanentCard != null)
                {
                    if (ShowingPermanentCard.WillRemoveFieldObject != null)
                    {
                        ShowingPermanentCard.WillRemoveFieldObject.transform.parent.gameObject.SetActive(true);
                        ShowingPermanentCard.WillRemoveFieldObject.SetActive(true);
                    }
                }
            }
        }
    }
    #endregion

    #region Hide "Remove Field" effect object
    public void HideWillRemoveFieldEffect()
    {
        if (TopCard != null)
        {
            if (ShowingPermanentCard != null)
            {
                if (ShowingPermanentCard.WillRemoveFieldObject != null)
                {
                    ShowingPermanentCard.WillRemoveFieldObject.SetActive(false);
                }
            }
        }
    }
    #endregion

    #region Show "Digivolution" effect object
    public void ShowWillEvolutionEffect()
    {
        if (TopCard != null)
        {
            if (ShowingPermanentCard != null)
            {
                if (ShowingPermanentCard.WillEvolutionObject != null)
                {
                    ShowingPermanentCard.WillEvolutionObject.transform.parent.gameObject.SetActive(true);
                    ShowingPermanentCard.WillEvolutionObject.SetActive(true);
                }
            }
        }
    }
    #endregion

    #region Hide "Digivolution" effect object
    public void HideWillEvolutionEffect()
    {
        if (TopCard != null)
        {
            if (ShowingPermanentCard != null)
            {
                if (ShowingPermanentCard.WillEvolutionObject != null)
                {
                    ShowingPermanentCard.WillEvolutionObject.SetActive(false);
                }
            }
        }
    }
    #endregion
}

