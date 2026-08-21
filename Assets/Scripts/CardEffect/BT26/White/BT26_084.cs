using System;
using System.Collections;
using System.Collections.Generic;

// Copipemon
namespace DCGO.CardEffects.BT26
{
    public class BT26_084 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Appmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 2));
            }
            #endregion

            #region Detach
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 1 of its [Seven Code] link cards, it doesn't leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "<Detach [Seven Code] trait> (When this Digimon would leave the battle area other than by your effects, by trashing 1 of its specified link cards, it doesn't leave.)";

                bool NotByYourEffects(Hashtable hashtable)
                    => !CardEffectCommons.IsByEffect(hashtable, ce => ce.EffectSourceCard != null && ce.EffectSourceCard.Owner == card.Owner);

                bool CanSelectLinkCardCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("Seven Code");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card)
                        && NotByYourEffects(hashtable);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionPermanent(p => p == card.PermanentOfThisCard())
                        && card.PermanentOfThisCard().LinkedCards.Exists(CanSelectLinkCardCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    if (thisPermanent != null && thisPermanent.LinkedCards.Exists(CanSelectLinkCardCondition))
                    {
                        CardSource selectedLinkCard = null;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectLinkCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 [Seven Code] trait link card to trash so this Digimon doesn't leave.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: thisPermanent.LinkedCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedLinkCard = cardSource;
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        if (selectedLinkCard != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new ITrashLinkCards(thisPermanent, new List<CardSource>() { selectedLinkCard }, activateClass).TrashLinkCards());

                            thisPermanent.willBeRemoveField = false;
                            thisPermanent.HideDeleteEffect();
                            thisPermanent.HideHandBounceEffect();
                            thisPermanent.HideDeckBounceEffect();
                            thisPermanent.HideWillRemoveFieldEffect();
                        }
                    }
                }
            }
            #endregion

            #region This Digimon gets linked - Reveal & Play
            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal top 3, may play/use 1 [Seven Code] card among them for 3 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT26_084_WhenLinked");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] [Once Per Turn] When this Digimon gets linked, reveal the top 3 cards of your deck. You may play or use 1 [Seven Code] trait card among them with the cost reduced by 3. Return the rest to the top or bottom of the deck.";

                bool ThisPermanentCondition(Permanent permanent) => permanent == card.PermanentOfThisCard();

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.EqualsTraits("Seven Code")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass, root: SelectCardEffect.Root.Library, isPlayOption: true, fixedCost: cardSource.GetCostItself - 3);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerWhenLinked(hashtable, ThisPermanentCondition, null);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.Owner.LibraryCards.Count >= 1;

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    CardSource selectedCard = null;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions: new SimplifiedSelectCardConditionClass[]
                        {
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition: CanSelectCardCondition,
                                message: "Select 1 [Seven Code] trait card to play or use with the cost reduced by 3.",
                                mode: SelectCardEffect.Mode.Custom,
                                maxCount: 1,
                                selectCardCoroutine: SelectCardCoroutine),
                        },
                        remainingCardsPlace: RemainingCardsPlace.DeckTopOrBottom,
                        activateClass: activateClass
                    ));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selectedCard != null)
                    {
                        int reduceCost = 3;

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect($"Cost -{reduceCost}", _ => true, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: PlayCondition, rootCondition: _ => true, isUpDown: () => true, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                        card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                        ICardEffect GetCardEffect(EffectTiming _timing)
                            => _timing == EffectTiming.None ? changeCostClass : null;

                        bool PlayCondition(CardSource cs) => cs == selectedCard;

                        int ChangeCost(CardSource cs, int cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                            => PlayCondition(cs) ? cost - reduceCost : cost;

                        if (selectedCard.IsOption)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                cardSources: new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: true,
                                root: SelectCardEffect.Root.Library));
                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: true,
                                isTapped: false,
                                root: SelectCardEffect.Root.Library,
                                activateETB: true));
                        }

                        card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                    }
                }
            }
            #endregion

            #region Link Condition
            if (timing == EffectTiming.None)
            {
                static bool LinkPermanentCondition(Permanent targetPermanent) => targetPermanent.TopCard.EqualsTraits("Appmon");

                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: LinkPermanentCondition, linkCost: 3, card: card));
            }
            #endregion

            #region Link
            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
            }
            #endregion

            #region When Linking - Bonus Link From Trash
            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Link 1 non-white lvl 4 or lower [System]/[Seven Code] card from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Linking] You may link 1 non-white level 4 or lower [System] or [Seven Code] trait card from your trash to this Digimon without paying the cost.";

                bool CanSelectCardCondition(CardSource cardSource)
                    => cardSource.HasLevel && cardSource.Level <= 4
                        && !cardSource.HasCardColor(CardColor.White)
                        && (cardSource.EqualsTraits("System") || cardSource.EqualsTraits("Seven Code"));

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerWhenLinking(hashtable, null, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        CardSource selectedCard = null;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to link to this Digimon.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        if (selectedCard != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new ILinkCard(payCost: false, card: selectedCard, permanent: card.PermanentOfThisCard(), cardEffect: activateClass).LinkCard());
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
