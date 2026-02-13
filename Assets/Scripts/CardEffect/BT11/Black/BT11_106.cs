using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT11
{
    public class BT11_106 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect($"Play Cost -1", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.TopCard.CardColors.Contains(CardColor.Black) && permanent.IsTamer))
                    {
                        return true;
                    }

                    return false;
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
                    return true;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
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

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Until the end of your opponent's turn, 1 of your Digimon with [Numemon], [Sukamon], [Nanimon], or [Etemon] in its name can't be blocked and gains \"[On Deletion] Gain 3 memory.\"";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.ContainsCardName("Numemon"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.ContainsCardName("Sukamon"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.ContainsCardName("Nanimon"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.ContainsCardName("Etemon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = 1;

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

                        selectPermanentEffect.SetUpCustomMessage(
                            "Selcect 1 Digimon that will get effects.",
                            "The opponent is selecting 1 Digimon that will get effects.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBeBlocked(
                                targetPermanent: permanent,
                                defenderCondition: null,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                effectName: "Unblockable"));

                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                CardSource _topCard = selectedPermanent.TopCard;

                                ActivateClass activateClass1 = new ActivateClass();
                                activateClass1.SetUpICardEffect("Memory +3", CanUseCondition1, selectedPermanent.TopCard);
                                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                                activateClass1.SetEffectSourcePermanent(selectedPermanent);
                                CardEffectCommons.AddEffectToPermanent(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilOpponentTurnEnd, card: card, cardEffect: activateClass1, timing: EffectTiming.OnDestroyedAnyone);

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));

                                string EffectDiscription1()
                                {
                                    return "[On Deletion] Gain 3 memory.";
                                }

                                bool CanUseCondition1(Hashtable hashtable1)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
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
                                        if (_topCard.Owner.CanAddMemory(activateClass1))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(_topCard.Owner.AddMemory(3, activateClass1));
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Reveal the top 3 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Reveal the top 3 cards of your deck. You may play 1 black Digimon card with a play cost of 3 or less among them without paying the cost. Trash the rest.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        if (cardSource.GetCostItself <= 3)
                        {
                            if (cardSource.HasPlayCost)
                            {
                                if (cardSource.IsDigimon)
                                {
                                    if (cardSource.CardColors.Contains(CardColor.Black))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 black Digimon card with a play cost of 3 or less.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: SelectCardCoroutine),
                        },
                        remainingCardsPlace: RemainingCardsPlace.Trash,
                        activateClass: activateClass,
                        canNoSelect: true
                    ));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: selectedCards,
                        activateClass: activateClass,
                        payCost: false, isTapped: false,
                        root: SelectCardEffect.Root.Library,
                        activateETB: true));
                }
            }

            return cardEffects;
        }
    }
}