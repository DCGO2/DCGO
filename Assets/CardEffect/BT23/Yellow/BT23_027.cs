using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Angemon 
namespace DCGO.CardEffects.BT23
{
    public class BT23_027 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Patamon")
                        || (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.IsLevel3 && targetPermanent.TopCard.HasCSTraits);

                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Barrier

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(false, card, null));
            }

            #endregion

            #region OP/WD Shared

            bool CanSelectDNACardCondition(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.CanPlayJogress(true))
                    {
                        if (cardSource.CardNames.Contains("Shakkoumon"))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                if (CardEffectCommons.IsOwnerTurn(card)
                    && card.Owner.GetBattleAreaDigimons().Count >= 2
                    && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectDNACardCondition))
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    #region Select Shakkoumon

                    int maxCount = 1;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDNACardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);


                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        yield return null;
                    }

                    selectHandEffect.SetUpCustomMessage("Select 1 [Shakkoumon] to DNA digivolve into.", "The opponent is selecting 1 card to DNA digivolve.");
                    selectHandEffect.SetNotShowCard();

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    #endregion

                    if (selectedCards.Count >= 1)
                    {
                        #region DNA Effect

                        foreach (CardSource selectedCard in selectedCards)
                        {
                            if (selectedCard.CanPlayJogress(true))
                            {
                                _jogressEvoRootsFrameIDs = new int[0];

                                yield return GManager.instance.photonWaitController.StartWait("Angemon_BT23_027");

                                if (card.Owner.isYou || GManager.instance.IsAI)
                                {
                                    GManager.instance.selectJogressEffect.SetUp_SelectDigivolutionRoots
                                                                (card: selectedCard,
                                                                isLocal: true,
                                                                isPayCost: true,
                                                                canNoSelect: true,
                                                                endSelectCoroutine_SelectDigivolutionRoots: EndSelectCoroutine_SelectDigivolutionRoots,
                                                                noSelectCoroutine: null);

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.selectJogressEffect.SelectDigivolutionRoots());

                                    IEnumerator EndSelectCoroutine_SelectDigivolutionRoots(List<Permanent> permanents)
                                    {
                                        if (permanents.Count == 2)
                                        {
                                            _jogressEvoRootsFrameIDs = permanents.Distinct().ToArray().Map(permanent => permanent.PermanentFrame.FrameID);
                                        }

                                        yield return null;
                                    }

                                    photonView.RPC("SetJogressEvoRootsFrameIDs", RpcTarget.All, _jogressEvoRootsFrameIDs);
                                }
                                else
                                {
                                    GManager.instance.commandText.OpenCommandText("The opponent is choosing a card to DNA digivolve.");
                                }

                                yield return new WaitWhile(() => !_endSelect);
                                _endSelect = false;

                                GManager.instance.commandText.CloseCommandText();
                                yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

                                if (_jogressEvoRootsFrameIDs.Length == 2)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { selectedCard }, "Played Card", true, true));

                                    PlayCardClass playCard = new PlayCardClass(
                                        cardSources: new List<CardSource>() { selectedCard },
                                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                        payCost: true,
                                        targetPermanent: null,
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Hand,
                                        activateETB: true);

                                    playCard.SetJogress(_jogressEvoRootsFrameIDs);

                                    yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());
                                }
                            }
                        }

                        #endregion
                    }
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1. then if its your turn, you may DNA into [Shakkoumon] from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] <Draw 1>. Then if it's your turn, 2 of your Digimon may DNA digivolve into [Shakkoumon] in the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1. then if its your turn, you may DNA into [Shakkoumon] from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] <Draw 1>. Then if it's your turn, 2 of your Digimon may DNA digivolve into [Shakkoumon] in the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region Barrier - ESS

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }

            #endregion

            return cardEffects;
        }

        #region Jogress Required
        private bool _endSelect = false;
        private int[] _jogressEvoRootsFrameIDs = new int[0];

        [PunRPC]
        public void SetJogressEvoRootsFrameIDs(int[] JogressEvoRootsFrameIDs)
        {
            this._jogressEvoRootsFrameIDs = JogressEvoRootsFrameIDs;
            _endSelect = true;
        }
        #endregion
    }
}