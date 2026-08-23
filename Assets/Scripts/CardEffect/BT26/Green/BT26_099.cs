using System.Collections;
using System.Collections.Generic;

// Training Manual
namespace DCGO.CardEffects.BT26
{
    public class BT26_099 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Use Req.
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource) => cardSource.EqualsTraits("DM");
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal top 3, add 1 [DM] card to hand, then place this in battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Reveal the top 3 cards of your deck. Add 1 [DM] trait card among them to the hand. Return the rest to the bottom of the deck. Then, place this card in the battle area.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectCardCondition(CardSource cardSource) => cardSource.EqualsTraits("DM");

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions: new SimplifiedSelectCardConditionClass[]
                        {
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition: CanSelectCardCondition,
                                message: "Select 1 [DM] trait card to add to your hand.",
                                mode: SelectCardEffect.Mode.AddHand,
                                maxCount: 1,
                                selectCardCoroutine: null),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                        activateClass: activateClass
                    ));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            #endregion

            #region All Turns - Reactive Delay
            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing this card, 1 Digimon may digivolve into a level 6 or lower [DM] card from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] When face-down cards are placed in any of your Digimon's digivolution cards, <Delay> (Trash this card in your battle area to activate the effect below.) Any of those Digimon may digivolve into a level 6 or lower [DM] trait Digimon card in the hand without paying the cost.";

                bool FaceDownCardCondition(CardSource cardSource) => cardSource.IsFaceDown;

                bool OwnerDigimonCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool CanSelectDigivolveTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.DigivolutionCards.Exists(FaceDownCardCondition);

                bool CanSelectDMCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon && cardSource.HasLevel && cardSource.Level <= 6 && cardSource.EqualsTraits("DM");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAddDigivolutionCard(hashtable, OwnerDigimonCondition, null, FaceDownCardCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<Permanent> triggeringPermanents = new List<Permanent>();

                    void AddTriggeringPermanent(Permanent permanent)
                    {
                        if (permanent != null && !triggeringPermanents.Contains(permanent))
                        {
                            triggeringPermanents.Add(permanent);
                        }
                    }

                    AddTriggeringPermanent(CardEffectCommons.GetPermanentFromHashtable(_hashtable));

                    // Other Digimon that had face-down cards placed on them by the same effect, in the same
                    // window, before this trigger got resolved (e.g. an effect that places 1 card face down under
                    // each of 2 selected Digimon at once) — each Digimon fires its own OnAddDigivolutionCards
                    // hashtable, so gather any still-pending sibling triggers for this same card's ability.
                    foreach (SkillInfo skillInfo in GManager.instance.autoProcessing.StackedSkillInfos)
                    {
                        if (skillInfo.Timing == EffectTiming.OnAddDigivolutionCards
                            && skillInfo.CardEffect != null
                            && skillInfo.CardEffect.EffectSourceCard == card)
                        {
                            AddTriggeringPermanent(CardEffectCommons.GetPermanentFromHashtable(skillInfo.Hashtable));
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<Permanent> permanents)
                    {
                        List<Permanent> eligiblePermanents = triggeringPermanents.Filter(CanSelectDigivolveTargetCondition);

                        if (eligiblePermanents.Count == 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                eligiblePermanents[0],
                                CanSelectDMCardCondition,
                                payCost: false,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null
                            ));
                        }
                        else if (eligiblePermanents.Count >= 2)
                        {
                            Permanent selectedPermanent = null;

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: eligiblePermanents.Contains,
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

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that may digivolve.", "The opponent is selecting 1 Digimon that may digivolve.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                    selectedPermanent,
                                    CanSelectDMCardCondition,
                                    payCost: false,
                                    reduceCostTuple: null,
                                    fixedCostTuple: null,
                                    ignoreDigivolutionRequirementFixedCost: -1,
                                    isHand: true,
                                    activateClass: activateClass,
                                    successProcess: null
                                ));
                            }
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(
                    card: card,
                    cardEffects: ref cardEffects,
                    effectName: "Activate this card's [Main] effects.");
            }
            #endregion

            return cardEffects;
        }
    }
}
