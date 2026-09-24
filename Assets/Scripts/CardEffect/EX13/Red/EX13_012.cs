using System;
using System.Collections;
using System.Collections.Generic;

// SaviorHuckmon
namespace DCGO.CardEffects.EX13
{
    public class EX13_012 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasText("Huckmon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared When Digivolving / When Attacking

            const int ReduceCost = 3;

            string PlayOrUseEffectName = "May play or use 1 white [Huckmon] text card from hand for 3 less";

            string PlayOrUseEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] You may play or use 1 white card with [Huckmon] in its text from your hand with the cost reduced by 3.";

            bool CanSelectCardCondition(CardSource cardSource, ActivateClass activateClass)
                => cardSource.HasCardColor(CardColor.White)
                    && cardSource.HasText("Huckmon")
                    && CardEffectCommons.CanPlayOrUse(cardSource, activateClass, fixedCost: Math.Max(0, cardSource.GetCostItself - ReduceCost));

            bool PlayOrUseAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanSelectCardCondition(cardSource, activateClass));

            IEnumerator PlayOrUseActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                CardSource selectedCard = null;

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: cardSource => CanSelectCardCondition(cardSource, activateClass),
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

                selectHandEffect.SetUpCustomMessage("Select 1 card to play/use.", "The opponent is selecting 1 card to play/use.");
                selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCard = cardSource;
                    yield return null;
                }

                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                if (selectedCard == null)
                {
                    activateClass.RemoveUse();
                    yield break;
                }

                #region reduce cost
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect($"Play/Use Cost -{ReduceCost}", _ => true, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: IsSelectedCard, rootCondition: _ => true, isUpDown: () => true, isCheckAvailability: () => false, isChangePayingCost: () => true);
                Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                ICardEffect GetCardEffect(EffectTiming _timing)
                    => _timing == EffectTiming.None ? changeCostClass : null;

                bool IsSelectedCard(CardSource cardSource)
                    => cardSource == selectedCard;

                int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    => IsSelectedCard(cardSource) ? cost - ReduceCost : cost;
                #endregion

                if (selectedCard.IsOption)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                        cardSources: new List<CardSource> { selectedCard },
                        activateClass: activateClass,
                        payCost: true,
                        root: SelectCardEffect.Root.Hand));
                }
                else
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: new List<CardSource> { selectedCard },
                        activateClass: activateClass,
                        payCost: true,
                        isTapped: false,
                        root: SelectCardEffect.Root.Hand,
                        activateETB: true));
                }

                #region release reduction
                card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                #endregion
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                PlayOrUseEffectName,
                PlayOrUseActivateCoroutine,
                PlayOrUseEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: PlayOrUseAdditionalActivateCondition,
                maxCountPerTurn: 1,
                hashValue: "EX13_012_WD_WA",
                whenDigivolving: true,
                whenAttacking: true);

            #endregion

            #region Alliance - ESS
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
