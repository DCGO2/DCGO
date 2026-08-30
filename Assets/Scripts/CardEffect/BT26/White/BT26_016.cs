using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Chronomon: Holy Mode
namespace DCGO.CardEffects.BT26
{
    public class BT26_016 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Engage
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.EngageSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving / When Attacking
            string SharedEffectName()
                => "May delete 1 opponent's Digimon with as much DP as this or less, then return 3 trashed cards for Recovery +1";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] You may delete 1 of your opponent's Digimon with as much DP as this Digimon or less. Then, by returning 3 cards in trashes to the bottom of the deck, <Recovery +1>.";

            bool CanSelectDeleteTargetCondition(Permanent permanent, ICardEffect activateClass)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.HasDP
                    && card.PermanentOfThisCard().HasDP
                    && permanent.DP <= card.PermanentOfThisCard().DP;

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionPermanent(permanent => CanSelectDeleteTargetCondition(permanent, activateClass))
                    || card.Owner.TrashCards.Count + card.Owner.Enemy.TrashCards.Count >= 3;

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isUsed = false;

                bool CanSelectDeleteTargetConditionBound(Permanent permanent) => CanSelectDeleteTargetCondition(permanent, activateClass);

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteTargetConditionBound))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDeleteTargetConditionBound,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents != null && permanents.Count > 0) isUsed = true;
                        yield return null;
                    }
                }

                List<CardSource> combinedTrashPool = card.Owner.TrashCards.Clone().Concat(card.Owner.Enemy.TrashCards.Clone()).ToList();

                if (combinedTrashPool.Count >= 3)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: _ => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 3 cards in either trash to return to the bottom of your deck.",
                        maxCount: 3,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Custom,
                        customRootCardList: combinedTrashPool,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        yield return null;
                    }

                    selectCardEffect.SetUpCustomMessage("Select 3 cards in either trash to return to the bottom of your deck.", "The opponent is selecting 3 cards in either trash to return to the bottom of their deck.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    if (selectedCards.Count == 3)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(selectedCards, cardEffect: activateClass));

                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());

                        isUsed = true;
                    }
                }

                if (!isUsed) activateClass.RemoveUse();
            }
            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                additionalActivateCondition: AdditionalActivateCondition,
                optional: false,
                isSkippable: true,
                maxCountPerTurn: 1,
                hashValue: "BT26_016_OPT",
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return top security card to prevent this Digimon from leaving", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT26_016_AllTurns");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When this Digimon would leave the battle area, by returning your top security card to the bottom of the deck, it doesn't leave.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.Owner.SecurityCards.Count >= 1;

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> topSecurity = new List<CardSource>() { card.Owner.SecurityCards[0] };
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(topSecurity, cardEffect: activateClass));

                    yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                        player: card.Owner,
                        refSkillInfos: ref ContinuousController.instance.nullSkillInfos,
                        activateClass).ReduceSecurity());

                    card.PermanentOfThisCard().willBeRemoveField = false;

                    card.PermanentOfThisCard().HideHandBounceEffect();
                    card.PermanentOfThisCard().HideDeckBounceEffect();
                    card.PermanentOfThisCard().HideWillRemoveFieldEffect();
                    card.PermanentOfThisCard().HideDeleteEffect();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
