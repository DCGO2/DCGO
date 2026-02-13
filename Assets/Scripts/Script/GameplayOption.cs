using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class GameplayOption : OffAnimation
{
    [SerializeField] Animator _anim;
    [SerializeField] Toggle _reverseOpponentsCardsToggle;
    [SerializeField] Toggle _showCutInAnimationToggle;
    [SerializeField] Toggle _turnSuspendedCardsToggle;
    [SerializeField] Toggle _checkBeforeEndingSelectionToggle;
    [SerializeField] Toggle _suspendedCardsDirectionIsLeftToggle;
    [SerializeField] Toggle _autoEffectOrderToggle;
    [SerializeField] Toggle _autoDeckBottomOrderToggle;
    [SerializeField] Toggle _autoDeckTopOrderToggle;
    [SerializeField] Toggle _autoMinDigivolutionCostToggle;
    [SerializeField] Toggle _autoMaxCardCountToggle;
    [SerializeField] Toggle _autoHatchToggle;

    public void Close()
    {
        Close_(true);
    }

    public void Close_(bool playSE)
    {
        if (playSE)
        {
            if (Opening.instance != null)
            {
                Opening.instance.PlayCancelSE();
            }

            else if (GManager.instance != null)
            {
                GManager.instance.PlayCancelSE();
            }
        }

        _anim.SetInteger("Open", 0);
        _anim.SetInteger("Close", 1);
    }

    public void Init()
    {
        Off();
    }

    public void Open()
    {
        if (ContinuousController.instance != null)
        {
            OptionUtility.InitToggle(
                toggle: _reverseOpponentsCardsToggle,
                onToggleChanged: OnReverseOpponentsCardsToggleChanged,
                value: ContinuousController.instance.reverseOpponentsCards
            );

            /*OptionUtility.InitToggle(
                toggle: _showCutInAnimationToggle,
                onToggleChanged: OnShowCutInAnimationToggleChanged,
                value: ContinuousController.instance.showCutInAnimation
            );*/

            OptionUtility.InitToggle(
                toggle: _turnSuspendedCardsToggle,
                onToggleChanged: OnTurnSuspendedCardsToggleChanged,
                value: ContinuousController.instance.turnSuspendedCards
            );

            OptionUtility.InitToggle(
                toggle: _checkBeforeEndingSelectionToggle,
                onToggleChanged: OnCheckBeforeEndingSelectionToggleChanged,
                value: ContinuousController.instance.checkBeforeEndingSelection
            );

            OptionUtility.InitToggle(
                toggle: _suspendedCardsDirectionIsLeftToggle,
                onToggleChanged: OnSuspendedCardsDirectionIsLeftToggleChanged,
                value: ContinuousController.instance.suspendedCardsDirectionIsLeft
            );

            OptionUtility.InitToggle(
                toggle: _autoEffectOrderToggle,
                onToggleChanged: OnAutoEffectOrderToggleChanged,
                value: ContinuousController.instance.autoEffectOrder
            );

            OptionUtility.InitToggle(
                toggle: _autoDeckBottomOrderToggle,
                onToggleChanged: OnAutoDeckBottomOrderToggleChanged,
                value: ContinuousController.instance.autoDeckBottomOrder
            );

            OptionUtility.InitToggle(
                toggle: _autoDeckTopOrderToggle,
                onToggleChanged: OnAutoDeckTopOrderToggleChanged,
                value: ContinuousController.instance.autoDeckTopOrder
            );

            OptionUtility.InitToggle(
                toggle: _autoMinDigivolutionCostToggle,
                onToggleChanged: OnAutoMinDigivolutionCostToggleChanged,
                value: ContinuousController.instance.autoMinDigivolutionCost
            );

            OptionUtility.InitToggle(
                toggle: _autoMaxCardCountToggle,
                onToggleChanged: OnAutoMaxCardCountToggleChanged,
                value: ContinuousController.instance.autoMaxCardCount
            );

            OptionUtility.InitToggle(
                toggle: _autoHatchToggle,
                onToggleChanged: OnAutoHatchToggleChanged,
                value: ContinuousController.instance.autoHatch
            );
        }

        gameObject.SetActive(true);
        _anim.SetInteger("Open", 1);
        _anim.SetInteger("Close", 0);
    }

    #region Show cut in animation
    public void OnShowCutInAnimationToggleChanged(bool value)
    {
        if (ContinuousController.instance == null) return;

        OptionUtility.OnToggleChanged(
            value: value,
            toggle: _showCutInAnimationToggle,
            onToggleChanged: OnShowCutInAnimationToggleChanged,
            settingRef: ref ContinuousController.instance.showCutInAnimation,
            saveAction: ContinuousController.instance.SaveShowCutInAnimation
        );
    }

    public void HandleShowCutInAnimationToggle()
    {
        OnShowCutInAnimationToggleChanged(!_showCutInAnimationToggle.isOn);
    }
    #endregion

    #region Reverse opponent's cards
    public void OnReverseOpponentsCardsToggleChanged(bool value)
    {
        if (ContinuousController.instance == null) return;

        OptionUtility.OnToggleChanged(
            value: value,
            toggle: _reverseOpponentsCardsToggle,
            onToggleChanged: OnReverseOpponentsCardsToggleChanged,
            settingRef: ref ContinuousController.instance.reverseOpponentsCards,
            saveAction: ContinuousController.instance.SaveReverseOpponentsCards
        );

        GManager.OnReverseOpponentsCardsChanged?.Invoke();
    }

    public void HandleReverseOpponentsCardsToggle()
    {
        OnReverseOpponentsCardsToggleChanged(!_reverseOpponentsCardsToggle.isOn);
    }
    #endregion

    #region Turn suspended cards
    public void OnTurnSuspendedCardsToggleChanged(bool value)
    {
        if (ContinuousController.instance == null) return;

        OptionUtility.OnToggleChanged(
            value: value,
            toggle: _turnSuspendedCardsToggle,
            onToggleChanged: OnTurnSuspendedCardsToggleChanged,
            settingRef: ref ContinuousController.instance.turnSuspendedCards,
            saveAction: ContinuousController.instance.SaveTurnSuspendedCards
        );
    }

    public void HandleTurnSuspendedCardsToggle()
    {
        OnTurnSuspendedCardsToggleChanged(!_turnSuspendedCardsToggle.isOn);
    }
    #endregion

    #region Check before ending selection
    public void OnCheckBeforeEndingSelectionToggleChanged(bool value)
    {
        if (ContinuousController.instance == null) return;

        OptionUtility.OnToggleChanged(
            value: value,
            toggle: _checkBeforeEndingSelectionToggle,
            onToggleChanged: OnCheckBeforeEndingSelectionToggleChanged,
            settingRef: ref ContinuousController.instance.checkBeforeEndingSelection,
            saveAction: ContinuousController.instance.SaveCheckBeforeEndingSelection
        );
    }

    public void HandleCheckBeforeEndingSelectionToggle()
    {
        OnCheckBeforeEndingSelectionToggleChanged(!_checkBeforeEndingSelectionToggle.isOn);
    }
    #endregion

    #region Suspended card is left
    public void OnSuspendedCardsDirectionIsLeftToggleChanged(bool value)
    {
        if (ContinuousController.instance == null) return;

        OptionUtility.OnToggleChanged(
            value: value,
            toggle: _suspendedCardsDirectionIsLeftToggle,
            onToggleChanged: OnSuspendedCardsDirectionIsLeftToggleChanged,
            settingRef: ref ContinuousController.instance.suspendedCardsDirectionIsLeft,
            saveAction: ContinuousController.instance.SaveSuspendedCardsDirectionIsLeft
        );
    }

    public void HandleSuspendedCardsDirectionIsLeftToggle()
    {
        OnSuspendedCardsDirectionIsLeftToggleChanged(!_suspendedCardsDirectionIsLeftToggle.isOn);
    }
    #endregion

    #region Auto effect order

    public void OnAutoEffectOrderToggleChanged(bool value)
    {
        if (ContinuousController.instance == null) return;

        OptionUtility.OnToggleChanged(
            value: value,
            toggle: _autoEffectOrderToggle,
            onToggleChanged: OnAutoEffectOrderToggleChanged,
            settingRef: ref ContinuousController.instance.autoEffectOrder,
            saveAction: ContinuousController.instance.SaveAutoEffectOrder
        );
    }

    public void HandleAutoEffectOrderToggle()
    {
        OnAutoEffectOrderToggleChanged(!_autoEffectOrderToggle.isOn);
    }
    #endregion

    #region Auto deck bottom order

    public void OnAutoDeckBottomOrderToggleChanged(bool value)
    {
        if (ContinuousController.instance == null) return;

        OptionUtility.OnToggleChanged(
            value: value,
            toggle: _autoDeckBottomOrderToggle,
            onToggleChanged: OnAutoDeckBottomOrderToggleChanged,
            settingRef: ref ContinuousController.instance.autoDeckBottomOrder,
            saveAction: ContinuousController.instance.SaveAutoDeckBottomOrder
        );
    }

    public void HandleAutoDeckBottomOrderToggle()
    {
        OnAutoDeckBottomOrderToggleChanged(!_autoDeckBottomOrderToggle.isOn);
    }
    #endregion

    #region Auto deck top order

    public void OnAutoDeckTopOrderToggleChanged(bool value)
    {
        if (ContinuousController.instance == null) return;

        OptionUtility.OnToggleChanged(
            value: value,
            toggle: _autoDeckTopOrderToggle,
            onToggleChanged: OnAutoDeckTopOrderToggleChanged,
            settingRef: ref ContinuousController.instance.autoDeckTopOrder,
            saveAction: ContinuousController.instance.SaveAutoDeckTopOrder
        );
    }

    public void HandleAutoDeckTopOrderToggle()
    {
        OnAutoDeckTopOrderToggleChanged(!_autoDeckTopOrderToggle.isOn);
    }
    #endregion

    #region Auto min digivolution cost

    public void OnAutoMinDigivolutionCostToggleChanged(bool value)
    {
        if (ContinuousController.instance == null) return;

        OptionUtility.OnToggleChanged(
            value: value,
            toggle: _autoMinDigivolutionCostToggle,
            onToggleChanged: OnAutoMinDigivolutionCostToggleChanged,
            settingRef: ref ContinuousController.instance.autoMinDigivolutionCost,
            saveAction: ContinuousController.instance.SaveAutoMinDigivolutionCost
        );
    }

    public void HandleAutoMinDigivolutionCostToggle()
    {
        OnAutoMinDigivolutionCostToggleChanged(!_autoMinDigivolutionCostToggle.isOn);
    }
    #endregion

    #region Auto max card count

    public void OnAutoMaxCardCountToggleChanged(bool value)
    {
        if (ContinuousController.instance == null) return;

        OptionUtility.OnToggleChanged(
            value: value,
            toggle: _autoMaxCardCountToggle,
            onToggleChanged: OnAutoMaxCardCountToggleChanged,
            settingRef: ref ContinuousController.instance.autoMaxCardCount,
            saveAction: ContinuousController.instance.SaveAutoMaxCardCount
        );
    }

    public void HandleAutoMaxCardCountToggle()
    {
        OnAutoMaxCardCountToggleChanged(!_autoMaxCardCountToggle.isOn);
    }
    #endregion

    #region Auto hatch

    public void OnAutoHatchToggleChanged(bool value)
    {
        if (ContinuousController.instance == null) return;

        OptionUtility.OnToggleChanged(
            value: value,
            toggle: _autoHatchToggle,
            onToggleChanged: OnAutoHatchToggleChanged,
            settingRef: ref ContinuousController.instance.autoHatch,
            saveAction: ContinuousController.instance.SaveAutoHatch
        );
    }

    public void HandleAutoHatchToggle()
    {
        OnAutoHatchToggleChanged(!_autoHatchToggle.isOn);
    }
    #endregion
}
