using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT18
{
    public class BT18_070 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4 && targetPermanent.TopCard.EqualsTraits("Hybrid")
                                                            && (targetPermanent.TopCard.CardColors.Contains(CardColor.Black) ||
                                                                targetPermanent.TopCard.CardColors.Contains(CardColor.Yellow));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Hand - Main

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Your black or yellow tamer digivolves into this card", CanUseCondition,
                    card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Hand] [Main] By placing 1 [Beetlemon] and [MetalKabuterimon] from your trash under 1 of your black or yellow Tamers, that Tamer digivolves into this card for digivolution cost of 3, ignoring its digivolution requirements.";
                }

                bool CanSelectHumanSpiritCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Beetlemon");
                }

                bool CanSelectBeastSpiritCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("MetalKabuterimon");
                }

                bool CanSelectTamerPermanentCondition(Permanent permanent)
                {
                    return permanent != null && CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card) &&
                           permanent.IsTamer &&
                           (permanent.TopCard.CardColors.Contains(CardColor.Black) ||
                            permanent.TopCard.CardColors.Contains(CardColor.Yellow));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return card.Owner.HandCards.Contains(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerPermanentCondition) &&
                           CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectHumanSpiritCardCondition) &&
                           CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectBeastSpiritCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerPermanentCondition))
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectTamerPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 tamer to digivolve.",
                            "The opponent is selecting 1 tamer to digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null &&
                            CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectHumanSpiritCardCondition) &&
                            CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectBeastSpiritCardCondition))
                        {
                            bool digivolve = false;


                            List<CardSource> selectedCards = new List<CardSource>();

                            SelectCardEffect selectCardEffect =
                                GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: (cardSource) =>
                                    CanSelectHumanSpiritCardCondition(cardSource) ||
                                    CanSelectBeastSpiritCardCondition(cardSource),
                                canTargetCondition_ByPreSelecetedList: CanTargetConditionByPreSelectedList,
                                canEndSelectCondition: CanEndSelectCondition,
                                canNoSelect: () => false,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message:
                                "Select cards to place on the bottom of Digivolution cards\n(cards will be placed in the digivolution cards so that cards with lower numbers are on top).",
                                maxCount: 2,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(
                                selectCardEffect.Activate());

                            bool CanTargetConditionByPreSelectedList(List<CardSource> cardSources,
                                CardSource cardSource)
                            {
                                if (cardSources.Count(CanSelectHumanSpiritCardCondition) >= 1)
                                {
                                    if (CanSelectHumanSpiritCardCondition(cardSource))
                                    {
                                        return false;
                                    }
                                }

                                if (cardSources.Count(CanSelectBeastSpiritCardCondition) >= 1)
                                {
                                    if (CanSelectBeastSpiritCardCondition(cardSource))
                                    {
                                        return false;
                                    }
                                }

                                return true;
                            }

                            bool CanEndSelectCondition(List<CardSource> cardSources)
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card,
                                        CanSelectHumanSpiritCardCondition))
                                {
                                    if (cardSources.Count(CanSelectHumanSpiritCardCondition) == 0)
                                    {
                                        return false;
                                    }
                                }

                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card,
                                        CanSelectBeastSpiritCardCondition))
                                {
                                    if (cardSources.Count(CanSelectBeastSpiritCardCondition) == 0)
                                    {
                                        return false;
                                    }
                                }

                                if (cardSources.Count(CanSelectHumanSpiritCardCondition) >= 2)
                                {
                                    return false;
                                }

                                if (cardSources.Count(CanSelectBeastSpiritCardCondition) >= 2)
                                {
                                    return false;
                                }

                                return true;
                            }

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);
                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    selectedPermanent.AddDigivolutionCardsBottom(selectedCards, activateClass));

                                if (selectedCards.Count == 2)
                                {
                                    digivolve = true;
                                }
                            }

                            if (digivolve)
                            {
                                if (!card.CanNotEvolve(selectedPermanent))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(
                                    CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                        targetPermanent: selectedPermanent,
                                        cardCondition: source => source == card,
                                        payCost: true,
                                        reduceCostTuple: null,
                                        fixedCostTuple: null,
                                        ignoreDigivolutionRequirementFixedCost: 3,
                                        isHand: true,
                                        activateClass: activateClass,
                                        successProcess: null,
                                        failedProcess: DigivolvedFailed(),
                                        isOptional: false));
                                }
                                else
                                {
                                    yield return ContinuousController.instance.StartCoroutine("DigivolvedFailed");
                                }

                            }

                            IEnumerator DigivolvedFailed()
                            {
                                IDiscardHand discard = new IDiscardHand(card, hashtable);

                                discard.Discard();
                                yield return null;
                            }
                        }
                    }
                }
            }

            #endregion

            #region Collision

            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(false, card, null));
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnAttackTargetChanged)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("Unsuspend_BT18_70");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] (Once Per Turn) When an attack target is switched, you may unsuspend this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnPermanentAttackTargetSwitch(hashtable, _ => true);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                }
            }

            #endregion

            #region When Attacking - ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DP -4000", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false,
                    EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("-4000DP_BT18_70");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] (Once Per Turn) 1 of your opponent's Digimon gets -4000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect =
                        GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -4000.",
                            "The opponent is selecting 1 Digimon that will get DP -4000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: permanent,
                                changeValue: -4000,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}