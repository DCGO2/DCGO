using System.Collections;
using System.Collections.Generic;

// MedievalGallantmon
namespace DCGO.CardEffects.EX4
{
    public class EX4_013 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Security
            if (timing == EffectTiming.None)
            {
                DontBattleSecurityDigimonClass dontBattleSecurityDigimonClass = new DontBattleSecurityDigimonClass();
                dontBattleSecurityDigimonClass.SetUpICardEffect("Ignore Battle", CanUseCondition, card);
                dontBattleSecurityDigimonClass.SetUpDontBattleSecurityDigimonClass(CardSourceCondition: CardSourceCondition);
                dontBattleSecurityDigimonClass.SetIsSecurityEffect(true);
                dontBattleSecurityDigimonClass.SetNotShowUI(true);
                cardEffects.Add(dontBattleSecurityDigimonClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanUseIgnoreBattle(hashtable, card);
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play this card without battling and return this Digimon to hand at the end of the turn", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Security] Play this card without battling and without paying the cost. At the end of the turn, return this Digimon to your hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnExecutingArea(card)
                        && CardEffectCommons.CanPlayAsNewPermanent(card, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: new List<CardSource>() { card },
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Security,
                        activateETB: true));

                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    ActivateClass activateClass1 = new ActivateClass();
                    activateClass1.SetUpICardEffect("Return the Digimon to hand", CanUseCondition1, card);
                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, "");
                    CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: activateClass1, timing: EffectTiming.OnEndTurn);

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    bool CanActivateCondition1(Hashtable hashtable)
                    {
                        return selectedPermanent.TopCard != null
                            && !selectedPermanent.CannotReturnToHand(activateClass1)
                            && !selectedPermanent.TopCard.CanNotBeAffected(activateClass1);
                    }

                    IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new HandBounceClaass(new List<Permanent>() { selectedPermanent }, CardEffectCommons.CardEffectHashtable(activateClass1)).Bounce());
                    }
                }
            }
            #endregion

            #region Shared OP/WA
            string SharedEffectName = "Delete 1 enemy Digimon with 6K DP or less, if none suspend 1 enemy Digimon and it doesn't unsuspend next enemy unsuspend phase";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenAttacking: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] Delete 1 of your opponent's Digimon with 6000 DP or less. If no Digimon was deleted by this effect, suspend 1 of your opponent's Digimon, and that Digimon doesn't unsuspend during your opponent's next unsuspend phase.";
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanSelectPermanentDeleteCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.DP <= card.Owner.MaxDP_DeleteEffect(6000, activateClass);
                }

                bool CanSelectPermanentSuspendCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                List<Permanent> deleteTargetPermanents = new List<Permanent>();

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentDeleteCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentDeleteCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        if (permanent != null) deleteTargetPermanents.Add(permanent);

                        yield return null;
                    }
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargetPermanents, activateClass: activateClass, successProcess: null, failureProcess: FailureProcess));

                IEnumerator FailureProcess()
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentSuspendCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentSuspendCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendNextActivePhase(
                                    targetPermanent: permanent,
                                    activateClass: activateClass
                                ));
                            }

                            yield return null;
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}