using System.Collections;
using System.Collections.Generic;
using System;

//Tuwarmon
namespace DCGO.CardEffects.EX10
{
    public class EX10_045 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Damemon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Digixros

            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros", CanUseCondition, card);
                addDigiXrosConditionClass.SetUpAddDigiXrosConditionClass(getDigiXrosCondition: GetDigiXros);
                addDigiXrosConditionClass.SetNotShowUI(true);
                cardEffects.Add(addDigiXrosConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                DigiXrosCondition GetDigiXros(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        DigiXrosConditionElement element = new DigiXrosConditionElement(CanSelectCardCondition, "Damemon");

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    if (cardSource.IsDigimon)
                                    {
                                        if (cardSource.CardNames_DigiXros.Contains("Damemon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        DigiXrosConditionElement element1 = new DigiXrosConditionElement(CanSelectCardCondition1, "ChuuChuumon");

                        bool CanSelectCardCondition1(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    if (cardSource.IsDigimon)
                                    {
                                        if (cardSource.CardNames_DigiXros.Contains("ChuuChuumon"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>() { element, element1 };

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 2);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region Collision

            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.CollisionSelfStaticEffect(false, card, null));
            }

            #endregion

            #region Rush

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #endregion

            #region OP/WD/WA Shared

            string SharedEffectDiscription(string tag)
            {
                return $"[{tag}] [Once Per Turn] By trashing any 1 digivolution card of your [Bagra Army] trait Digimon, 1 of your [Bagra Army] trait Digimon gains <Blocker> and <Retaliation> until your opponent's turn ends.";
            }

            bool CanSelectSource(CardSource source)
            {
                return source.HasBagraArmyTraits;
            }

            bool CanSelectPermanent(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                       CanSelectSource(permanent.TopCard);
            }

            bool CanSelectPermamentTrashDigivolution(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionCards.Exists(CanSelectSource);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermamentTrashDigivolution))
                {
                    #region Select Permament to trash Digivoltuion Cards

                    Permanent selectedPermanment = null;

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermamentTrashDigivolution));
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermamentTrashDigivolution,
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
                        selectedPermanment = permanent;
                        yield return null;
                    }

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon trash digivolution card", "The opponent is selecting 1 Digimon trash digivolution card");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    #endregion

                    if (selectedPermanment != null)
                    {
                        #region Select Digivolution Card to trash

                        CardSource selectedCard = null;
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectSource,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 digivolution card to discard.",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedPermanment.DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: null);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to discard.", "The opponent is selecting 1 digivolution card to discard.");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsAndProcessAccordingToResult(
                            targetPermanent: selectedPermanment,
                            targetDigivolutionCards: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                        IEnumerator SuccessProcess(List<CardSource> cardSources)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanent))
                            {
                                #region Select Permanent to gain effects

                                Permanent selectedPermanment1 = null;

                                int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanent));
                                SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect1.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanent,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    selectedPermanment1 = permanent;
                                    yield return null;
                                }

                                selectPermanentEffect1.SetUpCustomMessage("Select 1 Digimon to gain <Blocker> & <Retaliation>", "The opponent is selecting 1 Digimon to gain <Blocker> & <Retaliation>");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                                #endregion

                                if (selectedPermanment1 != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(
                                        targetPermanent: selectedPermanment1,
                                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                        activateClass: activateClass));

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRetaliation(
                                        targetPermanent: selectedPermanment1,
                                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                        activateClass: activateClass));
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 1 source, 1 digimon gains Blocker, Retaliation", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), 1, true, SharedEffectDiscription("On Play"));
                activateClass.SetHashString("OPWDWA_EX10_045");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 1 source, 1 digimon gains Blocker, Retaliation", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), 1, true, SharedEffectDiscription("When Digivolving"));
                activateClass.SetHashString("OPWDWA_EX10_045");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 1 source, 1 digimon gains Blocker, Retaliation", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), 1, true, SharedEffectDiscription("When Attacking"));
                activateClass.SetHashString("OPWDWA_EX10_045");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region Save

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.SaveEffect(card: card));
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.OnDigivolutionCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<Draw 1>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When effects trash this card from a [Bagra Army] trait Digimon's digivolution cards, <Draw 1>";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    Permanent trashedPermanent = CardEffectCommons.GetPermanentFromHashtable(hashtable);
                    return trashedPermanent.TopCard.EqualsTraits("Bagra Army") &&
                           CardEffectCommons.CanTriggerOnTrashSelfDigivolutionCard(hashtable, cardEffect => cardEffect != null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}