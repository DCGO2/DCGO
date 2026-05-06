using System.Collections;
using System.Collections.Generic;
using System.Linq;

// MirageGaogamon
namespace DCGO.CardEffects.BT25
{
    public class BT25_029 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent) => permanent.TopCard.ContainsCardName("Gaogamon") || permanent.TopCard.EqualsTraits("DATA SQUAD");
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null, level: 5));
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            #endregion

            #region Blocker
            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            #endregion

            #region Evade
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted) cardEffects.Add(CardEffectFactory.EvadeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            #endregion

            #endregion

            #region Shared WD/WA
            string SharedEffectName = "May return 1 enemy level 5 or lower digimon to hand, then by trashing bottom face down under a tamer, return 1 enemy lowest level digimon to hand";
            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] You may return 1 of your opponent's level 5 or lower Digimon to the hand. Then, by trashing the bottom face-down card under any of your Tamers, return 1 of your opponent's lowest level Digimon to the hand.";
            string SharedHashValue = "BT25_029_WD_WA";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    hashValue: SharedHashValue,
                    optional: false,
                    isSkippable: true,
                    whenDigivolving: true,
                    whenAttacking: true);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool Level5OrLowerPermanentCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.HasLevel
                    && permanent.Level <= 5;

                bool TamerWithFaceDownCardCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.HasFaceDownDigivolutionCards;

                bool LowestLevelPermanentCondition(Permanent permanent) => CardEffectCommons.IsMinLevel(permanent, card.Owner.Enemy);

                bool Used = false;

                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, Level5OrLowerPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: Level5OrLowerPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Bounce,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's Digimon to return to hand", "The opponent is selecting 1 Digimon to return to hand");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanent)
                    {
                        Used = true;
                        return null;
                    }
                }

                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, TamerWithFaceDownCardCondition))
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: TamerWithFaceDownCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 of your tamers to trash bottom face down card", "The opponent is selecting 1 of their tamers to trash bottom face down card");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    if (selectedPermanent != null)
                    {
                        var cardToTrash = selectedPermanent.DigivolutionCards.Last(x => x.IsFaceDown);
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsAndProcessAccordingToResult(
                        targetPermanent: selectedPermanent,
                        targetDigivolutionCards: new List<CardSource>() { cardToTrash },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                        IEnumerator SuccessProcess(List<CardSource> cardSources)
                        {
                            if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, LowestLevelPermanentCondition))
                            {
                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: LowestLevelPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Bounce,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's lowest level Digimon to return to hand", "The opponent is selecting 1 lowest level Digimon to return to hand");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            }

                            Used = true;
                        }
                    }
                }

                if (!Used) activateClass.RemoveUse();
            }
            #endregion

            #region All Turns

            #region All Turns Shared

            string AllTurnsSharedHashValue = "BT25-029-OnAddHand-OnTrashTamerCards";
            string AllTurnsSharedEffectName = "You may unsuspend this digimon";
            string AllTurnsSharedDescription = "[All Turns] [Once Per Turn] When effects add cards to your opponent's hand or trash cards from under your Tamers, this Digimon may unsuspend.";

            IEnumerator SharedActivateCoroutine1(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                    permanents: new List<Permanent>() { card.PermanentOfThisCard() },
                    cardEffect: activateClass
                    ).Unsuspend());
            }

            #endregion

            #region All Turns - On Add Hand
            if (timing == EffectTiming.OnAddHand)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(AllTurnsSharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine1(hash, activateClass), 1, true, AllTurnsSharedDescription);
                activateClass.SetHashString(AllTurnsSharedHashValue);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenAddHand(hashtable, player => player == card.Owner.Enemy, cardEffect => cardEffect != null);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }
            #endregion

            #region All Turns - On Trash Tamer Cards
            if (timing == EffectTiming.OnDigivolutionCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(AllTurnsSharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine1(hash, activateClass), 1, true, AllTurnsSharedDescription);
                activateClass.SetHashString(AllTurnsSharedHashValue);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnTrashDigivolutionCard(hashtable, IsYourTamerCondition, cardEffect => cardEffect != null, null);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool IsYourTamerCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card);
            }
            #endregion

            #endregion

            return cardEffects;
        }
    }
}
