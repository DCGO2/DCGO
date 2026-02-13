using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX8
{
    public class EX8_012 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Growlmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card, condition: null)
                );
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1 and trash 1 card from hand. Then this Digimon gains On Deletion.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] <Draw 1> and trash 1 card in your hand. Then, if [Growlmon] or [X Antibody] is in this Digimon's digivolution cards, until the end of your opponent's turn, this Digimon gains \"<On Deletion> You may play 1 card with [Guilmon] in its name from your trash without paying the cost.\"";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.LibraryCards.Count >= 1)
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                    if (card.Owner.HandCards.Count >= 1)
                    {
                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());
                    }

                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => (cardSource.EqualsCardName("Growlmon") || cardSource.EqualsCardName("X Antibody"))) >= 1)
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();
                        CardSource _topCard = selectedPermanent.TopCard;

                        ActivateClass activateClass1 = new ActivateClass();
                        activateClass1.SetUpICardEffect("Play 1 [Guilmon] from trash", CanUseCondition1, selectedPermanent.TopCard);
                        activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, true, EffectDiscription1());
                        activateClass1.SetEffectSourcePermanent(selectedPermanent);
                        CardEffectCommons.AddEffectToPermanent(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilOpponentTurnEnd, card: card, cardEffect: activateClass1, timing: EffectTiming.OnDestroyedAnyone);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));

                        string EffectDiscription1()
                        {
                            return "[On Deletion] You may play 1 card with [Guilmon] in its name from your trash without paying the cost.";
                        }

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            if (cardSource.ContainsCardName("Guilmon"))
                            {
                                if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool CanUseCondition1(Hashtable hashtable1)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(selectedPermanent))
                            {
                                if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable1, (permanent) => permanent == selectedPermanent))
                                {
                                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        bool CanActivateCondition1(Hashtable hashtable1)
                        {
                            if (CardEffectCommons.IsTopCardInTrashOnDeletion(hashtable1))
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(_topCard, CanSelectCardCondition))
                                {
                                    return true;                                   
                                }
                            }

                            return false;
                        }

                        IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(_topCard, (cardSource) => CanSelectCardCondition(cardSource)))
                            {
                                int maxCount = Math.Min(1, _topCard.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                                List<CardSource> selectedCards = new List<CardSource>();

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                            canTargetCondition: CanSelectCardCondition,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: null,
                                            canNoSelect: () => true,
                                            selectCardCoroutine: SelectCardCoroutine,
                                            afterSelectCardCoroutine: null,
                                            message: "Select 1 card with [Guilmon] in its name to play.",
                                            maxCount: maxCount,
                                            canEndNotMax: false,
                                            isShowOpponent: true,
                                            mode: SelectCardEffect.Mode.Custom,
                                            root: SelectCardEffect.Root.Trash,
                                            customRootCardList: null,
                                            canLookReverseCard: true,
                                            selectPlayer: _topCard.Owner,
                                            cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage("Select 1 card with [Guilmon] in its name to play.", "The opponent is selecting 1 card to play.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                                yield return StartCoroutine(selectCardEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);

                                    yield return null;
                                }

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass1, payCost: false, isTapped: false, root: SelectCardEffect.Root.Trash, activateETB: true));
                            }
                        }
                    }
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Memory+1_EX8_012");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn][Once Per Turn] When any of your opponent's Digimon is deleted, gain 1 memory.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition))
                            {
                                return true;
                            }
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
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}