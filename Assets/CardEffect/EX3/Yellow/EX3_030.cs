using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX3
{
    public class EX3_030 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal the top 4 cards of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Reveal the top 4 cards of your deck. Add 1 yellow card with [Angel], [Cherub], [Throne], [Authority], [Seraph] or [Virtue], other than [Three Great Angels], in one of its traits and 1 card with the [Four Great Dragons] trait among them to your hand. Place the rest at the bottom of your deck in any order.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardColors.Contains(CardColor.Yellow))
                    {
                        if (cardSource.HasAngelTraits)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("Four Great Dragons"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("FourGreatDragons"))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 4,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 yellow card with [Angel], [Cherub], [Throne], [Authority], [Seraph] or [Virtue], other than [Three Great Angels], in one of its traits.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition1,
                            message: "Select 1 card with the [Four Great Dragons] trait.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass
                    ));
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("The played Digimon gains Rush", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("GainRush_EX3_030");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When you play a Digimon with the [Four Great Dragons] trait, 1 of those Digimon gains <Rush> for the turn. (This Digimon may attack the turn it was played.)";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardTraits.Contains("Four Great Dragons"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardTraits.Contains("FourGreatDragons"))
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
                            if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition))
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

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<Permanent> permanents = CardEffectCommons.GetPlayedPermanentsFromEnterFieldHashtable(
                        hashtable: _hashtable,
                        rootCondition: null);

                    if (permanents != null)
                    {
                        bool CanSelectPermanentCondition(Permanent permanent)
                        {
                            if (PermanentCondition(permanent))
                            {
                                if (permanents.Contains(permanent))
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            Permanent selectedPermanent = null;

                            if (card.Owner.GetBattleAreaDigimons().Count(CanSelectPermanentCondition) == 1)
                            {
                                selectedPermanent = card.Owner.GetBattleAreaDigimons().Find(CanSelectPermanentCondition);
                            }

                            else
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

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get Rush.", "The opponent is selecting 1 Digimon that will get Rush.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    selectedPermanent = permanent;

                                    yield return null;
                                }
                            }

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(
                                    targetPermanent: selectedPermanent,
                                    effectDuration: EffectDuration.UntilEachTurnEnd,
                                    activateClass: activateClass));
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}