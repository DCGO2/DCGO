using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Tankdramon
namespace DCGO.CardEffects.BT25
{
    public class BT25_074 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolve Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("D-Brigade")
                            || targetPermanent.TopCard.EqualsTraits("ACCEL");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null, level: 4));
            }
            #endregion

            #region WD/WA Shared
            string SharedEffectName = "Reveal 3, may play 1 12 cost or lower [D-Brigade]/[ACCEL] trait with cost reduced by 5, trash rest";

            string SharedHashString = "BT25_074_WD_WA";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: SharedHashString,
                whenDigivolving: true,
                whenAttacking: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] [Once Per Turn] Reveal the top 3 cards of your deck. You may play 1 cost 12 or lower [D-Brigade] or [ACCEL] trait card among them with the cost reduced by 5. Trash the rest.";
            }

            IEnumerator SharedActivateCoroutine(Hashtable _hashtable, ActivateClass activateClass)
            {
                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 12
                        && (cardSource.EqualsTraits("D-Brigade")
                            || cardSource.EqualsTraits("ACCEL"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass, fixedCost: Math.Max(0, cardSource.GetCostItself - 5));
                }

                List<CardSource> selectedCards = new List<CardSource>();

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 play cost 12 or lower [D-Brigade] or [ACCEL] trait Digimon card to play with the cost reduced by 5.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: SelectCardCoroutine),
                    },
                    remainingCardsPlace: RemainingCardsPlace.Trash,
                    activateClass: activateClass,
                    canNoSelect: true
                ));

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCards.Add(cardSource);
                    yield return null;
                }

                #region reduce play cost
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect($"Play Cost -5", CanUseCondition1, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                ICardEffect GetCardEffect(EffectTiming _timing)
                {
                    if (_timing == EffectTiming.None)
                    {
                        return changeCostClass;
                    }

                    return null;
                }

                bool CanUseCondition1(Hashtable hashtable) => true;

                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource)
                        && RootCondition(root)
                        && PermanentsCondition(targetPermanents))
                    {
                        Cost -= 5;
                    }

                    return Cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    return targetPermanents == null || targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return true;
                }

                bool RootCondition(SelectCardEffect.Root root) => true;

                bool isUpDown() => true;
                #endregion

                #region Play the card
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                    cardSources: selectedCards,
                    activateClass: activateClass,
                    payCost: true,
                    isTapped: false,
                    root: SelectCardEffect.Root.Library,
                    activateETB: true));
                #endregion

                #region release effect reducing play cost
                card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                #endregion
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 enemy Digimon can't digivolve until their turn end", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT25_074_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When any of your [D-Brigade] or [ACCEL] trait Digimon are played, 1 of your opponent's Digimon can't digivolve until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PlayedPermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool PlayedPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.EqualsTraits("D-Brigade")
                            || permanent.TopCard.EqualsTraits("ACCEL"));
                }

                bool CanSelectPermanentCondition(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(targetPermanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 card to prevent from digivolving.", "Opponent is selecting 1 card to prevent from digivolving.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent selectedPermanent)
                        {
                            CanNotDigivolveClass canNotPutFieldClass = new CanNotDigivolveClass();
                            canNotPutFieldClass.SetUpICardEffect("Can't Digivolve", CanUseCondition1, card);
                            canNotPutFieldClass.SetUpCanNotEvolveClass(permanentCondition: PermanentCondition, cardCondition: CardCondition);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().DebuffSE);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));

                            bool CanUseCondition1(Hashtable hashtable) => true;

                            bool PermanentCondition(Permanent permanent)
                            {
                                return permanent == selectedPermanent
                                    && permanent.TopCard != null
                                    && !permanent.TopCard.CanNotBeAffected(canNotPutFieldClass);
                            }

                            bool CardCondition(CardSource cardSource)
                            {
                                return cardSource.Owner == card.Owner.Enemy
                                    && !cardSource.CanNotBeAffected(canNotPutFieldClass);
                            }

                            ICardEffect GetCardEffect(EffectTiming _timing)
                            {
                                if (_timing == EffectTiming.None)
                                {
                                    return canNotPutFieldClass;
                                }

                                return null;
                            }
                        }
                    }
                }
            }
            #endregion

            #region Inherited - Opponent's turn Reboot and Blocker
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOpponentTurn(card)
                        && (card.PermanentOfThisCard().TopCard.ContainsCardName("Chaosmon")
                            || card.PermanentOfThisCard().TopCard.EqualsTraits("D-Brigade")
                            || card.PermanentOfThisCard().TopCard.EqualsTraits("ACCEL"));
                }

                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: true, card: card, condition: Condition));
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: true, card: card, condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}
