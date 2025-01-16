using System;
using System.Collections;
using System.Collections.Generic;

public class EX5_071 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
            ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
            ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
            cardEffects.Add(ignoreColorConditionClass);

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent.TopCard.CardTraits.Contains("Deva"))
                    {
                        return true;
                    }

                    if (permanent.TopCard.CardTraits.Contains("Four Sovereigns"))
                    {
                        return true;
                    }

                    if (permanent.TopCard.CardTraits.Contains("FourSovereigns"))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (card.Owner.GetBattleAreaDigimons().Some(PermanentCondition))
                {
                    return true;
                }

                return false;
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource == card)
                {
                    return true;
                }

                return false;
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
                return "[Main] Reveal the top 3 cards of your deck. Place 1 card with [Deva]/[Four Sovereigns] trait among them as the bottom digivolution card of 1 of your Digimon or add it to your hand. Return the rest to the top or bottom of the deck.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.CardTraits.Contains("Deva"))
                {
                    return true;
                }

                if (cardSource.CardTraits.Contains("Four Sovereigns"))
                {
                    return true;
                }

                if (cardSource.CardTraits.Contains("FourSovereigns"))
                {
                    return true;
                }

                return false;
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (!permanent.IsToken)
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
                List<CardSource> selectedCards = new List<CardSource>();

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with [Deva]/[Four Sovereigns] trait.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: SelectCardCoroutine),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                    activateClass: activateClass,
                    canNoSelect: true
                ));

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCards.Add(cardSource);
                    yield return null;
                }

                if (selectedCards.Count >= 1)
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(selectedCards, "Selected Cards", true, true));

                    bool canSelectDigimon = CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                    bool canSelectHand = true;

                    if (canSelectDigimon || canSelectHand)
                    {
                        if (canSelectDigimon && canSelectHand)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Place on digivolution cards", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Add to hand", value : false, spriteIndex: 1),
                        };

                            string selectPlayerMessage = "Which effect will you select?";
                            string notSelectPlayerMessage = "The opponent is choosing effects.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }

                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectDigimon);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool selectDigimon = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (selectDigimon)
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
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
                                    yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(selectedCards, activateClass));
                                }
                            }
                        }

                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(selectedCards, "Cards added to hand", true, true));

                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(selectedCards, false, activateClass));
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Reveal the top 3 cards of deck");
        }

        return cardEffects;
    }
}
