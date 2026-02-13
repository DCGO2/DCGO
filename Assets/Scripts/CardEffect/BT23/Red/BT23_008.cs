using System;
using System.Collections;
using System.Collections.Generic;

// Greymon
namespace DCGO.CardEffects.BT23
{
    public class BT23_008 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.IsLevel3
                        && (targetPermanent.TopCard.ContainsCardName("Agumon") || targetPermanent.TopCard.HasCSTraits);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Raid

            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Main - OPT

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing the top card of this Digimon as bottom digivolution card, play 1 [Gabumon]/[Nokia Shiramine] from hand for 2 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("BT23_008_Main");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] [Once Per Turn] By placing this Digimon's top stacked card as its bottom digivolution card, you may play 1 [Gabumon] or [Nokia Shiramine] from your hand with the play cost reduced by 2.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                       && CardEffectCommons.IsOwnerTurn(card)
                       && card.PermanentOfThisCard().StackCards.Count >= 1;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass))
                    {
                        if (cardSource.IsDigimon && cardSource.EqualsCardName("Gabumon")) return true;
                        if (cardSource.IsTamer && cardSource.EqualsCardName("Nokia Shiramine")) return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card) && card.PermanentOfThisCard().StackCards.Count >= 1)
                    {
                        CardSource topCard = card.PermanentOfThisCard().TopCard;

                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                            new List<CardSource>() { topCard },
                            activateClass));

                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                        {
                            CardSource selectCard = null;
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, CanSelectCardCondition));

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

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectCard = cardSource;
                                yield return null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                            if (selectCard != null)
                            {
                                int cost = selectCard.BasePlayCostFromEntity - 2;
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                new List<CardSource>() { selectCard },
                                activateClass: activateClass,
                                payCost: true,
                                isTapped: false,
                                root: SelectCardEffect.Root.Hand,
                                activateETB: true,
                                fixedCost: cost));
                            }
                        }
                    }
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) && CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 2000, isInheritedEffect: true, card: card, condition: Condition));
            }

            #endregion

            return cardEffects;
        }
    }
}