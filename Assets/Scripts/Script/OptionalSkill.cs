using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using System.Linq;
using System;
public class OptionalSkill : MonoBehaviourPunCallbacks
{
    public string waitingText { get; set; } = "The opponent is considering whether to use the effect.";
    bool _endSelect = false;
    bool _useOptional = false;
    public IEnumerator SelectOptional(ICardEffect cardEffect)
    {
        List<string> _YesNoTexts = new List<string>() { "Use", "Not use" };

        _endSelect = false;
        _useOptional = false;

        string _Message = $"Will you use \"{cardEffect.EffectName}\"?";

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

            if (permanent != null)
            {
                if (permanent.ShowingPermanentCard != null)
                {
                    GManager.instance.hideCannotSelectObject.SetUpHideCannotSelectObject(new List<FieldPermanentCard>() { permanent.ShowingPermanentCard }, false);
                }
            }

            GManager.instance.commandText.OpenCommandText(_Message);

            List<Command_SelectCommand> commands = new List<Command_SelectCommand>()
            {
                new Command_SelectCommand(_YesNoTexts[0] ,() => photonView.RPC("SetUseOptional",RpcTarget.All,true),0),
            };

            GManager.instance.BackButton.OpenSelectCommandButton(_YesNoTexts[1], () => { photonView.RPC("SetUseOptional", RpcTarget.All, false); }, 0);

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
                _endSelect = true;
                _useOptional = RandomUtility.IsSucceedProbability(0.9f);
            }
        }

        yield return new WaitWhile(() => !_endSelect);
        _endSelect = false;

        GManager.instance.selectCommandPanel.Off();

        GManager.instance.BackButton.CloseSelectCommandButton();
        GManager.instance.hideCannotSelectObject.Close();

        GManager.instance.commandText.CloseCommandText();
        yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

        cardEffect.SetUseOptional(_useOptional);

        cardEffect.EffectSourceCard.Owner.TrashHandCard.gameObject.SetActive(false);
    }

    [PunRPC]
    public void SetUseOptional(bool useOptional)
    {
        _useOptional = useOptional;
        _endSelect = true;
    }
}
