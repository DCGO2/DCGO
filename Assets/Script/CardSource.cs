using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DCGO.CardEntities;

public class CardSource : MonoBehaviour
{
    #region card base entity

    CEntity_Base _cEntity_Base = null;

    #endregion

    #region PhotonView

    public PhotonView PhotonView { get; set; }

    #endregion

    #region owner

    public Player Owner { get; private set; } = null;

    #endregion

    #region card index

    public int CardIndex { get; private set; } = 0;

    #endregion

    #region set card base entity

    public void SetBaseData(CEntity_Base cEntity_Base, Player owner)
    {
        _cEntity_Base = cEntity_Base;
        Owner = owner;
        gameObject.name = _cEntity_Base.CardName_ENG;

        SetFace();
    }

    #endregion

    #region card effect controller

    public CEntity_EffectController cEntity_EffectController;

    #endregion

    #region whether this card is reversed

    public bool IsFlipped { get; private set; }

    #region set face

    public void SetFace(string str = "")
    {
        IsFlipped = false;

        if (str != "")
            UnityEngine.Debug.Log($"CARD FLIPPED: {this.BaseENGCardNameFromEntity}, from - {str}");

        GManager.OnCardFlippedChanged?.Invoke();
        GManager.OnSecurityStackChanged?.Invoke(Owner);
    }

    #endregion

    #region set reverse

    public void SetReverse()
    {
        IsFlipped = true;

        GManager.OnCardFlippedChanged?.Invoke();
        GManager.OnSecurityStackChanged?.Invoke(Owner);
    }

    #endregion

    #endregion

    #region Card ID setup

    public void SetUpCardIndex(int _cardIndex)
    {
        PhotonView _PhotonView = GetComponent<PhotonView>();

        _PhotonView ??= gameObject.AddComponent<PhotonView>();

        CardIndex = _cardIndex;

        _PhotonView.ViewID = CardIndex + 60;

        PhotonView = _PhotonView;
    }

    #endregion

    #region whether this card can be played

    public bool CanPlayFromHandDuringMainPhase
    {
        get
        {
            if (CanNotPlayThisOption)
            {
                return false;
            }

            if (_cEntity_Base.IsPermanent)
            {
                if (!CanPlayJogress(true))
                {
                    if (!CanPutFieldThisPermanentCard(true, null))
                    {
                        return false;
                    }
                }
            }
            else if (IsOption)
            {
                #region whether cost can be paid

                List<int> costs = new List<int>() { this.PayingCost(SelectCardEffect.Root.Hand, null, checkAvailability: true) };

                bool canPayCost = costs.Some(cost => Owner.MaxMemoryCost >= cost);

                if (!canPayCost) return false;

                #endregion
            }

            return true;
        }
    }

    #endregion

    #region Whether this option card's effect prevents it from being played

    public bool CanNotPlayThisOption
    {
        get
        {
            if (!IsOption)
            {
                return false;
            }

            #region the effects of players

            if (GManager.instance.turnStateMachine.gameContext.Players
                    .Map(player => player.EffectList(EffectTiming.None))
                    .Flat()
                    .Some(cardEffect => cardEffect is ICanNotPlayCardEffect
                    && cardEffect.CanUse(null)
                    && ((ICanNotPlayCardEffect)cardEffect).CanNotPlay(this)))
            {
                return true;
            }

            #endregion

            #region the effects of permanents

            if (GManager.instance.turnStateMachine.gameContext.Players
                .Map(player => player.GetFieldPermanents())
                .Flat()
                .Map(permanent => permanent.EffectList(EffectTiming.None))
                .Flat()
                .Some(cardEffect => cardEffect is ICanNotPlayCardEffect
                && cardEffect.CanUse(null)
                && ((ICanNotPlayCardEffect)cardEffect).CanNotPlay(this)))
            {
                return true;
            }

            #endregion

            #region the effects of itself

            if (PermanentOfThisCard() == null)
            {
                if (EffectList(EffectTiming.None)
                        .Some(cardEffect => cardEffect is ICanNotPlayCardEffect
                            && cardEffect.CanUse(null)
                            && ((ICanNotPlayCardEffect)cardEffect).CanNotPlay(this)))
                {
                    return true;
                }
            }

            #endregion

            #region Whether the color requirement is met

            if (!MatchColorRequirement)
            {
                return true;
            }

            #endregion

            return false;
        }
    }

    #endregion

    #region Whether the color requirement is met

    public bool MatchColorRequirement
    {
        get
        {
            if (IsOption)
            {
                #region "ignore color requirement" effects

                #region the effects of permanents

                if (GManager.instance.turnStateMachine.gameContext.Players
                    .Map(player => player.GetFieldPermanents())
                    .Flat()
                    .Map(permanent => permanent.EffectList(EffectTiming.None))
                    .Flat()
                    .Some(cardEffect => cardEffect is IIgnoreColorConditionEffect
                        && cardEffect.CanUse(null)
                        && ((IIgnoreColorConditionEffect)cardEffect).IgnoreColorCondition(this)))
                {
                    return true;
                }

                #endregion

                #region the effects of players

                if (GManager.instance.turnStateMachine.gameContext.Players
                        .Map(player => player.EffectList(EffectTiming.None))
                        .Flat()
                        .Some(cardEffect => cardEffect is IIgnoreColorConditionEffect
                            && cardEffect.CanUse(null)
                            && ((IIgnoreColorConditionEffect)cardEffect).IgnoreColorCondition(this)))
                {
                    return true;
                }

                #endregion

                #region the effects of itself

                if (EffectList(EffectTiming.None)
                        .Some(cardEffect => cardEffect is IIgnoreColorConditionEffect
                            && cardEffect.CanUse(null)
                            && ((IIgnoreColorConditionEffect)cardEffect).IgnoreColorCondition(this)))
                {
                    return true;
                }

                #endregion

                #endregion

                bool matchColorRequirement = CardColors.Every(cardColor =>
                    Owner.GetFieldPermanents().Some(permanent =>
                        !permanent.TopCard.IsOption && permanent.TopCard.CardColors.Contains(cardColor)));

                if (!matchColorRequirement)
                {
                    return false;
                }
            }

            return true;
        }
    }

    #endregion

    #region Card object in hand displaying this card

    public HandCard ShowingHandCard { get; private set; }

    public void SetShowingHandCard(HandCard showingHandCard) => ShowingHandCard = showingHandCard;

    #endregion

    #region Permanent in the field containing this card

    public Permanent PermanentOfThisCard()
    {
        return Owner.GetFieldPermanents().Find(permanent =>
            (permanent.cardSources.Contains(this)));
    }

    #endregion

    #region initialize

    public void Init()
    {
        cEntity_EffectController.InitUseCountThisTurn();
        SetFace();
    }

    #endregion

    #region base card colors from entity

    public List<CardColor> BaseCardColorsFromEntity => _cEntity_Base.cardColors.Distinct().ToList();

    #endregion

    #region base card colors

    public List<CardColor> BaseCardColors
    {
        get
        {
            List<CardColor> baseCardColors = BaseCardColorsFromEntity;

            #region the effects that changes base card colors

            #region the effects of itself

            if (PermanentOfThisCard() == null)
            {
                EffectList(EffectTiming.None)
                    .Filter(cardEffect => cardEffect is IChangeBaseCardColorEffect && cardEffect.CanUse(null))
                    .ForEach(cardEffect => baseCardColors = ((IChangeBaseCardColorEffect)cardEffect).GetBaseCardColors(baseCardColors, this));
            }

            #endregion

            #region the effects of permanents

            GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                .Map(player => player.GetFieldPermanents())
                .Flat()
                .Map(permanent => permanent.EffectList(EffectTiming.None))
                .Flat()
                .Filter(cardEffect => cardEffect is IChangeBaseCardColorEffect && cardEffect.CanUse(null))
                .ForEach(cardEffect => baseCardColors = ((IChangeBaseCardColorEffect)cardEffect).GetBaseCardColors(baseCardColors, this));

            #endregion

            #endregion

            baseCardColors = baseCardColors.Distinct().ToList();

            return baseCardColors;
        }
    }

    #endregion

    #region card colors

    public List<CardColor> CardColors
    {
        get
        {
            List<CardColor> cardColors = BaseCardColors;

            #region the effects that changes card colors

            #region the effects of itself

            if (PermanentOfThisCard() == null)
            {
                EffectList(EffectTiming.None)
                    .Filter(cardEffect => cardEffect is IChangeCardColorEffect && cardEffect.CanUse(null))
                    .ForEach(cardEffect => cardColors = ((IChangeCardColorEffect)cardEffect).GetCardColors(cardColors, this));
            }

            #endregion

            #region the effects of permanents

            GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                .Map(player => player.GetFieldPermanents())
                .Flat()
                .Map(permanent => permanent.EffectList(EffectTiming.None))
                .Flat()
                .Filter(cardEffect => cardEffect is IChangeCardColorEffect && cardEffect.CanUse(null))
                .ForEach(cardEffect => cardColors = ((IChangeCardColorEffect)cardEffect).GetCardColors(cardColors, this));

            #endregion

            #endregion

            cardColors = cardColors.Distinct().ToList();

            return cardColors;
        }
    }

    #endregion

    #region evoCosts from entity

    public List<EvoCost> BaseEvoCostsFromEntity => _cEntity_Base.EvoCosts;

    #endregion

    #region evoCosts

    public List<Func<Permanent, int>> EvoCosts(CardEffectCommons.IgnoreRequirement ignore, bool checkAvailability)
    {
        List<Func<Permanent, int>> EvoCosts = new List<Func<Permanent, int>>();

        #region the effects that add evoCosts

        #region the effects of players

        EvoCosts = EvoCosts
            .Concat(
                GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                    .Map(player => player.EffectList(EffectTiming.None))
                    .Flat()
                    .Filter(cardEffect => cardEffect is IAddDigivolutionRequirementEffect && cardEffect.CanUse(null))
                    .Map<ICardEffect, Func<Permanent, int>>(cardEffect =>
                        (targetPermanent) => ((IAddDigivolutionRequirementEffect)cardEffect).GetEvoCost(targetPermanent, this, checkAvailability)))
                    .ToList();

        #endregion

        #region the effects of permanents

        EvoCosts = EvoCosts
            .Concat(
                GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                    .Map(player => player.GetFieldPermanents())
                    .Flat()
                    .Map(permanent => permanent.EffectList(EffectTiming.None))
                    .Flat()
                    .Filter(cardEffect => cardEffect is IAddDigivolutionRequirementEffect && cardEffect.CanUse(null))
                    .Map<ICardEffect, Func<Permanent, int>>(cardEffect =>
                        (targetPermanent) => ((IAddDigivolutionRequirementEffect)cardEffect).GetEvoCost(targetPermanent, this, checkAvailability)))
                    .ToList();

        #endregion

        #region the effects of itself

        if (PermanentOfThisCard() == null || PermanentOfThisCard().DigivolutionCards.Contains(this))
        {
            EvoCosts = EvoCosts
                .Concat(
                    EffectList(EffectTiming.None)
                        .Filter(cardEffect => cardEffect is IAddDigivolutionRequirementEffect && cardEffect.CanUse(null))
                        .Map<ICardEffect, Func<Permanent, int>>(cardEffect =>
                            (targetPermanent) => ((IAddDigivolutionRequirementEffect)cardEffect).GetEvoCost(targetPermanent, this, checkAvailability)))
                        .ToList();
        }

        #endregion

        #endregion

        EvoCosts = EvoCosts
            .Concat(
                BaseEvoCostsFromEntity
                    .Map<EvoCost, Func<Permanent, int>>(evoCost =>
                    (targetPermanent) =>
                    {
                        if (ignore.Equals(CardEffectCommons.IgnoreRequirement.All) && Owner.CanIgnoreDigivolutionRequirement(targetPermanent, this))
                            return evoCost.MemoryCost;

                        if ((ignore.Equals(CardEffectCommons.IgnoreRequirement.Color) && Owner.CanIgnoreDigivolutionRequirement(targetPermanent, this))
                            || targetPermanent.TopCard.CardColors.Contains(evoCost.CardColor))
                        {
                            if ((ignore.Equals(CardEffectCommons.IgnoreRequirement.Level) && Owner.CanIgnoreDigivolutionRequirement(targetPermanent, this))
                                || targetPermanent.Level == evoCost.Level)
                            {
                                return evoCost.MemoryCost;
                            }
                        }

                        return -1;
                    }))
                    .ToList();

        return EvoCosts;
    }

    #endregion

    #region evo cost list

    public List<int> CostList(Permanent targetPermanent, bool ignoreLevel, bool checkAvailability)
    {
        CardEffectCommons.IgnoreRequirement ignore = CardEffectCommons.IgnoreRequirement.None;

        if (ignoreLevel)
            ignore = CardEffectCommons.IgnoreRequirement.Level;

        return EvoCosts(ignore, checkAvailability)
                .Filter(evoCost => evoCost(targetPermanent) >= 0)
                .Map(evoCost => evoCost(targetPermanent));
    }

    #endregion

    #region cost list

    #region cost to pay

    public int PayingCost(SelectCardEffect.Root root, List<Permanent> targetPermanents, bool checkAvailability = false, bool ignoreLevel = false, int FixedCost = -1)
    {
        int baseCost = _cEntity_Base.PlayCost;

        if (targetPermanents != null)
        {
            if (targetPermanents.Count == 1)
            {
                Permanent targetPermanent = targetPermanents[0];

                if (targetPermanent != null)
                {
                    List<int> costList = CostList(targetPermanent, ignoreLevel: ignoreLevel, checkAvailability: checkAvailability);

                    if (costList.Count >= 1)
                    {
                        baseCost = costList.Min();
                    }
                }
            }
        }

        return GetPayingCostWithBaseCost(baseCost, root, targetPermanents, checkAvailability: checkAvailability, FixedCost: FixedCost);
    }

    #endregion

    #region cost that will be paid taking into account baseCost

    public int GetPayingCostWithBaseCost(int baseCost, SelectCardEffect.Root root, List<Permanent> targetPermanents, bool checkAvailability = false, int FixedCost = -1)
    {
        int Cost = FixedCost >= 0 ? FixedCost : baseCost;

        bool isEvolution = targetPermanents != null && targetPermanents.Some((permanent) => permanent != null);

        #region DigiXros

        if (!isEvolution)
        {
            if (HasDigiXros)
            {
                if (Owner.CanReduceCost(null, this))
                {
                    //AI
                    if (!(!Owner.isYou && GManager.instance.IsAI))
                    {
                        if (checkAvailability)
                        {
                            return 0;
                        }
                    }

                    if (!checkAvailability)
                    {
                        SelectDigiXrosClass selectDigiXrosClass = GManager.instance.GetComponent<SelectDigiXrosClass>();

                        if (selectDigiXrosClass != null)
                        {
                            if (selectDigiXrosClass.playCard == this)
                            {
                                Cost -= selectDigiXrosClass.selectedDigicrossCards.Count * digiXrosCondition.reduceCostPerCard;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Assembly

        if (!isEvolution)
        {
            if (HasAssembly)
            {
                if (Owner.CanReduceCost(null, this))
                {
                    //AI
                    if (!(!Owner.isYou && GManager.instance.IsAI))
                    {
                        if (checkAvailability)
                        {
                            return 0;
                        }
                    }

                    if (!checkAvailability)
                    {
                        SelectAssemblyClass selectAssemblyClass = GManager.instance.GetComponent<SelectAssemblyClass>();

                        if (selectAssemblyClass != null)
                        {
                            if (selectAssemblyClass.playCard == this)
                            {
                                if (selectAssemblyClass.selectedAssemblyCards.Count == assemblyCondition.elementCount)
                                    Cost -= assemblyCondition.reduceCost;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        Cost = GetChangedCostItselef(Cost, root, targetPermanents, checkAvailability);

        Cost = GetChangedPayingCost(Cost, root, targetPermanents, checkAvailability);

        if (Cost < 0)
        {
            Cost = 0;
        }

        return Cost;
    }

    #endregion

    #region base play cost from entity

    public int BasePlayCostFromEntity
    {
        get
        {
            return _cEntity_Base.PlayCost;
        }
    }

    #endregion

    #region get card cost of itself (refered by card effects)

    public int GetCostItself => Math.Max(0, GetChangedCostItselef(BasePlayCostFromEntity, SelectCardEffect.Root.None, new List<Permanent>() { PermanentOfThisCard() }));

    #endregion

    #region get card cost of itself taking into account card effects

    public int GetChangedCostItselef(int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents, bool checkAvailability = false)
    {
        if (checkAvailability)
        {
            if (GManager.instance.IsAI)
            {
                if (targetPermanents != null)
                {
                    if (targetPermanents.Some(permanent => permanent != null && permanent.TopCard != null && !permanent.TopCard.Owner.isYou))
                    {
                        return Math.Max(0, Cost);
                    }
                }
            }
        }

        #region card effects that changes card cost

        List<ICardEffect> changeCostCardEffects = new List<ICardEffect>();

        #region the effects of permanents

        changeCostCardEffects = changeCostCardEffects
            .Concat(
            GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                .Map(player => player.GetFieldPermanents())
                .Flat()
                .Map(permanent => permanent.EffectList(EffectTiming.None))
                .Flat()
                .Filter(cardEffect => cardEffect is IChangeCostEffect && cardEffect.CanUse(null)
                    && ((IChangeCostEffect)cardEffect).CardCondition(this)
                    && !(((IChangeCostEffect)cardEffect).IsCheckAvailability() && !checkAvailability)
                    && !((IChangeCostEffect)cardEffect).IsChangePayingCost()))
                .ToList();

        #endregion

        #region the effects of players

        changeCostCardEffects = changeCostCardEffects
            .Concat(
            GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                .Map(player => player.EffectList(EffectTiming.None))
                .Flat()
                .Filter(cardEffect => cardEffect is IChangeCostEffect && cardEffect.CanUse(null)
                    && ((IChangeCostEffect)cardEffect).CardCondition(this)
                    && !(((IChangeCostEffect)cardEffect).IsCheckAvailability() && !checkAvailability)
                    && !((IChangeCostEffect)cardEffect).IsChangePayingCost()))
                .ToList();

        #endregion

        #region the effects of itself

        if (PermanentOfThisCard() == null)
        {
            changeCostCardEffects = changeCostCardEffects
            .Concat(
            EffectList(EffectTiming.None)
            .Filter(cardEffect => cardEffect is IChangeCostEffect && cardEffect.CanUse(null)
                && ((IChangeCostEffect)cardEffect).CardCondition(this)
                && !(((IChangeCostEffect)cardEffect).IsCheckAvailability() && !checkAvailability)
                && !((IChangeCostEffect)cardEffect).IsChangePayingCost()))
            .ToList();
        }

        #endregion

        List<ICardEffect> changeCostCardEffects_NotIsUpDown = changeCostCardEffects
            .Filter(cardEffect => !((IChangeCostEffect)cardEffect).IsUpDown());

        List<ICardEffect> changeCostCardEffects_IsUpDown = changeCostCardEffects
            .Filter(cardEffect => ((IChangeCostEffect)cardEffect).IsUpDown());

        changeCostCardEffects_NotIsUpDown
            .ForEach(cardEffect => Cost = ((IChangeCostEffect)cardEffect).GetCost(Cost, this, root, targetPermanents));

        changeCostCardEffects_IsUpDown
            .ForEach(cardEffect => Cost = ((IChangeCostEffect)cardEffect).GetCost(Cost, this, root, targetPermanents));

        #endregion

        return Math.Max(0, Cost);
    }

    #endregion

    #region get card cost to pay of itself taking into account card effects

    public int GetChangedPayingCost(int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents, bool checkAvailability = false)
    {
        #region card effects that changes card cost to pay

        List<ICardEffect> changePayingCostCardEffects = new List<ICardEffect>();

        #region the effects of permanents

        changePayingCostCardEffects = changePayingCostCardEffects
            .Concat(
            GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                .Map(player => player.GetFieldPermanents())
                .Flat()
                .Map(permanent => permanent.EffectList(EffectTiming.None))
                .Flat()
                .Filter(cardEffect => cardEffect is IChangeCostEffect && cardEffect.CanUse(null)
                    && ((IChangeCostEffect)cardEffect).CardCondition(this)
                    && !(((IChangeCostEffect)cardEffect).IsCheckAvailability() && !checkAvailability)
                    && ((IChangeCostEffect)cardEffect).IsChangePayingCost()))
                .ToList();

        #endregion

        #region the effects of players

        changePayingCostCardEffects = changePayingCostCardEffects
            .Concat(
            GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                .Map(player => player.EffectList(EffectTiming.None))
                .Flat()
                .Filter(cardEffect => cardEffect is IChangeCostEffect && cardEffect.CanUse(null)
                    && ((IChangeCostEffect)cardEffect).CardCondition(this)
                    && !(((IChangeCostEffect)cardEffect).IsCheckAvailability() && !checkAvailability)
                    && ((IChangeCostEffect)cardEffect).IsChangePayingCost()))
                .ToList();

        #endregion

        #region the effects of itself

        if (PermanentOfThisCard() == null)
        {
            changePayingCostCardEffects = changePayingCostCardEffects
            .Concat(
            EffectList(EffectTiming.None)
                .Filter(cardEffect => cardEffect is IChangeCostEffect && cardEffect.CanUse(null)
                    && ((IChangeCostEffect)cardEffect).CardCondition(this)
                    && !(((IChangeCostEffect)cardEffect).IsCheckAvailability() && !checkAvailability)
                    && ((IChangeCostEffect)cardEffect).IsChangePayingCost()))
                .ToList();
        }

        #endregion

        List<ICardEffect> changePayingCostCardEffects_NotIsUpDown = changePayingCostCardEffects
            .Filter(cardEffect => !((IChangeCostEffect)cardEffect).IsUpDown());

        List<ICardEffect> changePayingCostCardEffects_IsUpDown = changePayingCostCardEffects
        .Filter(cardEffect => ((IChangeCostEffect)cardEffect).IsUpDown());

        changePayingCostCardEffects_NotIsUpDown
            .ForEach(cardEffect => Cost = ((IChangeCostEffect)cardEffect).GetCost(Cost, this, root, targetPermanents));

        changePayingCostCardEffects_IsUpDown
            .ForEach(cardEffect => Cost = ((IChangeCostEffect)cardEffect).GetCost(Cost, this, root, targetPermanents));

        #endregion

        return Math.Max(0, Cost);
    }

    #endregion

    #endregion

    #region level

    public int Level => TreatedLevel;

    #endregion

    #region Level

    public int TreatedLevel
    {
        get
        {
            int treatedLevel = 0;

            treatedLevel = _cEntity_Base.Level;

            if (!HasLevel)
                treatedLevel = 1145140;

            #region Check Card Change Level Effects

            foreach (ICardEffect cardEffect in EffectList(EffectTiming.None))
            {
                if (cardEffect is IChangeCardLevelEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        treatedLevel = ((IChangeCardLevelEffect)cardEffect).GetCardLevel(treatedLevel, this);
                    }
                }
            }

            #endregion

            return treatedLevel;
        }
    }

    #endregion

    #region effect list

    public List<ICardEffect> EffectList(EffectTiming timing)
    {
        return EffectList_ForCard(timing, this);
    }

    #endregion

    #region effect list for 1 card

    public List<ICardEffect> EffectList_ForCard(EffectTiming timing, CardSource cardSource)
    {
        List<ICardEffect> _EffectList = cEntity_EffectController
            .GetCardEffects(timing, cardSource)
            .Filter(cardEffect => cardEffect != null);

        foreach (ICardEffect cardEffect in _EffectList)
        {
            if (cardEffect.EffectSourceCard == null)
            {
                cardEffect.SetEffectSourceCard(this);
            }
        }

        return _EffectList;
    }

    #endregion

    #region effect list except added effects

    public List<ICardEffect> EffectList_ExceptAddedEffects(EffectTiming timing)
    {
        return EffectList_ForCard_ExceptAddedEffects(timing, this);
    }

    #endregion

    #region effect list for 1 card

    public List<ICardEffect> EffectList_ForCard_ExceptAddedEffects(EffectTiming timing, CardSource cardSource)
    {
        List<ICardEffect> _EffectList = cEntity_EffectController
            .GetCardEffects_ExceptAddedEffects(timing, cardSource)
            .Filter(cardEffect => cardEffect != null);

        foreach (ICardEffect cardEffect in _EffectList)
        {
            if (cardEffect.EffectSourceCard == null)
            {
                cardEffect.SetEffectSourceCard(this);
            }
        }

        return _EffectList;
    }

    #endregion

    #region whether this card can declare skill

    public bool CanDeclareSkill => CanDeclareSkillList.Count > 0;

    #endregion

    #region effect list that this card can declare

    public List<ICardEffect> CanDeclareSkillList
    {
        get
        {
            return EffectList(EffectTiming.OnDeclaration)
            .Filter(cardEffect => cardEffect is ActivateICardEffect && cardEffect.CanUse(null));
        }
    }

    #endregion

    #region whether this card can not be affected by the effect

    public bool CanNotBeAffected(ICardEffect _cardEffect)
    {
        if (_cardEffect == null) return false;

        #region the effects of permanents

        if (GManager.instance.turnStateMachine.gameContext.Players
            .Map(player => player.GetFieldPermanents())
            .Flat()
            .Map(permanent => permanent.EffectList(EffectTiming.None))
            .Flat()
            .Some(cardEffect => cardEffect is ICanNotAffectedEffect
                && cardEffect.CanUse(null)
                && ((ICanNotAffectedEffect)cardEffect).CanNotAffect(this, _cardEffect)))
        {
            return true;
        }

        #endregion

        #region the effects of players

        if (GManager.instance.turnStateMachine.gameContext.Players
                .Map(player => player.EffectList(EffectTiming.None))
                .Flat()
                .Some(cardEffect => cardEffect is ICanNotAffectedEffect
                    && cardEffect.CanUse(null)
                    && ((ICanNotAffectedEffect)cardEffect).CanNotAffect(this, _cardEffect)))
        {
            return true;
        }

        #endregion

        #region the effects of itself

        if (PermanentOfThisCard() == null)
        {
            if (EffectList(EffectTiming.None)
                    .Some(cardEffect => cardEffect is ICanNotAffectedEffect
                        && cardEffect.CanUse(null)
                        && ((ICanNotAffectedEffect)cardEffect).CanNotAffect(this, _cardEffect)))
            {
                return true;
            }
        }

        #endregion

        return false;
    }

    #endregion

    #region whether this card can be played at the frame

    public bool CanPlayCardTargetFrame(FieldCardFrame frame, bool PayCost, ICardEffect cardEffect, SelectCardEffect.Root root = SelectCardEffect.Root.Hand, int fixedCost = -1, bool isBreedingArea = false, CardEffectCommons.IgnoreRequirement ignore = CardEffectCommons.IgnoreRequirement.None)
    {
        bool isBattleAreaFrame = FieldCardFrame.isBattleAreaFrameID(frame.FrameID);

        if (isBreedingArea)
        {
            if (isBattleAreaFrame)
            {
                return false;
            }
        }

        if (PermanentOfThisCard() != null)
        {
            if (this == PermanentOfThisCard().TopCard)
            {
                if (PermanentOfThisCard().HasNoDigivolutionCards)
                {
                    return false;
                }
            }
        }

        if (frame.player != Owner)
        {
            return false;
        }

        if (PayCost && frame.GetFramePermanent() == null)
        {
            int cost = PayingCost(root, new List<Permanent>() { frame.GetFramePermanent() }, checkAvailability: true, FixedCost: fixedCost);

            if (Owner.MaxMemoryCost < cost)
            {
                return false;
            }
        }

        if (frame.GetFramePermanent() != null)
        {
            if (!CanEvolve(frame.GetFramePermanent(), true, ignore))
            {
                return false;
            }
        }
        else
        {
            if (!CanEnterField(cardEffect))
            {
                return false;
            }
        }

        if (!isBattleAreaFrame && frame.GetFramePermanent() == null && !isBreedingArea)
        {
            return false;
        }

        /*if (IsDigiEgg)
        {
            if (isBattleAreaFrame)
            {
                return false;
            }
        }
        else
        {
            if (!isBattleAreaFrame && frame.GetFramePermanent() == null && !isBreedingArea)
            {
                return false;
            }
        }*/

        return true;
    }

    #endregion

    #region whether there is at least 1 frame where this card can be placed

    public bool CanPutFieldThisPermanentCard(bool PayCost, ICardEffect cardEffect, bool isBreedingArea = false)
    {
        return Owner.fieldCardFrames.Some((frame) => CanPlayCardTargetFrame(frame, PayCost, cardEffect, isBreedingArea: isBreedingArea));
    }

    #endregion

    #region whether this card can be placed on the field by the effect

    public bool CanEnterField(ICardEffect _cardEffect)
    {
        #region the effects of permanents

        if (GManager.instance.turnStateMachine.gameContext.Players
            .Map(player => player.GetFieldPermanents())
            .Flat()
            .Map(permanent => permanent.EffectList(EffectTiming.None))
            .Flat()
            .Some(cardEffect => cardEffect is ICanNotPutFieldEffect
                && cardEffect.CanUse(null)
                && ((ICanNotPutFieldEffect)cardEffect).CanNotPutField(this, _cardEffect)))
        {
            return false;
        }

        #endregion

        #region the effects of players

        if (GManager.instance.turnStateMachine.gameContext.Players
                .Map(player => player.EffectList(EffectTiming.None))
                .Flat()
                .Some(cardEffect => cardEffect is ICanNotPutFieldEffect
                    && cardEffect.CanUse(null)
                    && ((ICanNotPutFieldEffect)cardEffect).CanNotPutField(this, _cardEffect)))
        {
            return false;
        }

        #endregion

        #region the effects of itself

        if (PermanentOfThisCard() == null)
        {
            if (EffectList(EffectTiming.None)
                    .Some(cardEffect => cardEffect is ICanNotPutFieldEffect
                        && cardEffect.CanUse(null)
                        && ((ICanNotPutFieldEffect)cardEffect).CanNotPutField(this, _cardEffect)))
            {
                return false;
            }
        }

        #endregion

        return true;
    }

    #endregion

    #region whether the permanent can digivolve into this card

    public bool CanEvolve(Permanent targetPermanent, bool checkAvailability, CardEffectCommons.IgnoreRequirement ignore = CardEffectCommons.IgnoreRequirement.None)
    {
        if (targetPermanent != null)
        {
            if (CanNotEvolve(targetPermanent))
            {
                return false;
            }

            if (targetPermanent.TopCard != null)
            {
                foreach (Func<Permanent, int> EvoCost in EvoCosts(ignore, checkAvailability))
                {
                    if (EvoCost(targetPermanent) >= 0)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    #endregion

    #region whether the permanent's digivolution into this card is prohibited

    public bool CanNotEvolve(Permanent targetPermanent)
    {
        if (targetPermanent.IsToken)
        {
            return true;
        }

        if (IsToken)
        {
            return true;
        }

        #region card effects that can't digivolve

        #region the effects of permanents

        if (GManager.instance.turnStateMachine.gameContext.Players
            .Map(player => player.GetFieldPermanents())
            .Flat()
            .Map(permanent => permanent.EffectList(EffectTiming.None))
            .Flat()
            .Some(cardEffect => cardEffect is ICanNotDigivolveEffect
                && cardEffect.CanUse(null)
                && ((ICanNotDigivolveEffect)cardEffect).CanNotEvolve(targetPermanent, this)))
        {
            return true;
        }

        #endregion

        #region the effects of players

        if (GManager.instance.turnStateMachine.gameContext.Players
                .Map(player => player.EffectList(EffectTiming.None))
                .Flat()
                .Some(cardEffect => cardEffect is ICanNotDigivolveEffect
                    && cardEffect.CanUse(null)
                    && ((ICanNotDigivolveEffect)cardEffect).CanNotEvolve(targetPermanent, this)))
        {
            return true;
        }

        #endregion

        #region the effects of itself

        if (PermanentOfThisCard() == null)
        {
            if (EffectList(EffectTiming.None)
                    .Some(cardEffect => cardEffect is ICanNotDigivolveEffect
                        && cardEffect.CanUse(null)
                        && ((ICanNotDigivolveEffect)cardEffect).CanNotEvolve(targetPermanent, this)))
            {
                return true;
            }
        }

        #endregion

        #endregion

        return false;
    }

    #endregion

    #region ENG card name from entity

    public string BaseENGCardNameFromEntity => _cEntity_Base.CardName_ENG;

    #endregion

    #region JPN card name from entity

    public string BaseJPNCardNameFromEntity => _cEntity_Base.CardName_ENG;

    #endregion

    #region base card names

    public List<string> BaseCardNames
    {
        get
        {
            List<string> baseCardNames = new List<string>() { BaseENGCardNameFromEntity };

            Permanent thisPermanent = PermanentOfThisCard();
            bool isPermanent = PermanentOfThisCard() != null;
            bool isDigivolutionCard = isPermanent && thisPermanent.DigivolutionCards.Contains(this);

            #region card effects that change base card names

            if (isDigivolutionCard)
            {
                EffectList_ExceptAddedEffects(EffectTiming.None)
                    .Filter(cardEffect => cardEffect is IChangeBaseCardNameEffect && cardEffect.CanUse(null))
                    .ForEach(cardEffect => baseCardNames = ((IChangeBaseCardNameEffect)cardEffect).ChangeBaseCardNames(baseCardNames, this));
            }
            else
            {
                #region the effects of itself

                if (PermanentOfThisCard() == null)
                {
                    EffectList(EffectTiming.None)
                        .Filter(cardEffect => cardEffect is IChangeBaseCardNameEffect && cardEffect.CanUse(null))
                        .ForEach(cardEffect => baseCardNames = ((IChangeBaseCardNameEffect)cardEffect).ChangeBaseCardNames(baseCardNames, this));
                }

                #endregion

                #region the effects of permanents

                GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                    .Map(player => player.GetFieldPermanents())
                    .Flat()
                    .Map(permanent => permanent.EffectList(EffectTiming.None))
                    .Flat()
                    .Filter(cardEffect => cardEffect is IChangeBaseCardNameEffect && cardEffect.CanUse(null))
                    .ForEach(cardEffect => baseCardNames = ((IChangeBaseCardNameEffect)cardEffect).ChangeBaseCardNames(baseCardNames, this));

                #endregion

                #region the effects of players

                GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                    .Map(player => player.EffectList(EffectTiming.None))
                    .Flat()
                    .Filter(cardEffect => cardEffect is IChangeBaseCardNameEffect && cardEffect.CanUse(null))
                    .ForEach(cardEffect => baseCardNames = ((IChangeBaseCardNameEffect)cardEffect).ChangeBaseCardNames(baseCardNames, this));

                #endregion
            }

            #endregion

            return baseCardNames;
        }
    }

    #endregion

    #region card names

    public List<string> CardNames
    {
        get
        {
            List<string> cardNames = BaseCardNames.Clone();

            #region the effects of itself

            EffectList_ExceptAddedEffects(EffectTiming.None)
            .Filter(cardEffect => cardEffect != null)
            .Filter(cardEffect => cardEffect is IChangeCardNamesEffect && cardEffect.CanUse(null))
            .ForEach(cardEffect => cardNames = ((IChangeCardNamesEffect)cardEffect).ChangeCardNames(cardNames, this));

            #endregion

            return cardNames.Distinct().ToList();
        }
    }

    #endregion

    #region Whether target other card's name has same name as this

    public bool HasSameCardName(CardSource cardSource) => cardSource.CardNames.Count((cardName) => EqualsCardName(cardName)) >= 1;

    //public bool HasSameCardName(CardSource cardSource) => cardSource.CardNames.Count((cardName) => CardNames.Contains(cardName)) >= 1;

    #endregion

    #region whether this card has at least 1 card name that equals the string

    /// <summary>
    /// Used to check if specified name is exactly in cards name values ("dra" is not "Commandramon")
    /// </summary>
    /// <param name="string">value to check for</param>
    /// <author>Mike Bunch</author>
    public bool EqualsCardName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        string replaced = name.Replace(" ", "");
        string lower = name.ToLower();

        return CardNames.Some((cardName) =>
        cardName.Equals(name)
        || cardName.Equals(replaced)
        || cardName.Replace(" ", "").Equals(replaced)
        || cardName.Equals(lower)
        || cardName.ToLower().Equals(lower));
    }

    #endregion

    #region whether this card has at least 1 card name that contains the string

    /// <summary>
    /// Used to check if specified trait is within cards name values ("dra" is in "Commandramon")
    /// </summary>
    /// <param trait="string">value to check for</param>
    /// <author>Mike Bunch</author>
    public bool ContainsCardName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        string replaced = name.Replace(" ", "");
        string lower = name.ToLower();

        return CardNames.Some((cardName) =>
        cardName.Contains(name)
        || cardName.Contains(replaced)
        || cardName.Contains(lower)
        || cardName.ToLower().Contains(lower));
    }

    #endregion

    #region whether this card has at least 1 card name that contains "Greymon"

    public bool HasGreymonName
    {
        get
        {
            if (CardNames.Some((cardName) => (cardName.Contains("Greymon") || cardName.Contains("greymon"))
            && cardName != "DoruGreymon"
            && cardName != "BurningGreymon"
            && cardName != "DexDoruGreymon"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has at least 1 card name that contains "Garurumon"

    public bool HasGarurumonName
    {
        get
        {
            if (CardNames.Some((cardName) => (cardName.Contains("Garurumon") || cardName.Contains("garurumon"))
            && cardName != "KendoGarurumon"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has at least 1 card name that contains "Impmon"

    public bool HasImpmonName
    {
        get
        {
            if (CardNames.Some((cardName) => (cardName.Contains("Impmon") || cardName.Contains("impmon"))
            && cardName != "Blimpmon"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has at least 1 card name that contains "Dramon"

    public bool HasDramonName
    {
        get
        {
            if (CardNames.Some((cardName) => (cardName.Contains("Dramon") || cardName.Contains("dramon"))
            && cardName != "Indramon"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has at least 1 card name that contains "AAntibody"

    public bool HasXAntiBodyName => CardNames.Some(DataBase.IsContainingXAntibodyString);

    #endregion

    #region whether this card has at least 1 trait that equals the string

    /// <summary>
    /// Used to check if specified trait is exactly in cards trait values ("Sky Dragon" is not "Dragon")
    /// </summary>
    /// <param trait="string">value to check for</param>
    /// <author>Mike Bunch</author>
    public bool EqualsTraits(string trait)
    {
        if (string.IsNullOrEmpty(trait))
            return false;

        string replaced = trait.Replace(" ", "").ToLower();

        return CardTraits.Some(cardTrait => cardTrait.Equals(trait) || cardTrait.ToLower().Equals(replaced));
    }

    #endregion

    #region whether this card has at least 1 trait that contains the string

    /// <summary>
    /// Used to check if specified trait is within cards trait values ("Sky Dragon" contains "Dragon")
    /// </summary>
    /// <param trait="string">value to check for</param>
    /// <author>Mike Bunch</author>
    public bool ContainsTraits(string trait)
    {
        if (string.IsNullOrEmpty(trait))
            return false;

        string replaced = trait.Replace(" ", "").ToLower();

        return CardTraits.Some(cardTrait => cardTrait.Contains(trait) || cardTrait.ToLower().Contains(replaced));
    }

    #endregion

    #region whether this card has at least 1 trait that contains "Bird"

    public bool HasBirdTraits
    {
        get
        {
            if (ContainsTraits("Avian"))
            {
                return true;
            }

            if (ContainsTraits("Bird"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has at least 1 trait that contains "Beast"

    public bool HasBeastTraits
    {
        get
        {
            if (ContainsTraits("Beast"))
            {
                return true;
            }

            if (ContainsTraits("Animal") && !ContainsTraits("Sea"))
            {
                return true;
            }

            if (ContainsTraits("Sovereign"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has at least 1 trait that contains "Plant"

    public bool HasPlantTraits
    {
        get
        {
            if (ContainsTraits("Vegetation"))
            {
                return true;
            }

            if (ContainsTraits("Plant"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has at least 1 trait that contains "Fairy"

    public bool HasFairyTraits
    {
        get
        {
            if (ContainsTraits("Fairy"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has at least 1 trait that contains "Dragon"

    public bool HasDragonTraits
    {
        get
        {
            if (ContainsTraits("Dragon"))
            {
                return true;
            }

            if (ContainsTraits("saur"))
            {
                return true;
            }

            if (ContainsTraits("Ceratopsian"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has at least 1 trait that contains "Aqua"

    public bool HasAquaTraits
    {
        get
        {
            if (ContainsTraits("Aqua"))
            {
                return true;
            }

            if (ContainsTraits("Sea Animal"))
            {
                return true;
            }

            if (ContainsTraits("SeaAnimal"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has at least 1 trait that contains "Angel"

    public bool HasAngelTraits
    {
        get
        {
            if (CardTraits.Count((trait) => (trait.Contains("Angel") || trait.Contains("angel")) && trait != "Three Great Angels" && trait != "ThreeGreatAngels") >= 1)
            {
                return true;
            }

            if (ContainsTraits("Cherub"))
            {
                return true;
            }

            if (ContainsTraits("Throne"))
            {
                return true;
            }

            if (ContainsTraits("Authority"))
            {
                return true;
            }

            if (ContainsTraits("Seraph"))
            {
                return true;
            }

            if (ContainsTraits("Virtue"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has 1 of the "Angel", "Archangel" or "Three Great Angels" trait

    public bool HasAngelTraitRestrictive
    {
        get
        {
            if (ContainsTraits("Three Great Angels") || ContainsTraits("ThreeGreatAngels"))
            {
                return true;
            }

            if (ContainsTraits("Archangel"))
            {
                return true;
            }

            if (CardTraits.Count(trait =>
                    (trait.Contains("Angel") || trait.Contains("angel"))
                    && trait != "Archangel"
                    && trait != "Fallen Angel" && trait != "FallenAngel"
                    && trait != "Three Great Angels" && trait != "ThreeGreatAngels") >= 1)
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has at least 1 trait that contains "Avian", "Bird", "Beast", "Animal", "Sovereign", other than "Sea Animal"

    public bool HasAvianBeastAnimalTraits
    {
        get
        {
            if (ContainsTraits("Avian"))
                return true;

            if (ContainsTraits("Bird"))
                return true;

            if (ContainsTraits("Beast"))
                return true;

            if (ContainsTraits("Animal") && !ContainsTraits("Sea Animal"))
                return true;

            if (ContainsTraits("Sovereign"))
                return true;

            return false;
        }
    }

    #endregion

    #region whether this card has at least 1 trait that contains "Hybrid", "Ten Warriors"

    public bool HasHybridTenWarriorsTraits
    {
        get
        {
            if (ContainsTraits("Hybrid"))
                return true;

            if (ContainsTraits("Ten Warriors"))
                return true;

            return false;
        }
    }

    #endregion

    #region whether this card has "XAntibody" trait

    public bool HasXAntibodyTraits => CardTraits.Some(DataBase.IsXAntibodyString);

    #endregion

    #region whether this card has "Royal Knight" trait

    public bool HasRoyalKnightTraits
    {
        get
        {
            return EqualsTraits("Royal Knight");
        }
    }

    #endregion

    #region whether this card has "SoC" trait

    public bool HasSocTraits
    {
        get
        {
            if (CardTraits.Contains("SoC"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has "D-Brigade"/"DigiPolice trait

    public bool HasDBrigadeorDigiPoliceTraits
    {
        get
        {
            if (CardTraits.Contains("D-Brigade"))
            {
                return true;
            }

            if (CardTraits.Contains("DigiPolice"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has at least 1 trait that contains "Beast Dragon"

    public bool HasBeastDragonTraits
    {
        get
        {
            if (ContainsTraits("BeastDragon"))
            {
                return true;
            }

            if (ContainsTraits("Beast Dragon"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has at least 1 trait that contains "DigiPolice"

    public bool HasDigiPoliceTraits
    {
        get
        {
            if (ContainsTraits("DigiPolice"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has at least 1 trait that contains "Light Fang" or "Night Claw"

    public bool HasLightFangNightClawTraits => ContainsTraits("Light Fang") || ContainsTraits("Night Claw");

    #endregion

    #region whether this card has at least 1 trait that contains "Light Fang/Night Claw"

    public bool HasLightFangOrNightClawTraits
    {
        get
        {
            if (ContainsTraits("Light Fang"))
            {
                return true;
            }

            if (ContainsTraits("Night Claw"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region [Onmyōjutsu] or [Plug-In] trait

    public bool HasOnmyoOrPluginTraits
    {
        get
        {
            if (EqualsTraits("Plug-In"))
            {
                return true;
            }
            if (EqualsTraits("Onmyōjutsu"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has the string in text

    public bool HasText(string text)
    {
        if (String.IsNullOrEmpty(text)) return false;

        text = DataBase.ReplaceToASCII(text);
        string replaced = text.Replace(" ", "");
        string lower = text.ToLower();

        List<string> checkStrings = new List<string>()
        {
            DataBase.ReplaceToASCII(_cEntity_Base.CardName_ENG),
            DataBase.ReplaceToASCII(_cEntity_Base.EffectDiscription_ENG),
            DataBase.ReplaceToASCII(_cEntity_Base.InheritedEffectDiscription_ENG),
            DataBase.ReplaceToASCII(_cEntity_Base.SecurityEffectDiscription_ENG),
        };

        foreach (string attribute in _cEntity_Base.Attribute_ENG)
            checkStrings.Add(DataBase.ReplaceToASCII(attribute));

        foreach (string attribute in _cEntity_Base.Type_ENG)
            checkStrings.Add(DataBase.ReplaceToASCII(attribute));

        if (jogressCondition.Count > 0)
        {
            foreach (JogressCondition jogress in jogressCondition)
            {
                foreach (JogressConditionElement element in jogress.elements)
                    checkStrings.Add(element.SelectMessage);
            }
        }

        foreach (string checkString in checkStrings)
        {
            if (!string.IsNullOrEmpty(checkString))
            {
                if (checkString.Contains(text))
                {
                    return true;
                }

                if (checkString.Contains(replaced))
                {
                    return true;
                }

                if (checkString.Contains(lower))
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region whether this card has <Save> in text

    public bool HasSaveText => HasText("<Save>");

    #endregion

    #region whether this card has "Pulsemon" in text

    public bool HasPulsemonText => HasText("Pulsemon");

    #endregion

    #region card names checked when digixros

    public List<string> CardNames_DigiXros
    {
        get
        {
            List<string> cardNames_DigiXros = CardNames.Clone();

            #region the effects of itself

            EffectList(EffectTiming.None)
                .Filter(cardEffect => cardEffect is IChangeCardNamesForDigiXrosEffect && cardEffect.CanUse(null))
                .ForEach(cardEffect => cardNames_DigiXros = ((IChangeCardNamesForDigiXrosEffect)cardEffect).ChangeCardNamesForDigiXros(cardNames_DigiXros, this));

            #endregion

            return cardNames_DigiXros;
        }
    }

    #endregion

    #region card level checked when Assembly

    public List<int> Level_Assembly
    {
        get
        {
            List<int> cardLevel_Assembly = new List<int>();
            cardLevel_Assembly.Add(TreatedLevel);

            #region the effects of itself

            EffectList(EffectTiming.None)
                .Filter(cardEffect => cardEffect is IChangeCardLevelForAssemblyEffect && cardEffect.CanUse(null))
                .ForEach(cardEffect => cardLevel_Assembly = ((IChangeCardLevelForAssemblyEffect)cardEffect).ChangeCardLevelForAssembly(cardLevel_Assembly, this));

            #endregion

            return cardLevel_Assembly;
        }
    }

    #endregion

    #region whether this card has at least 1 card name checked when digixros that equals the string

    /// <summary>
    /// Used to check if specified name is exactly in cards name values ("dra" is not "Commandramon")
    /// </summary>
    /// <param name="string">value to check for</param>
    /// <author>Mike Bunch</author>
    public bool EqualsCardNameDigiXros(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        string replaced = name.Replace(" ", "");
        string lower = name.ToLower();

        return CardNames_DigiXros.Some((cardName) =>
        cardName.Equals(name)
        || cardName.Equals(replaced)
        || cardName.Equals(lower)
        || cardName.ToLower().Equals(lower));
    }

    #endregion

    #region whether this card has at least 1 card name that contains the string

    /// <summary>
    /// Used to check if specified trait is within cards name values ("dra" is in "Commandramon")
    /// </summary>
    /// <param trait="string">value to check for</param>
    /// <author>Mike Bunch</author>
    public bool ContainsCardNameDigiXros(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        string replaced = name.Replace(" ", "");
        string lower = name.ToLower();

        return CardNames_DigiXros.Some((cardName) =>
        cardName.Contains(name)
        || cardName.Contains(replaced)
        || cardName.Contains(lower)
        || cardName.ToLower().Contains(lower));
    }

    #endregion

    #region preferered frame to play

    public FieldCardFrame PreferredFrame()
    {
        FieldCardFrame PreferredFrame = Owner.fieldCardFrames[0];

        List<FieldCardFrame> fieldCardFrames =
        Owner.fieldCardFrames
            .Filter(fieldCardFrame => fieldCardFrame.IsBattleAreaFrame()
                && fieldCardFrame.IsEmptyFrame());

        if (_cEntity_Base != null)
        {
            if (IsDigimon || IsDigiEgg)
            {
                if (Owner.isYou)
                {
                    fieldCardFrames = fieldCardFrames
                        .OrderByDescending(value => value.Frame.transform.parent.localPosition.y)
                        .ThenBy(value => Mathf.Abs(value.Frame.transform.parent.localPosition.x))
                        .ThenBy(value => value.Frame.transform.parent.localPosition.x)
                        .ToList();
                }
                else
                {
                    fieldCardFrames = fieldCardFrames
                        .OrderBy(value => value.Frame.transform.parent.localPosition.y)
                        .ThenBy(value => Mathf.Abs(value.Frame.transform.parent.localPosition.x))
                        .ThenByDescending(value => value.Frame.transform.parent.localPosition.x)
                        .ToList();

                    int[] correctOrder = new int[] { 4, 3, 5, 2, 6, 1, 7, 0, 8, 11, 10, 12, 9, 13, 14, 15, };

                    List<FieldCardFrame> correctOrderFrames = new List<FieldCardFrame>();

                    for (int i = 0; i < correctOrder.Length; i++)
                    {
                        foreach (FieldCardFrame frame in fieldCardFrames)
                        {
                            if (frame.FrameID == correctOrder[i])
                            {
                                correctOrderFrames.Add(frame);
                            }
                        }
                    }

                    fieldCardFrames = correctOrderFrames;
                }
            }
            else if (IsTamer || IsOption)
            {
                if (Owner.isYou)
                {
                    fieldCardFrames = fieldCardFrames
                        .OrderBy(value => value.Frame.transform.parent.localPosition.y)
                        .ThenBy(value => Mathf.Abs(value.Frame.transform.parent.localPosition.x))
                        .ThenBy(value => value.Frame.transform.parent.localPosition.x)
                        .ToList();
                }
                else
                {
                    fieldCardFrames = fieldCardFrames
                        .OrderByDescending(value => value.Frame.transform.parent.localPosition.y)
                        .ThenBy(value => Mathf.Abs(value.Frame.transform.parent.localPosition.x))
                        .ThenByDescending(value => value.Frame.transform.parent.localPosition.x)
                        .ToList();
                }
            }

            if (fieldCardFrames.Count >= 1)
            {
                PreferredFrame = fieldCardFrames[0];
            }
        }

        return PreferredFrame;
    }

    #endregion

    #region Whether this card has DP

    public bool HasDP => IsDigimon || _cEntity_Base.DP > 0 || BaseDP > 0;

    #endregion

    #region base card DP

    public int BaseCardDP => _cEntity_Base.DP;
    public int BaseDP = 0;

    #endregion

    #region DP

    public int CardDP
    {
        get
        {
            int cardDP = -1;

            if (HasDP)
            {
                cardDP = BaseCardDP;
                cardDP += BaseDP;

                #region card effects that change card DP

                List<ICardEffect> changeCardDPCardEffects = new List<ICardEffect>();

                #region the effects of permanents

                changeCardDPCardEffects = changeCardDPCardEffects
                    .Concat(
                    GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                        .Map(player => player.GetFieldPermanents())
                        .Flat()
                        .Map(permanent => permanent.EffectList(EffectTiming.None))
                        .Flat()
                        .Filter(cardEffect => cardEffect is IChangeCardDPEffect && cardEffect.CanUse(null)
                            && ((IChangeCardDPEffect)cardEffect).CardCondition(this)))
                        .ToList();

                #endregion

                #region the effects of players

                changeCardDPCardEffects = changeCardDPCardEffects
                    .Concat(
                    GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer
                        .Map(player => player.EffectList(EffectTiming.None))
                        .Flat()
                        .Filter(cardEffect => cardEffect is IChangeCardDPEffect && cardEffect.CanUse(null)
                            && ((IChangeCardDPEffect)cardEffect).CardCondition(this)))
                        .ToList();

                #endregion

                List<ICardEffect> cardEffects_ChangeDP_IsUpDown = changeCardDPCardEffects
                    .Filter(cardEffect => ((IChangeCardDPEffect)cardEffect).IsUpDown());

                List<ICardEffect> cardEffects_ChangeDP_NotIsUpDown = changeCardDPCardEffects
                    .Filter(cardEffect => !((IChangeCardDPEffect)cardEffect).IsUpDown());

                cardEffects_ChangeDP_IsUpDown
                    .ForEach(cardEffect => cardDP = ((IChangeCardDPEffect)cardEffect).GetDP(cardDP, this));

                cardEffects_ChangeDP_NotIsUpDown
                    .ForEach(cardEffect => cardDP = ((IChangeCardDPEffect)cardEffect).GetDP(cardDP, this));

                #endregion
            }

            return Math.Max(0, cardDP);
        }
    }

    #endregion

    #region Link DP

    public int LinkDP => _cEntity_Base.LinkDP;

    #endregion

    #region Set DP - Must be used in conjunction with when removed field

    public void SetDP(int value)
    {
        BaseDP = value;
    }

    #endregion

    #region whether this card is token

    public bool IsToken { get; private set; } = false;

    public void SetIsToken(bool isToken) => IsToken = isToken;

    #endregion

    #region Will this card be trashed from sources

    public bool willBeRemoveSources { get; set; } = false;

    #endregion

    #region whether this card can not be trashed from digivolution cards

    public bool CanNotTrashFromDigivolutionCards(ICardEffect _cardEffect)
    {
        if (willBeRemoveSources)
            return true;

        #region the effects of permanents

        if (GManager.instance.turnStateMachine.gameContext.Players
            .Map(player => player.GetFieldPermanents())
            .Flat()
            .Map(permanent => permanent.EffectList(EffectTiming.None))
            .Flat()
            .Some(cardEffect => cardEffect is ICanNotTrashFromDigivolutionCardsEffect
            && cardEffect.CanUse(null)
            && ((ICanNotTrashFromDigivolutionCardsEffect)cardEffect).CanNotTrashFromDigivolutionCards(this, _cardEffect)))
        {
            return true;
        }

        #endregion

        #region the effects of players

        if (GManager.instance.turnStateMachine.gameContext.Players
                .Map(player => player.EffectList(EffectTiming.None))
                .Flat()
                .Some(cardEffect => cardEffect is ICanNotTrashFromDigivolutionCardsEffect
                && cardEffect.CanUse(null)
                && ((ICanNotTrashFromDigivolutionCardsEffect)cardEffect).CanNotTrashFromDigivolutionCards(this, _cardEffect)))
        {
            return true;
        }

        #endregion

        #region the effects of itself

        if (PermanentOfThisCard() == null)
        {
            if (EffectList(EffectTiming.None)
                    .Some(cardEffect => cardEffect is ICanNotTrashFromDigivolutionCardsEffect
                    && cardEffect.CanUse(null)
                    && ((ICanNotTrashFromDigivolutionCardsEffect)cardEffect).CanNotTrashFromDigivolutionCards(this, _cardEffect)))
            {
                return true;
            }
        }

        #endregion

        return false;
    }

    #endregion

    #region whether this card has [Blocker]

    public bool HasBlocker =>
        EffectList(EffectTiming.None)
            .Some(cardEffect => cardEffect is BlockerClass
                && !cardEffect.IsInheritedEffect && !cardEffect.IsSecurityEffect);

    #endregion

    #region whether this card has [DigiBurst]

    public bool HasDigiBurst
    {
        get
        {
            if (_cEntity_Base != null)
            {
                if (!string.IsNullOrEmpty(_cEntity_Base.EffectDiscription_JPN))
                {
                    bool result = Regex.IsMatch(_cEntity_Base.EffectDiscription_JPN, DataBase.DigiburstRegex);

                    if (result)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    #endregion

    #region whether this card has [DigiXros]

    public bool HasDigiXros => digiXrosCondition != null;

    #endregion

    #region whether this card has [Assembly]

    public bool HasAssembly => assemblyCondition != null;

    #endregion

    #region traits

    public List<string> CardTraits
    {
        get
        {
            List<string> traits =
                _cEntity_Base.Form_ENG
                    .Filter(s => !string.IsNullOrEmpty(s))
                .Concat(_cEntity_Base.Attribute_ENG
                    .Filter(s => !string.IsNullOrEmpty(s)))
                .Concat(_cEntity_Base.Type_ENG
                    .Filter(s => !string.IsNullOrEmpty(s)))
                .ToList();

            #region the effects of itself

            EffectList(EffectTiming.None)
                .Filter(cardEffect => cardEffect is IChangeTraitsEffect && cardEffect.CanUse(null))
                .ForEach(cardEffect => traits = ((IChangeTraitsEffect)cardEffect).ChangTraits(traits, this));

            #endregion

            return traits;
        }
    }

    #endregion

    #region whether this card has [On Deletion] effect

    public bool HasOnDeletionEffect =>
        EffectList(EffectTiming.OnDestroyedAnyone)
            .Some(cardEffect => cardEffect is ActivateICardEffect
                && !cardEffect.IsInheritedEffect && !cardEffect.IsSecurityEffect
                && cardEffect.IsOnDeletion);

    #endregion

    #region whether this card has [On Play] effect

    public bool HasOnPlayEffect =>
        EffectList(EffectTiming.OnEnterFieldAnyone)
            .Some(cardEffect => cardEffect is ActivateICardEffect
                && !cardEffect.IsInheritedEffect && !cardEffect.IsSecurityEffect
                && cardEffect.IsOnPlay);

    #endregion

    #region whether this card has [When Digivolving] effect

    public bool HasWhenDigivolvingEffect =>
        EffectList(EffectTiming.OnEnterFieldAnyone)
            .Some(cardEffect => cardEffect is ActivateICardEffect
                && !cardEffect.IsInheritedEffect && !cardEffect.IsSecurityEffect
                && cardEffect.IsWhenDigivolving);

    #endregion

    #region whether this card has [Digisorption]

    public bool HasDigisorption =>
        EffectList(EffectTiming.BeforePayCost)
            .Some(cardEffect => cardEffect is ActivateICardEffect
                && !cardEffect.IsInheritedEffect && !cardEffect.IsSecurityEffect
                && !string.IsNullOrEmpty(cardEffect.EffectDiscription)
                && cardEffect.EffectDiscription.Contains("Digisorption -"));

    #endregion

    #region whether this card has [Blitz]

    public bool HasBlitz =>
        EffectList(EffectTiming.OnEnterFieldAnyone)
            .Some(cardEffect => cardEffect is ActivateICardEffect
                && !cardEffect.IsInheritedEffect && !cardEffect.IsSecurityEffect
                && (cardEffect.CanTrigger(CardEffectCommons.WhenDigivolvingCheckHashtableOfCard(this)) || cardEffect.CanTrigger(CardEffectCommons.OnPlayCheckHashtableOfCard(this)))
                && !string.IsNullOrEmpty(cardEffect.EffectDiscription)
                && cardEffect.EffectDiscription.Contains("Blitz"));

    #endregion

    #region whether this card has [Retaliation]

    public bool HasRetaliation =>
        EffectList(EffectTiming.OnDestroyedAnyone)
            .Some(cardEffect => cardEffect is ActivateICardEffect
                && !cardEffect.IsInheritedEffect && !cardEffect.IsSecurityEffect
                && cardEffect.CanTrigger(CardEffectCommons.OnDeletionHashtable(new List<Permanent>() { new Permanent(new List<CardSource>() { this }) }, null, null, false))
                && cardEffect.EffectName == "Retaliation");

    #endregion

    #region whether this card has [Fortitude]

    public bool HasFortitude =>
        EffectList(EffectTiming.OnDestroyedAnyone)
            .Some(cardEffect => cardEffect is ActivateICardEffect
                && !cardEffect.IsInheritedEffect && !cardEffect.IsSecurityEffect
                && cardEffect.CanTrigger(CardEffectCommons.OnDeletionHashtable(new List<Permanent>() { new Permanent(new List<CardSource>() { this }) }, null, null, false))
                && cardEffect.EffectName == "Fortitude");

    #endregion

    #region whether this card has inherited effect

    public bool HasInheritedEffect
    {
        get
        {
            foreach (EffectTiming timing in Enum.GetValues(typeof(EffectTiming)))
            {
                if (EffectList(timing)
                    .Some(cardEffect => cardEffect.IsInheritedEffect
                    && !cardEffect.IsDisabled))
                {
                    return true;
                }
            }

            return false;
        }
    }

    #endregion

    #region DNA digivolution requirement

    public List<JogressCondition> jogressCondition
    {
        get
        {
            List<JogressCondition> addJogressConditionEffect =
            EffectList(EffectTiming.None)
            .Filter(cardEffect => cardEffect is IAddJogressConditionEffect
                && cardEffect.CanUse(null)
                && ((IAddJogressConditionEffect)cardEffect).GetJogressCondition(this) != null)
            .Select(cardEffect => ((IAddJogressConditionEffect)cardEffect).GetJogressCondition(this))
            .ToList();

            return addJogressConditionEffect;
        }
    }

    #endregion

    #region Link requirement

    public LinkCondition linkCondition
    {
        get
        {
            ICardEffect addLinkConditonEffect =
            EffectList(EffectTiming.None)
            .Find(cardEffect => cardEffect is IAddLinkConditionEffect
                && cardEffect.CanUse(null)
                && ((IAddLinkConditionEffect)cardEffect).GetLinkCondition(this) != null);

            if (addLinkConditonEffect != null) return ((IAddLinkConditionEffect)addLinkConditonEffect).GetLinkCondition(this);

            return null;
        }
    }

    #endregion

    #region whether this card can DNA digivolve

    public bool CanPlayJogress(bool PayCost)
    {
        if (jogressCondition != null)
        {
            foreach (JogressCondition condition in jogressCondition)
            {
                if (Owner.GetBattleAreaDigimons().Count >= condition.elements.Length)
                {
                    List<Permanent[]> permanentsList = ParameterComparer.Enumerate(Owner.GetBattleAreaDigimons(), condition.elements.Length).ToList();

                    foreach (Permanent[] permanents in permanentsList)
                    {
                        if (permanents != null)
                        {
                            if (permanents.Length == condition.elements.Length)
                            {
                                if (permanents.Length == 2)
                                {
                                    if (condition.elements[0].EvoRootCondition(permanents[0]) && !this.CanNotEvolve(permanents[0]))
                                    {
                                        if (condition.elements[1].EvoRootCondition(permanents[1]) && !this.CanNotEvolve(permanents[1]))
                                        {
                                            if (PayCost)
                                            {
                                                int cost = condition.cost;

                                                cost = GetChangedCostItselef(cost, SelectCardEffect.Root.Hand, permanents.ToList(), checkAvailability: true);

                                                if (Owner.MaxMemoryCost < cost)
                                                {
                                                    return false;
                                                }
                                            }

                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    #endregion

    #region whether target permanent can DNA digivolve into this card

    public bool CanJogressFromTargetPermanent(Permanent targetPermanent, bool PayCost)
    {
        if (targetPermanent != null)
        {
            if (targetPermanent.TopCard != null)
            {
                foreach (JogressCondition condition in jogressCondition)
                {
                    if (targetPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(targetPermanent))
                    {
                        if (this.CanPlayJogress(PayCost))
                        {
                            if (condition != null)
                            {
                                if (condition.elements.ToList().Count((element) => element.EvoRootCondition(targetPermanent)) >= 1)
                                {
                                    if (!this.CanNotEvolve(targetPermanent))
                                    {
                                        if (PayCost)
                                        {
                                            int cost = condition.cost;

                                            cost = GetChangedCostItselef(cost, SelectCardEffect.Root.Hand, new List<Permanent>() { targetPermanent, new Permanent(new List<CardSource>()) }, checkAvailability: true);

                                            if (Owner.MaxMemoryCost < cost)
                                            {
                                                return false;
                                            }
                                        }

                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    #endregion

    #region whether target permanents can DNA digivolve into this card

    public bool CanJogressFromTargetPermanents(List<Permanent> targetPermanents, bool PayCost)
    {
        if (targetPermanents != null)
        {
            if (targetPermanents.Count == 2)
            {
                if (this.CanPlayJogress(PayCost))
                {
                    foreach (JogressCondition condition in jogressCondition)
                    {
                        if (condition != null)
                        {
                            List<Permanent[]> permanentsList = ParameterComparer.Enumerate(targetPermanents, 2).ToList();

                            foreach (Permanent[] permanents in permanentsList)
                            {
                                if (condition.elements.Length == permanents.Length)
                                {
                                    bool canJogress = true;

                                    for (int i = 0; i < permanents.Length; i++)
                                    {
                                        if (permanents[i] != null)
                                        {
                                            if (permanents[i].TopCard != null)
                                            {
                                                if ((!condition.elements[i].EvoRootCondition(permanents[i])) || this.CanNotEvolve(permanents[i]))
                                                {
                                                    canJogress = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (canJogress)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    #endregion

    #region whether this card has level

    public bool HasLevel
    {
        get
        {
            if (_cEntity_Base == null || _cEntity_Base.HasLevel)
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card is level 2

    public bool IsLevel2 => (_cEntity_Base == null || _cEntity_Base.HasLevel) && _cEntity_Base.Level == 2;

    #endregion

    #region whether this card is level 3

    public bool IsLevel3 => (_cEntity_Base == null || _cEntity_Base.HasLevel) && _cEntity_Base.Level == 3;

    #endregion

    #region whether this card is level 4

    public bool IsLevel4 => (_cEntity_Base == null || _cEntity_Base.HasLevel) && _cEntity_Base.Level == 4;

    #endregion

    #region whether this card is level 5

    public bool IsLevel5 => (_cEntity_Base == null || _cEntity_Base.HasLevel) && _cEntity_Base.Level == 5;

    #endregion

    #region whether this card is level 6

    public bool IsLevel6 => (_cEntity_Base == null || _cEntity_Base.HasLevel) && _cEntity_Base.Level == 6;

    #endregion

    #region whether this card is linked

    public bool IsLinked
    {
        get
        {
            return PermanentOfThisCard().LinkedCards.Contains(this);
        }
    }

    #endregion

    #region DigiXros requirement

    public DigiXrosCondition digiXrosCondition
    {
        get
        {
            foreach (ICardEffect cardEffect in this.EffectList(EffectTiming.None))
            {
                if (cardEffect is IAddDigiXrosConditionEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        DigiXrosCondition digiXrosCondition = ((IAddDigiXrosConditionEffect)cardEffect).GetDigiXrosCondition(this);

                        if (digiXrosCondition != null)
                        {
                            return digiXrosCondition;
                        }
                    }
                }
            }

            return null;
        }
    }

    #endregion

    #region Blast Digivolution requirement

    public BurstDigivolutionCondition burstDigivolutionCondition
    {
        get
        {
            foreach (ICardEffect cardEffect in this.EffectList(EffectTiming.None))
            {
                if (cardEffect is IAddBurstDigivolutionConditionEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        BurstDigivolutionCondition burstDigivolutionCondition = ((IAddBurstDigivolutionConditionEffect)cardEffect).GetBurstDigivolutionCondition(this);

                        if (burstDigivolutionCondition != null)
                        {
                            return burstDigivolutionCondition;
                        }
                    }
                }
            }

            return null;
        }
    }

    #endregion

    #region App Fusion requirement

    public AppFusionCondition appFusionCondition
    {
        get
        {
            foreach (ICardEffect cardEffect in this.EffectList(EffectTiming.None))
            {
                if (cardEffect is IAddAppFusionConditionEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        AppFusionCondition appFusionCondition = ((IAddAppFusionConditionEffect)cardEffect).GetAppFusionCondition(this);

                        if (appFusionCondition != null)
                        {
                            return appFusionCondition;
                        }
                    }
                }
            }

            return null;
        }
    }

    #endregion

    #region Assembly requirement

    public AssemblyCondition assemblyCondition
    {
        get
        {
            foreach (ICardEffect cardEffect in this.EffectList(EffectTiming.None))
            {
                if (cardEffect is IAddAssemblyConditionEffect)
                {
                    if (cardEffect.CanUse(null))
                    {
                        AssemblyCondition assemblyCondition = ((IAddAssemblyConditionEffect)cardEffect).GetAssemblyCondition(this);

                        if (assemblyCondition != null)
                        {
                            return assemblyCondition;
                        }
                    }
                }
            }

            return null;
        }
    }

    #endregion

    #region whether this card can Burst digivolve

    public bool CanPlayBurst(bool PayCost)
    {
        if (burstDigivolutionCondition != null)
        {
            if (PayCost)
            {
                int cost = burstDigivolutionCondition.cost;

                cost = GetChangedCostItselef(cost, SelectCardEffect.Root.Hand, new List<Permanent>() { new Permanent(new List<CardSource>()) }, checkAvailability: true);

                if (Owner.MaxMemoryCost < cost)
                {
                    return false;
                }
            }

            List<Permanent> availableDigimon = new List<Permanent>();
            availableDigimon.AddRange(Owner.GetBattleAreaDigimons());
            availableDigimon.AddRange(Owner.GetBreedingAreaPermanents());

            if (availableDigimon.Count >= 1)
            {
                foreach (Permanent digimon in availableDigimon)
                {
                    if (digimon != null)
                    {
                        if (!this.CanNotEvolve(digimon))
                        {
                            if (burstDigivolutionCondition.digimonCondition(digimon))
                            {
                                foreach (Permanent tamer in Owner.GetBattleAreaPermanents())
                                {
                                    if (tamer != digimon)
                                    {
                                        if (burstDigivolutionCondition.tamerCondition(tamer))
                                        {
                                            if (!tamer.CannotReturnToHand(null))
                                            {
                                                if (PayCost)
                                                {
                                                    int cost = burstDigivolutionCondition.cost;

                                                    cost = GetChangedCostItselef(cost, SelectCardEffect.Root.Hand, new List<Permanent>() { digimon }, checkAvailability: true);

                                                    if (Owner.MaxMemoryCost < cost)
                                                    {
                                                        return false;
                                                    }
                                                }

                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    #endregion

    #region whether this card can Link

    public bool CanLink(bool PayCost, bool allowBreeding = false)
    {
        if (linkCondition != null)
        {
            if (PayCost)
            {
                int cost = linkCondition.cost;

                if (Owner.MaxMemoryCost < cost)
                {
                    return false;
                }
            }
            if (allowBreeding)
            {
                if (Owner.GetFieldPermanents().Count >= 1)
                {
                    foreach (Permanent digimon in Owner.GetFieldPermanents())
                    {
                        if (digimon != null)
                        {
                            if (linkCondition.digimonCondition(digimon))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else
            {
                if (Owner.GetBattleAreaDigimons().Count >= 1)
                {
                    foreach (Permanent digimon in Owner.GetBattleAreaDigimons())
                    {
                        if (digimon != null)
                        {
                            if (linkCondition.digimonCondition(digimon))
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

    #region whether target permanent can Burst digivolve into this card

    public bool CanBurstDigivolutionFromTargetPermanent(Permanent targetPermanent, bool PayCost)
    {
        if (targetPermanent != null)
        {
            if (targetPermanent.TopCard != null)
            {
                if (targetPermanent.TopCard.Owner.GetFieldPermanents().Contains(targetPermanent) || targetPermanent.TopCard.Owner.GetBreedingAreaPermanents().Contains(targetPermanent))
                {
                    if (this.CanPlayBurst(PayCost))
                    {
                        if (burstDigivolutionCondition != null)
                        {
                            if (!this.CanNotEvolve(targetPermanent))
                            {
                                if (burstDigivolutionCondition.digimonCondition(targetPermanent))
                                {
                                    foreach (Permanent tamer in Owner.GetBattleAreaPermanents())
                                    {
                                        if (tamer != targetPermanent)
                                        {
                                            if (burstDigivolutionCondition.tamerCondition(tamer))
                                            {
                                                if (!tamer.CannotReturnToHand(null))
                                                {
                                                    if (PayCost)
                                                    {
                                                        int cost = burstDigivolutionCondition.cost;

                                                        cost = GetChangedCostItselef(cost, SelectCardEffect.Root.Hand, new List<Permanent>() { targetPermanent }, checkAvailability: true);

                                                        if (Owner.MaxMemoryCost < cost)
                                                        {
                                                            return false;
                                                        }
                                                    }

                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }

    #endregion

    #region whether target permanent can Link into this card

    public bool CanLinkToTargetPermanent(Permanent targetPermanent, bool PayCost, bool allowBreeding = false)
    {
        if (targetPermanent != null)
        {
            if (targetPermanent.TopCard != null && !targetPermanent.TopCard.IsToken)
            {
                if(allowBreeding || !targetPermanent.TopCard.Owner.GetBreedingAreaPermanents().Contains(targetPermanent))
                {
                    if (this.CanLink(PayCost, allowBreeding))
                    {
                        if (linkCondition != null)
                        {
                            if (linkCondition.digimonCondition(targetPermanent))
                            {
                                if (PayCost)
                                {
                                    int cost = linkCondition.cost;

                                    cost = GetChangedCostItselef(cost, SelectCardEffect.Root.Hand, new List<Permanent>() { targetPermanent }, checkAvailability: true);

                                    if (Owner.MaxMemoryCost < cost)
                                    {
                                        return false;
                                    }
                                }

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

    #region whether target permanent can App Fusion into this card

    public bool CanAppFusionFromTargetPermanent(Permanent targetPermanent, bool PayCost, SelectCardEffect.Root root = SelectCardEffect.Root.Hand)
    {
        if (targetPermanent != null)
        {
            if (targetPermanent.TopCard != null)
            {
                if (appFusionCondition != null)
                {
                    if (!this.CanNotEvolve(targetPermanent))
                    {
                        if (appFusionCondition.digimonCondition(targetPermanent))
                        {
                            foreach (CardSource linkedCard in targetPermanent.LinkedCards)
                            {
                                if (appFusionCondition.linkedCondition(targetPermanent, linkedCard))
                                {
                                    if (PayCost)
                                    {
                                        int cost = appFusionCondition.cost;

                                        cost = GetChangedCostItselef(cost, root, new List<Permanent>() { targetPermanent }, checkAvailability: true);

                                        if (Owner.MaxMemoryCost < cost)
                                        {
                                            return false;
                                        }
                                    }

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

    #endregion

    #region Whether this card's digiXrosCondition contains target card

    public bool IsContainDigiXrosCondition(CardSource cardSource)
    {
        if (cardSource != null)
        {
            if (cardSource.Owner == Owner)
            {
                if (PermanentOfThisCard() != null)
                {
                    if (PermanentOfThisCard().TopCard.HasDigiXros)
                    {
                        if (this == PermanentOfThisCard().TopCard)
                        {
                            if (digiXrosCondition != null)
                            {
                                if (digiXrosCondition.elements.Count((element) => element.CardCondition(cardSource)) >= 1)
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

    #endregion

    #region CardID

    public string CardID => _cEntity_Base.CardID;

    #endregion

    #region whether this card is Digimon

    public bool IsDigimon => CardKind == CardKind.Digimon;

    #endregion

    #region whether this card is DigiEgg

    public bool IsDigiEgg => CardKind == CardKind.DigiEgg;

    #endregion

    #region whether this card is Tamer

    public bool IsTamer => CardKind == CardKind.Tamer;

    #endregion

    #region whether this card is Option

    public bool IsOption => CardKind == CardKind.Option;

    #endregion

    #region Wheter this card is permanent

    public bool IsPermanent => _cEntity_Base.IsPermanent;

    #endregion

    #region Whether to have Play Cost

    public bool HasPlayCost => _cEntity_Base.HasPlayCost;

    #endregion

    #region Whether to have Use Cost

    public bool HasUseCost => _cEntity_Base.HasUseCost;

    #endregion

    #region Inherited Effects ENG discription

    public string InheritedEffectDiscription_ENG => DataBase.ReplaceToASCII(_cEntity_Base.InheritedEffectDiscription_ENG);

    #endregion

    #region Inherited Effects JPN discription

    public string InheritedEffectDiscription_JPN => DataBase.ReplaceToASCII(_cEntity_Base.InheritedEffectDiscription_JPN);

    #endregion

    #region Link Effects

    public string LinkEffectDiscription => DataBase.ReplaceToASCII(_cEntity_Base.LinkEffect);

    #endregion

    #region Card Sprite

    public Sprite CardSprite => _cEntity_Base.CardSprite;

    public async Task<Sprite> GetCardSprite()
    {
        return await _cEntity_Base.GetCardSprite();
    }

    #endregion

    #region SetID

    public string SetID => _cEntity_Base.SetID;

    #endregion

    #region CardEntityIndex

    public int CardEntityIndex => _cEntity_Base.CardIndex;

    #endregion

    #region Card Kind

    public CardKind CardKind => _cEntity_Base.cardKind;

    #endregion

    #region ACE

    public bool IsACE => _cEntity_Base.IsACE;

    #endregion

    #region OverflowMemory

    public int OverflowMemory => _cEntity_Base.OverflowMemory;

    #endregion

    #region Whether this card is being revealed

    public bool IsBeingRevealed { get; set; } = false;

    #endregion

    #region The permanent that belonged to this card just before it left the field

    public Permanent PermanentJustBeforeRemoveField { get; set; } = null;

    #endregion

    #region whether this card has "SEEKERS" trait

    public bool HasSeekersTraits
    {
        get
        {
            if (CardTraits.Contains("SEEKERS"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has "ADVENTURE" trait

    public bool HasAdventureTraits
    {
        get
        {
            if (CardTraits.Contains("ADVENTURE"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has "Hero" trait

    public bool HasHeroTraits
    {
        get
        {
            return EqualsTraits("Hero");
        }
    }

    #endregion

    #region whether this card has "Chronicle" trait

    public bool HasChronicleTraits
    {
        get
        {
            if (CardTraits.Contains("Chronicle"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has "Device" trait

    public bool HasDeviceTraits
    {
        get
        {
            if (CardTraits.Contains("Device"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has "Three Musketeers" trait

    public bool HasThreeMusketeersTraits
    {
        get
        {
            if (CardTraits.Contains("Three Musketeers"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has "Royal Base" trait

    public bool HasRoyalBaseTraits
    {
        get
        {
            if (CardTraits.Contains("Royal Base"))
            {
                return true;
            }

            return false;
        }
    }

    #endregion

    #region whether this card has "Appmon" trait

    public bool HasAppmonTraits
    {
        get
        {
            return EqualsTraits("Appmon");
        }
    }

    #endregion

    #region whether this card has "WG" trait

    public bool HasWGTraits
    {
        get
        {
            return EqualsTraits("WG");
        }
    }

    #endregion

    #region whether this card has "DM" trait

    public bool HasDMTraits
    {
        get
        {
            return EqualsTraits("DM");
        }
    }

    #endregion

    #region whether this card has "CS" trait

    public bool HasCSTraits
    {
        get
        {
            return EqualsTraits("CS");
        }
    }

    #endregion

    #region whether this card has "TS" trait

    public bool HasTSTraits
    {
        get
        {
            return EqualsTraits("TS");
        }
    }

    #endregion

    #region whether this card has "Iliad" trait

    public bool HasIliadTraits
    {
        get
        {
            return EqualsTraits("Iliad");
        }
    }

    #endregion

    #region whether this card has "Unidentified" trait

    public bool HasUnidentifiedTraits
    {
        get
        {
            return EqualsTraits("Unidentified");
        }
    }

    #endregion

    #region whether this card has "Flame" trait

    public bool HasFlameTraits
    {
        get
        {
            return EqualsTraits("Flame");
        }
    }

    #endregion

    #region whether this card has "Eater" trait

    public bool HasEaterTraits
    {
        get
        {
            return EqualsTraits("Eater");
        }
    }

    #endregion

    #region whether this card has "Hudie" trait

    public bool HasHudieTraits
    {
        get
        {
            return EqualsTraits("Hudie");
        }
    }

    #endregion

    #region whether this card has "Sea Animal" trait

    public bool HasSeaAnimalTraits
    {
        get
        {
            return EqualsTraits("Sea Animal");
        }
    }

    #endregion

    #region whether this card has "Puppet" trait

    public bool HasPuppetTraits
    {
        get
        {
            return EqualsTraits("Puppet");
        }
    }

    #endregion

    #region whether this card has "Ver.1" trait

    public bool HasVer1Traits
    {
        get
        {
            return EqualsTraits("Ver.1");
        }
    }

    #endregion

    #region whether this card has "Ver.2" trait

    public bool HasVer2Traits
    {
        get
        {
            return EqualsTraits("Ver.2");
        }
    }

    #endregion

    #region whether this card has "Ver.3" trait

    public bool HasVer3Traits
    {
        get
        {
            return EqualsTraits("Ver.3");
        }
    }

    #endregion

    #region whether this card has "Ver.4" trait

    public bool HasVer4Traits
    {
        get
        {
            return EqualsTraits("Ver.4");
        }
    }

    #endregion

    #region whether this card has "Ver.5" trait

    public bool HasVer5Traits
    {
        get
        {
            return EqualsTraits("Ver.5");
        }
    }

    #endregion

    #region whether this card has "Aquatic" trait

    public bool HasAquaticTraits
    {
        get
        {
            return EqualsTraits("Aquatic");
        }
    }

    #endregion

    #region whether this card has "LIBERATOR" trait

    public bool HasLiberatorTraits
    {
        get
        {
            return EqualsTraits("LIBERATOR");
        }
    }

    #endregion

    #region whether this card has "Standard" Appmon Grade trait

    public bool HasStandardAppTraits
    {
        get
        {
            return EqualsTraits("Stnd.");
        }
    }

    #endregion

    #region whether this card has "Super" Appmon Grade trait

    public bool HasSuperAppTraits
    {
        get
        {
            return EqualsTraits("Sup.");
        }
    }

    #endregion

    #region whether this card has "Ultimate" Appmon Grade trait

    public bool HasUltimateAppTraits
    {
        get
        {
            return EqualsTraits("Ult.");
        }
    }

    #endregion

    #region whether this card has "Holy Beast" trait

    public bool HasHolyBeastTraits
    {
        get
        {
            return EqualsTraits("Holy Beast");
        }
    }

    #endregion

    #region whether this card has "Archangel" trait

    public bool HasArchAngelTraits
    {
        get
        {
            return EqualsTraits("Archangel");
        }
    }

    #endregion

    #region whether this card has "Fallen Angel" trait

    public bool HasFallenAngelTraits
    {
        get
        {
            return EqualsTraits("Fallen Angel");
        }
    }

    #endregion

    #region whether this card has "Bagra Army" trait

    public bool HasBagraArmyTraits
    {
        get
        {
            return EqualsTraits("Bagra Army");
        }
    }

    #endregion

    #region whether this card has "Evil" trait

    public bool HasEvilTraits
    {
        get
        {
            return EqualsTraits("Evil");
        }
    }

    #endregion

    #region whether this card has "Wizard" trait

    public bool HasWizardTraits
    {
        get
        {
            return EqualsTraits("Wizard");
        }
    }

    #endregion

    #region whether this card has "Leviathan" trait

    public bool HasLeviathanTraits
    {
        get
        {
            return EqualsTraits("Leviathan");
        }
    }

    #endregion

    #region whether this card has "Undead" trait

    public bool HasUndeadTraits
    {
        get
        {
            return EqualsTraits("Undead");
        }
    }

    #endregion

    #region whether this card has "Galaxy" trait

    public bool HasGalaxyTraits
    {
        get
        {
            return EqualsTraits("Galaxy");
        }
    }

    #endregion

    #region whether this card has "Rock"/"Mineral trait

    public bool HasRockMineralTraits
    {
        get
        {
            return EqualsTraits("Rock") || EqualsTraits("Mineral");
        }
    }

    #endregion

    #region whether this card has Sea Beast trait

    public bool HasSeaBeastTraits
    {
        get
        {
            return EqualsTraits("Sea Beast");
		}
    }

    #endregion

    #region whether this card has Twilight trait

    public bool HasTwilightTrait
    {
        get
        {
            return EqualsTraits("Twilight");
        }
    }

    #endregion

    #region whether this card has Angel trait

    public bool HasAngelStrictTraits
    {
        get
        {
            return EqualsTraits("Angel");
		}
    }

    #endregion

    #region whether this card has Dark Masters trait

    public bool HasDarkMastersTrait
    {
        get
        {
            return EqualsTraits("Dark Masters");
        }
    }

    #endregion

    #region whether this card has Ghost trait

    public bool HasGhostTraits
    {
        get
        {
            return EqualsTraits("Ghost");
        }
    }

    #endregion

    #region whether this card has Dark Animal trait

    public bool HasDarkAnimalTraits
    {
        get
        {
            return EqualsTraits("Dark Animal");
        }
    }

    #endregion
}

public class JogressCondition

{
    public JogressCondition(JogressConditionElement[] elements, int cost)
    {
        this.elements = new JogressConditionElement[2];

        if (this.elements.Length == elements.Length)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                this.elements[i] = elements[i];
            }
        }

        this.cost = cost;
    }

    public JogressConditionElement[] elements { get; private set; } = new JogressConditionElement[2];
    public int cost { get; private set; } = 0;
}

public class JogressConditionElement
{
    public JogressConditionElement(Func<Permanent, bool> evoRootCondition, string selectMessage)
    {
        EvoRootCondition = evoRootCondition;

        SelectMessage = selectMessage;
    }

    Func<Permanent, bool> _evoRootCondition = null;

    public Func<Permanent, bool> EvoRootCondition
    {
        get
        {
            return permanent => _evoRootCondition == null || _evoRootCondition(permanent);
        }

        private set
        {
            _evoRootCondition = value;
        }
    }

    public string SelectMessage { get; private set; } = "";
}

public class DigiXrosCondition
{
    public DigiXrosCondition(List<DigiXrosConditionElement> elements, Func<List<CardSource>, CardSource, bool> CanTargetCondition_ByPreSelecetedList, int reduceCostPerCard)
    {
        this.elements = new List<DigiXrosConditionElement>();

        foreach (DigiXrosConditionElement element in elements)
        {
            this.elements.Add(element);
        }

        this.CanTargetCondition_ByPreSelecetedList = CanTargetCondition_ByPreSelecetedList;
        this.reduceCostPerCard = reduceCostPerCard;
    }

    public List<DigiXrosConditionElement> elements { get; private set; } = new List<DigiXrosConditionElement>();
    public Func<List<CardSource>, CardSource, bool> CanTargetCondition_ByPreSelecetedList { get; private set; } = null;

    public int reduceCostPerCard { get; private set; } = 0;
}

public class DigiXrosConditionElement
{
    public DigiXrosConditionElement(Func<CardSource, bool> cardCondition, string selectMessage, bool skipAllIfNoSelect = false)
    {
        this.CardCondition = cardCondition;

        this.selectMessage = selectMessage;
        this.skipAllIfNoSelect = skipAllIfNoSelect;
    }

    public Func<CardSource, bool> CardCondition { get; private set; } = null;
    public string selectMessage { get; private set; } = "";
    public bool skipAllIfNoSelect { get; private set; } = false;
}

public class BurstDigivolutionCondition
{
    public BurstDigivolutionCondition(Func<Permanent, bool> tamerCondition, string selectTamerMessage, Func<Permanent, bool> digimonCondition, string selectDigimonMessage, int cost)
    {
        this.tamerCondition = tamerCondition;
        this.selectTamerMessage = selectTamerMessage;
        this.digimonCondition = digimonCondition;
        this.selectDigimonMessage = selectDigimonMessage;
        this.cost = cost;
    }

    public Func<Permanent, bool> tamerCondition { get; private set; } = null;
    public string selectTamerMessage { get; private set; } = null;
    public Func<Permanent, bool> digimonCondition { get; private set; } = null;
    public string selectDigimonMessage { get; private set; } = null;

    public int cost { get; private set; } = 0;
}

public class LinkCondition
{
    public LinkCondition(Func<Permanent, bool> digimonCondition, int cost)
    {
        this.digimonCondition = digimonCondition;
        this.cost = cost;
    }

    public Func<Permanent, bool> digimonCondition { get; private set; } = null;
    public int cost { get; private set; } = 0;
}

public class AppFusionCondition
{
    public AppFusionCondition(Func<Permanent, CardSource, bool> linkedCondition, Func<Permanent, bool> digimonCondition, int cost)
    {
        this.linkedCondition = linkedCondition;
        this.digimonCondition = digimonCondition;
        this.cost = cost;
    }

    public Func<Permanent, CardSource, bool> linkedCondition { get; private set; } = null;
    public Func<Permanent, bool> digimonCondition { get; private set; } = null;

    public int cost { get; private set; } = 0;
}

public class AssemblyCondition
{
    //Method to work with older form of Assembly, 1 single condition X times
    public AssemblyCondition(AssemblyConditionElement element, Func<List<CardSource>, CardSource, bool> CanTargetCondition_ByPreSelecetedList, string selectMessage, int elementCount, int reduceCost)
    {
        element.ElementCount = elementCount;
        element.selectMessage = selectMessage;
        element.CanTargetCondition_ByPreSelecetedList = CanTargetCondition_ByPreSelecetedList;
        this.elements = new List<AssemblyConditionElement>(){ element };
        this.elementCount = elementCount;
        this.reduceCost = reduceCost;
    }

    //Method to work with A x B x C... DigiXros like conditions
    public AssemblyCondition(List<AssemblyConditionElement> elements, int reduceCost)
    {
        this.elements = elements;
        this.elementCount = elements.Select(element => element.ElementCount).Sum();
        this.reduceCost = reduceCost;
    }

    public List<AssemblyConditionElement> elements { get; private set; } = new List<AssemblyConditionElement>();
    public int elementCount { get; private set; } = 0;
    public int reduceCost { get; private set; } = 0;
}

public class AssemblyConditionElement
{
    public AssemblyConditionElement(Func<CardSource, bool> cardCondition, bool skipAllIfNoSelect = true, string selectMessage = null, int elementCount = 0, Func<List<CardSource>, CardSource, bool> CanTargetCondition_ByPreSelecetedList = null)
    {
        this.CardCondition = cardCondition;
        this.skipAllIfNoSelect = skipAllIfNoSelect;
        this.selectMessage = selectMessage;
        this.ElementCount = elementCount;
        this.CanTargetCondition_ByPreSelecetedList = CanTargetCondition_ByPreSelecetedList;
    }

    public Func<CardSource, bool> CardCondition { get; set; } = null;
    public bool skipAllIfNoSelect { get; set; } = true;

    public int ElementCount { get; set; } = 0;

    public Func<List<CardSource>, CardSource, bool> CanTargetCondition_ByPreSelecetedList { get; set; } = null;

    public string selectMessage { get; set; } = "";
}