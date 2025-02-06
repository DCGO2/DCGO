using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;

namespace DCGO.CardEffects.EX8
{
    public class EX8_070 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 1 source, gain collision, piercing, reboot, +3000 DP, and can't be returned to hand or deck by opponent", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] By trashing any 1 digivolution card of 1 of your [Mineral] or [Rock] trait Digimon, until the end of your opponent's turn, it gains <Collision>, <Piercing> and <Reboot>, gets +3000 DP, and can't be returned to the hand or deck by your opponent's effects.";
                }

                bool HasProperTrait(CardSource source)
                {
                    return source.EqualsTraits("Mineral") || source.EqualsTraits("Rock");
                }

                bool DigimonWithProperSource(Permanent permanent)
                {
                    return CardEffectCommons.IsOwnerPermanent(permanent, card) &&
                           permanent.IsDigimon &&
                           HasProperTrait(permanent.TopCard) &&
                           permanent.DigivolutionCards.Count() > 0;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card) && 
                           CardEffectCommons.HasMatchConditionPermanent(DigimonWithProperSource);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool cardsTrashed = false;
                    Permanent selectedPermanent = null;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: DigimonWithProperSource,
                        cardCondition: HasProperTrait,
                        maxCount: 1,
                        canNoTrash: true,
                        isFromOnly1Permanent: true,
                        activateClass: activateClass,
                        afterSelectionCoroutine: AfterTrashedCards
                    ));

                    IEnumerator AfterTrashedCards(Permanent permanent, List<CardSource> cards)
                    {
                        if (cards.Count == 1)
                            cardsTrashed = true;

                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (cardsTrashed)
                    {
                        AddSkillClass addSkillClass = new AddSkillClass();
                        addSkillClass.SetUpICardEffect("Gain Collision", CanUseCondition1, card);
                        addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);

                        selectedPermanent.TopCard.Owner.UntilOpponentTurnEndEffects.Add((_timing) => addSkillClass);

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return true;
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            return PermanentCondition(selectedPermanent);
                        }

                        bool PermanentCondition(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    if(permanent == selectedPermanent)
                                        return true;
                                }
                            }

                            return false;
                        }

                        //yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCollision(selectedPermanent, EffectDuration.UntilOpponentTurnEnd, activateClass));
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(selectedPermanent, EffectDuration.UntilOpponentTurnEnd, activateClass));
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainReboot(selectedPermanent, EffectDuration.UntilOpponentTurnEnd, activateClass));
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(selectedPermanent, 3000, EffectDuration.UntilOpponentTurnEnd, activateClass));

                        bool CardEffectCondition(ICardEffect cardEffect)
                        {
                            return CardEffectCommons.IsOpponentEffect(cardEffect, card);
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotReturnToHand(
                            targetPermanent: selectedPermanent,
                            cardEffectCondition: CardEffectCondition,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            effectName: "Can't return to hand by opponent's effects"));

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotReturnToDeck(
                            targetPermanent: selectedPermanent,
                            cardEffectCondition: CardEffectCondition,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            effectName: "Can't return to deck by opponent's effects"));

                        List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                        {
                            if (_timing == EffectTiming.OnCounterTiming)
                            {
                                bool CardSourceCondition(CardSource cardSource)
                                {
                                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource))
                                    {
                                        if (cardSource == selectedPermanent.TopCard)
                                        {
                                            if (PermanentCondition(selectedPermanent))
                                            {
                                                return true;
                                            }
                                        }
                                    }

                                    return false;
                                }

                                bool Condition()
                                {
                                    return CardSourceCondition(cardSource);
                                }

                                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(false, cardSource, Condition));
                            }

                            return cardEffects;
                        }
                    }
                }
            }
            #endregion

            #region Security Effet
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Delete 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Delete 1 of your opponent's Digimon with the lowest play cost.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, true))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}