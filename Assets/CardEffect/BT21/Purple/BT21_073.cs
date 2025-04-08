using System.Collections;
using System.Collections.Generic;
using System.Linq;

//BT21_073 Charismon
namespace DCGO.CardEffects.BT21
{
    public class BT21_073 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition - Sup.
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Sup.");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region  Appfusion
            if (timing == EffectTiming.None)
            {
                AddAppFusionConditionClass addAppFusionConditionClass = new AddAppFusionConditionClass();
                addAppFusionConditionClass.SetUpICardEffect($"App Fusion", CanUseCondition, card);
                addAppFusionConditionClass.SetUpAddAppFusionConditionClass(getAppFusionCondition: GetAppFusion);
                addAppFusionConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAppFusionConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                AppFusionCondition GetAppFusion(CardSource cardSource)
                {
                    string[] validCards = { "Sociamon", "Gossipmon" };
                    if (cardSource == card)
                    {
                        bool linkCondition(CardSource source)
                        {
                            foreach (string name in validCards)
                            {
                                if (source.EqualsCardName(name))
                                {
                                    foreach(string otherName in validCards)
                                    {
                                        if(otherName != name && source.PermanentOfThisCard().TopCard.EqualsCardName(otherName))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                            return false;
                        }

                        bool digimonCondition(Permanent permanent)
                        {
                            if (!card.CanNotEvolve(permanent))
                            {
                                foreach(string name in validCards)
                                {
                                    if (permanent.TopCard.EqualsCardName(name))
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        AppFusionCondition AppFusionCondition = new AppFusionCondition(
                            linkedCondition: linkCondition,
                            selectLinkMessage: "1 [Sociamon] or [Gossipmon]",
                            digimonCondition: digimonCondition,
                            selectDigimonMessage: "1 [Sociamon] or [Gossipmon]",
                            cost: 0);

                        return AppFusionCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region On Play/When Digivolving shared
            bool CanLinkCondition(CardSource cardSource)
            {
                return cardSource.HasLevel && cardSource.Level <= 4 && cardSource.CanLinkFromTargetPermanent(card.PermanentOfThisCard(), false);
            }

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return isExistOnField(card) && 
                    (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanLinkCondition) ||
                    card.PermanentOfThisCard().DigivolutionCards.Exists(CanLinkCondition));
            }
            #endregion

            #region On Play
            if(timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Link from trash or sources", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] You may link 1 level 4 or lower Digimon card from your trash or this Digimon's digivolution cards to this Digimon without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanLinkCondition);
                    bool canSelectSources = card.PermanentOfThisCard().DigivolutionCards.Exists(CanLinkCondition);

                    if(canSelectSources || canSelectTrash)
                    {
                        if(canSelectTrash && canSelectSources)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                            {
                                new(message: "From sources", value: true, spriteIndex: 0),
                                new(message: "From trash", value: false, spriteIndex: 1),
                            };

                            string selectPlayerMessage = "From which area do you select a card?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }

                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectSources);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool fromSources = GManager.instance.userSelectionManager.SelectedBoolValue;


                        List<CardSource> selectedCards = new List<CardSource>();

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        if (fromSources)
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanLinkCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 digivolution card to link.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to link.", "The opponent is selecting 1 digivolution card to link.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Linked Card");

                            yield return StartCoroutine(selectCardEffect.Activate());
                        }

                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                            selectCardEffect.SetUp(
                                canTargetCondition: CanLinkCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to link.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to link.", "The opponent is selecting 1 digivolution card to link.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Linked Card");

                            yield return StartCoroutine(selectCardEffect.Activate());
                        }

                        if (selectedCards.Count >= 1)
                        {
                            yield return card.PermanentOfThisCard().AddLinkCard(selectedCards[0], activateClass);
                        }
                    }
                
                    
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Link from trash or sources", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] You may link 1 level 4 or lower Digimon card from your trash or this Digimon's digivolution cards to this Digimon without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanLinkCondition);
                    bool canSelectSources = card.PermanentOfThisCard().DigivolutionCards.Exists(CanLinkCondition);

                    if (canSelectSources || canSelectTrash)
                    {
                        if (canSelectTrash && canSelectSources)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                            {
                                new(message: "From sources", value: true, spriteIndex: 0),
                                new(message: "From trash", value: false, spriteIndex: 1),
                            };

                            string selectPlayerMessage = "From which area do you select a card?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }

                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectSources);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool fromSources = GManager.instance.userSelectionManager.SelectedBoolValue;


                        List<CardSource> selectedCards = new List<CardSource>();

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        if (fromSources)
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanLinkCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 digivolution card to link.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to link.", "The opponent is selecting 1 digivolution card to link.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Linked Card");

                            yield return StartCoroutine(selectCardEffect.Activate());
                        }

                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                            selectCardEffect.SetUp(
                                canTargetCondition: CanLinkCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to link.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to link.", "The opponent is selecting 1 digivolution card to link.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Linked Card");

                            yield return StartCoroutine(selectCardEffect.Activate());
                        }

                        if (selectedCards.Count >= 1)
                        {
                            yield return card.PermanentOfThisCard().AddLinkCard(selectedCards[0], activateClass);
                        }
                    }


                }
            }
            #endregion

            #region Your turn
            if(timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Taunt when linked", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT21_073_tauntOnLink");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn][Once Per Turn] When this Digimon gets linked, give 1 of your opponent's Digimon \"[Start of Your Main Phase] This Digimon attacks.\" until their turn ends.";
                }

                bool targetPermanentCondition( Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool LinkedPermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenLinked(hashtable, LinkedPermanentCondition, null);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card) && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, targetPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(targetPermanentCondition))
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: targetPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get effects.",
                            "The opponent is selecting 1 Digimon that will get effects.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            ActivateClass activateClassDebuff = new ActivateClass();
                            activateClassDebuff.SetUpICardEffect("Attack with this Digimon", CanUseConditionDebuff,
                                selectedPermanent.TopCard);
                            activateClassDebuff.SetUpActivateClass(CanActivateConditionDebuff, ActivateCoroutineDebuff, -1, false,
                                EffectDescriptionDebuff());
                            activateClassDebuff.SetEffectSourcePermanent(selectedPermanent);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                    .CreateDebuffEffect(selectedPermanent));
                            }

                            string EffectDescriptionDebuff()
                            {
                                return "[Start of Your Main Phase] Attack with this Digimon.";
                            }

                            bool CanUseConditionDebuff(Hashtable hashtable1)
                            {
                                return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(selectedPermanent) &&
                                       selectedPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(selectedPermanent) &&
                                       GManager.instance.turnStateMachine.gameContext.TurnPlayer == selectedPermanent.TopCard.Owner;
                            }

                            bool CanActivateConditionDebuff(Hashtable hashtable1)
                            {
                                return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(selectedPermanent) &&
                                       !selectedPermanent.TopCard.CanNotBeAffected(activateClass) &&
                                       selectedPermanent.CanAttack(activateClassDebuff);
                            }

                            IEnumerator ActivateCoroutineDebuff(Hashtable hashtableDebuff)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent) &&
                                    selectedPermanent.CanAttack(activateClassDebuff))
                                {
                                    SelectAttackEffect selectAttackEffect =
                                        GManager.instance.GetComponent<SelectAttackEffect>();

                                    selectAttackEffect.SetUp(
                                        attacker: selectedPermanent,
                                        canAttackPlayerCondition: () => true,
                                        defenderCondition: _ => true,
                                        cardEffect: activateClassDebuff);

                                    selectAttackEffect.SetCanNotSelectNotAttack();

                                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                }
                            }

                            ICardEffect GetCardEffect(EffectTiming timingDebuff)
                            {
                                return timingDebuff == EffectTiming.OnStartMainPhase ? activateClassDebuff : null;
                            }
                        }
                    }
                }
            }
            #endregion

            #region Linking
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.HasAppmonTraits;
                }
                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 3, card: card));
            }

            if(timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
            }
            #endregion

            #region Link effect
            if (timing == EffectTiming.WhenRemoveField)
            {
                List<Permanent> removedPermanents = new List<Permanent>();

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash a linked card to prevent this digimon from leaving", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsLinkedEffect(true);
                activateClass.SetHashString("AllTurns_BT21_073");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns][Once Per Turn] When this Digimon would leave the battle area, by trashing 1 of this Digimon's link cards, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistLinked(card) &&
                           CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                    
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (permanent == card.PermanentOfThisCard())
                    {
                        if (permanent.LinkedCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return true;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistLinked(card) &&
                           !card.PermanentOfThisCard().HasNoLinkCards;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashLinkedCards(CanSelectPermanentCondition, CanSelectCardCondition, 1, true, true, activateClass,afterSelectionCoroutine: AfterSelectCardCoroutine));

                    bool trashed = false;

                    IEnumerator AfterSelectCardCoroutine(Permanent permanent, List<CardSource> cardSources)
                    {
                        if(cardSources.Count == 1)
                        {
                            trashed = true;
                        }
                        yield return null;
                    }

                    if (trashed)
                    {
                        card.PermanentOfThisCard().willBeRemoveField = false;

                        card.PermanentOfThisCard().HideHandBounceEffect();
                        card.PermanentOfThisCard().HideDeckBounceEffect();
                        card.PermanentOfThisCard().HideWillRemoveFieldEffect();
                        card.PermanentOfThisCard().HideDeleteEffect();
                    }
                }
            }
            #endregion

            return cardEffects; 
        }
    }
}