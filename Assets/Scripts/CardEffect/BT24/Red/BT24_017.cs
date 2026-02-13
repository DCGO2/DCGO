using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Medusamon
namespace DCGO.CardEffects.BT24
{
    public class BT24_017: CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Raid
            if(timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Progress
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ProgressSelfStaticEffect(false, card, null));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete lowest DP Digimon, Return 2 cards from their trash to deck to play 2 Tokens and gain 2k DP per opponent's Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Digivolving] Delete 1 of your opponent's lowest DP Digimon. Then, by returning 2 cards from their trash to the bottom of the deck, they play 2 [Petrification] Tokens. (Digimon/White/3000 DP/ [Your Turn] This Digimon can't suspend. [On Deletion] Trash your top security card.) After, this Digimon gets +2000 DP for each of your opponent's Digimon until their turn ends.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool CanDeleteCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);
                }       

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region Delete 1 lowest DP
                    if (CardEffectCommons.HasMatchConditionPermanent(CanDeleteCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanDeleteCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select a digimon to delete.", "Opponent is selectign a Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                    #endregion

                    #region Conditional Effects

                    if (card.Owner.Enemy.TrashCards.Count >= 2)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();
                        
                        #region Bottom Deck 2 from trash

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: (card) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 2 cards to bottom deck",
                            maxCount: 2,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: card.Owner.Enemy.TrashCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select cards to bottom deck", "Your opponent is 2 cards to bottom deck");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Selected Cards");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (selectedCards.Count == 2)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ReturnRevealedCardsToLibraryBottom(
                                remainingCards: selectedCards,
                                activateClass: activateClass));

                            #region Play 2 Tokens

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPetrificationToken(activateClass));
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPetrificationToken(activateClass));

                            #endregion

                            #region Gain DP per enemy digimon

                            int count = card.Owner.Enemy.GetBattleAreaDigimons().Count();
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: card.PermanentOfThisCard(),
                                changeValue: 2000 * count,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));                        

                            #endregion
                        }
                    }
                    #endregion
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
