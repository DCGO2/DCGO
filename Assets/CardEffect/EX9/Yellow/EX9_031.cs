using System;
using System.Collections;
using System.Collections.Generic;

// Etemon
namespace DCGO.CardEffects.EX9
{
    public class EX9_031 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Sukamon Digivolution Cost Reduction

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Sukamon") && targetPermanent.TopCard.IsLevel4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition,
                    digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Level 4 DM Digivolution Cost Reduction

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4 && targetPermanent.TopCard.EqualsTraits("DM");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition,
                    digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Ver3 Digivolution Cost Reduction

            if (timing == EffectTiming.None)
            {
                Permanent selectedPermanent = null;
                bool Condition()
                {
                    return !CardEffectCommons.IsExistOnField(card);
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card) && targetPermanent.TopCard.EqualsTraits("Ver.3");
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                int ReduceCost()
                {
                    return selectedPermanent.DigivolutionCards.Filter(x => x.IsFlipped).Count;
                }

                cardEffects.Add(CardEffectFactory.ChangeDigivolutionCostStaticEffect<Func<int>>(
                    changeValue: () => -ReduceCost(),
                    permanentCondition: (permanent) => { selectedPermanent = permanent; return PermanentCondition(permanent); },
                    cardCondition: CardSourceCondition,
                    rootCondition: RootCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    setFixedCost: false));
            }

            #endregion

            #region Security Attack +1

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(
                    changeValue: 1,
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #endregion

            #region When Digivolving/When Attacking Shared

            IEnumerator SharedActivateCoroutine(ActivateClass activateClass)
            {
                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) && permanent.TopCard == card;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return card.PermanentOfThisCard().DigivolutionCards.FindLast(x => x.IsFlipped);
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                    permanentCondition: CanSelectPermanentCondition,
                    cardCondition: CanSelectCardCondition,
                    maxCount: 1,
                    canNoTrash: false,
                    isFromOnly1Permanent: true,
                    activateClass: activateClass
                ));

                if (card.Owner.LibraryCards.Count >= 1 && card.Owner.CanAddSecurity(activateClass))
                {
                    yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 face down digivolution card, to Recovery +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, _ => SharedActivateCoroutine(activateClass), 1, true, EffectDiscription());
                activateClass.SetHashString("Recovery_EX9_031");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] [Once Per Turn] By trashing this Digimon's bottom face-down digivolution card, <Recovery +1 (Deck)> (Place the top card of your deck on top of your security stack).";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 face down digivolution card, to Recovery +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, _ => SharedActivateCoroutine(activateClass), 1, true, EffectDiscription());
                activateClass.SetHashString("Recovery_EX9_031");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] By trashing this Digimon's bottom face-down digivolution card, <Recovery +1 (Deck)> (Place the top card of your deck on top of your security stack).";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DP -4000", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("ESS_EX9_031");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When your security stack is removed from, 1 of your opponent's Digimon gets -4000 DP for the turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => player == card.Owner);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -4000.", "The opponent is selecting 1 Digimon that will get DP -4000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -4000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}