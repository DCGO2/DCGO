using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX4
{
    public class EX4_063 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Terriermon] or [Lopmon] from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] If you have 1 or fewer Digimon in play, you may play 1 [Terriermon] or [Lopmon] from your hand without paying the cost. Digimon played by this effect can't digivolve and are deleted at the end of your opponent's turn.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardNames.Contains("Terriermon") || cardSource.CardNames.Contains("Lopmon"))
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass, root: SelectCardEffect.Root.Hand))
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
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Count <= 1)
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
                                                payCost: false,
                                                isTapped: false,
                                                root: SelectCardEffect.Root.Hand,
                                                activateETB: true));


                        foreach (CardSource selectedCard in selectedCards)
                        {
                            if (CardEffectCommons.IsExistOnBattleArea(selectedCard))
                            {
                                Permanent selectedPermanent = selectedCard.PermanentOfThisCard();

                                if (selectedPermanent != null)
                                {
                                    CanNotDigivolveClass canNotEvolveClass = new CanNotDigivolveClass();
                                    canNotEvolveClass.SetUpICardEffect("Can't digivolve", CanUseCondition1, card);
                                    canNotEvolveClass.SetUpCanNotEvolveClass(permanentCondition: PermanentCondition, cardCondition: CardCondition);
                                    selectedPermanent.PermanentEffects.Add((_timing) => canNotEvolveClass);

                                    bool CanUseCondition1(Hashtable hashtable)
                                    {
                                        return CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent);
                                    }

                                    bool PermanentCondition(Permanent permanent)
                                    {
                                        return permanent == selectedPermanent;
                                    }

                                    bool CardCondition(CardSource cardSource)
                                    {
                                        return true;
                                    }
                                }

                                if (selectedPermanent != null)
                                {
                                    ActivateClass activateClass1 = new ActivateClass();
                                    activateClass1.SetUpICardEffect("Delete the Digimon", CanUseCondition1, card);
                                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, "");
                                    card.Owner.UntilOpponentTurnEndEffects.Add(GetCardEffect);

                                    bool CanUseCondition1(Hashtable hashtable)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                        {
                                            if (CardEffectCommons.IsOpponentTurn(card))
                                            {
                                                return true;
                                            }
                                        }

                                        return false;
                                    }

                                    bool CanActivateCondition1(Hashtable hashtable)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                        {
                                            if (selectedPermanent.CanBeDestroyedBySkill(activateClass1))
                                            {
                                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass1))
                                                {
                                                    return true;
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(new List<Permanent>() { selectedPermanent }, CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                                    }

                                    ICardEffect GetCardEffect(EffectTiming _timing)
                                    {
                                        if (_timing == EffectTiming.OnEndTurn)
                                        {
                                            return activateClass1;
                                        }

                                        return null;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("Digisorption-1_EX4_063");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When one of your Digimon with [Terriermon] or [Lopmon] in its digivolution cards would digivolve, by suspending this Tamer, reduce the digivolution cost by 1.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.DigivolutionCards.Count((evoRoot) => evoRoot.CardNames.Contains("Terriermon") || evoRoot.CardNames.Contains("Lopmon")) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return true;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentCondition, CardCondition))
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
                        if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                        new List<Permanent>() { card.PermanentOfThisCard() },
                        CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition1, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

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
                                    Cost -= 1;
                                }
                            }
                        }

                        return Cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        if (targetPermanents != null)
                        {
                            if (targetPermanents.Count(PermanentCondition) >= 1)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    bool PermanentCondition(Permanent targetPermanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card);
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        return cardSource.Owner == card.Owner;
                    }

                    bool RootCondition(SelectCardEffect.Root root)
                    {
                        return true;
                    }

                    bool isUpDown()
                    {
                        return true;
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            return cardEffects;
        }
    }
}