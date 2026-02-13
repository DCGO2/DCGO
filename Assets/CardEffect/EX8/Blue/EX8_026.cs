using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX8
{
    public class EX8_026 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("DS") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Blast Digivolve
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<De-Digivolve 1>, Return 1 Digimon with 7 cost or less to bottom of deck ", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] [When Digivolving] <De-Digivolve 1> 1 of your opponent's Digimon. Then, return 1 of your opponent's Digimon with a play cost of 7 or less to the bottom of the deck.";
                }

                bool OpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool OpponentsDigimonToReturn(Permanent permanent)
                {
                    return OpponentsDigimon(permanent) &&
                           permanent.TopCard.HasPlayCost && permanent.TopCard.GetCostItself <= 7;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && 
                           CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimon);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent dedigivolveSelected = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: OpponentsDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.", "The opponent is selecting 1 Digimon to De-Digivolve.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        dedigivolveSelected = permanent;
                        yield return null;
                    }

                    if(dedigivolveSelected != null)
                        yield return ContinuousController.instance.StartCoroutine(new IDegeneration(dedigivolveSelected, 1, activateClass).Degeneration());

                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimonToReturn))
                    {
                        SelectPermanentEffect selectReturnEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectReturnEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: OpponentsDigimonToReturn,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                        cardEffect: activateClass);

                        selectReturnEffect.SetUpCustomMessage("Select 1 Digimon to return to bottom of deck.", "The opponent is selecting 1 Digimon to return to bottom of deck.");
                        
                        yield return ContinuousController.instance.StartCoroutine(selectReturnEffect.Activate());
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<De-Digivolve 1>, Return 1 Digimon with 7 cost or less to bottom of deck ", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] <De-Digivolve 1> 1 of your opponent's Digimon. Then, return 1 of your opponent's Digimon with a play cost of 7 or less to the bottom of the deck.";
                }

                bool OpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool OpponentsDigimonToReturn(Permanent permanent)
                {
                    return OpponentsDigimon(permanent) &&
                           permanent.TopCard.HasPlayCost && permanent.TopCard.GetCostItself <= 7;
                }

                bool CanUseCondition(Hashtable hashtable)
                {                       
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimon);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent dedigivolveSelected = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: OpponentsDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.", "The opponent is selecting 1 Digimon to De-Digivolve.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        dedigivolveSelected = permanent;
                        yield return null;
                    }

                    if (dedigivolveSelected != null)
                        yield return ContinuousController.instance.StartCoroutine(new IDegeneration(dedigivolveSelected, 1, activateClass).Degeneration());

                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimonToReturn))
                    {
                        SelectPermanentEffect selectReturnEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectReturnEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: OpponentsDigimonToReturn,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                        cardEffect: activateClass);

                        selectReturnEffect.SetUpCustomMessage("Select 1 Digimon to return to bottom of deck.", "The opponent is selecting 1 Digimon to return to bottom of deck.");

                        yield return ContinuousController.instance.StartCoroutine(selectReturnEffect.Activate());
                    }
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.None)
            {
                CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                canNotSuspendClass.SetUpICardEffect("Opponents Digimon Can't Suspend", CanUseCondition, card);
                canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: CantSuspendCondition);
                cardEffects.Add(canNotSuspendClass);

                bool CantSuspendCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            if (!permanent.TopCard.CanNotBeAffected(canNotSuspendClass))
                                return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return card.Owner.MemoryForPlayer >= 1;
                    }

                    return false;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}