using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT16
{
    public class BT16_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce opponent's Digimon cost, then delete a 6 cost or less Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Reveal the top 3 cards of your deck. For each card with the [D-Brigade] or [DigiPolice] trait among them, reduce the play cost of all of your opponent's Digimon by 1 for the turn. Return the revealed cards to the top or bottom of the deck. Then, delete 1 of your opponent's Digimon with a play cost of 4 or less.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasPlayCost && permanent.TopCard.GetCostItself <= 4)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("D-Brigade") || cardSource.CardTraits.Contains("DigiPolice"))
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
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int costReduction = 0;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                        revealCount: 3,
                        simplifiedSelectCardCondition:
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: (cardSource) => false,
                            message: "",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: -1,
                            selectCardCoroutine: null),
                        remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                        activateClass: activateClass,
                        revealedCardsCoroutine: RevealedCardsCoroutine
                    ));

                    IEnumerator RevealedCardsCoroutine(List<CardSource> revealedCards)
                    {
                        costReduction = revealedCards.Count(CardCondition);

                        if (costReduction > 0)
                        {
                            bool PermanentCondition(Permanent permanent)
                            {
                                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangePlayCostPlayerEffect(
                                permanentCondition: PermanentCondition,
                                changeValue: -costReduction,
                                setFixedCost: false,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                        }

                        yield return null;
                    }

                    if (CardEffectCommons.MatchConditionOpponentsPermanentCount(card, PermanentCondition) > 0)
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: PermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 card to delete.", "The opponent is selecting 1 card to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce opponent's Digimon cost, then delete a 4 cost or less Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Reveal the top 3 cards of your deck. For each card with the [D-Brigade] or [DigiPolice] trait among them, reduce the play cost of all of your opponent's Digimon by 1 for the turn. Return the revealed cards to the top or bottom of the deck. Then, delete 1 of your opponent's Digimon with a play cost of 4 or less.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasPlayCost && permanent.TopCard.GetCostItself <= 4)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("D-Brigade") || cardSource.CardTraits.Contains("DigiPolice"))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int costReduction = 0;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndProcessForAll(
                        revealCount: 3,
                        simplifiedSelectCardCondition:
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition: (cardSource) => false,
                            message: "",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: -1,
                            selectCardCoroutine: null),
                        remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                        activateClass: activateClass,
                        revealedCardsCoroutine: RevealedCardsCoroutine
                    ));

                    IEnumerator RevealedCardsCoroutine(List<CardSource> revealedCards)
                    {
                        costReduction = revealedCards.Count(CardCondition);

                        if (costReduction > 0)
                        {
                            bool PermanentCondition(Permanent permanent)
                            {
                                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangePlayCostPlayerEffect(
                                permanentCondition: PermanentCondition,
                                changeValue: -costReduction,
                                setFixedCost: false,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                        }

                        yield return null;
                    }

                    if (CardEffectCommons.MatchConditionOpponentsPermanentCount(card, PermanentCondition) > 0)
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: PermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 card to delete.", "The opponent is selecting 1 card to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            #region All Turns - ESS

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("De-Digivolve 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("De-Digivolve_BT16_060");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When one of your other Digimon with the [D-Brigade] or [DigiPolice] trait is played, <De-Digivolve 1> 1 of your opponent's Digimon.";
                }

                bool OpponentPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard != card)
                        {
                            if (permanent.TopCard.IsDigimon)
                            {
                                if (permanent.TopCard.CardTraits.Contains("D-Brigade") || permanent.TopCard.CardTraits.Contains("DigiPolice"))
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
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.MatchConditionOpponentsPermanentCount(card, permanent => permanent.IsDigimon) > 0)
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectCardCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 card to de-digivolve.", "The opponent is selecting 1 card to de-digivolve.");

                        yield return StartCoroutine(selectPermanentEffect.Activate());
                    }

                    IEnumerator SelectCardCoroutine(Permanent permanent)
                    {
                        if (permanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IDegeneration(permanent, 1, activateClass).Degeneration());
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}