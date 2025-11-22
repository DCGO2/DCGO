using System.Collections;
using System.Collections.Generic;
using System;

//Proganomon
namespace DCGO.CardEffects.EX10
{
    public class EX10_032 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Hand Main

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Landramon] from trash under 1 [Sunarizamon], to digivolve for 3", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Hand] [Main] If you have [Close], by placing 1 [Landramon] from your trash as any of your [Sunarizamon]'s bottom digivolution card, it digivolves into this card for a digivolution cost of 3, ignoring digivolution requirements.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnHand(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsClose)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsSunarizamon)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsLandramon);
                }

                bool IsClose(Permanent permanent)
                {
                    return permanent.IsTamer && permanent.TopCard.EqualsCardName("Close");
                }

                bool IsSunarizamon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.EqualsCardName("Sunarizamon");
                }

                bool IsLandramon(CardSource source)
                {
                    return source.IsDigimon && source.EqualsCardName("Landramon");
                }

                bool IsProganomon(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnHand(cardSource) && cardSource == card;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsClose))
                    {
                        Permanent sunarizamon = null;
                        CardSource landramon = null;

                        #region Select Landramon From Trash

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, IsLandramon));
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        selectCardEffect.SetUp(
                                    canTargetCondition: IsLandramon,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 [Landramon] to add as digivolution source",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            landramon = cardSource;
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 1 [Landramon] to add as digivolution source.", "The opponent is selecting 1 [Landramon] to add as digivolution source.");
                        yield return StartCoroutine(selectCardEffect.Activate());

                        #endregion

                        if (landramon != null)
                        {
                            #region Select Sunarizamon Permanent

                            int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, IsSunarizamon));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsSunarizamon,
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
                                sunarizamon = permanent;
                                yield return null;
                            }

                            selectPermanentEffect.SetUpCustomMessage("Select 1 [Sunarizamon] to add digivolution sources.", "The opponent is selecting 1 [Sunarizamon] to add digivolution sources.");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            #endregion

                            if (sunarizamon != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(sunarizamon.AddDigivolutionCardsBottom(new List<CardSource>() { landramon }, activateClass));
                                if (sunarizamon.DigivolutionCards.Contains(landramon))
                                {
                                    #region Digivolve into MarineBullmon

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                        sunarizamon,
                                        IsProganomon,
                                        payCost: true,
                                        reduceCostTuple: null,
                                        fixedCostTuple: null,
                                        ignoreDigivolutionRequirementFixedCost: 3,
                                        isHand: true,
                                        activateClass: activateClass,
                                        successProcess: null,
                                        ignoreSelection: true,
                                        failedProcess: FailureProcess()));

                                    #endregion
                                }

                                IEnumerator FailureProcess()
                                {
                                    List<IDiscardHand> discardHands = new List<IDiscardHand>() { new IDiscardHand(card, null) };
                                    yield return ContinuousController.instance.StartCoroutine(new IDiscardHands(discardHands, null).DiscardHands());
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region OP/WD/WA Shared

            string SharedEffectDiscription(string tag)
            {
                return $"[{tag}] By trashing any 1 [Mineral] or [Rock] trait card from your Digimon's digivolution cards, 1 of your such Digimon gains <Collision>, <Piercing> and +3000 DP until your opponent's turn ends.";
            }

            bool CanSelectSource(CardSource source)
            {
                return source.HasRockMineralTraits;
            }

            bool CanSelectPermanent(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                       (permanent.TopCard.EqualsTraits("Mineral") || permanent.TopCard.EqualsTraits("Rock"));
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
                        canNoSelect: true,
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
                                    canNoSelect: () => true,
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

                                selectPermanentEffect1.SetUpCustomMessage("Select 1 Digimon to gain <Collision>, <Piercing>, +3K DP", "The opponent is selecting 1 Digimon to gain <Collision>, <Piercing>, +3K DP");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                                #endregion

                                if (selectedPermanment1 != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCollision(
                                        targetPermanent: selectedPermanment1,
                                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                        activateClass: activateClass));

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(
                                        targetPermanent: selectedPermanment1,
                                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                        activateClass: activateClass));

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                        targetPermanent: selectedPermanment1,
                                        changeValue: 3000,
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
                activateClass.SetUpICardEffect("By trashing 1 source, 1 digimon gains Collision, Piercing, +3K DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, SharedEffectDiscription("On Play"));
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
                activateClass.SetUpICardEffect("By trashing 1 source, 1 digimon gains Collision, Piercing, +3K DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, SharedEffectDiscription("When Digivolving"));
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
                activateClass.SetUpICardEffect("By trashing 1 source, 1 digimon gains Collision, Piercing, +3K DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, SharedEffectDiscription("When Attacking"));
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

            #region Trash Source - ESS

            if (timing == EffectTiming.OnDigivolutionCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("De-Digivolve 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "When effects trash this card from a [Mineral] or [Rock] trait Digimon's digivolution cards, <De-Digivolve 1> 1 of your opponent's Digimon.";
                }

                bool CanSelectOpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    Permanent trashedPermanent = CardEffectCommons.GetPermanentFromHashtable(hashtable);
                    return (trashedPermanent.TopCard.EqualsTraits("Mineral") || trashedPermanent.TopCard.EqualsTraits("Rock")) &&
                           CardEffectCommons.CanTriggerOnTrashSelfDigivolutionCard(hashtable, cardEffect => cardEffect != null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnTrash(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectOpponentsDigimon))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentsDigimon,
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
                        yield return ContinuousController.instance.StartCoroutine(new IDegeneration(permanent, 1, activateClass).Degeneration());
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}