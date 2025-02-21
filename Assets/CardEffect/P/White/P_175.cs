using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.P
{
    public class P_175 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of your turn
            if (timing == EffectTiming.OnStartTurn)
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve, for reduced cost of 2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When any of your Digimon with the [Rock Dragon] or [Machine Dragon] trait are played, by suspending this Tamer, 1 of your level 4 or higher Digimon may digivolve into a Digimon card with the [Rock Dragon], [Earth Dragon], [Machine Dragon] or [Sky Dragon] trait in the hand with the digivolution cost reduced by 2.";
                }

                bool IsYourRockMachineDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsOwnerPermanent(permanent, card) &&
                           (permanent.TopCard.EqualsTraits("Rock Dragon") || permanent.TopCard.EqualsTraits("Machine Dragon"));
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if(permanent.TopCard.HasLevel && permanent.TopCard.Level >= 4)
                        {
                            foreach (CardSource cardSource in card.Owner.HandCards)
                            {
                                if (IsYourDigimonToDigivolve(cardSource))
                                {
                                    if (cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, true, activateClass))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool IsYourDigimonToDigivolve(CardSource source)
                {
                    return source.EqualsTraits("Rock Dragon") ||
                           source.EqualsTraits("Earth Dragon") ||
                           source.EqualsTraits("Machine Dragon") ||
                           source.EqualsTraits("Sky Dragon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return isExistOnField(card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, IsYourRockMachineDigimon);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card) &&
                           CardEffectCommons.CanActivateSuspendCostEffect(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    Permanent selectedPermanent = null;

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to digivolve.", "The opponent is selecting 1 Digimon to digivolve.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: selectedPermanent,
                                cardCondition: IsYourDigimonToDigivolve,
                                payCost: true,
                                reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null));
                    }
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            #endregion

            return cardEffects;
        }
    }
}