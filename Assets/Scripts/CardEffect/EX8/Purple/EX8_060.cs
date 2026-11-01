using System.Collections;
using System.Collections.Generic;

// Myotismon
namespace DCGO.CardEffects.EX8
{
    public class EX8_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("NSo");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 4)
                );
            }
            #endregion

            #region When Attacking    
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may play 1 3 cost or less NSo Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] You may play 1 [NSo] trait Digimon card with a play cost of 3 or less from your trash without paying the cost.";
                }

                bool DigimonToPlay(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 3
                        && cardSource.EqualsTraits("NSo")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Trash);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, DigimonToPlay);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: DigimonToPlay,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: SelectCardCoroutine,
                        message: "Select 1 Digimon card to play.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage(
                        "Select 1 Digimon card to play.",
                        "The opponent is selecting 1 Digimon card to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Play Card");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(List<CardSource> sources)
                    {
                        selectedCards = sources;
                        yield return null;
                    }

                    if (selectedCards.Count > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Trash,
                            activateETB: true));
                    }
                }
            }         
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DNA Digivolve.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("DNADigivolve_EX8_060");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] [Once Per Turn] When your Digimon are played or digivolve, if any of them have the [NSo] trait, 2 of your Digimon may DNA digivolve into a Digimon card with the [NSo] trait in the hand. Then, that DNA digivolved Digimon may attack.";
                }

                bool PermanentConditionNSoEnterField(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.CardTraits.Contains("NSo");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentConditionNSoEnterField)
                            || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentConditionNSoEnterField));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectDNACardCondition);
                }

                bool CanSelectDNACardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.CanPlayJogress(true)
                        && cardSource.ContainsTraits("NSo");
                }
                
                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                            CanSelectDNACardCondition,
                            payCost: true,
                            isHand: true,
                            activateClass,
                            successProcess: OnDNASuccess
                        ));

                    IEnumerator OnDNASuccess(CardSource cardSource)
                    {
                        if (cardSource.PermanentOfThisCard().CanAttack(activateClass))
                        {
                            SelectAttackEffect selectAttackEffect =
                                GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: cardSource.PermanentOfThisCard(),
                                canAttackPlayerCondition: () => true,
                                defenderCondition: (permanent) => true,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect
                                .Activate());
                        }
                    }
                }
            }
            #endregion
            
            #region Inherited Effect
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete your another Digimon to unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Unsuspend_EX8_060");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] [Once Per Turn] By deleting 1 of your other Digimon, this Digimon unsuspends.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent != card.PermanentOfThisCard();
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool isUsed = false;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                isUsed = true;

                                Permanent thisCardPermanent = card.PermanentOfThisCard();

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { permanent },
                                activateClass: activateClass,
                                successProcess: permanents => SuccessProcess(),
                                failureProcess: null));

                                IEnumerator SuccessProcess()
                                {
                                    if (thisCardPermanent.TopCard != null)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { thisCardPermanent }, activateClass).Unsuspend());
                                    }
                                }
                            }
                        }
                    }

                    if (!isUsed) activateClass.RemoveUse();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}