using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCGO.CardEffects.BT18
{
    public class BT18_034 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Cupimon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 5,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }
            #endregion

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 card from hand to trash the opponents top security or Recovery +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By trashing 1 card in your hand, your opponent may trash their top security card. If this effect didn't trash, <Revovery +1>";
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
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.HandCards.Count >= 1)
                    {
                        bool discarded = false;

                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }

                        if (discarded)
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
                }
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 card from hand to trash the opponents top security or Recovery +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By trashing 1 card in your hand, your opponent may trash their top security card. If this effect didn't trash, <Revovery +1>";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.HandCards.Count >= 1)
                    {
                        bool discarded = false;

                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }

                        if (discarded)
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
                }
            }
            #endregion

            #region End of Your Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place one of your level 6 Digimon on top of your security stack to digivolve into [Lucemon: Chaos Mode] in the trash without paying the cost.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EOT_BT18-034");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[End of Your Turn] [Once Per Turn] By placing one of your level 6 Digimon on top of your security stack, this Digimon may digivolve into [Lucemon: Chaos Mode] in the trash without paying the cost.";
                }

                bool IsPermanentLevel6Condition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.IsLevel6)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool IsCardLucemonChaosModeCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.ContainsCardName("Lucemon: Chaos Mode") || cardSource.ContainsCardName("Lucemon:ChaosMode") || cardSource.ContainsCardName("Lucemon Chaos Mode") || cardSource.ContainsCardName("LucemonChaosMode"))
                        {
                            if (cardSource.CanEvolve(card.PermanentOfThisCard(), true))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
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
                        if (CardEffectCommons.HasMatchConditionPermanent(IsPermanentLevel6Condition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsPermanentLevel6Condition))
                    {
                        Permanent selectedPermanent = null;

                        int maxCount = Math.Min(1,
                            CardEffectCommons.MatchConditionPermanentCount(IsPermanentLevel6Condition));

                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsPermanentLevel6Condition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage(
                            "Select 1 Digimon to place on top of your security stack.",
                            "The opponent is selecting 1 Digimon to place on top of their security stack.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            if (selectedPermanent.TopCard != null)
                            {
                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    CardSource securityCard = selectedPermanent.TopCard;

                                    yield return ContinuousController.instance.StartCoroutine(
                                        new IPutSecurityPermanent(selectedPermanent,
                                            CardEffectCommons.CardEffectHashtable(activateClass),
                                            toTop: true).PutSecurity());

                                    if (selectedPermanent.TopCard == null)
                                    {
                                        if (securityCard.Owner.SecurityCards.Contains(securityCard))
                                        {
                                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsCardLucemonChaosModeCondition))
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(
                                                CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                                    targetPermanent: card.PermanentOfThisCard(),
                                                    cardCondition: IsCardLucemonChaosModeCondition,
                                                    payCost: false,
                                                    reduceCostTuple: null,
                                                    fixedCostTuple: null,
                                                    ignoreDigivolutionRequirementFixedCost: -1,
                                                    isHand: false,
                                                    activateClass: activateClass,
                                                    successProcess: null));
                                            }
                                        }
                                    }
                                }
                            }
                        }
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