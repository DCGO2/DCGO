using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT13
{
    public class BT13_075 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 card from trash to digivolution cards so that opponent Digimons can't attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By placing 1 Digimon card with the [X Antibody] or [Royal Knight] trait from your trash as this Digimon's bottom digivolution card, all of your opponent's play cost 10 or higher Digimon can't attack players until the end of their turn.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasXAntibodyTraits)
                        {
                            return true;
                        }

                        if (cardSource.HasRoyalKnightTraits)
                        {
                            return true;
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
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (!card.PermanentOfThisCard().IsToken)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        bool added = false;

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to place on bottom of digivolution cards.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");
                            selectCardEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));

                                added = true;
                            }
                        }

                        if (added)
                        {
                            bool AttackerCondition(Permanent Attacker)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(Attacker, card))
                                {
                                    if (Attacker.TopCard.GetCostItself >= 10)
                                    {
                                        if (Attacker.TopCard.HasPlayCost)
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            bool DefenderCondition(Permanent Defender)
                            {
                                return Defender == null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotAttackPlayerEffect(
                                attackerCondition: AttackerCondition,
                                defenderCondition: DefenderCondition,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                effectName: "Can't Attack"));
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 card from trash to digivolution cards so that opponent Digimons can't attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By placing 1 Digimon card with the [X Antibody] or [Royal Knight] trait from your trash as this Digimon's bottom digivolution card, all of your opponent's play cost 10 or higher Digimon can't attack players until the end of their turn.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasXAntibodyTraits)
                        {
                            return true;
                        }

                        if (cardSource.HasRoyalKnightTraits)
                        {
                            return true;
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
                        if (!card.PermanentOfThisCard().IsToken)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        bool added = false;

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to place on bottom of digivolution cards.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");
                            selectCardEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));

                                added = true;
                            }
                        }

                        if (added)
                        {
                            bool AttackerCondition(Permanent Attacker)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(Attacker, card))
                                {
                                    if (Attacker.TopCard.GetCostItself >= 10)
                                    {
                                        if (Attacker.TopCard.HasPlayCost)
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            bool DefenderCondition(Permanent Defender)
                            {
                                return Defender == null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotAttackPlayerEffect(
                                attackerCondition: AttackerCondition,
                                defenderCondition: DefenderCondition,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                effectName: "Can't Attack"));
                        }
                    }
                }
            }

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Prevent this Digimon from leaving play", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Substitute_BT13_075");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns][Once Per Turn] When an effect would remove this Digimon from the battle area, by returning 1 card with the [X Antibody] or [Royal Knight] trait from this Digimon's digivolution cards to the bottom of the deck, prevent that removal.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.Owner == card.Owner)
                    {
                        if (cardSource.HasXAntibodyTraits)
                        {
                            return true;
                        }

                        if (cardSource.HasRoyalKnightTraits)
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
                        if (CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card))
                        {
                            if (CardEffectCommons.IsByEffect(hashtable, null))
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

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = 1;

                            bool returnedToLibrary = false;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: CanEndSelectCondition,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 digivolution card to return to the bottom of deck.",
                                        maxCount: maxCount,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Custom,
                                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: null);

                            selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to return to the bottom of deck.", "The opponent is selecting 1 digivolution card to return to the bottom of deck.");
                            selectCardEffect.SetNotShowCard();

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            bool CanEndSelectCondition(List<CardSource> cardSources)
                            {
                                if (CardEffectCommons.HasNoElement(cardSources))
                                {
                                    return false;
                                }

                                return true;
                            }

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            if (selectedCards.Count == 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new ReturnToLibraryBottomDigivolutionCardsClass(card.PermanentOfThisCard(), selectedCards, CardEffectCommons.CardEffectHashtable(activateClass)).ReturnToLibraryBottomDigivolutionCards());

                                returnedToLibrary = true;
                            }

                            if (returnedToLibrary)
                            {
                                selectedPermanent.willBeRemoveField = false;

                                selectedPermanent.HideDeleteEffect();
                                selectedPermanent.HideHandBounceEffect();
                                selectedPermanent.HideDeckBounceEffect();
                                selectedPermanent.HideWillRemoveFieldEffect();
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}