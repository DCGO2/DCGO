using System.Collections;
using System.Collections.Generic;

//ST21 Gabumon
//Code largely copied from ST21 agumon, if error exists in one card then check the other too
namespace DCGO.CardEffects.ST21
{
    public class ST21_10 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return (targetPermanent.TopCard.HasAdventureTraits || targetPermanent.TopCard.EqualsTraits("HERO")) && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 2;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Warp Digivolution
            if (timing == EffectTiming.None)
            {
                bool enoughTamerColours()
                {
                    List<CardSource> tamerCards = new List<CardSource>();

                    foreach (Permanent permanent in card.Owner.GetBattleAreaPermanents())
                    {
                        if (permanent.IsTamer)
                        {
                            tamerCards.Add(permanent.TopCard);
                        }
                    }
                    return Combinations.GetDifferenetColorCardCount(tamerCards) >= 3;
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsOwnerTurn(card) && CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, (permanent) => permanent.IsDigimon && permanent.HasDP && permanent.DP >= 10000))
                        {
                            return true;
                        }
                        return enoughTamerColours();
                    }
                    return false;
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent == card.PermanentOfThisCard();
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        //compared to the code I was copying from (ST8-04), removed a check that owner matched between cardsource and card, as that should be handled by whose hand the card is in. If this breaks, try putting it back.
                        if (card.Owner.HandCards.Contains(cardSource))
                        {
                            return cardSource.EqualsCardName("MetalGarurumon");
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: true, card: card, condition: Condition, cardCondition: CardCondition));
            }
            #endregion

            #region draw + trash inherit
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1 and trash 1 card from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Draw_ST21_10");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking][Once Per Turn] <Draw 1>. (Draw 1 card from your deck.) Then, trash 1 card in your hand.";
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

                    if (card.Owner.HandCards.Count >= 1)
                    {
                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());
                    }
                }
            }
            #endregion  
            return cardEffects;
        }
    }
}