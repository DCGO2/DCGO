using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class BrainStormObject : MonoBehaviour
{
    [Header("ÉvÉåÉCÉÑÅ[")]
    public Player player;

    [Header("HandCards")]
    public List<HandCard> BrainStormHandCards = new List<HandCard>();

    public IEnumerator Init()
    {
        this.gameObject.SetActive(true);

        foreach (HandCard handCard in BrainStormHandCards)
        {
            handCard.gameObject.SetActive(true);
            handCard.transform.parent.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(Time.deltaTime);

        foreach (HandCard handCard in BrainStormHandCards)
        {
            handCard.gameObject.SetActive(false);
            handCard.gameObject.name = $"BrainStormObject_{player.PlayerName}";
        }
    }

    public IEnumerator BrainStormCoroutine(CardSource cardSource)
    {
        int count = -1;

        foreach (HandCard handCard in BrainStormHandCards)
        {
            if (handCard.gameObject.activeSelf)
            {
                count = BrainStormHandCards.IndexOf(handCard);

                if (handCard.cardSource == cardSource)
                {
                    yield break;
                }
            }
        }

        if (-1 <= count && count <= BrainStormHandCards.Count - 2)
        {
            HandCard handCard = BrainStormHandCards[count + 1];

            handCard.gameObject.SetActive(true);
            handCard.SetUpHandCard(cardSource);
            handCard.SetUpHandCardImage();
            handCard.Outline_Select.gameObject.SetActive(true);
            handCard.SetOrangeOutline();
            handCard.SkillNameText.transform.parent.gameObject.SetActive(false);
            handCard.IsExecuting = false;
        }

        yield return null;
    }

    public void EndBrainStorm()
    {
        foreach (HandCard handCard in BrainStormHandCards)
        {
            handCard.gameObject.SetActive(false);
        }
    }

    public IEnumerator CloseBrainstrorm(CardSource cardSource)
    {
        foreach (HandCard handCard in BrainStormHandCards)
        {
            if (handCard.gameObject.activeSelf && handCard.cardSource == cardSource)
            {
                handCard.gameObject.SetActive(false);
            }
        }

        yield return null;
    }

    int _timerCount = 0;
    int _updateFrame = 20;
    private void Update()
    {
        #region Update only once every few frames
        _timerCount++;

        if (_timerCount < _updateFrame)
        {
            return;
        }

        else
        {
            _timerCount = 0;
        }
        #endregion

        RotationCards();
    }

    void RotationCards()
    {
        if (!player.isYou)
        {
            if (ContinuousController.instance != null)
            {
                Quaternion localRotation = Quaternion.Euler(0, 0, 0);

                if (ContinuousController.instance.reverseOpponentsCards)
                {
                    localRotation = Quaternion.Euler(0, 0, 180);
                }

                foreach (HandCard handCard in BrainStormHandCards)
                {
                    handCard.transform.localRotation = localRotation;
                }
            }
        }
    }
}
