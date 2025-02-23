using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.BT19
{
    public class BT19_078 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("-1000DP for each digivolution source", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] For each digivolution card of 1 of your [Mother D-Reaper]s, 1 of your opponent's Digimon gets -1000 for the turn.";
                }

                bool IsMotherDReaper(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.EqualsCardName("Mother D-Reaper");
                }

                bool CanSelectOpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentsDigimon))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedMother = null;
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsMotherDReaper,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectMotherCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage($"Choose 1 [Mother D-Reaper].", "The opponent is choosing 1 [Mother D-Reaper].");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectMotherCoroutine(Permanent permanent)
                    {
                        selectedMother = permanent;

                        yield return null;
                    }

                    if(selectedMother != null)
                    {
                        selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentsDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage($"Select 1 Digimon that will get -{1000 * selectedMother.DigivolutionCards.Count} DP.", "The opponent is selecting 1 Digimon that will be DP reduced.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                                            targetPermanent: selectedPermanent,
                                                            changeValue: -1000 * selectedMother.DigivolutionCards.Count,
                                                            effectDuration: EffectDuration.UntilEachTurnEnd,
                                                            activateClass: activateClass));
                        }
                    }                    
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 this card as 1 of your [Mother D-Reaper] digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("PlaceUnder_BT19-078");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Place this Digimon as the bottom digivolution card of 1 of your [Mother D-Reaper] without [ADR-01 Jeri] in its digivolution cards.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (!permanent.IsToken)
                        {
                            if (IsMotherDReaper(permanent))
                            {
                                if(permanent.DigivolutionCards.Count(source => source.EqualsCardName("ADR-01 Jeri")) < 1)
                                    return true;
                            }
                        }
                    }

                    return false;
                }

                bool IsMotherDReaper(Permanent permanent)
                {
                    return  permanent.TopCard.Owner == card.Owner &&
                            permanent.TopCard.EqualsCardName("Mother D-Reaper");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (!card.PermanentOfThisCard().IsToken)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(IsMotherDReaper))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
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

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place in digivolution cards.", "The opponent is selecting 1 Digimon to place in digivolution cards.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                Permanent selectedPermanent = permanent;

                                yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { card.PermanentOfThisCard(), selectedPermanent } }, false, activateClass).PlacePermanentToDigivolutionCards());
                            }
                        }
                    }
                }
            }
            #endregion

            #region When Opponents Attacks
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may play 1 [ADR-01 Jeri] from this Digimon's digivolution cards, then change the attack target to the  Digimon played.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn] When any of your opponent's Digimon attack, you may play 1 [ADR-01 Jeri] from this Digimon's digivolution cards without paying the cost. If you played, you may change the attack target to the Digimon played by this effect.";
                }

                bool IsOpponentDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool IsADRJeri(CardSource source)
                {
                    return CardEffectCommons.CanPlayAsNewPermanent(source, false, activateClass) &&
                           source.EqualsCardName("ADR-01 Jeri");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOpponentTurn(card) &&
                           CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, IsOpponentDigimon);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().DigivolutionCards.Count(IsADRJeri) > 0;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool didPlay = false;
                    Permanent selectedPermanent = null;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsADRJeri,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: () => true,
                        canEndNotMax: false,
                        message: "Select 1 card to play",
                        isShowOpponent: true,
                        canLookReverseCard:false,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: SelectCardCoroutine,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Custom,
                        customRootCardList:card.PermanentOfThisCard().DigivolutionCards,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(List<CardSource> cardSources)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: cardSources,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Custom,
                            activateETB: true));

                        if(cardSources.Count > 0)
                        {
                            didPlay = true;
                            selectedPermanent = cardSources[0].PermanentOfThisCard();
                        }
                    }

                    if (didPlay)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Will you change attack target?";
                        string notSelectPlayerMessage = "The opponent is choosing whether or not to change attack target.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool willRedirect = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (willRedirect)
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(
                                activateClass,
                                false,
                                selectedPermanent));
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}