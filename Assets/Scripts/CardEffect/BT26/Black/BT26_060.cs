using System;
using System.Collections;
using System.Collections.Generic;

// Chronomon: Destroy Mode
namespace DCGO.CardEffects.BT26
{
    public class BT26_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 6 && targetPermanent.TopCard.HasText("Chronomon"))
                        || targetPermanent.TopCard.ContainsCardName("Giant Slayer");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Security A. +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Succession
            if (timing == EffectTiming.None)
            {
                bool CardCondition(CardSource cardSource)
                    => cardSource.HasLevel && cardSource.Level == 6 && cardSource.ContainsCardName("Chronomon");

                cardEffects.Add(CardEffectFactory.SuccessionSelfEffect(isInheritedEffect: false, card: card, condition: null, cardCondition: CardCondition));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName() => "Return the top 5 stacked cards of 3 opponent's Digimon to the top of their deck";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Return the top 5 stacked cards of 3 of your opponent's Digimon to the top of the deck.";

            bool CanSelectTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool SharedCanActivateCondition(Hashtable hashtable, ICardEffect activateClass)
                => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                    && CardEffectCommons.HasMatchConditionPermanent(CanSelectTargetCondition);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTargetCondition))
                {
                    int maxCount = Math.Min(3, CardEffectCommons.MatchConditionPermanentCount(CanSelectTargetCondition));

                    List<Permanent> selectedPermanents = new List<Permanent>();

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectTargetCondition,
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
                        selectedPermanents.Add(permanent);
                        yield return null;
                    }

                    selectPermanentEffect.SetUpCustomMessage($"Select {maxCount} Digimon to return the top 5 stacked cards from.", "The opponent is selecting Digimon to return the top 5 stacked cards from.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    foreach (Permanent selectedPermanent in selectedPermanents)
                    {
                        if (selectedPermanent.TopCard == null) continue;
                        if (selectedPermanent.TopCard.CanNotBeAffected(activateClass)) continue;

                        int returnCount = Math.Min(5, selectedPermanent.DigivolutionCards.Count);

                        if (returnCount >= 1)
                        {
                            List<CardSource> returnedCards = selectedPermanent.DigivolutionCards.GetRange(0, returnCount);
                            returnedCards.Reverse();

                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(returnedCards));
                        }
                    }
                }
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }
            #endregion

            #region All Turns - Reactive Delete
            if (timing == EffectTiming.OnAddLibraryAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May delete 1 opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When your effects add to decks, you may delete 1 of your opponent's Digimon.";

                bool CardSourceCondition(CardSource cardSource) => cardSource.Owner == card.Owner;

                bool CanSelectDeleteTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAddLibrary(hashtable, CardSourceCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteTargetCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    SelectPermanentEffect selectDeleteEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectDeleteEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDeleteTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectDeleteEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectDeleteEffect.Activate());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
