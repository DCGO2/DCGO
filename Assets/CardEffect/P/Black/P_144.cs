using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.P
{
    public class P_144 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.CardNames.Contains("Gotsumon"))
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

            #region Blocker/Can't Attack
            if(timing == EffectTiming.None)
            {
                bool HasGotsumonOrXAntibody(CardSource source)
                {
                    return source.ContainsCardName("Gotsumon") || source.ContainsCardName("X Antibody") || source.ContainsCardName("X-Antibody");
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        return card.PermanentOfThisCard().DigivolutionCards.Count(HasGotsumonOrXAntibody) == 0;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.CanNotAttackSelfStaticEffect(
                    defenderCondition: null,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    effectName: "Can't Attack"));

                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region When Target Changed - Unsuspened Blocker
            if (timing == EffectTiming.OnAttackTargetChanged)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend 1 of your Digimon with <Blocker>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Unsuspend_P_144");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn] (Once Per Turn) When an attack target is switched, you may unsuspend 1 of your Digimon with <Blocker>.";
                }

                bool PermanentHasBlocker(Permanent permanent)
                {
                    if(CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        return permanent.HasBlocker;

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return !CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(PermanentHasBlocker));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: PermanentHasBlocker,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.UnTap,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will unsuspend.", "The opponent is selecting 1 Digimon that will unsuspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            #region All Turns - DP Boost - ESS
            if (timing == EffectTiming.None)
            {
                string EffectDiscription()
                {
                    return "[All Turns] All of your Digimon with <Blocker> get +1000 DP.";
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.HasBlocker)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                    permanentCondition: PermanentCondition,
                    changeValue: 1000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition,
                    effectName: EffectDiscription));
            }
            #endregion

            return cardEffects;
        }
    }
}