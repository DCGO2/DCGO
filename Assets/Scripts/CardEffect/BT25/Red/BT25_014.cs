using System.Collections;
using System.Collections.Generic;

// Meramon
namespace DCGO.CardEffects.BT25
{
    public class BT25_014 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.EqualsTraits("TS");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 2, true, card, null, level: 3));
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 enemy digimon with 4000 DP or less, if none deleted <Draw 2>", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT25_014_Main");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] [Once Per Turn] By trashing 1 [Flame] or [TS] trait card from your hand, delete 1 of your opponent's Digimon with 4000 DP or less. If this effect didn't delete, <Draw 2>.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Flame") 
                            || cardSource.HasTSTraits;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.DP <= card.Owner.MaxDP_DeleteEffect(4000, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    selectHandEffect.SetUpCustomMessage("Select 1 Card to trash.", "The opponent is selecting 1 card to trash from their hand.");

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count >= 1)
                        {
                            List<Permanent> deleteTargetPermanents = new List<Permanent>();

                            bool failedToDelete = true;

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                                {
                                    deleteTargetPermanents = permanents.Clone();

                                    yield return null;
                                }
                            }

                            if (deleteTargetPermanents.Count > 0)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargetPermanents, activateClass: activateClass, successProcess: SuccessDeleteProcess, failureProcess: null));

                                IEnumerator SuccessDeleteProcess(List<Permanent> permanents)
                                {
                                    if (permanents.Count > 0)
                                        failedToDelete = false;

                                    yield return null;
                                }
                            }

                            if (failedToDelete)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());
                            }
                        }
                    }
                }
            }
            #endregion

            #region ESS - When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 enemy Digimon with 4000 DP or less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT25_014_Inherited");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] [Once Per Turn] Delete 1 of your opponent's Digimon with 4000 DP or less.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.DP <= card.Owner.MaxDP_DeleteEffect(4000, activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

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
