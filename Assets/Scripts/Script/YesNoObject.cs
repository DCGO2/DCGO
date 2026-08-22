using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class YesNoObject : MonoBehaviour
{
    private static readonly int CloseHash = Animator.StringToHash("Close");
    private static readonly int OpenHash = Animator.StringToHash("Open");

    // Layout sized for 3 tall buttons; shrink when we show 4 (Ranked) or 5 (Tournament/Bot).
    private const float CompactCellHeight = 128f;
    private const float CompactSpacingY = 14f;
    private const float CompactButtonsAnchoredY = -70f;

    public Text InfoText;

    public List<CommandButton> Buttons;

    public Animator anim;

    public GameObject CloseButton;

    //public Vector3 defaultPos = Vector3.zero;

    public bool CloseOnButtonClicked = true;

    Vector2? _defaultCellSize;
    Vector2? _defaultSpacing;
    float? _defaultButtonsAnchoredY;

    public void SetUpYesNoObject(List<UnityAction> OnClickActions, List<string> CommandTexts, string _InfoText, bool CanClose)
    {
        //this.transform.localPosition = defaultPos;

        EnsureButtonCapacity(OnClickActions.Count);
        FitButtonsLayout(OnClickActions.Count);

        for (int i = 0; i < Buttons.Count; i++)
        {
            Buttons[i].OnClickAction = null;

            if (i < OnClickActions.Count)
            {
                Buttons[i].gameObject.SetActive(true);

                Buttons[i].transform.GetChild(0).GetComponent<Text>().text = CommandTexts[i];

                int k = i;

                Buttons[i].OnClickAction = () => 
                {
                    if (!Buttons[k].GetComponent<Button>().interactable)
                        return;

                    OnClickActions[k]?.Invoke();
                    
                    if(this.CloseOnButtonClicked)
                    {
                        this.Close_(false);
                    }
                };
            }

            else
            {
                Buttons[i].gameObject.SetActive(false);
            }
        }

        InfoText.text = _InfoText;

        this.gameObject.SetActive(true);

        CloseButton.SetActive(CanClose);

        Open();
    }

    /// <summary>
    /// Authored battle-mode UI has Random / Room / Bot. Extra modes (Ranked, Tournament)
    /// are cloned at runtime so official Opening scenes still show all five choices.
    /// </summary>
    void EnsureButtonCapacity(int needed)
    {
        if (Buttons == null || Buttons.Count == 0 || Buttons.Count >= needed)
            return;

        while (Buttons.Count < needed)
        {
            const int insertAt = 1;
            var template = Buttons[0];
            var chromeSource = Buttons.Count > 1 ? Buttons[1] : template;

            var clone = Instantiate(template, template.transform.parent);
            clone.name = template.name + "_Extra" + Buttons.Count;
            clone.transform.SetSiblingIndex(insertAt);
            Buttons.Insert(insertAt, clone);

            CopyButtonChrome(clone, chromeSource);
        }
    }

    static void CopyButtonChrome(CommandButton target, CommandButton source)
    {
        if (target == null || source == null || target == source)
            return;

        var targetImage = target.GetComponent<Image>();
        var sourceImage = source.GetComponent<Image>();
        if (targetImage != null && sourceImage != null)
            targetImage.sprite = sourceImage.sprite;

        var targetButton = target.GetComponent<Button>();
        var sourceButton = source.GetComponent<Button>();
        if (targetButton == null || sourceButton == null)
            return;

        var spriteState = targetButton.spriteState;
        spriteState.highlightedSprite = sourceButton.spriteState.highlightedSprite;
        spriteState.pressedSprite = sourceButton.spriteState.pressedSprite;
        spriteState.selectedSprite = sourceButton.spriteState.selectedSprite;
        spriteState.disabledSprite = sourceButton.spriteState.disabledSprite;
        targetButton.spriteState = spriteState;
    }

    void FitButtonsLayout(int activeCount)
    {
        if (Buttons == null || Buttons.Count == 0)
            return;

        var buttonsParent = Buttons[0].transform.parent as RectTransform;
        if (buttonsParent == null)
            return;

        var grid = buttonsParent.GetComponent<GridLayoutGroup>();
        if (grid == null)
            return;

        if (!_defaultCellSize.HasValue)
        {
            _defaultCellSize = grid.cellSize;
            _defaultSpacing = grid.spacing;
            _defaultButtonsAnchoredY = buttonsParent.anchoredPosition.y;
        }

        if (activeCount >= 5)
        {
            grid.cellSize = new Vector2(_defaultCellSize.Value.x, 100f);
            grid.spacing = new Vector2(_defaultSpacing.Value.x, 8f);
            buttonsParent.anchoredPosition = new Vector2(
                buttonsParent.anchoredPosition.x,
                -40f);
            SetButtonIconSize(56f);
        }
        else if (activeCount >= 4)
        {
            grid.cellSize = new Vector2(_defaultCellSize.Value.x, CompactCellHeight);
            grid.spacing = new Vector2(_defaultSpacing.Value.x, CompactSpacingY);
            buttonsParent.anchoredPosition = new Vector2(
                buttonsParent.anchoredPosition.x,
                CompactButtonsAnchoredY);
            SetButtonIconSize(72f);
        }
        else
        {
            grid.cellSize = _defaultCellSize.Value;
            grid.spacing = _defaultSpacing.Value;
            if (_defaultButtonsAnchoredY.HasValue)
            {
                buttonsParent.anchoredPosition = new Vector2(
                    buttonsParent.anchoredPosition.x,
                    _defaultButtonsAnchoredY.Value);
            }
            SetButtonIconSize(100f);
        }
    }

    void SetButtonIconSize(float size)
    {
        for (int i = 0; i < Buttons.Count; i++)
        {
            if (Buttons[i] == null)
                continue;

            var t = Buttons[i].transform;
            for (int c = 0; c < t.childCount; c++)
            {
                var child = t.GetChild(c);
                if (child.name.IndexOf("icon", System.StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                var rt = child as RectTransform;
                if (rt != null)
                    rt.sizeDelta = new Vector2(size, size);
            }
        }
    }

    public void Off()
    {
        this.gameObject.SetActive(false);
        Close_(false);
    }

    public void Open()
    {
        this.gameObject.SetActive(true);
        anim.SafeSetInt(OpenHash, 1);
        anim.SafeSetInt(CloseHash, 0);
    }

    public void Close()
    {
        Close_(true);
    }

    public void Close_(bool playSE)
    {
        if (playSE)
        {
            Opening.instance.PlayCancelSE();
        }

        anim.SafeSetInt(OpenHash, 0);
        anim.SafeSetInt(CloseHash, 1);
    }
}
