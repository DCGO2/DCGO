using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Yujin Ozora
namespace DCGO.CardEffects.P
{
    public class P_234 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal 4, add 1, bot deck rest", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    =>  "[On Play] Reveal the top 4 cards of your deck. Add 1 [System], [Navi], [Tool] or [Leviathan] trait card among them to the hand. Return the rest to the bottom of the deck.";

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("System")
                        || cardSource.EqualsTraits("Navi")
                        || cardSource.EqualsTraits("Tool")
                        || cardSource.EqualsTraits("Leviathan");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 4,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition:CanSelectCardCondition,
                                message: "Select 1 card with the [System], [Navi], [Tool] or [Leviathan] trait.",
                                mode: SelectCardEffect.Mode.AddHand,
                                maxCount: 1,
                                selectCardCoroutine: null)
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass
                    ));
                }
            }

            #endregion

            #region Your Turn - Link card trashed
            if (timing == EffectTiming.OnLinkCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending, you may link from hand for -2 cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() 
                    => "[Your Turn] When effect trash any of your Digimon's link cards, by suspending this Tamer, you may link 1 [System], [Navi], [Tool] or [Leviathan] trait card from your hand to 1 of your Digimon with the cost reduced by 2.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerOnTrashLinkedCard(
                            hashtable,
                            perm => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(perm, card),
                            cardEffect => cardEffect != null,
                            source => source != null);
                }
    
                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return (cardSource.EqualsTraits("System")
                            || cardSource.EqualsTraits("Navi")
                            || cardSource.EqualsTraits("Tool")
                            || cardSource.EqualsTraits("Leviathan"))
                        && cardSource.CanLink(true);
                }

                bool CanSelectPermanentCondition(Permanent permanent, CardSource cardSource)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && cardSource.CanLinkToTargetPermanent(permanent, true);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());
                    
                    #region Link Cost Reduction
                    ICardEffect GetCardEffect(EffectTiming _timing)
                    {
                        if (_timing == EffectTiming.None)
                        {
                            return CardEffectFactory.GrantedReduceLinkCostClass(
                                card: card, 
                                reducedCost: 2,
                                cardSourceCondition: _ => true,
                                permanentCondition: _ => true,
                                rootCondition: _ => true
                            );
                        }

                        return null;
                    }

                    card.Owner.UntilCalculateFixedCostEffect.Add(GetCardEffect);
                    #endregion

                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 card to link.", "The opponent is selecting 1 card to link.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, permanent => CanSelectPermanentCondition(permanent, cardSource)))
                            {
                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: permanent => CanSelectPermanentCondition(permanent, cardSource),
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to link to", "Your opponent is choosing 1 digimon to link to");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new ILinkCard(true, cardSource, permanent, activateClass).LinkCard());
                                }
                            }
                        }
                    }

                    #region Remove Link Cost Reduction
                    card.Owner.UntilCalculateFixedCostEffect.Remove(GetCardEffect);
                    #endregion
                }
            }
            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}
