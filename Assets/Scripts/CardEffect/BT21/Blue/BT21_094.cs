using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT21
{
    public class BT21_094 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main Effect

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Main] Reveal the top 3 cards of your deck. Add 1 card with [Davis Motomiya] in its name in its name and 1 card with the [Free] trait among them to the hand. Trash the rest. Then, place this card in the battle area.";
                }

                bool CanSelectDavisCardCondition(CardSource cardSource)
                {
                    return cardSource.ContainsCardName("Davis Motomiya");
                }

                bool CanSelectFreeCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Free");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                            revealCount: 3,
                            simplifiedSelectCardConditions:
                            new SimplifiedSelectCardConditionClass[]
                            {
                                new(
                                    canTargetCondition: CanSelectDavisCardCondition,
                                    message: "Select 1 card with [Davis Motomiya] in its name in its name.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null),
                                new(
                                    canTargetCondition: CanSelectFreeCardCondition,
                                    message: "Select 1 card with the [Free] trait.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null),
                            },
                            remainingCardsPlace: RemainingCardsPlace.Trash,
                            activateClass: activateClass
                        ));

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }

            #endregion

            #region Delay Effect

            if (timing == EffectTiming.WhenTopCardTrashed)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into a Digimon card with [Armor Form] trait in your hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetHashString("DigivolveIntoArmorForm_BT21_094");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] When the top stacked card of any your [Armor Form] trait Digimon is trashed, [Delay] • 1 of your Digimon may digivolve into a Digimon card with the [Armor Form] in the hand without paying the cost.";
                }

                bool TrashedCardCondition(CardSource cardSource)
                {
                    return cardSource.Owner == card.Owner &&
                           cardSource.EqualsTraits("Armor Form");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanDeclareOptionDelayEffect(card) &&
                           CardEffectCommons.CanTriggerWhenTopCardTrashed(hashtable, TrashedCardCondition);
                }

                bool CanSelectHandCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           cardSource.EqualsTraits("Armor Form");
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           card.Owner.HandCards.Where(CanSelectHandCardCondition).Any(cardSource =>
                               cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                            activateClass: activateClass, successProcess: permanents => SuccessProcess(),
                            failureProcess: null));
                }

                IEnumerator SuccessProcess()
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage(
                            "Select 1 Digimon to digivolve.",
                            "The opponent is selecting 1 Digimon to digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: selectedPermanent,
                                cardCondition: CanSelectHandCardCondition,
                                payCost: false,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null));
                        }
                    }
                }
            }

            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(
                    card: card,
                    cardEffects: ref cardEffects,
                    effectName: "Reveal the top 3 cards of your deck");
            }

            #endregion

            return cardEffects;
        }
    }
}