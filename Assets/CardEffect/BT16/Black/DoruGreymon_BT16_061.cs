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
            if (timing == EffectTiming.OnDestroyedAnyone || timing == EffectTiming.OnEndBattle)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 digimon 5 cost or less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("PlayDigimon_BT16_061");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] (Once Per Turn) When this Digimon deletes another Digimon, you may play 1 card with the [X Antibody] or [SoC] trait and a play cost of 5 or less from your trash without paying the cost.";
                }

                bool SelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.HasPlayCost && cardSource.GetCostItself <= 5)
                    {
                        if (cardSource.ContainsTraits("X Antibody") || cardSource.CardTraits.Contains("X-Antibody"))
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

                bool DeletionCardEffect(ICardEffect cardEffect)
                {
                    return (cardEffect.EffectSourceCard.PermanentOfThisCard() == card.PermanentOfThisCard());
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsByBattle(hashtable))
                        {
                            bool WinnerCondition(Permanent permanent)
                            {
                                if (permanent == null)
                                    permanent = card.PermanentOfThisCard();

                                return permanent.cardSources.Contains(card);
                            }
                            bool LoserCondition(Permanent permanent) => permanent.IsDigimon && CardEffectCommons.IsOpponentPermanent(permanent, card);

                            if (CardEffectCommons.CanTriggerWhenDeleteOpponentDigimon(hashtable, WinnerCondition, LoserCondition))
                            {
                                return true;
                            }
                        }

                        if(CardEffectCommons.IsByEffect(hashtable, DeletionCardEffect))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
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
                                activateETB:true));
                        }
                    }
                }
            }
            #endregion


            return cardEffects;
        }
    }
}