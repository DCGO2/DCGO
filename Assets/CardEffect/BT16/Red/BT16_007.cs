using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class BT16_007 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            # region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.CardNames.Contains("Poromon"))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region When another Digimon is Played or Evolved, Gain 1 memory
            if (timing is EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When on of your other Digimon is played or digivolves, if that Digimon is yellow or has the [Free] trait, gain 1 memory.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent != card.PermanentOfThisCard())
                        {
                            if (permanent.IsDigimon)
                            {
                                if (permanent.TopCard.CardColors.Contains(CardColor.Yellow) ||
                                    permanent.TopCard.ContainsTraits("Free"))
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
                        if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition) || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition))
                        {
                            if (CardEffectCommons.IsOwnerTurn(card))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.CanAddMemory(activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            #region inherited effect
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 of your opponent's Digimon", CanUseCondition, card);
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] Suspend 1 of your opponent's Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }

                    return false;
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
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
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