using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SelectCommandPanel : MonoBehaviour
{
    [Header("Command Selection Button Prefab")]
    public SelectCommand selectCommandPrefab;

    private void Start()
    {
        oldCommandTextPos = GManager.instance.commandText.transform.localPosition;
    }

    #region Open command selection panel
    public Coroutine SetUpCommandButton(List<Command_SelectCommand> commands)
    {
        if (this.gameObject.activeSelf)
        {
            return null;
        }

        this.transform.parent.gameObject.SetActive(true);
        this.gameObject.SetActive(true);

        GetComponent<Animator>().SetInteger("Close", 0);

        return StartCoroutine(SetUpCommandButtonCoroutine(commands));
    }

    Vector3 oldCommandTextPos = Vector3.zero;

    IEnumerator SetUpCommandButtonCoroutine(List<Command_SelectCommand> commands)
    {
        oldCommandTextPos = GManager.instance.commandText.transform.localPosition;

        if (commands.Count >= 2)
        {
            //GManager.instance.commandText.transform.localPosition += new Vector3(0, 86 * (commands.Count - 1));
            GManager.instance.commandText.transform.localPosition += new Vector3(0, 90 * (commands.Count - 1));
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        yield return new WaitWhile(() => transform.childCount > 0);

        if (commands != null)
        {
            GManager.instance.sideBar.SetUpSideBar();

            commands.Reverse();

            foreach (Command_SelectCommand command in commands)
            {
                SelectCommand _selectCommandButton = Instantiate(selectCommandPrefab, transform);

                _selectCommandButton.OpenSelectCommandButton(command.CommandName, command.Command, command.SpriteIndex);

                _selectCommandButton.OnClickEvent.AddListener(() => { for (int i = 0; i < this.transform.childCount; i++) { this.transform.GetChild(i).gameObject.SetActive(false); } });

                _selectCommandButton.OnClickEvent.AddListener(CloseSelectCommandPanel);
            }

            if (commands.Count == 0)
            {
                Off();
            }
        }

        else
        {
            Off();
        }
    }
    #endregion

    #region Close command selection panel
    public void CloseSelectCommandPanel()
    {
        if (!this.gameObject.activeSelf)
        {
            return;
        }

        GetComponent<Animator>().SetInteger("Close", 1);
    }

    bool first = false;

    public void Off()
    {
        Off(true);
    }

    public void Off(bool returnDefaultPos)
    {
        this.gameObject.SetActive(false);

        GManager.instance.sideBar.OffSideBar(returnDefaultPos);

        if (first)
        {
            GManager.instance.commandText.transform.localPosition = oldCommandTextPos;
        }

        first = true;
    }
    #endregion
}

public class Command_SelectCommand
{
    public string CommandName { get; set; }
    public UnityAction Command { get; set; }

    public int SpriteIndex { get; set; }

    public Command_SelectCommand(string _CommandName, UnityAction _Command, int _SpriteIndex)
    {
        CommandName = _CommandName;
        Command = _Command;
        SpriteIndex = _SpriteIndex;
    }
}