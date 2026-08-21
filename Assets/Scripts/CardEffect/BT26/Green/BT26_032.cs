using System;
using System.Collections;
using System.Collections.Generic;

// Ceresmon // Famis
namespace DCGO.CardEffects.BT26
{
    public class BT26_032 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Ceresmon") && targetPermanent.TopCard.BasePlayCostFromEntity == 12;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Succession
            if (timing == EffectTiming.None)
            {
                bool CardCondition(CardSource cardSource) => cardSource.EqualsCardName("Ceresmon");

                cardEffects.Add(CardEffectFactory.SuccessionSelfEffect(isInheritedEffect: false, card: card, condition: null, cardCondition: CardCondition));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent's suspended Digimon -5000 DP, then by suspending 1 Digimon on your turn, play/use 1 [Vegetation]/[TS] for 5 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Digivolving] All of your opponent's suspended Digimon get -5000 DP until their turn ends. Then, by suspending 1 Digimon, if it's your turn, you may play or use 1 card with the [Vegetation] or [TS] trait from your hand with the cost reduced by 5.";

                bool SuspendedOpponentDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.IsSuspended;

                bool CanSelectSuspendCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && !permanent.IsSuspended && permanent.CanSuspend;

                bool CanSelectHandCardCondition(CardSource cardSource)
                    => (cardSource.EqualsTraits("Vegetation") || cardSource.HasTSTraits)
                        && ((cardSource.IsOption && !cardSource.CanNotPlayThisOption)
                            || (cardSource.HasPlayCost && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass, fixedCost: cardSource.GetCostItself - 5)));

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDPPlayerEffect(
                        permanentCondition: SuspendedOpponentDigimonCondition,
                        changeValue: -5000,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass));

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendCondition))
                    {
                        Permanent selectedSuspendTarget = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectSuspendCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedSuspendTarget = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedSuspendTarget != null && CardEffectCommons.IsOwnerTurn(card) && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCardCondition))
                        {
                            CardSource selectedCard = null;

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectHandCardCondition,
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

                            IEnumerator SelectCardCoroutine(CardSource cs)
                            {
                                selectedCard = cs;
                                yield return null;
                            }

                            selectHandEffect.SetUpCustomMessage("Select 1 [Vegetation]/[TS] card to play/use.", "The opponent is selecting 1 [Vegetation]/[TS] card to play/use.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                            if (selectedCard != null)
                            {
                                int reduceCost = 5;

                                ChangeCostClass changeCostClass = new ChangeCostClass();
                                changeCostClass.SetUpICardEffect($"Play/Use Cost -{reduceCost}", _ => true, card);
                                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: PlayCondition, rootCondition: _ => true, isUpDown: () => true, isCheckAvailability: () => false, isChangePayingCost: () => true);
                                Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                                card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                                ICardEffect GetCardEffect(EffectTiming _timing)
                                    => _timing == EffectTiming.None ? changeCostClass : null;

                                bool PlayCondition(CardSource cs) => cs == selectedCard;

                                int ChangeCost(CardSource cs, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                {
                                    if (PlayCondition(cs))
                                    {
                                        cost -= reduceCost;
                                    }

                                    return cost;
                                }

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

                                card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                            }
                        }
                    }
                }
            }
            #endregion

            #endregion

            #region Option Effects

            #region Use Req.
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                    => cardSource.HasTSTraits;
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 2 opponent's Digimon/Tamer, then 3 can't unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] You may suspend 2 of your opponent's Digimon or Tamers. Then, 3 of their Digimon or Tamers can't unsuspend until their turn ends.";

                bool CanSelectSuspendCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer)
                        && !permanent.IsSuspended && permanent.CanSuspend;

                bool CanSelectCantUnsuspendCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendCondition))
                    {
                        int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectSuspendCondition));

                        SelectPermanentEffect selectSuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectSuspendEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectSuspendCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: true,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        selectSuspendEffect.SetUpCustomMessage("Select up to 2 Digimon or Tamers to suspend.", "The opponent is selecting up to 2 Digimon or Tamers to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectSuspendEffect.Activate());
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectCantUnsuspendCondition))
                    {
                        int maxCount = Math.Min(3, CardEffectCommons.MatchConditionPermanentCount(CanSelectCantUnsuspendCondition));

                        SelectPermanentEffect selectCantUnsuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectCantUnsuspendEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCantUnsuspendCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectCantUnsuspendEffect.SetUpCustomMessage("Select 3 Digimon or Tamers that can't unsuspend.", "The opponent is selecting 3 Digimon or Tamers that can't unsuspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectCantUnsuspendEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspend(permanent, EffectDuration.UntilOpponentTurnEnd, activateClass, null, "Can't unsuspend"));
                        }
                    }
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion

            #endregion

            return cardEffects;
        }
    }
}
