using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DCGO.CardEffects.BT16
{
    public class DoruGreymon_BT16_061 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.ContainsCardName("Dorugamon"))
                    {
                        return true;
                    }

                    if(targetPermanent.TopCard.Level == 4 && targetPermanent.TopCard.ContainsTraits("SoC"))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Collision
            if(timing == EffectTiming.OnCounterTiming)
            {                
                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(false,card, null));
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnAttackTargetChanged)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve this digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When an attack target is switched, this Digimon with a Tamer card with the [SoC] trait in its digivolution cards may digivolve into a Digimon card with the [Beast Dragon], [Undead] or [SoC] trait in your hand without paying the cost.";
                }

                bool HasTamerCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("SoC"))
                    {
                        if (cardSource.IsTamer)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.HasBeastDragonTraits || cardSource.CardTraits.Contains("Undead") || cardSource.CardTraits.Contains("SoC"))
                    {
                        if (cardSource.IsDigimon)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(HasTamerCardCondition) >= 1)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                       targetPermanent: card.PermanentOfThisCard(),
                       cardCondition: CanSelectCardCondition,
                       payCost: false,
                       reduceCostTuple: null,
                       fixedCostTuple: null,
                       ignoreDigivolutionRequirementFixedCost: -1,
                       isHand: true,
                       activateClass: activateClass,
                       successProcess: null));
                }
            }
            #endregion

            #region All Turns - ESS
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 digimon 5 cost or less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] (Once Per Turn) When this Digimon deletes another Digimon, you may play 1 card with the [X Antibody] or [SoC] trait and a play cost of 5 or less from your trash without paying the cost.";
                }

                bool SelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.HasPlayCost && cardSource.GetCostItself <= 5)
                    {
                        if (cardSource.CardTraits.Contains("X Antibody") || cardSource.CardTraits.Contains("XAntibody"))
                        {
                            return true;
                        }

                        if (cardSource.ContainsTraits("SoC"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        foreach (CardSource cardSource in card.Owner.TrashCards)
                        {
                            if (SelectCardCondition(cardSource))
                            {
                                if (cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        bool WinnerCondition(Permanent permanent) => permanent.cardSources.Contains(card);
                        bool LoserCondition(Permanent permanent) => CardEffectCommons.IsOpponentPermanent(permanent, card);

                        if (CardEffectCommons.CanTriggerWhenDeleteOpponentDigimon(hashtable: hashtable, winnerCondition: WinnerCondition, loserCondition: LoserCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, SelectCardCondition))
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: SelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Select 1 card to play",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent:false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass
                        );

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> selectedCardSources)
                    {
                        if (selectedCardSources.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources:selectedCardSources,
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Trash,
                                activateETB:false));
                        }
                    }
                }
            }
            #endregion


            return cardEffects;
        }
    }
}