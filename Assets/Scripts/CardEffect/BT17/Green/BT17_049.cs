using System.Collections;
using System.Collections.Generic;
using System;

namespace DCGO.CardEffects.BT17
{
    public class BT17_049 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Turuiemon") || targetPermanent.TopCard.EqualsCardName("Wendigomon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Green/Yellow Level 3", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may play 1 green or yellow level 3 Digimon card from your trash without paying the cost.";
                }

                bool CanPlayCondition(CardSource source)
                {
                    if (source.IsDigimon)
                    {
                        if(source.HasLevel && source.Level == 3)
                        {
                            if(source.CardColors.Contains(CardColor.Green) || source.CardColors.Contains(CardColor.Yellow))
                            {
                                if (CardEffectCommons.CanPlayAsNewPermanent(source, false, activateClass))
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
                    if(CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlayCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanPlayCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: SelectCardCoroutine,
                        message: "Select 1 card to play.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if(cardSources.Count > 0)
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: cardSources, 
                                activateClass: activateClass, 
                                payCost: false, 
                                isTapped: false, 
                                root: SelectCardEffect.Root.Trash, 
                                activateETB: true));
                    }
                }
            }
            #endregion

            #region End of Attack - ESS
            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By deleting 1 digimon, Play 1 from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("EndofAttack_BT17_049");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Attack] [Once Per Turn] By deleting 1 of your other suspended Digimon, you may play 1 level 3 Digimon card with the [Beast] trait from your trash without paying the cost.";
                }

                bool CanPlayCondition(CardSource source)
                {
                    if (source.IsDigimon)
                    {
                        if (source.HasLevel && source.Level == 3)
                        {
                            if (source.CardTraits.Contains("Beast"))
                            {
                                if (CardEffectCommons.CanPlayAsNewPermanent(source, false, activateClass))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool IsDeletableDigimon(Permanent permanent)
                {
                    if(CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if(permanent != card.PermanentOfThisCard())
                        {
                            return permanent.IsSuspended;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsDeletableDigimon));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsDeletableDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: SelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                            targetPermanents: permanents, 
                            activateClass: activateClass, 
                            successProcess: permanents => SuccessProcess(), 
                            failureProcess: null));
                    }

                    IEnumerator SuccessProcess()
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanPlayCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: SelectCardCoroutine,
                            message: "Select 1 card to play.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectCardEffect.Activate());
                    }

                    IEnumerator SelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count > 0)
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: cardSources,
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Trash,
                                activateETB: true));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}