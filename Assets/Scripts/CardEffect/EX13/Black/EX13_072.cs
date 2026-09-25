using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Kota Domoto
namespace DCGO.CardEffects.EX13
{
    public class EX13_072 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.StartOfYourMainPhaseClass(
                    card,
                    "By trashing 1 [Chronicle] trait card from hand, <Draw 1> and gain 1 memory",
                    ActivateCoroutine,
                    EffectDiscription(),
                    additionalActivateCondition: AdditionalActivateCondition,
                    optional: false,
                    isSkippable: true
                    ));

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By trashing 1 [Chronicle] trait card from your hand, <Draw 1> and gain 1 memory.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasChronicleTraits;
                }

                bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                {
                    return CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
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
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                        }
                    }
                }
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend this Tamer to use [X Antibody] or a [Chronicle] Option with cost -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When one of your [Chronicle] trait Digimon attacks, by suspending this Tamer, you may use 1 [X Antibody] or 1 Option card with the [Chronicle] trait from your hand with the cost reduced by 1.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.HasChronicleTraits;
                }

                bool CanSelectOptionCondition(CardSource cardSource)
                {
                    // Memory can go down to 10 on the opponent's side, taking the -1 reduction into account
                    return cardSource.IsOption &&
                           (cardSource.EqualsCardName("X Antibody") || cardSource.HasChronicleTraits) &&
                           !cardSource.CanNotPlayThisOption &&
                           card.Owner.MaxMemoryCost >= cardSource.GetCostItself - 1;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.CanActivateSuspendCostEffect(card) &&
                           CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectOptionCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    if (!card.PermanentOfThisCard().IsSuspended)
                    {
                        yield break;
                    }

                    CardSource selectedCard = null;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOptionCondition,
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

                    selectHandEffect.SetUpCustomMessage("Select 1 card to use.", "The opponent is selecting 1 card to use.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Used Card");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selectedCard == null)
                    {
                        yield break;
                    }

                    #region reduce use cost

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect("Use Cost -1", _ => true, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: _ => true, isUpDown: () => true, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    Func<EffectTiming, ICardEffect> getCardEffect = _timing => _timing == EffectTiming.None ? changeCostClass : null;
                    card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        return cardSource == selectedCard;
                    }

                    int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    {
                        if (CardSourceCondition(cardSource) &&
                            (targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0))
                        {
                            Cost -= 1;
                        }

                        return Cost;
                    }

                    #endregion

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                        cardSources: new List<CardSource> { selectedCard },
                        activateClass: activateClass,
                        payCost: true,
                        root: SelectCardEffect.Root.Hand));

                    #region release effect

                    card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);

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
