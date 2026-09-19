using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX7
{
    public class EX7_013 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Three Musketeers") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5 || targetPermanent.TopCard.HasText("ThreeMusketeers") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Option then Draw", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may use 1 Option card with the [Three Musketeers] trait from your hand without paying the cost. Then, draw cards until there are 6 cards in your hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsOption)
                    {
                        if (cardSource.CardTraits.Contains("Three Musketeers") || cardSource.CardTraits.Contains("ThreeMusketeers"))
                        {
                            if (cardSource.HasUseCost)
                            {
                                if (!cardSource.CanNotPlayThisOption)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                    {
                        int maxCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
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

                        selectHandEffect.SetUpCustomMessage(
                            "Select 1 option card to use.",
                            "The opponent is selecting 1 option card to use.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Used Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }
                    }

                    if (selectedCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayOptionCards(
                                cardSources: selectedCards,
                                activateClass: activateClass,
                                payCost: false,
                                root: SelectCardEffect.Root.Hand
                            )
                        );
                    }

                    if (card.Owner.LibraryCards.Count >= 1 && card.Owner.HandCards.Count < 6)
                    {
                        while (card.Owner.HandCards.Count < 6)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                        }
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Option then Draw", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may use 1 Option card with the [Three Musketeers] trait from your hand without paying the cost. Then, draw cards until there are 6 cards in your hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsOption)
                    {
                        if (cardSource.CardTraits.Contains("Three Musketeers") || cardSource.CardTraits.Contains("ThreeMusketeers"))
                        {
                            if (cardSource.HasUseCost)
                            {
                                if (!cardSource.CanNotPlayThisOption)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if(CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if(CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
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

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                    {
                        int maxCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
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

                        selectHandEffect.SetUpCustomMessage(
                            "Select 1 option card to use.",
                            "The opponent is selecting 1 option card to use.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Used Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }
                    }

                    if (selectedCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayOptionCards(
                                cardSources: selectedCards,
                                activateClass: activateClass,
                                payCost: false,
                                root: SelectCardEffect.Root.Hand
                            )
                        );
                    }

                    if (card.Owner.LibraryCards.Count >= 1 && card.Owner.HandCards.Count < 6)
                    {
                        while (card.Owner.HandCards.Count < 6)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                        }
                    }
                }
            }
            #endregion

            #region End of Your Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 digivolution card to give <Security Atk +1> and attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("ReturnOption_EX7-013");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] [Once Per Turn] By trashing 1 Option card in this Digimon's digivolution card, 1 of your Digimon gains <Security Attack +1> for the turn and that Digimon attacks.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsOption)
                    {
                        if (!cardSource.CanNotTrashFromDigivolutionCards(activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
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
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool trashed = false;
                    Permanent selectedPermanent = null;

                    List<CardSource> selectedCards = new List<CardSource>();

                     SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                     selectCardEffect.SetUp(
                                 canTargetCondition: CanSelectCardCondition,
                                 canTargetCondition_ByPreSelecetedList: null,
                                 canEndSelectCondition: null,
                                 canNoSelect: () => false,
                                 selectCardCoroutine: SelectCardCoroutine,
                                 afterSelectCardCoroutine: null,
                                 message: "Select 1 Option card to discard.",
                                 maxCount: 1,
                                 canEndNotMax: false,
                                 isShowOpponent: true,
                                 mode: SelectCardEffect.Mode.Custom,
                                 root: SelectCardEffect.Root.Custom,
                                 customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                 canLookReverseCard: true,
                                 selectPlayer: card.Owner,
                                 cardEffect: null);

                     selectCardEffect.SetUpCustomMessage("Select 1 Option card to discard.", "The opponent is selecting 1 Option card to discard.");

                     yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                     IEnumerator SelectCardCoroutine(CardSource cardSource)
                     {
                         selectedCards.Add(cardSource);

                         yield return null;
                     }

                     if (selectedCards.Count >= 1)
                     {
                         yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(card.PermanentOfThisCard(), selectedCards, activateClass).TrashDigivolutionCards());

                         trashed = true;
                     }

                     if (trashed)
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

                             selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get Security Attack +1.", "The opponent is selecting 1 Digimon that will get Security Attack +1.");

                             yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                             IEnumerator SelectPermanentCoroutine(Permanent permanent)
                             {
                                selectedPermanent = permanent;

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                                    targetPermanent: permanent, 
                                    changeValue: 1, 
                                    effectDuration: EffectDuration.UntilEachTurnEnd, 
                                    activateClass: activateClass));
                             }

                            if(selectedPermanent != null)
                            {
                                if (selectedPermanent.CanAttack(activateClass))
                                {
                                    SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                    selectAttackEffect.SetUp(
                                        attacker: selectedPermanent,
                                        canAttackPlayerCondition: () => true,
                                        defenderCondition: (permanent) => true,
                                        cardEffect: activateClass);

                                    selectAttackEffect.SetCanNotSelectNotAttack();

                                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                }
                            }
                         }
                     }                    
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
