using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT5_104 : CEntity_Effect
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
                return "[Main] Trigger <De-Digivolve 2> on 1 of your opponent's Digimon. (Trash up to 2 cards from the top of one of your opponent's Digimon. If it has no digivolution cards, or becomes a level 3 Digimon, you can't trash any more cards.) Then, if you have a [Diaboromon] in play, you may play 1 [Diaboromon] Token without paying its memory cost. (Diaboromon Tokens are level 6 white Digimon with a memory cost of 14, 3000 DP, and are Mega form, Unidentified type, and Unknown attribute.)";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.", "The opponent is selecting 1 Digimon to De-Digivolve.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        Permanent selectedPermanent = permanent;

                        yield return ContinuousController.instance.StartCoroutine(new IDegeneration(selectedPermanent, 2, activateClass).Degeneration());
                    }
                }

                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent.TopCard.CardNames.Contains("Diaboromon")))
                {
                    if (card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame()) >= 1)
                    {
                        if (card.Owner.isYou)
                        {
                            GManager.instance.commandText.OpenCommandText("Will you play a [Diaboromon] Token?");

                            List<Command_SelectCommand> command_SelectCommands = new List<Command_SelectCommand>()
                                {
                                    new Command_SelectCommand($"Play 1 Token", () => photonView.RPC("SetPlayToken", RpcTarget.All, true), 0),
                                    new Command_SelectCommand($"Not Play", () => photonView.RPC("SetPlayToken", RpcTarget.All, false), 1),
                                };

                            GManager.instance.selectCommandPanel.SetUpCommandButton(command_SelectCommands);
                        }

                        else
                        {
                            GManager.instance.commandText.OpenCommandText("The opponent is choosing whether to play a token.");

                            #region AIƒ‚[ƒh
                            if (GManager.instance.IsAI)
                            {
                                SetPlayToken(RandomUtility.IsSucceedProbability(0.5f));
                            }
                            #endregion
                        }

                        yield return new WaitWhile(() => !endSelect);
                        endSelect = false;

                        GManager.instance.commandText.CloseCommandText();
                        yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                        if (playToken)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayDiaboromonToken(activateClass));
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"De-Digivolve 2 to 1 Digimon and play a [Diaboromon] Token");
        }

        return cardEffects;
    }

    bool endSelect = false;
    bool playToken = false;

    [PunRPC]
    public void SetPlayToken(bool playToken)
    {
        this.playToken = playToken;
        endSelect = true;
    }
}
