using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.P
{
    public class P_151 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Color Requirment
            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
                cardEffects.Add(ignoreColorConditionClass);

                bool HasLiberatorTrait(Permanent permanent)
                {
                    if (permanent.TopCard.ContainsTraits("Liberator") || permanent.TopCard.ContainsTraits("LIBERATOR"))
                    {
                        if(permanent.IsDigimon || permanent.IsTamer)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasLiberatorTrait);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }
            }
            #endregion

            #region Option Skill
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal top 3, Add 1 with [LIBERATOR] trait. Then Play 1 with [LIBERATOR] trait", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Reveal the top 3 cards of your deck. Add 1 card with the [LIBERATOR] trait among them to the hand. Return the rest to the bottom of the deck.Then, you may play 1 card with the [LIBERATOR] trait and a play cost of 3 or less from your hand without paying the cost.";
                }

                bool SelectLiberatorTrait(CardSource cardSource)
                {
                    return cardSource.ContainsTraits("LIBERATOR");
                }

                bool SelectPlayTarget(CardSource cardSource)
                {
                    if(cardSource.HasPlayCost && cardSource.GetCostItself <= 3)
                    {
                        if(CardEffectCommons.CanPlayAsNewPermanent(
                            cardSource: cardSource,
                            payCost: false,
                            cardEffect: activateClass))
                        {
                            return SelectLiberatorTrait(cardSource);
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:SelectLiberatorTrait,
                            message: "Select 1 card with [LIBERATOR] trait.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass
                    ));

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SelectPlayTarget,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: PlaySelectedCard,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    IEnumerator PlaySelectedCard(List<CardSource> selectedCards)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true));
                    }
                }
            }
            #endregion

            #region Security Skill
            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(
                    card: card, 
                    cardEffects: ref cardEffects, 
                    effectName: $"Reveal top 3, Add 1 with [LIBERATOR] trait. Then Play 1 with [LIBERATOR] trait");
            }
            #endregion

            return cardEffects;
        }
    }
}