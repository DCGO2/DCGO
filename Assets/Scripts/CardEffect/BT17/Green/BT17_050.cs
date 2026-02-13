using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.BT17
{
    public class BT17_050 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Hand - Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend one of your opponents Digimon, Then attack an opponents suspended Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsDigimonEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Hand] [Main] By paying 4 cost and placing this card as the bottom digivolution card of 1 of your level 5 or higher Digimon, you may suspend 1 of your opponent’s Digimon and attack an opponent’s Digimon with the Digimon this card was placed under.";
                }

                bool IsLevel5OorHigher(Permanent targetPermanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card))
                    {
                        if (targetPermanent.TopCard.HasLevel && targetPermanent.Level >= 5)
                            return true;
                    }

                    return false;
                }

                bool IsEnemyPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnHand(card))
                        return CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsLevel5OorHigher);

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;
                    bool willsuspend = false;

                    if (CardEffectCommons.HasMatchConditionPermanent(IsLevel5OorHigher))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsLevel5OorHigher));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsLevel5OorHigher,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to add bottom digivolution source.", "The opponent is selecting 1 Digimon to add bottom digivolution source.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(-4, activateClass));
                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(
                                new List<CardSource>() { card },
                                activateClass));
                            }
                        }
                    }

                    if(selectedPermanent != null)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Will you suspend opponents digimon, and attack?";
                        string notSelectPlayerMessage = "The opponent is choosing whether or not to suspend opponents digimon, and attack.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        willsuspend = GManager.instance.userSelectionManager.SelectedBoolValue;
                    }

                    if (willsuspend)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(IsEnemyPermanent))
                        {
                            int enemyCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsEnemyPermanent));

                            SelectPermanentEffect selectEnemyEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectEnemyEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsEnemyPermanent,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: enemyCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Tap,
                                cardEffect: activateClass);

                            selectEnemyEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                            yield return ContinuousController.instance.StartCoroutine(selectEnemyEffect.Activate());

                            if (selectedPermanent.CanAttack(activateClass))
                            {
                                SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                selectAttackEffect.SetUp(
                                    attacker: selectedPermanent,
                                    canAttackPlayerCondition: () => false,
                                    defenderCondition: (permanent) => true,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                            }
                        }
                    }
                }
            }
            #endregion

            #region End of Attack
            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place this digimon as the bottom digivolutions card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Attack] You may place this Digimon as the bottom digivolution card of 1 of your other Digimon.";
                }

                bool CanSelectMyOtherDigimon(Permanent permanent)
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
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectMyOtherDigimon))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectMyOtherDigimon))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectMyOtherDigimon));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectMyOtherDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get a digivolution card.", "The opponent is selecting 1 Digimon that will get a digivolution card.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.CloseBrainstrorm(card));

                                yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { card }, activateClass));
                            }
                        }
                    }
                }
            }
            #endregion

            #region All Turns - Protection
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Parasitemon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When an opponent’s effect would delete this Digimon, you may play 1 [Parasitemon] from this Digimon’s digivolution cards without paying the cost.";
                }

                bool CardEffectCondition(ICardEffect cardEffect)
                {
                    return CardEffectCommons.IsOpponentEffect(cardEffect, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                        {
                            if (cardSource.EqualsCardName("Parasitemon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card))
                        {
                            if (CardEffectCommons.IsByEffect(hashtable, CardEffectCondition))
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
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 1;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource),
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: SelectCardCoroutine,
                                    message: "Select 1 card to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedPermanent.DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select digivolution cards to play.", "The opponent is selecting digivolution cards to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Cards");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if(cardSources != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: cardSources,
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.DigivolutionCards,
                                    activateETB: true));
                            }
                        }
                    }
                }
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 3000, isInheritedEffect: true, card: card, condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}