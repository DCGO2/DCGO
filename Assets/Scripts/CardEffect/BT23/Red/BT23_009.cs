using System;
using System.Collections;
using System.Collections.Generic;

// Coachmon 
namespace DCGO.CardEffects.BT23
{
    public class BT23_009 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition - Stnd.

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasStandardAppTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Link
            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
            }
            #endregion

            #region Link Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent) => targetPermanent.IsDigimon && targetPermanent.TopCard.HasAppmonTraits;
                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 2, card: card));
            }

            #endregion

            #region When Linked - OPT

            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 digimon gains 4K DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("BT23-009_WL");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When this Digimon gets linked, 1 of your Digimon gets +4000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenLinked(hashtable, ThisDigimonPermanentCondition, null);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool ThisDigimonPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent == card.PermanentOfThisCard();
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));


                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to gain 4K DP", "Opponent is selecting 1 digimon to gain 4K DP");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());


                        if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: selectedPermanent,
                            changeValue: 4000,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));
                    }
                }
            }

            #endregion

            #region End of Your Turn - OPT

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This digimon may attack a player", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsLinkedEffect(true);
                activateClass.SetHashString("BT23-009_OET");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] [Once Per Turn] This Digimon may attack a player.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.PermanentOfThisCard().CanAttack(activateClass))
                    {
                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: card.PermanentOfThisCard(),
                            canAttackPlayerCondition: () => true,
                            defenderCondition: (permanent) => false,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }

                }
            }

            #endregion

            return cardEffects;
        }
    }
}