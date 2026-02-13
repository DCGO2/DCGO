using System;
using System.Collections;
using System.Collections.Generic;

// Infermon
namespace DCGO.CardEffects.BT22
{
    public class BT22_059 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4 && targetPermanent.TopCard.HasCSTraits;
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

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete a 5 or less play cost digimon, then if you have [Arata Sanada]/[Eater Adam], gain immunity to DP reduction and bounce", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Delete 1 of your opponent's Digimon with a play cost of 5 or less. Then, if you have [Arata Sanada] or [Eater Adam], your opponent's effects can't reduce this Digimon's DP or return it to hands or decks until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool OpponentDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasPlayCost && permanent.TopCard.BasePlayCostFromEntity <= 5;
                }

                bool IsArataOrAdam(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && (permanent.TopCard.EqualsCardName(name: "Arata Sanada") || permanent.TopCard.EqualsCardName(name: "Eater Adam"));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, OpponentDigimon))
                    {
                        #region Delete Opponent's Digimon

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, OpponentDigimon));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsArataOrAdam))
                    {
                        Permanent permanent = card.PermanentOfThisCard();

                        #region Gain Immunity to DP Minus and Bounce

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.GainImmuneFromDPMinus(permanent, null, EffectDuration.UntilOpponentTurnEnd, activateClass, "Immune from DP reduction"));

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.GainCanNotReturnToDeck(permanent, null, EffectDuration.UntilOpponentTurnEnd, activateClass, "Immune from bounce to deck"));

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.GainCanNotReturnToHand(permanent, null, EffectDuration.UntilOpponentTurnEnd, activateClass, "Immune from bounce to hand"));

                        #endregion
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete a 5 or less play cost digimon, then if you have [Arata Sanada]/[Eater Adam], gain immunity to DP reduction and bounce", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Delete 1 of your opponent's Digimon with a play cost of 5 or less. Then, if you have [Arata Sanada] or [Eater Adam], your opponent's effects can't reduce this Digimon's DP or return it to hands or decks until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool OpponentDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasPlayCost && permanent.TopCard.BasePlayCostFromEntity <= 5;
                }

                bool IsArataOrAdam(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && (permanent.TopCard.EqualsCardName(name: "Arata Sanada") || permanent.TopCard.EqualsCardName(name: "Eater Adam"));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, OpponentDigimon))
                    {
                        #region Delete Opponent's Digimon

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, OpponentDigimon));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsArataOrAdam))
                    {
                        Permanent permanent = card.PermanentOfThisCard();

                        #region Gain Immunity to DP Minus and Bounce

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.GainImmuneFromDPMinus(permanent, null, EffectDuration.UntilOpponentTurnEnd, activateClass, "Immune from DP reduction"));

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.GainCanNotReturnToDeck(permanent, null, EffectDuration.UntilOpponentTurnEnd, activateClass, "Immune from bounce to deck"));

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.GainCanNotReturnToHand(permanent, null, EffectDuration.UntilOpponentTurnEnd, activateClass, "Immune from bounce to hand"));

                        #endregion
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Diaboromon] Token", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT22_059_PlayDiaboromonToken");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When any of your Digimon with the [Unidentified] trait are deleted, you may play 1 [Diaboromon] Token without paying the cost. (Digimon/Cost 14/Lv.6/White/Mega/Unknown/Unidentified/3000 DP)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, IsUnidentified);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool IsUnidentified(Permanent permanent)
                {
                    return permanent.TopCard.IsDigimon &&
                           permanent.TopCard.HasUnidentifiedTraits &&
                           CardEffectCommons.IsOwnerPermanent(permanent, card) &&
                           permanent != card.PermanentOfThisCard();
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayDiaboromonToken(activateClass));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}