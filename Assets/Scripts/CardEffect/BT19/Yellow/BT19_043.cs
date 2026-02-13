using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.BT19
{
    public class BT19_043 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Lucemon") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level >= 5;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash both players top security card to prevent this Digimon from leaving Battle Area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("TrashSecurityToStay_LucemonXAntibody_BT19_043");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns][Once Per Turn] When this Digimon would leave the battle area, if a card with [Lucemon] in its name is in this Digimon's digivolution cards, by trashing both players' top security cards, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card))
                        {
                           return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.ContainsCardName("Lucemon")) >= 1)
                        {
                            if (card.Owner.SecurityCards.Count >= 1 && card.Owner.Enemy.SecurityCards.Count >= 1)
                                return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                     yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                         player: card.Owner,
                         destroySecurityCount: 1,
                         cardEffect: activateClass,
                         fromTop: true).DestroySecurity());

                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                         player: card.Owner.Enemy,
                         destroySecurityCount: 1,
                         cardEffect: activateClass,
                         fromTop: true).DestroySecurity());


                    Permanent thisCardPermanent = card.PermanentOfThisCard();

                     thisCardPermanent.willBeRemoveField = false;

                     thisCardPermanent.HideDeleteEffect();
                     thisCardPermanent.HideHandBounceEffect();
                     thisCardPermanent.HideDeckBounceEffect();
                     thisCardPermanent.HideWillRemoveFieldEffect();

                     yield return null;                    
                }
            }
            #endregion

            #region End of Your Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent may trash 1 card from secuirty or Recovery +1 and delete 1 Digimon or Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("TrashSecurity_LucemonXAntibody_BT19_043");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] [Once Per Turn] Your opponent may trash their top security card. If this effect didn't trash, <Recovery +1 (Deck)>, and delete 1 of their Digimon or Tamers.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;                   
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (permanent != null)
                    {
                        if (permanent.TopCard != null)
                        {
                            if (permanent.TopCard.Owner == card.Owner.Enemy)
                            {
                                if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                {                                 
                                    if (permanent.IsDigimon || permanent.IsTamer)
                                    {
                                        return true;
                                    }                                   
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {   
                    if(card.Owner.Enemy.SecurityCards.Count > 0)
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

                        yield return new WaitWhile(() => !endSelect);
                        endSelect = false;

                        GManager.instance.commandText.CloseCommandText();
                        yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);
                    }
                    else
                    {
                        SetDoDiscard(false);
                    }
                    

                    if (!doDiscard)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());

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
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
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
            #endregion

            return cardEffects;
        }

        #region Extras
        bool endSelect = false;
        bool doDiscard = false;

        [PunRPC]
        public void SetDoDiscard(bool doDiscard)
        {
            this.doDiscard = doDiscard;
            endSelect = true;
        }
        #endregion 
    }
}