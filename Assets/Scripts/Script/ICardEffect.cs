using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Card Effect
public abstract class ICardEffect
{
    #region Set up effect

    public void SetUpICardEffect(string effectName, Func<Hashtable, bool> canUseCondition, CardSource card)
    {
        SetEffectSourceCard(card);
        SetEffectSourcePermanent(null);
        SetMaxCountPerTurn(-1);
        SetEffectName(effectName);
        SetEffectDiscription("");
        SetHashString("");
        SetOnProcessCallbuck(null);
        SetRootCardEffect(null);

        SetCanUseCondition(canUseCondition);
        SetCanActivateCondition(null);

        SetIsOptional(false);
        SetUseOptional(false);
        SetIsDeclarative(false);
        SetIsInheritedEffect(false);
        SetIsLinkedEffect(false);
        SetIsSecurityEffect(false);
        SetIsCounterEffect(false);
        SetIsDigimonEffect(false);
        SetIsTamerEffect(false);
        SetChainActivationCount(-1);
        SetIsBackgroundProcess(false);
        SetNotShowUI(false);

        if (!(this is ActivateICardEffect))
        {
            SetIsDigimonEffect(card != null && CardEffectCommons.IsExistOnBattleArea(card) && card.PermanentOfThisCard().IsDigimon);
            SetIsTamerEffect(card != null && CardEffectCommons.IsExistOnBattleArea(card) && card.PermanentOfThisCard().IsTamer);
        }
    }

    #endregion

    #region The source card of this effect

    CardSource _effectSourceCard = null;

    public CardSource EffectSourceCard
    {
        get
        {
            if (EffectSourcePermanent != null)
            {
                if (EffectSourcePermanent.TopCard != null)
                {
                    return EffectSourcePermanent.TopCard;
                }
            }

            return _effectSourceCard;
        }

        private set { _effectSourceCard = value; }
    }

    public void SetEffectSourceCard(CardSource effectSourceCard)
    {
        EffectSourceCard = effectSourceCard;
    }

    #endregion

    #region The source permanent of this effect

    private Permanent _effectSourcePermanent = null;

    public Permanent EffectSourcePermanent
    {
        get { return _effectSourcePermanent; }
        private set { _effectSourcePermanent = value; }
    }

    public void SetEffectSourcePermanent(Permanent effectSourcePermanent)
    {
        EffectSourcePermanent = effectSourcePermanent;
    }

    #endregion

    #region Maximum number of times this effect can be used in a turn

    int _maxCountPerTurn = 114514;

    public int MaxCountPerTurn
    {
        get { return _maxCountPerTurn; }
        private set
        {
            if (value > 0)
            {
                _maxCountPerTurn = value;
            }
        }
    }

    public void SetMaxCountPerTurn(int maxCountPerTurn)
    {
        MaxCountPerTurn = maxCountPerTurn;
    }

    #endregion

    #region Effect name

    string _effectName = "";

    public string EffectName
    {
        get { return _effectName; }
        private set
        {
            if (string.IsNullOrEmpty(value))
            {
                _effectName = "";
            }
            else
            {
                _effectName = value;
            }
        }
    }

    internal void SetEffectName(string effectName)
    {
        EffectName = effectName;
    }

    #endregion

    #region Effect discription

    string _effectDiscription = "";

    public string EffectDiscription
    {
        get { return _effectDiscription; }
        private set
        {
            if (string.IsNullOrEmpty(value))
            {
                _effectDiscription = "";
            }
            else
            {
                _effectDiscription = value;
            }
        }
    }

    internal void SetEffectDiscription(string effectDiscription)
    {
        EffectDiscription = effectDiscription;
    }

    #endregion

    #region Hash value for identification of same effects

    string _hashString = "";

    public string HashString
    {
        get { return _hashString; }
        private set
        {
            if (string.IsNullOrEmpty(value))
            {
                _hashString = "";
            }
            else
            {
                _hashString = value;
            }
        }
    }

    public void SetHashString(string hashString)
    {
        HashString = hashString;
    }

    #endregion

    #region Callbacks when this effect is executed

    UnityAction _onProcessCallbuck = null;

    public UnityAction OnProcessCallbuck
    {
        get { return _onProcessCallbuck; }
        private set { _onProcessCallbuck = value; }
    }

    public void SetOnProcessCallbuck(UnityAction onProcessCallbuck)
    {
        OnProcessCallbuck = onProcessCallbuck;
    }

    #endregion

    #region Effect giving this effect

    ICardEffect _rootCardEffect = null;

    public ICardEffect RootCardEffect
    {
        get { return _rootCardEffect; }
        private set { _rootCardEffect = value; }
    }

    public void SetRootCardEffect(ICardEffect rootCardEffect)
    {
        RootCardEffect = rootCardEffect;
    }

    #endregion

    #region Determine whether this effect can be used

    #region Condition for which this triggering effect triggers, this static effect applies or this declarative effect can be declared

    Func<Hashtable, bool> _canUseCondition = null;

    public Func<Hashtable, bool> CanUseCondition
    {
        get { return _canUseCondition; }
        private set { _canUseCondition = value; }
    }

    public void SetCanUseCondition(Func<Hashtable, bool> canUseCondition)
    {
        CanUseCondition = canUseCondition;
    }

    #endregion

    #region Condition for which this triggering effect activates

    Func<Hashtable, bool> _canActivateCondition = null;

    public Func<Hashtable, bool> CanActivateCondition
    {
        get { return _canActivateCondition; }
        private set { _canActivateCondition = value; }
    }

    public void SetCanActivateCondition(Func<Hashtable, bool> canActivateCondition)
    {
        CanActivateCondition = canActivateCondition;
    }

    #endregion

    #region Whether this triggering effect triggers

    public bool CanTrigger(Hashtable hashtable)
    {
        #region Effects not available before the start of the game

        if (GManager.instance != null)
        {
            if (GManager.instance.turnStateMachine != null)
            {
                if (GManager.instance.turnStateMachine.gameContext != null)
                {
                    if (!GManager.instance.turnStateMachine.DoneStartGame)
                    {
                        return false;
                    }
                }
            }
        }

        #endregion

        #region Effect availability determined by the maximum number of times it can be used in a turn

        if (EffectSourceCard.cEntity_EffectController.isOverMaxCountPerTurn(this, MaxCountPerTurn))
        {
            return false;
        }

        #endregion

        #region Determination of availability for each effect

        if (CanUseCondition == null || !CanUseCondition(hashtable))
        {
            return false;
        }

        #endregion

        return true;
    }

    #endregion

    #region Whether this triggering effect activates

    public bool CanActivate(Hashtable hashtable)
    {
        #region Effect availability determined by the maximum number of times it can be used in a turn

        if (EffectSourceCard.cEntity_EffectController.isOverMaxCountPerTurn(this, MaxCountPerTurn))
        {
            return false;
        }

        #endregion

        #region Determination of availability for each effect

        if (CanActivateCondition != null && !CanActivateCondition(hashtable))
        {
            return false;
        }

        #endregion

        #region Determination of availability for Inheritated/Linked Effect

        if (this is ActivateICardEffect)
        {
            if (EffectSourceCard != null)
            {
                if (EffectSourceCard.PermanentOfThisCard() != null)
                {
                    if (IsInheritedEffect || IsLinkedEffect)
                    {
                        if (EffectSourceCard == EffectSourceCard.PermanentOfThisCard().TopCard)
                            return false;

                        if (EffectSourceCard.IsFlipped)
                            return false;

                        if (!EffectSourceCard.PermanentOfThisCard().IsDigimon)
                            return false;

                        if (IsLinkedEffect && !EffectSourceCard.PermanentOfThisCard().LinkedCards.Contains(EffectSourceCard))
                            return false;
                    }
                    else
                    {
                        if (EffectSourceCard != EffectSourceCard.PermanentOfThisCard().TopCard)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        #endregion

        #region Determination of availability due to be disabled

        if (IsDisabled)
        {
            return false;
        }

        #endregion
        //TODO: Look into this for the on deletion General issue
        #region Determination whether the permanent is same as when triggered

        if (IsInheritedEffect || IsLinkedEffect)
        {
            if (this is ActivateICardEffect)
            {
                Permanent PermanentWhenTriggered = ((ActivateICardEffect)this).PermanentWhenTriggered;

                if (PermanentWhenTriggered != null)
                {
                    if (EffectSourceCard != null)
                    {
                        Permanent currentPermanent = EffectSourceCard.PermanentOfThisCard();

                        if (currentPermanent != null)
                        {
                            if (currentPermanent != PermanentWhenTriggered)
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

    #endregion

    #region Whether this static effect applies , or this declarative effect can be declared

    public bool CanUse(Hashtable hashtable)
    {
        if (!CanTrigger(hashtable) || !CanActivate(hashtable))
        {
            return false;
        }

        return true;
    }

    #endregion

    #endregion

    #region Changable bool properties

    #region Whether this effect is an optional effect

    bool _isOptional = false;

    public bool IsOptional
    {
        get { return _isOptional; }
        private set { _isOptional = value; }
    }

    public void SetIsOptional(bool isOptional)
    {
        IsOptional = isOptional;
    }

    #endregion

    #region Whether to use this optional effect

    bool _useOptional = false;

    public bool UseOptional
    {
        get { return _useOptional; }
        private set { _useOptional = value; }
    }

    public void SetUseOptional(bool useOptional)
    {
        UseOptional = useOptional;
    }

    #endregion

    #region Whether this effect is a declarative effect

    bool _isDeclarative = false;

    public bool IsDeclarative
    {
        get { return _isDeclarative; }
        private set { _isDeclarative = value; }
    }

    public void SetIsDeclarative(bool isDeclarative)
    {
        IsDeclarative = isDeclarative;
    }

    #endregion

    #region Whether this effect is an Inherited Effect

    bool _isInheritedEffect = false;

    public bool IsInheritedEffect
    {
        get { return _isInheritedEffect; }
        private set { _isInheritedEffect = value; }
    }

    public void SetIsInheritedEffect(bool isInheritatedEffect)
    {
        IsInheritedEffect = isInheritatedEffect;
    }

    #endregion

    #region Whether this effect is an Linked Effect

    bool _isLinkededEffect = false;

    public bool IsLinkedEffect
    {
        get { return _isLinkededEffect; }
        private set { _isLinkededEffect = value; }
    }

    public void SetIsLinkedEffect(bool isLinkededEffect)
    {
        IsLinkedEffect = isLinkededEffect;
    }

    #endregion

    #region Whether this effect is an Security Effect

    bool _isSecurityEffect = false;

    public bool IsSecurityEffect
    {
        get { return _isSecurityEffect; }
        private set { _isSecurityEffect = value; }
    }

    public void SetIsSecurityEffect(bool isSecurityEffect)
    {
        IsSecurityEffect = isSecurityEffect;
    }

    #endregion

    #region Whether this effect is an Counter Effect

    bool _isCounterEffect = false;

    public bool IsCounterEffect
    {
        get { return _isCounterEffect; }
        private set { _isCounterEffect = value; }
    }

    public void SetIsCounterEffect(bool isCounterEffect)
    {
        IsCounterEffect = isCounterEffect;
    }

    #endregion

    #region Whether this effect is Digimon's effect

    bool _isDigimonEffect = false;

    public bool IsDigimonEffect
    {
        get { return _isDigimonEffect; }
        private set { _isDigimonEffect = value; }
    }

    public void SetIsDigimonEffect(bool isDigimonEffect)
    {
        IsDigimonEffect = isDigimonEffect;
    }

    #endregion

    #region Whether this effect is Tamer's effect

    bool _isTamerEffect = false;

    public bool IsTamerEffect
    {
        get { return _isTamerEffect; }
        private set { _isTamerEffect = value; }
    }

    public void SetIsTamerEffect(bool isTamerEffect)
    {
        IsTamerEffect = isTamerEffect;
    }

    #endregion

    #region How many times this effect can activate per chain

    int _chainActivations = -1;

    public int ChainActivations
    {
        get { return _chainActivations; }
        private set { _chainActivations = value; }
    }

    public void SetChainActivationCount(int chainActivations)
    {
        ChainActivations = chainActivations;
    }

    #endregion

    #region Whether this effect is carried out in the background

    bool _isBackgroundProcess = false;

    public bool IsBackgroundProcess
    {
        get { return _isBackgroundProcess; }
        private set { _isBackgroundProcess = value; }
    }

    public void SetIsBackgroundProcess(bool isBackgroundProcess)
    {
        IsBackgroundProcess = isBackgroundProcess;
    }

    #endregion

    #region Whether this effect should be displayed in the UI

    bool _isNotShowUI = false;

    public bool IsNotShowUI
    {
        get { return _isNotShowUI; }
        private set { _isNotShowUI = value; }
    }

    public void SetNotShowUI(bool isNotShowUI)
    {
        IsNotShowUI = isNotShowUI;
    }

    #endregion

    #endregion

    #region Read only bool properties

    #region Whether this effect is disabled

    public bool IsDisabled
    {
        get
        {
            return CheckEffectDisabledClass.isDisabled(this);
        }
    }

    #endregion

    #region Whether this effect is [On Play] effect

    public bool IsOnPlay
    {
        get
        {
            if (EffectSourceCard != null)
            {
                if (!string.IsNullOrEmpty(EffectDiscription))
                {
                    Debug.Log($"Effect Description: {EffectDiscription.StartsWith("[On Play]")}");
                    if (EffectDiscription.StartsWith("[On Play]"))
                    {
                        Hashtable hashtable = CardEffectCommons.OnPlayCheckHashtableOfCard(EffectSourceCard);
                        Debug.Log($"Can Trigger: {CanTrigger(hashtable)}");
                        if (CanTrigger(hashtable))
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

    #region Whether this effect is [When Digivolving] effect

    public bool IsWhenDigivolving
    {
        get
        {
            if (EffectSourceCard != null)
            {
                if (!string.IsNullOrEmpty(EffectDiscription))
                {
                    if (EffectDiscription.StartsWith("[When Digivolving]"))
                    {
                        Hashtable hashtable = CardEffectCommons.WhenDigivolvingCheckHashtableOfCard(EffectSourceCard);

                        if (CanTrigger(hashtable))
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

    #region Whether this effect is [On Deletion] effect

    public bool IsOnDeletion
    {
        get
        {
            if (EffectSourceCard != null)
            {
                if (!string.IsNullOrEmpty(EffectDiscription))
                {
                    if (EffectDiscription.StartsWith("[On Deletion]"))
                    {
                        Permanent effectPermanent = EffectSourceCard.PermanentOfThisCard() ?? new Permanent(new List<CardSource>() { EffectSourceCard });

                        Hashtable hashtable = CardEffectCommons.OnDeletionHashtable(new List<Permanent>() { effectPermanent }, null, null, false);

                        if (CanTrigger(hashtable))
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

    #region Whether this effect is [On Attack] effect

    public bool IsOnAttack
    {
        get
        {
            if (EffectSourceCard != null)
            {
                if (!string.IsNullOrEmpty(EffectDiscription))
                {
                    if (EffectDiscription.StartsWith("[When Attacking]"))
                    {
                        Hashtable hashtable = CardEffectCommons.OnAttackCheckHashtableOfCard(EffectSourceCard, null);

                        if (CanTrigger(hashtable))
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

    #endregion

    #region Whether the target effect is the same as this effect

    public bool IsSameEffect(ICardEffect cardEffect)
    {
        if (cardEffect != null)
        {
            if (cardEffect == this)
            {
                return true;
            }

            if (cardEffect.EffectSourceCard != null)
            {
                if (this.EffectSourceCard != null)
                {
                    if (cardEffect.EffectSourceCard == this.EffectSourceCard)
                    {
                        if (HasSameHashString() && HasSameRootCardEffect())
                        {
                            return true;
                        }

                        bool HasSameHashString()
                        {
                            if (string.IsNullOrEmpty(cardEffect.HashString))
                            {
                                if (string.IsNullOrEmpty(this.HashString))
                                {
                                    return true;
                                }
                            }

                            if (!string.IsNullOrEmpty(cardEffect.HashString))
                            {
                                if (!string.IsNullOrEmpty(this.HashString))
                                {
                                    if (cardEffect.HashString == this.HashString)
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        bool HasSameRootCardEffect()
                        {
                            if (cardEffect.RootCardEffect == null)
                            {
                                if (this.RootCardEffect == null)
                                {
                                    return true;
                                }
                            }

                            if (cardEffect.RootCardEffect != null)
                            {
                                if (this.RootCardEffect != null)
                                {
                                    if (cardEffect.RootCardEffect == this.RootCardEffect)
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }
                    }
                }
            }
        }

        return false;
    }

    #endregion
}

#region Calculation order

public enum CalculateOrder
{
    UpValue,
    DownValue,
    UpToConstant,
    UpDownValue,
    DownToConstant,
}

#endregion

#region Effect Duration

public enum EffectDuration
{
    UntilEachTurnEnd,
    UntilOpponentTurnEnd,
    UntilOwnerTurnEnd,
    UntilEndAttack,
    UntilEndBattle,
    UntilOwnerActivePhase,
    UntilCalculateFixedCost,
    UntilNextUntap,
}

#endregion

#region Timing of effect triggers

public enum EffectTiming
{
    None,
    OnUseOption,
    OnDeclaration,
    OnEnterFieldAnyone,
    OnGetDamage,
    OptionSkill,
    OnDestroyedAnyone,
    WhenDigisorption,
    WhenRemoveField,
    WhenPermanentWouldBeDeleted,
    WhenReturntoLibraryAnyone,
    WhenReturntoHandAnyone,
    WhenUntapAnyone,
    OnEndAttackPhase,
    OnEndTurn,
    OnStartTurn,
    OnEndMainPhase,
    OnDraw,
    OnAddHand,
    OnLoseSecurity,
    OnAddSecurity,
    OnUseDigiburst,
    OnDiscardHand,
    OnDiscardSecurity,
    OnDiscardLibrary,
    OnKnockOut,
    OnMove,
    OnEndCoinToss,
    OnUseAttack,
    OnTappedAnyone,
    OnUnTappedAnyone,
    OnAddDigivolutionCards,
    OnAllyAttack,
    OnCounterTiming,
    OnBlockAnyone,
    OnSecurityCheck,
    OnAttackTargetChanged,
    OnEndBlockDesignation,
    SecuritySkill,
    OnStartMainPhase,
    OnStartBattle,
    OnEndBattle,
    OnDetermineDoSecurityCheck,
    OnEndAttack,
    BeforePayCost,
    AfterPayCost,
    OnDigivolutionCardDiscarded,
    OnDigivolutionCardReturnToDeckBottom,
    OnReturnCardsToLibraryFromTrash,
    OnPermamemtReturnedToHand,
    OnReturnCardsToHandFromTrash,
    AfterEffectsActivate,
    WhenWouldDigivolutionCardDiscarded,
    WhenLinked,
    WhenTopCardTrashed,
    RulesTiming,
    OnRemovedField,
    OnLinkCardDiscarded,
    OnFaceUpSecurityIncreased
}

#endregion

#region card effect that processes

public interface ActivateICardEffect
{
    IEnumerator Activate(Hashtable hash);

    public Permanent PermanentWhenTriggered { get; set; }
    public CardSource TopCardWhenTriggered { get; set; }
}

public static class ActivateICardEffectExtensionClass
{
    #region Optional

    public static IEnumerator Activate_Optional(this ActivateICardEffect activateICardEffect)
    {
        if (((ICardEffect)activateICardEffect).IsOptional)
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<OptionalSkill>().SelectOptional((ICardEffect)activateICardEffect));
        }
    }

    #endregion

    #region Effect

    public static IEnumerator Activate_Effect(this ActivateICardEffect activateICardEffect)
    {
        CardSource card = ((ICardEffect)activateICardEffect).EffectSourceCard;

        if (card != null)
        {
            ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowActivateCardEffectDiscription((ICardEffect)activateICardEffect));

            Permanent permanent = card.PermanentOfThisCard();

            if (permanent != null)
            {
                if (permanent.ShowingPermanentCard != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ActivateFieldPokemonSkillEffect(permanent, (ICardEffect)activateICardEffect));
                }
            }
            else if (card.Owner.HandCards.Contains(card))
            {
                if (card.ShowingHandCard != null)
                {
                    bool showEffect = false;

                    if (!string.IsNullOrEmpty(((ICardEffect)activateICardEffect).EffectDiscription))
                    {
                        if (((ICardEffect)activateICardEffect).EffectDiscription.Contains("[Hand]"))
                        {
                            showEffect = true;
                        }
                    }

                    if (showEffect)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ActivateHandCardSkillEffect(card, (ICardEffect)activateICardEffect));
                    }
                }
            }
            else if (CardEffectCommons.IsExistOnTrash(card))
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ActivateTrashCardSkillEffect((ICardEffect)activateICardEffect));
            }
            else if (card.Owner.ExecutingCards.Contains(card))
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ActivateExecutingCardSkillEffect(card, (ICardEffect)activateICardEffect));
            }

            yield return new WaitForSeconds(0.4f);
        }
    }

    #endregion

    #region Processing

    public static IEnumerator Activate_Execute(this ActivateICardEffect activateICardEffect, Hashtable hash)
    {
        if (((ICardEffect)activateICardEffect).UseOptional || !((ICardEffect)activateICardEffect).IsOptional)
        {
            ((ICardEffect)activateICardEffect).OnProcessCallbuck?.Invoke();
            ((ICardEffect)activateICardEffect).SetOnProcessCallbuck(null);

            //Handling Effect
            ((ICardEffect)activateICardEffect).EffectSourceCard.cEntity_EffectController.RegisterUseEfffectThisTurn(((ICardEffect)activateICardEffect));
            yield return ContinuousController.instance.StartCoroutine(activateICardEffect.Activate(hash));
        }
    }

    #endregion

    #region Optional→ Effect → Processing

    public static IEnumerator Activate_Optional_Effect_Execute(this ActivateICardEffect activateICardEffect, Hashtable hash, bool isCheckOptional = true, UnityAction<ICardEffect> useEffectCallback = null)
    {
        CardSource card = ((ICardEffect)activateICardEffect).EffectSourceCard;
        FieldPermanentCard fieldPermanentCard = null;
        HandCard handCard = null;

        if (card != null)
        {
            if (card.PermanentOfThisCard() != null)
            {
                fieldPermanentCard = card.PermanentOfThisCard().ShowingPermanentCard;
            }

            if (card.Owner.HandCards.Contains(card))
            {
                if (card.ShowingHandCard != null)
                {
                    handCard = card.ShowingHandCard;
                }
            }
        }

        bool oldIsExecuting = GManager.instance.turnStateMachine.isExecuting;

        GManager.instance.turnStateMachine.isExecuting = true;

        if (fieldPermanentCard != null)
        {
            fieldPermanentCard.OnSelectEffect(1.1f);
            fieldPermanentCard.Outline_Select.gameObject.SetActive(true);
            fieldPermanentCard.SetOrangeOutline();
        }

        if (handCard != null)
        {
            if (card.Owner.isYou)
            {
                bool isHandEffect = false;

                if (!string.IsNullOrEmpty(((ICardEffect)activateICardEffect).EffectDiscription))
                {
                    if (((ICardEffect)activateICardEffect).EffectDiscription.Contains("[Hand]"))
                    {
                        isHandEffect = true;
                    }
                }

                if (isHandEffect)
                {
                    handCard.Outline_Select.SetActive(true);
                    handCard.SetOrangeOutline();
                }
            }
        }
        UnityEngine.Debug.Log($"Activate_Optional_Effect_Execute: {(activateICardEffect is ICardEffect)}");
        if (activateICardEffect is ICardEffect)
        {
            UnityEngine.Debug.Log($"Activate_Optional_Effect_Execute: {((ICardEffect)activateICardEffect).EffectSourceCard.BaseENGCardNameFromEntity}");
            //Optional effect activation selection
            if (((ICardEffect)activateICardEffect).IsOptional)
            {
                if (isCheckOptional)
                {
                    yield return ContinuousController.instance.StartCoroutine(Activate_Optional(activateICardEffect));
                }
                else
                {
                    ((ICardEffect)activateICardEffect).SetUseOptional(true);
                }
            }

            //Optional effect activation selection → cost → processing
            yield return ContinuousController.instance.StartCoroutine(Activate_Effect_Execute(activateICardEffect, hash, useEffectCallback));
        }

        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players)
        {
            player.TrashHandCard.gameObject.SetActive(false);
            player.TrashHandCard.IsExecuting = false;
        }

        yield return new WaitForSeconds(Time.deltaTime * 2);

        if (fieldPermanentCard != null)
        {
            fieldPermanentCard.OffUsingSkillEffect();
            fieldPermanentCard.OffSkillName();
            fieldPermanentCard.RemoveSelectEffect();
        }

        foreach (FieldPermanentCard fieldPokemonCard in card.Owner.FieldPermanentObjects)
        {
            fieldPokemonCard.OffUsingSkillEffect();
            fieldPokemonCard.OffSkillName();
        }

        if (handCard != null)
        {
            handCard.Outline_Select.SetActive(false);
        }

        if (card != null)
        {
            GManager.instance.turnStateMachine.isExecuting = oldIsExecuting;
        }
    }

    #endregion

    #region Effect → Processing

    public static IEnumerator Activate_Effect_Execute(this ActivateICardEffect activateICardEffect, Hashtable hash, UnityAction<ICardEffect> useEffectCallback)
    {
        //cost → effect processing
        if (((ICardEffect)activateICardEffect).UseOptional || !((ICardEffect)activateICardEffect).IsOptional)
        {
            useEffectCallback?.Invoke((ICardEffect)activateICardEffect);

            Debug.Log($"{((ICardEffect)activateICardEffect).EffectName} was used");

            #region Add log

            CardSource card = ((ICardEffect)activateICardEffect).EffectSourceCard;

            if (card != null)
            {
                PlayLog.OnAddLog?.Invoke($"\nEffect:\n{card.BaseENGCardNameFromEntity}({card.CardID})\n\"{((ICardEffect)activateICardEffect).EffectName}\"\n");
            }

            #endregion

            //effect
            yield return ContinuousController.instance.StartCoroutine(Activate_Effect(activateICardEffect));

            yield return ContinuousController.instance.StartCoroutine(Activate_Execute(activateICardEffect, hash));
        }

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(null, EffectTiming.AfterEffectsActivate));

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.RuleProcess());
    }

    #endregion
}

#endregion