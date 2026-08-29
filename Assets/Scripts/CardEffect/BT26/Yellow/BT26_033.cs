using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Jupitermon // Wide Plasment
namespace DCGO.CardEffects.BT26
{
    public class BT26_033 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 4,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 5));
            }
            #endregion

            #region Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Engage
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.EngageSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add top sec to hand, may play/use 1 [Iliad]/[TS] from hand cost -5", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] Add your top security card to the hand. Then, if it's your turn, you may play or use 1 [Iliad] or [TS] trait card from your hand with the cost reduced by 5.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region Top Sec to Hand
                    if (card.Owner.SecurityCards.Count > 0)
                    {
                        CardSource topCard = card.Owner.SecurityCards[0];

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { topCard }, false, activateClass));

                        yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                            player: card.Owner,
                            refSkillInfos: ref ContinuousController.instance.nullSkillInfos,
                            activateClass).ReduceSecurity());
                    }
                    #endregion

                    #region If your turn, play/use Iliad cost -5
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        CardSource selectedCard = null;

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return (cardSource.EqualsTraits("Iliad")
                                || cardSource.EqualsTraits("TS"))
                                && ((cardSource.IsOption
                                        && !cardSource.CanNotPlayThisOption)
                                    || (cardSource.HasPlayCost
                                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: true, cardEffect: activateClass, fixedCost: cardSource.GetCostItself - 5)));
                        }

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

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCard = cardSource;
                                yield return null;
                            }

                            selectHandEffect.SetUpCustomMessage("Select 1 [Iliad] or [TS] card to play/use.", "The opponent is selecting 1 [Iliad] or [TS] card to play/use.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");
                            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                        }

                        if (selectedCard != null)
                        {
                            #region Reduce Cost by 5
                            IEnumerator ReduceCost()
                            {
                                if (card.Owner.CanReduceCost(null, card))
                                {
                                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                                }

                                Hashtable reduceHash = new Hashtable
                                {
                                    { "CardEffect", activateClass }
                                };

                                ChangeCostClass changeCostClass = new ChangeCostClass();
                                changeCostClass.SetUpICardEffect("Iliad cost: -5", _ => true, card);
                                changeCostClass.SetUpChangeCostClass(
                                    changeCostFunc: ChangeCost,
                                    cardSourceCondition: cs => cs != null && cs.Owner == card.Owner,
                                    rootCondition: _ => true,
                                    isUpDown: () => true,
                                    isCheckAvailability: () => false,
                                    isChangePayingCost: () => true);
                                card.Owner.UntilCalculateFixedCostEffect.Add(_ => changeCostClass);
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(reduceHash));

                                int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                {
                                    if (cardSource != null && cardSource.Owner == card.Owner)
                                    {
                                        cost -= 5;
                                    }
                                    return cost;
                                }
                            }
                            #endregion

                            if (selectedCard.IsOption)
                            {
                                yield return ContinuousController.instance.StartCoroutine(ReduceCost());
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                    cardSources: new List<CardSource>() { selectedCard },
                                    activateClass: activateClass,
                                    payCost: true,
                                    root: SelectCardEffect.Root.Hand));
                            }
                            else
                            {
                                yield return ContinuousController.instance.StartCoroutine(ReduceCost());
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: new List<CardSource>() { selectedCard },
                                    activateClass: activateClass,
                                    payCost: true,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Hand,
                                    activateETB: true));
                            }
                        }

                    }
                    #endregion
                }
            }
            #endregion

            #region All Turns Protect TS
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing top stacked card as bottom security, card doesn't leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When any of your [TS] trait Digimon or Tamers would leave the battle area, by placing this Digimon's top stacked card as the bottom security card, they don't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.PermanentOfThisCard().DigivolutionCards.Count > 0
                        && card.Owner.CanAddSecurity(activateClass);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer)
                        && permanent.TopCard.HasTSTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> protectedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable)
                                .Filter(PermanentCondition);

                    Permanent thisPermanent = card.PermanentOfThisCard();
                    CardSource topStacked = card;

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(topStacked, thisPermanent));

                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(topStacked, toTop: false, faceUp: false));

                    foreach (Permanent permanent in protectedPermanents)
                    {
                        permanent.willBeRemoveField = false;
                        permanent.HideDeleteEffect();
                        permanent.HideHandBounceEffect();
                        permanent.HideDeckBounceEffect();
                        permanent.HideWillRemoveFieldEffect();
                    }
                }
            }
            #endregion

            #endregion

            #region Option Effects

            #region Ignore Colour Requirement
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("TS");
                }
            }
            #endregion

            #region Option Use Cost
            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Option use cost increased by number of security cards", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(
                    changeCostFunc: ChangeCost,
                    cardSourceCondition: CardSourceCondition,
                    rootCondition: RootCondition,
                    isUpDown: () => false,
                    isCheckAvailability: () => false,
                    isChangePayingCost: () => true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource) && RootCondition(root) && PermanentsCondition(targetPermanents))
                    {
                        cost += card.Owner.SecurityCards.Count;
                    }
                    return cost;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    return targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0;
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete opponent's all Digimon with the lowest DP, Recovery +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] Delete all of your opponent's Digimon with the lowest DP. Then, <Recovery +1>.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool LowestDPCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> destroyTargets = card.Owner.Enemy.GetBattleAreaDigimons().Filter(LowestDPCondition);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: destroyTargets,
                        activateClass: activateClass,
                        successProcess: null,
                        failureProcess: null));

                    yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
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
