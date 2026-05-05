using Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class OptionalSkill : MonoBehaviourPunCallbacks
{
    public string waitingText { get; set; } = "The opponent is considering whether to use the effect.";

    bool _useOptional = false;
    public IEnumerator SelectOptional(ICardEffect cardEffect, Hashtable hash)
    {
        List<string> _YesNoTexts = new List<string>() { "Use", "Not use" };

        Player player = cardEffect.EffectSourceCard.Owner;

        _useOptional = false;

        string _Message;

        List<Permanent> effectTargets = cardEffect.EffectTargets != null ? cardEffect.EffectTargets(hash) : null;

        if (effectTargets == null || effectTargets.Count == 0)
        {
            _Message = $"Will you use \"{cardEffect.EffectName}\"?";
        }
        else
        {
            _Message = $"Will you use \"{cardEffect.EffectName}\" targeting {string.Join(", ", effectTargets.Select(permanent => permanent.TopCard.CardNames[0]))}?";
        }

        yield return GManager.instance.photonWaitController.StartWait("SelectOptional");

        #region ƒgƒ‰ƒbƒVƒ…‚ÌƒJ[ƒh‚ð•\Ž¦
        if (cardEffect != null)
        {
            if (cardEffect.EffectSourceCard != null)
            {
                if (cardEffect.EffectSourceCard.Owner.TrashCards.Contains(cardEffect.EffectSourceCard) || cardEffect.EffectSourceCard.Owner.LostCards.Contains(cardEffect.EffectSourceCard))
                {
                    if (cardEffect.EffectSourceCard.Owner.TrashHandCard != null)
                    {
                        if (!cardEffect.EffectSourceCard.Owner.TrashHandCard.gameObject.activeSelf)
                        {
                            cardEffect.EffectSourceCard.Owner.TrashHandCard.gameObject.SetActive(true);
                            cardEffect.EffectSourceCard.Owner.TrashHandCard.SetUpHandCard(cardEffect.EffectSourceCard);
                            cardEffect.EffectSourceCard.Owner.TrashHandCard.SetUpHandCardImage();
                            cardEffect.EffectSourceCard.Owner.TrashHandCard.OnOutline();
                            cardEffect.EffectSourceCard.Owner.TrashHandCard.SetBlueOutline();
                            cardEffect.EffectSourceCard.Owner.TrashHandCard.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
                        }
                    }
                }
            }
        }
        #endregion

        if (cardEffect.EffectSourceCard.Owner.isYou)
        {
            Permanent permanent = cardEffect.EffectSourceCard.PermanentOfThisCard();
            List<FieldPermanentCard> highlightPermanents = new List<FieldPermanentCard>();

            if (permanent != null)
            {
                if (permanent.ShowingPermanentCard != null)
                {
                    highlightPermanents.Add(permanent.ShowingPermanentCard);
                
                }
            }

            if (effectTargets != null)
            {
                foreach (Permanent targetPermanent in effectTargets)
                {
                    if (targetPermanent.ShowingPermanentCard != null)
                    {
                        highlightPermanents.Add(targetPermanent.ShowingPermanentCard);
                    }
                }
            }

            if (highlightPermanents.Count > 0)
            {
                GManager.instance.hideCannotSelectObject.SetUpHideCannotSelectObject(highlightPermanents, false);
            }

            GManager.instance.commandText.OpenCommandText(_Message);

            List<Command_SelectCommand> commands = new List<Command_SelectCommand>()
            {
                new Command_SelectCommand(_YesNoTexts[0] ,() => photonView.RPC("SetUseOptional",RpcTarget.All, player.PlayerID, true),0),
            };

            GManager.instance.BackButton.OpenSelectCommandButton(_YesNoTexts[1], () => { photonView.RPC("SetUseOptional", RpcTarget.All, player.PlayerID, false); }, 0);

            GManager.instance.selectCommandPanel.SetUpCommandButton(commands);
        }

        else
        {
            bool ShowOpponentMessage = cardEffect.EffectDiscription.Contains("[Hand]") && GManager.instance.autoProcessing.executingMultipleSkills != null && !GManager.instance.autoProcessing.executingMultipleSkills.IsOnlyHandEffectStacked;

            if (ShowOpponentMessage)
            {
                GManager.instance.commandText.OpenCommandText(waitingText);
            }

            if (GManager.instance.IsAI)
            {
                SetUseOptional(player.PlayerID, RandomUtility.IsSucceedProbability(0.9f));
            }
        }

        yield return new WaitUntil(() => player.HasPlayerSelection());
        ValueSelection valueSelection = player.DequeuePlayerSelection<ValueSelection>();
        _useOptional = valueSelection != null ? valueSelection.ValueAsBool() : false;

        GManager.instance.selectCommandPanel.Off();

        GManager.instance.BackButton.CloseSelectCommandButton();
        GManager.instance.hideCannotSelectObject.Close();

        GManager.instance.commandText.CloseCommandText();
        yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

        cardEffect.SetUseOptional(_useOptional);

        cardEffect.EffectSourceCard.Owner.TrashHandCard.gameObject.SetActive(false);
    }

    [PunRPC]
    public void SetUseOptional(int playerID, bool useOptional)
    {
        Player selectionPlayer = GManager.instance.GetPlayerFromID(playerID);

        if (selectionPlayer == null)
        {
            return;
        }

        selectionPlayer.QueuePlayerSelection(new ValueSelection(useOptional));
    }
}
