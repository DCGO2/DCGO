using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Landramon
namespace DCGO.CardEffects.EX10
{
    public class EX10_028 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 1 sources, 1 Digimon gains <Reboot>, <Blocker> and +3000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By trashing any 1 card with the [Mineral] or [Rock] trait from your Digimon's digivolution cards, 1 of your Digimon with the [Mineral] or [Rock] trait gains <Reboot>, <Blocker> and +3000 DP until your opponent's turn ends.";
                }

                bool HasProperTrait(CardSource source)
                {
                    return !source.CanNotTrashFromDigivolutionCards(activateClass) &&
                           (source.EqualsTraits("Mineral") || source.EqualsTraits("Rock"));
                }

                bool HasProperAmountOfSources()
                {
                    int totalSourceCount = 0;

                    foreach (Permanent permanent in card.Owner.GetBattleAreaDigimons())
                    {
                        totalSourceCount += permanent.DigivolutionCards.Count(HasProperTrait);
                    }

                    return totalSourceCount >= 1;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        return permanent.DigivolutionCards.Count(HasProperTrait) >= 1;

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                        return HasProperAmountOfSources();

                    return false;
                }

                bool CanSelectYourDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           (permanent.TopCard.EqualsTraits("Mineral") || permanent.TopCard.EqualsTraits("Rock"));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int trashedCount = 0;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: CanSelectPermanentCondition,
                        cardCondition: HasProperTrait,
                        maxCount: 1,
                        canNoTrash: false,
                        isFromOnly1Permanent: false,
                        activateClass: activateClass,
                        afterSelectionCoroutine: AfterTrashedCards
                    ));

                    IEnumerator AfterTrashedCards(Permanent permanent, List<CardSource> cards)
                    {
                        trashedCount += cards.Count;

                        yield return null;
                    }

                    if (trashedCount == 1)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectYourDigimon))
                        {
                            Permanent selectedPermanent = null;

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectYourDigimon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectTargetPermanent,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 of your Digimon to gain effects & 3K DP", "The opponent is selecting 1 of their Digimon to gain effects & 3K DP");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectTargetPermanent(Permanent permanent)
                            {
                                selectedPermanent = permanent;
                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainReboot(
                                    targetPermanent: selectedPermanent,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass
                                ));

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(
                                    targetPermanent: selectedPermanent,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass
                                ));

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                    targetPermanent: selectedPermanent,
                                    changeValue: 3000,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass
                                ));
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 1 sources, 1 Digimon gains <Reboot>, <Blocker> and +3000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By trashing any 1 card with the [Mineral] or [Rock] trait from your Digimon's digivolution cards, 1 of your Digimon with the [Mineral] or [Rock] trait gains <Reboot>, <Blocker> and +3000 DP until your opponent's turn ends.";
                }

                bool HasProperTrait(CardSource source)
                {
                    return !source.CanNotTrashFromDigivolutionCards(activateClass) &&
                           (source.EqualsTraits("Mineral") || source.EqualsTraits("Rock"));
                }

                bool HasProperAmountOfSources()
                {
                    int totalSourceCount = 0;

                    foreach (Permanent permanent in card.Owner.GetBattleAreaDigimons())
                    {
                        totalSourceCount += permanent.DigivolutionCards.Count(HasProperTrait);
                    }

                    return totalSourceCount >= 1;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        return permanent.DigivolutionCards.Count(HasProperTrait) >= 1;

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                        return HasProperAmountOfSources();

                    return false;
                }

                bool CanSelectYourDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           (permanent.TopCard.EqualsTraits("Mineral") || permanent.TopCard.EqualsTraits("Rock"));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int trashedCount = 0;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: CanSelectPermanentCondition,
                        cardCondition: HasProperTrait,
                        maxCount: 1,
                        canNoTrash: false,
                        isFromOnly1Permanent: false,
                        activateClass: activateClass,
                        afterSelectionCoroutine: AfterTrashedCards
                    ));

                    IEnumerator AfterTrashedCards(Permanent permanent, List<CardSource> cards)
                    {
                        trashedCount += cards.Count;

                        yield return null;
                    }

                    if (trashedCount == 1)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectYourDigimon))
                        {
                            Permanent selectedPermanent = null;

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectYourDigimon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectTargetPermanent,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 of your Digimon to gain effects & 3K DP", "The opponent is selecting 1 of their Digimon to gain effects & 3K DP");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectTargetPermanent(Permanent permanent)
                            {
                                selectedPermanent = permanent;
                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainReboot(
                                    targetPermanent: selectedPermanent,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass
                                ));

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(
                                    targetPermanent: selectedPermanent,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass
                                ));

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                    targetPermanent: selectedPermanent,
                                    changeValue: 3000,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass
                                ));
                            }
                        }
                    }
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.OnDigivolutionCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 4 cost or less Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When effects trash this card from a [Mineral] or [Rock] trait Digimon's digivolution cards, delete 1 of your opponent's Digimon with a play cost of 4 or less.";
                }

                bool CanSelectOpponentsDigimon(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                        return permanent.TopCard.HasPlayCost && permanent.TopCard.GetCostItself <= 4;

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    Permanent trashedPermanent = CardEffectCommons.GetPermanentFromHashtable(hashtable);
                    return (trashedPermanent.TopCard.EqualsTraits("Mineral") || trashedPermanent.TopCard.EqualsTraits("Rock")) &&
                           CardEffectCommons.CanTriggerOnTrashSelfDigivolutionCard(hashtable, cardEffect => cardEffect != null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectOpponentsDigimon))
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
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }                        
                }
            }

            #endregion

            return cardEffects;
        }
    }
}