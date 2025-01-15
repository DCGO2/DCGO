using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT3_102 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] Your opponent may trash the top card of their security stack. If they don't, trigger <Recovery +1 (Deck)>. (Place the top card of your deck on top of your security stack.)";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.Enemy.SecurityCards.Count == 0)
                {
                    SetDoDiscard(false);
                }

                else
                {
                    if (!card.Owner.isYou)
                    {
                        GManager.instance.commandText.OpenCommandText("Will you discard the top card of your security?");

                        List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>()
                                {
                                    new Command_SelectCommand($"Discard", () => photonView.RPC("SetDoDiscard", RpcTarget.All, true), 0),
                                    new Command_SelectCommand($"Not Discard", () => photonView.RPC("SetDoDiscard", RpcTarget.All, false), 1),
                                };

                        GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);
                    }

                    else
                    {
                        GManager.instance.commandText.OpenCommandText("The opponent is choosing whether to discard security.");

                        #region AI
                        if (GManager.instance.IsAI)
                        {
                            SetDoDiscard(RandomUtility.IsSucceedProbability(0.5f));
                        }
                        #endregion
                    }
                }

                yield return new WaitWhile(() => !endSelect);
                endSelect = false;

                GManager.instance.commandText.CloseCommandText();
                yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                if (!doDiscard)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                }

                else
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                    player: card.Owner.Enemy,
                    destroySecurityCount: 1,
                    cardEffect: activateClass,
                    fromTop: true).DestroySecurity());
                }
            }
        }

        return cardEffects;
    }

    bool endSelect = false;
    bool doDiscard = false;

    [PunRPC]
    public void SetDoDiscard(bool doDiscard)
    {
        this.doDiscard = doDiscard;
        endSelect = true;
    }
}
