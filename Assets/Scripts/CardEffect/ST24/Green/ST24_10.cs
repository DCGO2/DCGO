using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Lilamon
namespace DCGO.CardEffects.ST24
{
    public class ST24_10 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("DATA SQUAD");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 4, permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Shared OP/WD/WA
            string SharedEffectName = "Suspend 1 enemy Digimon/Tamer, by trashing 2 bottom face-down cards from your Tamers, may digivolve into a [DATA SQUAD] in the hand for free";

            string SharedEffectHash = "ST24_10_SharedEffect";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    maxCountPerTurn: 1,
                    hashValue: SharedEffectHash,
                    onPlay: true,
                    whenDigivolving: true,
                    whenAttacking: true);

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] Suspend 1 of your opponent's Digimon or Tamers. It can't unsuspend until their turn ends. Then, by trashing 2 bottom face-down cards from under any of your Tamers, this Digimon may digivolve into a [DATA SQUAD] trait Digimon card in the hand without paying the cost.";

            bool CanSelectPermanentConditionForSuspend(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon
                        || permanent.IsTamer);
            }

            bool TamerWithOneFaceDownSource(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Any(CanSelectTrashSourceCardCondition);
            }

            bool TamerWith2OrMoreFaceDownSources(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Count(CanSelectTrashSourceCardCondition) > 1;
            }

            bool CanSelectTrashSourceCardCondition(CardSource cardSource)
            {
                return cardSource.IsFlipped;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentConditionForSuspend));

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentConditionForSuspend,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon/Tamer to suspend and give can't unsuspend", "The opponent is selecting 1 Digimon/Tamer to suspend and give can't unsuspend");

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    List<Permanent> _targetPermanents = new List<Permanent>();
                    Permanent selectedPermanent = permanent;
                    _targetPermanents.Add(selectedPermanent);

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(_targetPermanents, hashtable).Tap());

                        CanNotUnsuspendClass canNotUnsuspendClass = new CanNotUnsuspendClass();
                        canNotUnsuspendClass.SetUpICardEffect("Can't Unsuspend", CanUseCondition1, card);
                        canNotUnsuspendClass.SetUpCanNotUntapClass(PermanentCondition: PermanentCondition);
                        selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => canNotUnsuspendClass);

                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                        }

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return selectedPermanent.TopCard != null
                                && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                        }

                        bool PermanentCondition(Permanent permanent)
                        {
                            return permanent == selectedPermanent;
                        }
                    }
                }

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                if (CardEffectCommons.HasMatchConditionPermanent(TamerWith2OrMoreFaceDownSources) || CardEffectCommons.MatchConditionPermanentCount(TamerWithOneFaceDownSource) > 1)
                {
                    bool trashed = false;

                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();
                    int maxCount1 = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(TamerWithOneFaceDownSource));

                    selectPermanentEffect1.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TamerWithOneFaceDownSource,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: maxCount1,
                        canNoSelect: true,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect1.SetUpCustomMessage("Select Tamer(s) to trash bottom face-down cards from", "The opponent is selecting Tamer(s) to trash bottom face-down cards from");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    bool CanEndSelectCondition(List<Permanent> permanents)
                    {
                        return permanents.Count == 2
                                || (permanents.Count > 0
                            && permanents[0].DigivolutionCards.Count(CanSelectTrashSourceCardCondition) >= 2);
                    }

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents.Count == 1)
                        {
                            trashed = true;
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 2, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));
                        }
                        else if (permanents.Count == 2)
                        {
                            trashed = true;
                            foreach (Permanent selectedPermanent in permanents)
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: 1, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));
                        }
                    }

                    if (trashed)
                    { 
                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource.EqualsTraits("DATA SQUAD");
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: CanSelectCardCondition,
                        payCost: false,
                        reduceCostTuple: null,
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true, activateClass: activateClass,
                        successProcess: null));
                    }
                }
            }
            #endregion

            #region Inherited
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash the bottom face-down card from 1 Tamer to not leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("ST24_10_Inherited");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon with [Rosemon] in its name or the [DATA SQUAD] trait would leave the battle area, by trashing the bottom face-down card from under any of your Tamers, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && (card.PermanentOfThisCard().TopCard.ContainsCardName("Rosemon")
                            || card.PermanentOfThisCard().TopCard.EqualsTraits("DATA SQUAD"))
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionPermanent(TamerWithOneFaceDownSource);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(TamerWithOneFaceDownSource))
                    {
                        Permanent thisCardPermanent = card.PermanentOfThisCard();
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: TamerWithOneFaceDownSource,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to trash 1 bottom face-down card from", "The opponent is selecting 1 Tamer to trash 1 bottom face-down card from");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: new List<Permanent>() { permanent }[0], trashCount: 1, isFromTop: false, activateClass: activateClass, CanSelectTrashSourceCardCondition));

                            thisCardPermanent.willBeRemoveField = false;

                            thisCardPermanent.HideDeleteEffect();
                            thisCardPermanent.HideHandBounceEffect();
                            thisCardPermanent.HideDeckBounceEffect();
                            thisCardPermanent.HideWillRemoveFieldEffect();

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
