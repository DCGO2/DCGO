using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//BT21_074 Satellamon
namespace DCGO.CardEffects.BT21
{
    public class BT21_074 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution condition
            if(timing == EffectTiming.None)
            {
                bool Condition(Permanent permanent)
                {
                    return permanent.Level == 4 && permanent.TopCard.HasText("Three Musketeers");
                }
            }
            #endregion

            #region On Play/When Digivolving shared
            bool CanActivateConditionSharedOP(Hashtable hashtable)
            {
                return isExistOnField(card) &&
                    (card.Owner.HandCards.Count >= 1 || card.Owner.TrashCards.Count >= 1) &&
                    CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanTuckUnderCondition);
            }

            bool CanTuckUnderCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) && !permanent.IsToken;
            }

            void ActivateDeDigivolveProtection()
            {
                ImmuneFromDeDigivolveClass immuneFromDeDigivolveClass = new ImmuneFromDeDigivolveClass();
                immuneFromDeDigivolveClass.SetUpICardEffect("Isn't affected by <De-Digivolve>", CanUseImmunityCondition,
                    card.PermanentOfThisCard().TopCard);
                immuneFromDeDigivolveClass.SetUpImmuneFromDeDigivolveClass(PermanentCondition: PermanentImmunityCondition);
                card.PermanentOfThisCard().UntilOpponentTurnEndEffects.Add(_ => immuneFromDeDigivolveClass);
            }

            bool CanUseImmunityCondition(Hashtable hashtable1)
            {
                return card.PermanentOfThisCard().TopCard != null;
            }

            bool PermanentImmunityCondition(Permanent permanent)
            {
                return permanent == card.PermanentOfThisCard();
            }
            #endregion

            #region OnPlay/WhenDigivolving/WhenAttacking shared
            bool CanTuckOrTrash(CardSource cardSource)
            {
                return card.EqualsTraits("Appmon") || card.HasThreeMusketeersTraits;
            }
            #endregion

            #region On Play
            if(timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Tuck to get protections", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionSharedOP, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] By placing 1 [Appmon] or [Three Musketeers] trait card from your hand or trash as any of your Digimon's bottom digivolution cards, until your opponent's turn ends, their effects can't return that Digimon to hand or decks or affect it with <De-Digivolve> effects.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = card.Owner.HandCards.Count(CanTuckOrTrash) >= 1;
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanTuckOrTrash);

                    if (canSelectHand || canSelectTrash)
                    {
                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                            {
                                new(message: "From hand", value: true, spriteIndex: 0),
                                new(message: "From trash", value: false, spriteIndex: 1),
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

                        List<CardSource> selectedCards = new List<CardSource>();

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        if (fromHand)
                        {
                            int maxCount = 1;

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanTuckOrTrash,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }

                        else
                        {
                            int maxCount = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanTuckOrTrash,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to place on bottom of digivolution cards.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");
                            selectCardEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }

                        if (selectedCards.Count >= 1)
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, CanTuckUnderCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanTuckUnderCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get a digivolution card.", "The opponent is selecting 1 Digimon that will get a digivolution card.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                Permanent selectedPermanent = permanent;

                                if (selectedPermanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCards[0] }, activateClass));

                                    #region Immunities

                                    bool CardEffectCondition(ICardEffect cardEffect)
                                    {
                                        return CardEffectCommons.IsOpponentEffect(cardEffect, card);
                                    }

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotReturnToHand(
                                        targetPermanent: card.PermanentOfThisCard(),
                                        cardEffectCondition: CardEffectCondition,
                                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                        activateClass: activateClass,
                                        effectName: "Can't return to hand by opponent's effects"));

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotReturnToDeck(
                                        targetPermanent: card.PermanentOfThisCard(),
                                        cardEffectCondition: CardEffectCondition,
                                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                        activateClass: activateClass,
                                        effectName: "Can't return to deck by opponent's effects"));

                                    ActivateDeDigivolveProtection();
                                    #endregion
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region When Digivolving (tuck)
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Tuck to get protections", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionSharedOP, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] By placing 1 [Appmon] or [Three Musketeers] trait card from your hand or trash as any of your Digimon's bottom digivolution cards, until your opponent's turn ends, their effects can't return that Digimon to hand or decks or affect it with <De-Digivolve> effects.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = card.Owner.HandCards.Count(CanTuckOrTrash) >= 1;
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanTuckOrTrash);

                    if (canSelectHand || canSelectTrash)
                    {
                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                            {
                                new(message: "From hand", value: true, spriteIndex: 0),
                                new(message: "From trash", value: false, spriteIndex: 1),
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

                        List<CardSource> selectedCards = new List<CardSource>();

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        if (fromHand)
                        {
                            int maxCount = 1;

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanTuckOrTrash,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }

                        else
                        {
                            int maxCount = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanTuckOrTrash,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to place on bottom of digivolution cards.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");
                            selectCardEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }

                        if (selectedCards.Count >= 1)
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, CanTuckUnderCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanTuckUnderCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get a digivolution card.", "The opponent is selecting 1 Digimon that will get a digivolution card.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                Permanent selectedPermanent = permanent;

                                if (selectedPermanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCards[0] }, activateClass));

                                    #region Immunities

                                    bool CardEffectCondition(ICardEffect cardEffect)
                                    {
                                        return CardEffectCommons.IsOpponentEffect(cardEffect, card);
                                    }

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotReturnToHand(
                                        targetPermanent: card.PermanentOfThisCard(),
                                        cardEffectCondition: CardEffectCondition,
                                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                        activateClass: activateClass,
                                        effectName: "Can't return to hand by opponent's effects"));

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotReturnToDeck(
                                        targetPermanent: card.PermanentOfThisCard(),
                                        cardEffectCondition: CardEffectCondition,
                                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                        activateClass: activateClass,
                                        effectName: "Can't return to deck by opponent's effects"));

                                    ActivateDeDigivolveProtection();
                                    #endregion
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region When Digivolving/When Attacking shared
            bool CanActivateConditionAtkShared(Hashtable hashtable)
            {
                return isExistOnField(card) && card.PermanentOfThisCard().DigivolutionCards.Some(CanTuckOrTrash);
            }

            bool DedigivolveTarget(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }
            #endregion

            #region When Digivolving (trash)
            if(timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash source to de-digivolve", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionAtkShared, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT21_074De-digivolve");
                cardEffects.Add(activateClass);
                    
                string EffectDescription()
                {
                    return "[When Digivolving][Once Per Turn] By trashing 1 card with the [Appmon] or [Three Musketeers] trait from your Digimon's digivolution cards, <De-Digivolve 1> 1 of your opponent's Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool trashed = false;

                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: CanTuckOrTrash,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 [Three Musketeers] or [Appmon] card to trash.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: null);

                    selectCardEffect.SetUpCustomMessage("Select 1 [Three Musketeers] or [Appmon] card to trash.", "The opponent is selecting 1 [Three Musketeers] or [Appmon] card to trash.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (selectedCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(card.PermanentOfThisCard(), selectedCards, activateClass).TrashDigivolutionCards());

                        trashed = true;
                        selectedCards.Clear();
                    }

                    if (trashed)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(DedigivolveTarget))
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: DedigivolveTarget,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectedDedigivolveTarget,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.", "The opponent is selecting 1 Digimon to De-Digivolve.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectedDedigivolveTarget(Permanent target)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IDegeneration(target, 1, activateClass).Degeneration());
                            }
                        }
                    }
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash source to de-digivolve", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionAtkShared, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT21_074De-digivolve");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving][Once Per Turn] By trashing 1 card with the [Appmon] or [Three Musketeers] trait from your Digimon's digivolution cards, <De-Digivolve 1> 1 of your opponent's Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool trashed = false;

                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: CanTuckOrTrash,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 [Three Musketeers] or [Appmon] card to trash.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: null);

                    selectCardEffect.SetUpCustomMessage("Select 1 [Three Musketeers] or [Appmon] card to trash.", "The opponent is selecting 1 [Three Musketeers] or [Appmon] card to trash.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (selectedCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(card.PermanentOfThisCard(), selectedCards, activateClass).TrashDigivolutionCards());

                        trashed = true;
                        selectedCards.Clear();
                    }

                    if (trashed)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(DedigivolveTarget))
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: DedigivolveTarget,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectedDedigivolveTarget,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.", "The opponent is selecting 1 Digimon to De-Digivolve.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectedDedigivolveTarget(Permanent target)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IDegeneration(target, 1, activateClass).Degeneration());
                            }
                        }
                    }
                }
            }
            #endregion

            #region Ability to link
            if (timing == EffectTiming.OnDeclaration)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.HasAppmonTraits;
                }
                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 3, card: card));

            }
            #endregion

            #region When Linking
            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete level 4", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Linking] Delete 1 of your opponent's level 4 or lower Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenLinking(hashtable, null, card);
                }

                bool DeletionTargetCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) && permanent.TopCard.HasLevel && permanent.Level <= 4;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card) && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, DeletionTargetCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if(CardEffectCommons.HasMatchConditionOpponentsPermanent(card, DeletionTargetCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = 1;

                        selectPermanentEffect.SetUp(

                            selectPlayer: card.Owner,
                            canTargetCondition: DeletionTargetCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
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