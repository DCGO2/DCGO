using System.Collections;
using System.Collections.Generic;

// Dosukomon
namespace DCGO.CardEffects.BT23
{
    public class BT23_021 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region App Fusion (Dokamon, Perorimon, Musclemon)

            if (timing == EffectTiming.None)
            {
                AddAppFusionConditionClass addAppFusionConditionClass = new AddAppFusionConditionClass();
                addAppFusionConditionClass.SetUpICardEffect($"App Fusion", (hashtable) => true, card);
                addAppFusionConditionClass.SetUpAddAppFusionConditionClass(getAppFusionCondition: GetAppFusion);
                addAppFusionConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAppFusionConditionClass);

                AppFusionCondition GetAppFusion(CardSource cardSource)
                {
                    bool linkCondition(Permanent permanent, CardSource source)
                    {
                        if (source != null && source != card)
                        {
                            if (permanent.TopCard.EqualsCardName("Dokamon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Perorimon") || x.EqualsCardName("Musclemon")))
                                {
                                    return true;
                                }
                            }

                            if (permanent.TopCard.EqualsCardName("Perorimon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Dokamon") || x.EqualsCardName("Musclemon")))
                                {
                                    return true;
                                }
                            }

                            if (permanent.TopCard.EqualsCardName("Musclemon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Dokamon") || x.EqualsCardName("Perorimon")))
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }
                    bool digimonCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        {
                            if (permanent.TopCard.EqualsCardName("Dokamon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Perorimon") || x.EqualsCardName("Musclemon")))
                                {
                                    return true;
                                }
                            }

                            if (permanent.TopCard.EqualsCardName("Perorimon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Dokamon") || x.EqualsCardName("Musclemon")))
                                {
                                    return true;
                                }
                            }

                            if (permanent.TopCard.EqualsCardName("Musclemon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Dokamon") || x.EqualsCardName("Perorimon")))
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }

                    if (cardSource == card)
                    {
                        AppFusionCondition AppFusionCondition = new AppFusionCondition(
                            linkCondition,
                            digimonCondition,
                            0);

                        return AppFusionCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasStandardAppTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Link Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasAppmonTraits;
                }
                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 2, card: card));
            }

            #endregion

            #region Link

            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
            }

            #endregion

            #endregion

            #region WD/WA OPT Shared

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent thisPermament = card.PermanentOfThisCard();
                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel && cardSource.IsLevel3
                        && cardSource.CanLinkToTargetPermanent(thisPermament, false);
                }

                bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                bool canSelectDigivolutionSources = thisPermament.StackCards.Exists(CanSelectCardCondition);

                if (canSelectHand || canSelectDigivolutionSources)
                {
                    if (canSelectHand && canSelectDigivolutionSources)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From digivolution", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "From which area do you select a card?";
                        string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                    }
                    else
                    {
                        GManager.instance.userSelectionManager.SetBool(canSelectHand);
                    }

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                    CardSource selectedCard = null;

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (fromHand)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 card to add as link", "The opponent is selecting 1 card to add as link");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }
                    else
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to add as link.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            customRootCardList: thisPermament.StackCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card  to add as link.", "The opponent is selecting 1 card to add as link.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(thisPermament.AddLinkCard(addedLinkCard: selectedCard, cardEffect: activateClass));
                }
            }

            #endregion

            #region When Digivolving - OPT

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Link 1 level 3 digimon from hand or digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), 1, true, EffectDiscription());
                activateClass.SetHashString("BT23_021_WD/WA");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] [Once Per Turn] You may link 1 level 3 Digimon card from your hand or this Digimon's digivolution cards to this Digimon without paying the cost.";
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

            #region When Attacking - OPT

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Link 1 level 3 digimon from hand or digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), 1, true, EffectDiscription());
                activateClass.SetHashString("BT23_021_WD/WA");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] You may link 1 level 3 Digimon card from your hand or this Digimon's digivolution cards to this Digimon without paying the cost.";
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

            #region AT/ESS Shared

            IEnumerator SharedActivateCoroutine1(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanNotBeDestroyedByBattleCondition(Permanent permanent, Permanent AttackingPermanent, Permanent DefendingPermanent, CardSource DefendingCard)
                {
                    if (permanent == AttackingPermanent)
                    {
                        return true;
                    }

                    if (permanent == DefendingPermanent)
                    {
                        return true;
                    }

                    return false;
                }

                Permanent thisPermanent = card.PermanentOfThisCard().TopCard.PermanentOfThisCard();
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBeDeletedByBattle(
                    targetPermanent: thisPermanent,
                    canNotBeDestroyedByBattleCondition: CanNotBeDestroyedByBattleCondition,
                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                    activateClass: activateClass,
                    effectName: "Can't be desstroyed by battle"));
            }

            #endregion

            #region All Turns - OPT

            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain immunity from battle", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine1(hash, activateClass), 1, false, EffectDiscription());
                activateClass.SetHashString("BT23_021_WL");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When this Digimon gets linked, it can't be deleted in battle until your opponent's turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenLinked(hashtable, perm => perm == card.PermanentOfThisCard(), null);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region Link ESS - OPT

            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain immunity from battle", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Linking] This Digimon can't be deleted in battle until your opponent's turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenLinking(hashtable, null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool CanNotBeDestroyedByBattleCondition(Permanent permanent, Permanent AttackingPermanent, Permanent DefendingPermanent, CardSource DefendingCard)
                    {
                        if (permanent == AttackingPermanent)
                        {
                            return true;
                        }

                        if (permanent == DefendingPermanent)
                        {
                            return true;
                        }

                        return false;
                    }

                    Permanent thisPermanent = card.PermanentOfThisCard().TopCard.PermanentOfThisCard();
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBeDeletedByBattle(
                        targetPermanent: thisPermanent,
                        canNotBeDestroyedByBattleCondition: CanNotBeDestroyedByBattleCondition,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass,
                        effectName: "Can't be deleted in battle"));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}