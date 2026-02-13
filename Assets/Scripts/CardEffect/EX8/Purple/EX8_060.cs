using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

namespace DCGO.CardEffects.EX8
{
    public class EX8_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4 && targetPermanent.TopCard.EqualsTraits("NSo");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region When Attacking
            
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may play 1 3 cost or less NSo Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] You may play 1 [NSo] trait Digimon card with a play cost of 3 or less from your trash without paying the cost.";
                }

                bool DigimonToPlay(CardSource cardSource)
                {
                    return CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Trash) &&
                           cardSource.IsDigimon &&
                           cardSource.HasPlayCost &&
                           cardSource.GetCostItself <= 3 &&
                           cardSource.EqualsTraits("NSo");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if(CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, DigimonToPlay);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: DigimonToPlay,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: SelectCardCoroutine,
                        message: "Select 1 Digimon card to play.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage(
                        "Select 1 Digimon card to play.",
                        "The opponent is selecting 1 Digimon card to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Play Card");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(List<CardSource> sources)
                    {
                        selectedCards = sources;
                        yield return null;
                    }
                    
                    if (selectedCards.Count > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Trash,
                            activateETB: true));
                    }
                }
            }
            
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DNA Digivolve.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("DNADigivolve_EX8_060");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] [Once Per Turn] When your Digimon are played or digivolve, if any of them have the [NSo] trait, 2 of your Digimon may DNA digivolve into a Digimon card with the [NSo] trait in the hand. Then, that DNA digivolved Digimon may attack.";
                }

                bool PermanentConditionNSoEnterField(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardTraits.Contains("NSo"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentConditionNSoEnterField))
                            {
                                return true;
                            }

                            if (CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentConditionNSoEnterField))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanSelectDNACardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.CanPlayJogress(true) &&
                           cardSource.ContainsTraits("NSo");
                }
                
                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();
                    
                    bool DNADigivolved = false;
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

                    selectHandEffect.SetUpCustomMessage("Select 1 card to DNA digivolve.",
                        "The opponent is selecting 1 card to DNA digivolve.");
                    selectHandEffect.SetNotShowCard();

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (selectedCards.Count >= 1)
                    {
                        foreach (CardSource selectedCard in selectedCards)
                        {
                            if (selectedCard.CanPlayJogress(true))
                            {
                                _jogressEvoRootsFrameIDs = Array.Empty<int>();

                                yield return GManager.instance.photonWaitController.StartWait("Myotismon_EX8_060");

                                if (card.Owner.isYou || GManager.instance.IsAI)
                                {
                                    GManager.instance.selectJogressEffect.SetUp_SelectDigivolutionRoots
                                    (card: selectedCard,
                                        isLocal: true,
                                        isPayCost: true,
                                        canNoSelect: true,
                                        endSelectCoroutine_SelectDigivolutionRoots: EndSelectCoroutineSelectDigivolutionRoots,
                                        noSelectCoroutine: null);

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.selectJogressEffect
                                        .SelectDigivolutionRoots());

                                    IEnumerator EndSelectCoroutineSelectDigivolutionRoots(List<Permanent> permanents)
                                    {
                                        if (permanents.Count == 2)
                                        {
                                            _jogressEvoRootsFrameIDs = permanents.Distinct().ToArray()
                                                .Map(permanent => permanent.PermanentFrame.FrameID);
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
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                        .ShowCardEffect(new List<CardSource>() { selectedCard }, "Played Card", true, true));

                                    PlayCardClass playCard = new PlayCardClass(
                                        cardSources: new List<CardSource>() { selectedCard },
                                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                        payCost: true,
                                        targetPermanent: null,
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Hand,
                                        activateETB: true);

                                    playCard.SetJogress(_jogressEvoRootsFrameIDs);
                                    DNADigivolved = true;
                                    yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());
                                }
                                
                                if (DNADigivolved)
                                {
                                    if (selectedCard.PermanentOfThisCard() != null)
                                    {
                                        if (selectedCard.PermanentOfThisCard().CanAttack(activateClass))
                                        {
                                            SelectAttackEffect selectAttackEffect =
                                                GManager.instance.GetComponent<SelectAttackEffect>();

                                            selectAttackEffect.SetUp(
                                                attacker: selectedCard.PermanentOfThisCard(),
                                                canAttackPlayerCondition: () => true,
                                                defenderCondition: (permanent) => true,
                                                cardEffect: activateClass);

                                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect
                                                .Activate());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            
            #region Inherited Effect
            
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete your another Digimon to unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Unsuspend_EX8_060");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] [Once Per Turn] By deleting 1 of your other Digimon, this Digimon unsuspends.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent != card.PermanentOfThisCard())
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, card.Owner.GetBattleAreaDigimons().Count(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent thisCardPermanent = card.PermanentOfThisCard();

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { permanent },
                            activateClass: activateClass,
                            successProcess: permanents => SuccessProcess(),
                            failureProcess: null));

                            IEnumerator SuccessProcess()
                            {
                                if (thisCardPermanent.TopCard != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { thisCardPermanent }, activateClass).Unsuspend());
                                }
                            }
                        }
                    }
                }
            }
            
            #endregion

            return cardEffects;
        }
            
        #region Photon DNA

        bool _endSelect;
        int[] _jogressEvoRootsFrameIDs = Array.Empty<int>();

        [PunRPC]
        public void SetJogressEvoRootsFrameIDs(int[] jogressEvoRootsFrameIDs)
        {
            this._jogressEvoRootsFrameIDs = jogressEvoRootsFrameIDs;
            _endSelect = true;
        }

        #endregion
    }
}