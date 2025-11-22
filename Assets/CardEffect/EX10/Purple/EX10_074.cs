using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//Beelzemon Ace
namespace DCGO.CardEffects.EX10
{
    public class EX10_074 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return card.Owner.TrashCards.Count >= 20;
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasImpmonName &&
                           targetPermanent.TopCard.IsLevel3;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: Condition));
            }

            #endregion

            #region Blast Digivolve

            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }

            #endregion

            #region Trash, then Delete effect

            string FirstEffectShortText() => "Trash top 2 from deck, Delete 1 play cost 6 or higher";
            string FirstEffectDiscription(string tag)
            {
                return $"[{tag}] Trash the top 2 cards of your deck. Then, delete 1 of your opponent's play cost 6 or lower Digimon. For every 10 cards in your trash, add 3 to the play cost maximum.";
            }

            bool FirstCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            IEnumerator FirstActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(2, card.Owner, activateClass).AddTrashCardsFromLibraryTop());

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.HasPlayCost &&
                           permanent.TopCard.GetCostItself <= 6 + (3 * Mathf.FloorToInt(card.Owner.TrashCards.Count / 10));
                }

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(FirstEffectShortText(), CanUseCondition, card);
                activateClass.SetUpActivateClass(FirstCanActivateCondition, (hashtable) => FirstActivateCoroutine(hashtable, activateClass), -1, false, FirstEffectDiscription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(FirstEffectShortText(), CanUseCondition, card);
                activateClass.SetUpActivateClass(FirstCanActivateCondition, (hashtable) => FirstActivateCoroutine(hashtable, activateClass), -1, false, FirstEffectDiscription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(FirstEffectShortText(), CanUseCondition, card);
                activateClass.SetUpActivateClass(FirstCanActivateCondition, (hashtable) => FirstActivateCoroutine(hashtable, activateClass), -1, false, FirstEffectDiscription("When Attacking"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
            }

            #endregion

            #endregion

            #region Return 2, De-Digivolve 2 effect

            string SecondEffectShortText() => "Return 2 cards to top of deck, <De-Digivolve 2>";
            string SecondEffectDiscription(string tag)
            {
                return $"[{tag}] If you have 10 or more cards in your trash, by returning 2 non-Digi-Egg cards from your trash to the top of the deck, <De-Digivolve 2> 1 of your opponent's Digimon.";
            }

            bool SecondCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            IEnumerator SecondActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool cardsAdded = false;

                if (card.Owner.TrashCards.Count >= 10)
                {
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    bool CanSelectCardCondition(CardSource cardSource)
                    {
                        return !cardSource.IsDigiEgg;
                    }

                    selectCardEffect.SetUp(
                    canTargetCondition: CanSelectCardCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    canNoSelect: () => true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                    message: "Select cards to place at the top of the deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).",
                    maxCount: 2,
                    canEndNotMax: false,
                    isShowOpponent: false,
                    mode: SelectCardEffect.Mode.Custom,
                    root: SelectCardEffect.Root.Trash,
                    customRootCardList: null,
                    canLookReverseCard: true,
                    selectPlayer: card.Owner,
                    cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count == 2)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(cardSources));
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Top Cards", true, true));
                            cardsAdded = true;
                        }
                    }

                    if (cardsAdded)
                    {
                        Permanent selectedPermanent = null;
                        bool CanSelectPermanentCondition(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;
                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IDegeneration(selectedPermanent, 2, activateClass).Degeneration());
                            }
                        }
                    }
                }
            }

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SecondEffectShortText(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SecondCanActivateCondition, (hashtable) => SecondActivateCoroutine(hashtable, activateClass), -1, false, SecondEffectDiscription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SecondEffectShortText(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SecondCanActivateCondition, (hashtable) => SecondActivateCoroutine(hashtable, activateClass), -1, false, SecondEffectDiscription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }

            #endregion

            #endregion

            return cardEffects;
        }
    }
}