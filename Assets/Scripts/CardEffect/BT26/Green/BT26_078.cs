using System.Collections;
using System.Collections.Generic;

// Cherubimon
namespace DCGO.CardEffects.BT26
{
    public class BT26_078 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("TS");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Trash Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By returning this card to bottom of deck, 1 played [Chronomon]/[Titan] Digimon gains Rush and Execute", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Trash] [Your Turn] When any of your [Chronomon] text or [Titan] trait Digimon are played, if your opponent has 5 or more memory, by returning this card to the bottom of the deck, 1 of them gains Rush and Execute for the turn.";

                bool MatchingPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.HasText("Chronomon") || permanent.TopCard.EqualsTraits("Titan"));

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnTrash(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && card.Owner.Enemy.MemoryForPlayer >= 5
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, MatchingPermanentCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnTrash(card)
                        && card.Owner.Enemy.MemoryForPlayer >= 5;

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> permanents = CardEffectCommons.GetPlayedPermanentsFromEnterFieldHashtable(hashtable: hashtable, rootCondition: null);

                    if (permanents != null)
                    {
                        List<Permanent> matchingPermanents = permanents.Filter(MatchingPermanentCondition);

                        if (matchingPermanents.Count >= 1)
                        {
                            List<CardSource> cardSources = new List<CardSource>() { card };
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(cardSources));
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(cardSources, "Deck Bottom Card", true, true));

                            Permanent selectedPermanent = null;

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: permanent => matchingPermanents.Contains(permanent),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
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

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainExecute(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                            }
                        }
                    }
                }
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName() => "By deleting this Digimon, play 1 play cost 12 or lower [Chronomon]/[Titan] card from trash";

            string SharedEffectDescription(string tag)
                => $"[{tag}] By deleting this Digimon, you may play 1 play cost 12 or lower [Chronomon] text or [Titan] trait card from your trash without paying the cost.";

            bool CanSelectCardCondition(CardSource cardSource, ICardEffect activateClass)
                => cardSource.HasPlayCost
                    && cardSource.GetCostItself <= 12
                    && (cardSource.HasText("Chronomon") || cardSource.EqualsTraits("Titan"))
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                    targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                    activateClass: activateClass,
                    successProcess: SuccessProcess,
                    failureProcess: null));

                IEnumerator SuccessProcess(List<Permanent> permanents)
                {
                    bool CanSelectCardConditionBound(CardSource cardSource) => CanSelectCardCondition(cardSource, activateClass);

                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardConditionBound))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                            canTargetCondition: CanSelectCardConditionBound,
                            root: SelectCardEffect.Root.Trash,
                            cardEffect: activateClass,
                            payCost: false));
                    }
                }
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            return cardEffects;
        }
    }
}
