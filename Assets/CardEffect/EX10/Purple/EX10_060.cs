using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Lucemon: Satan Mode
namespace DCGO.CardEffects.EX10
{
    public class EX10_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared Methods

            bool IsOpponentDigimonOrTamer(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);
            }

            bool IsLucemonLarva(CardSource cardSource, ActivateClass activateClass)
            {
                return cardSource.IsDigimon
                    && cardSource.EqualsCardName("Lucemon: Larva")
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, isBreedingArea: true);
            }

            #region WD/WA Once Per Turn Shared Activate Coroutine

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool hasOpponentDeleted = false;

                if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentDigimonOrTamer))
                {

                    Permanent selectedPermanent = null;

                    #region Opponent Selects 1 Digimon or Tamer

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsOpponentDigimonOrTamer));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner.Enemy,
                        canTargetCondition: IsOpponentDigimonOrTamer,
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
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    #endregion

                    #region Attempt to delete selected permanent

                    if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent> { selectedPermanent },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<Permanent> deletedPermanents)
                    {
                        hasOpponentDeleted = true;
                        yield return null;
                    }

                    #endregion
                }

                if (!hasOpponentDeleted)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());

                    if (card.PermanentOfThisCard().CanUnsuspend) yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                        permanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        cardEffect: activateClass).Unsuspend());
                }
            }

            #endregion

            #region OP/WD Shared Activate Coroutine

            IEnumerator SharedActivateCoroutine1(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, cardsource => IsLucemonLarva(cardsource, activateClass)))
                {
                    CardSource selectedCard = null;

                    #region Select 1 [Lucemon: Larva] from Trash

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, cardsource => IsLucemonLarva(cardsource, activateClass)));
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: cardsource => IsLucemonLarva(cardsource, activateClass),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 [Lucemon: Larva] to play in breeding.",
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
                        selectedCard = cardSource;
                        yield return null;
                    }

                    selectCardEffect.SetUpCustomMessage("Select 1 [Lucemon: Larva] to play in breeding.", "The opponent is selecting 1 [Lucemon: Larva] to play in breeding.");
                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    #endregion

                    if (selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Trash,
                            activateETB: true,
                            isBreedingArea: true));

                        if (CardEffectCommons.IsExistOnBreedingAreaDigimon(selectedCard))
                        {
                            List<Permanent> selectedPermanents = card.Owner.Enemy.GetBattleAreaDigimons()
                                .Where(digimon => CardEffectCommons.IsMaxLevel(digimon, card.Owner.Enemy))
                                .ToList();

                            if (selectedPermanents.Any()) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                targetPermanents: selectedPermanents,
                                activateClass: activateClass,
                                successProcess: null,
                                failureProcess: null));
                        }
                    }
                }
            }

            #endregion

            #endregion

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel5 && targetPermanent.TopCard.EqualsCardName("Lucemon: Chaos Mode");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 6, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 1 [Lucemon: Larva] from trash in breeding, delete all opponents highest level digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine1(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By playing 1 [Lucemon: Larva] from your trash to your empty breeding area without paying the cost, delete all of your opponent's Digimon with the highest level.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, cardsource => IsLucemonLarva(cardsource, activateClass))
                        && !card.Owner.GetBreedingAreaPermanents().Any();
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 1 [Lucemon: Larva] from trash in breeding, delete all opponents highest level digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine1(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By playing 1 [Lucemon: Larva] from your trash to your empty breeding area without paying the cost, delete all of your opponent's Digimon with the highest level.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, cardsource => IsLucemonLarva(cardsource, activateClass))
                        && !card.Owner.GetBreedingAreaPermanents().Any();
                }
            }

            #endregion

            #region When Digivolving - Once Per Turn

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("your opponent may delete a digimon or tamer. if they didnt trash their top security & unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), 1, false, EffectDiscription());
                activateClass.SetHashString("EX10_060_WDWA");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] [Once Per Turn] Your opponent may delete 1 of their Digimon or Tamers. If this effect didn't delete, trash their top security card and this Digimon unsuspends.";
                }

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

            #region When Attacking - Once Per Turn

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("your opponent may delete a digimon or tamer. if they didnt trash their top security & unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), 1, false, EffectDiscription());
                activateClass.SetHashString("EX10_060_WDWA");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] Your opponent may delete 1 of their Digimon or Tamers. If this effect didn't delete, trash their top security card and this Digimon unsuspends.";
                }

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

            return cardEffects;
        }
    }
}