using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

// Shoutmon X7
namespace DCGO.CardEffects.AD1
{
    public class AD1_006 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolve Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Blue Flare")
                            || targetPermanent.TopCard.EqualsTraits("Xros Heart");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 6, permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP / WD / WA

            string SharedHashString = "AD1_006_OP_WD_WA";

            string SharedEffectName = "May bottom deck an opponent's Digimon with same or less DP as this. May unsuspend";

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] You may return 1 of your opponent's Digimon with as much or less DP as this Digimon to the bottom of the deck. Then, this Digimon may unsuspend.";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsWeakerEnemyDigimon)
                        || card.PermanentOfThisCard().IsSuspended);
            }

            bool IsWeakerEnemyDigimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.HasDP
                    && permanent.DP <= card.PermanentOfThisCard().DP;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsWeakerEnemyDigimon))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsWeakerEnemyDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                Permanent permanent = card.PermanentOfThisCard();

                if (permanent.IsSuspended && permanent.CanUnsuspend)
                {
                    string selectPlayerMessage = "Will you unsuspend this card?";
                    string notSelectPlayerMessage = "The opponent is choosing if they will unsuspend.";

                    List<SelectionElement<bool>> command_SelectCommands = new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: $"Yes", value: true, spriteIndex: 0),
                        new SelectionElement<bool>(message: $"No", value: false, spriteIndex: 1),
                    };

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: command_SelectCommands, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool unsuspend = GManager.instance.userSelectionManager.SelectedBoolValue;

                    if (unsuspend)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                            new List<Permanent>() { permanent },
                            activateClass).Unsuspend());
                    }
                }
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("On Play"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("When Digivolving"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("When Attacking"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place up to 4 [Xros Heart] or [Blue Flare] Digimon from this Digimon's digivolution cards under a tamer, then play 1 more for free", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] When this Digimon would leave the battle area other than by DigiXros, from this Digimon's digivolution cards, you may place up to 4 [Xros Heart] or [Blue Flare] trait Digimon cards under 1 of your Tamers and play 1 such card without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card)
                        && !CardEffectCommons.IsLeavingForDigiXros(hashtable);
                }

                bool IsXrosHeartOrBlueFlare(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && (cardSource.EqualsTraits("Blue Flare") || cardSource.EqualsTraits("Xros Heart"));
                }

                bool CanPlayXrosHeartOrBlueFlare(CardSource cardSource)
                {
                    return IsXrosHeartOrBlueFlare(cardSource)
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                bool CanSelectSaveTamerPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                           && permanent.IsTamer;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.PermanentOfThisCard().DigivolutionCards.Some(IsXrosHeartOrBlueFlare);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool placedCards = false;

                    #region Place up to 4 cards under a tamer
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectSaveTamerPermanentCondition))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = Math.Min(4, card.PermanentOfThisCard().DigivolutionCards.Count(IsXrosHeartOrBlueFlare));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: IsXrosHeartOrBlueFlare,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select digivolution cards to place under 1 of your Tamers.",
                            maxCount: maxCount,
                            canEndNotMax: true,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage(
                            "Select digivolution cards to place under 1 of your Tamers.",
                            "The opponent is selecting digivolution cards to place under 1 of your Tamers.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            
                            yield return null;
                        }

                        Permanent selectedPermanent = null;

                        if (selectedCards.Count > 0)
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectSaveTamerPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to place the chosen cards under.",
                                "The opponent is selecting 1 Tamer to place the chosen cards under.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                placedCards = true;
                                yield return ContinuousController.instance.StartCoroutine(
                                    selectedPermanent.AddDigivolutionCardsBottom(selectedCards, activateClass));
                            }
                        }
                    }
                    #endregion

                    #region Play 1 card
                    if (placedCards && card.PermanentOfThisCard().DigivolutionCards.Some(CanPlayXrosHeartOrBlueFlare))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanPlayXrosHeartOrBlueFlare,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 digivolution card to play.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage(
                            "Select 1 digivolution card to play.",
                            "The opponent is selecting 1 digivolution card to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        if (selectedCards.Count > 0) yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayPermanentCards(
                                cardSources: selectedCards,
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.DigivolutionCards,
                                activateETB: true));
                    }
                    #endregion
                }
            }
            #endregion

            #region Digixros
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.DigiXrosEffectFromNames(card, 2, null, 
                    "OmniShoutmon", 
                    "ZeigGreymon", 
                    "Ballistamon", 
                    "Dorulumon", 
                    "Starmons", 
                    "Sparrowmon"));
            }
            #endregion

            return cardEffects;
        }
    }
}
