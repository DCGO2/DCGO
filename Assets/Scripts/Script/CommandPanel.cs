using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[System.Serializable]
public class CommandPanel : MonoBehaviour
{
    [Header("コマンドパネルオブジェクト")]
    [SerializeField] GameObject CommandPanelObject;

    [Header("コマンドパネル背景")]
    [SerializeField] RectTransform CommandPanelBackGround;

    [Header("ボタン親")]
    [SerializeField] Transform buttonParent;

    bool first = false;
    private void Start()
    {
        if (!false)
        {
            first = true;

#if !UNITY_EDITOR && UNITY_ANDROID
            CommandPanelObject.transform.localScale *= 1.5f;
#endif
        }
    }

    public bool isActive()
    {
        return CommandPanelObject.gameObject.activeSelf;
    }

    public void SetUpCommandPanel(List<CardCommand> CardCommands, FieldPermanentCard fieldPokemonCard, HandCard handCard)
    {
        if (CardCommands.Count == 0)
        {
            return;
        }

        List<CardCommandButton> Buttons = new List<CardCommandButton>();

        for (int i = 0; i < buttonParent.childCount; i++)
        {
            Buttons.Add(new CardCommandButton(buttonParent.GetChild(i).GetComponent<Button>(), buttonParent.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>()));
        }

        for (int i = 0; i < Buttons.Count; i++)
        {
            Buttons[i].CloseFieldUnitCommandButton();
        }

        for (int i = 0; i < Buttons.Count; i++)
        {
            if (i < CardCommands.Count)
            {
                Buttons[i].SetUpFieldUnitCommandButton(CardCommands[i].ButtonMessage, CardCommands[i].OnClickAction, CardCommands[i].Active, CardCommands[i].color);
            }

            else
            {
                break;
            }
        }

        //CommandPanelBackGround.sizeDelta = new Vector2(CommandPanelBackGround.sizeDelta.x, 4.6f * (FieldUnitCommands.Count + 1));

        GridLayoutGroup grid = CommandPanelObject.GetComponentInChildren<GridLayoutGroup>();

        float defaultWidth = 241.42f;
        int constraint = CardCommands.Count + 1;

        if (grid != null)
        {
            constraint = grid.constraintCount;
        }
        UnityEngine.Debug.Log($"PANEL WIDTH: {defaultWidth}, {CardCommands.Count}, {constraint}, {Mathf.CeilToInt((float)CardCommands.Count / (float)constraint)}");
        CommandPanelBackGround.sizeDelta = new Vector2(defaultWidth * Mathf.CeilToInt((float)CardCommands.Count / (float)constraint), 27.07f * (CardCommands.Count - 1) + 104.63f);

        if (fieldPokemonCard != null)
        {
            if (fieldPokemonCard.ThisPermanent != null)
            {
                if (fieldPokemonCard.ThisPermanent.IsSuspended)
                {
                    CommandPanelObject.transform.localRotation = Quaternion.Euler(0, 0, 90);
                }

                else
                {
                    CommandPanelObject.transform.localRotation = Quaternion.Euler(0, 0, 180);
                }
            }
        }

        else if (handCard != null)
        {
            CommandPanelObject.transform.localRotation = Quaternion.Euler(0, 0, 180);
        }

        if (handCard != null)
        {
            if (handCard.cardSource != null)
            {
                if(!CardEffectCommons.IsExistOnTrash(handCard.cardSource))
                    CommandPanelObject.transform.localPosition = new Vector2(0f, 31f);
                else
                {
                    CommandPanelObject.transform.localPosition = new Vector2(0f, -125f);
                    CommandPanelObject.transform.localScale = new Vector3(4f, 4f, 1f);
                }
            }
        }

        CommandPanelObject.SetActive(true);
    }

    public void CloseCommandPanel()
    {
        if (CommandPanelObject != null)
        {
            CommandPanelObject.SetActive(false);
        }
    }
}

#region コマンド
public class CardCommand
{
    public CardCommand(string ButtonMessage, UnityAction OnClickAction, bool Active, Color color)
    {
        this.ButtonMessage = ButtonMessage;
        this.OnClickAction = OnClickAction;
        this.Active = Active;
        this.color = color;
    }

    public string ButtonMessage { get; set; }
    public UnityAction OnClickAction { get; set; }
    public bool Active { get; set; }
    public Color color { get; set; }
}
#endregion

#region コマンドボタン
public class CardCommandButton
{
    public Button button;

    public TextMeshProUGUI ButtonText;

    public CardCommandButton(Button button, TextMeshProUGUI ButtonText)
    {
        this.button = button;
        this.ButtonText = ButtonText;
    }

    public void SetUpFieldUnitCommandButton(string ButtonMessage, UnityAction OnClickAtion, bool Active, Color color)
    {
        ButtonText.text = ButtonMessage;
        button.gameObject.SetActive(true);
        button.interactable = Active;
        button.image.color = color;

        button.onClick.RemoveAllListeners();

        if (Active)
        {
            button.onClick.AddListener(() => { OnClickAtion?.Invoke(); });
        }
    }

    public void CloseFieldUnitCommandButton()
    {
        button.gameObject.SetActive(false);
    }
}
#endregion