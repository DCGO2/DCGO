using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT15
{
    public class BT15_024 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.ContainsCardName("Gabumon"))
                    {
                        if (targetPermanent.TopCard.HasLevel)
                        {
                            if (targetPermanent.Level == 3)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1 or Play 1 tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If you have a Tamer with [Matt Ishida] in its name, <Draw 1>. If you don't have a Tamer with [Matt Ishida] in its name, you may play 1 Tamer card with [Matt Ishida] in its name from your hand with the play cost reduced by 3.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.IsTamer)
                        {
                            if (permanent.TopCard.ContainsCardName("Matt Ishida"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: true, cardEffect: activateClass))
                    {
                        if (cardSource.IsTamer)
                        {
                            if (cardSource.ContainsCardName("Matt Ishida"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, PermanentCondition))
                        {
                            if (card.Owner.LibraryCards.Count >= 1)
                            {
                                return true;
                            }
                        }

                        if (!CardEffectCommons.HasMatchConditionOwnersPermanent(card, PermanentCondition))
                        {
                            if (card.Owner.HandCards.Count >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, PermanentCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }

                    if (!CardEffectCommons.HasMatchConditionOwnersPermanent(card, PermanentCondition))
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Will you play 1 [Matt Ishida] from your hand?";
                        string notSelectPlayerMessage = "The opponent is choosing whether or not to play a Tamer.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool willPlayMatt = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (willPlayMatt)
                        {
                            #region reduce play cost

                            ChangeCostClass changeCostClass = new ChangeCostClass();
                            changeCostClass.SetUpICardEffect($"Play Cost -3", CanUseCondition1, card);
                            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                            Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                            card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                            ICardEffect GetCardEffect(EffectTiming _timing)
                            {
                                if (_timing == EffectTiming.None)
                                {
                                    return changeCostClass;
                                }

                                return null;
                            }

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return true;
                            }

                            int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                            {
                                if (CardSourceCondition(cardSource))
                                {
                                    if (RootCondition(root))
                                    {
                                        if (PermanentsCondition(targetPermanents))
                                        {
                                            Cost -= 3;
                                        }
                                    }
                                }

                                return Cost;
                            }

                            bool PermanentsCondition(List<Permanent> targetPermanents)
                            {
                                if (targetPermanents == null)
                                {
                                    return true;
                                }
                                else
                                {
                                    if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool CardSourceCondition(CardSource cardSource)
                            {
                                if (cardSource.IsTamer)
                                {
                                    if (cardSource.ContainsCardName("Matt Ishida"))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool RootCondition(SelectCardEffect.Root root)
                            {
                                return true;
                            }

                            bool isUpDown()
                            {
                                return true;
                            }

                            #endregion

                            if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                            {
                                List<CardSource> selectedCards = new List<CardSource>();

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

                                selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                                yield return StartCoroutine(selectHandEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);

                                    yield return null;
                                }

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: selectedCards,
                                    activateClass: activateClass,
                                    payCost: true,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Hand,
                                    activateETB: true));
                            }

                            #region release effect reducing play cost

                            card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);

                            #endregion
                        }
                    }
                }
            }

            #endregion

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Draw1_BT15_024");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking][Once Per Turn] When this Digimon attacks a player, <Draw 1>. (Draw 1 card from your deck.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }

            return cardEffects;
        }
    }
}