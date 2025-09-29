using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Magneticdramon
namespace DCGO.CardEffects.EX10
{
    public class EX10_036 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.HasMatchConditionOwnersPermanent(card, permanent =>
                        permanent.TopCard.EqualsCardName("Close")
                    );
                }

                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Proganomon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 6, ignoreDigivolutionRequirement: false, card: card, condition: Condition));
            }

            #endregion

            #region Fragment

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                string EffectDiscription()
                {
                    return "<Fragment <3>> (When this Digimon would be deleted, by trashing any 3 of its digivolution cards, it isn’t deleted.)";
                }

                cardEffects.Add(CardEffectFactory.FragmentSelfEffect(isInheritedEffect: false, card: card, condition: null, trashValue: 3, effectName: "Fragment <3>", effectDiscription: EffectDiscription()));
            }

            #endregion

            #region When Digivolving - Add Sources OPT
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 3 cards from trash as bottom sources", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("WDWA_EX10_036");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] [Once Per Turn] By placing 3 [Mineral] or [Rock] trait cards from your trash as this Digimon's bottom digivolution cards, it unsuspends.";
                }

                bool HasProperTrait(CardSource source)
                {
                    return source.EqualsTraits("Mineral") || source.EqualsTraits("Rock");

                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, HasProperTrait) >= 3;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: HasProperTrait,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        canNoSelect: () => false,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select [Mineral] or [Rock] to place on bottom of digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                        maxCount: 3,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");
                    selectCardEffect.SetUpCustomMessage("Select [Mineral] or [Rock] to place on bottom of digivolution cards.", "The opponent is selecting [Mineral] or [Rock] to place on bottom of digivolution cards.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    bool CanEndSelectCondition(List<CardSource> cardSources)
                    {
                        if (CardEffectCommons.HasNoElement(cardSources))
                        {
                            return false;
                        }

                        return true;
                    }

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (selectedCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));
                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                    }
                }
            }
            #endregion

            #region When Attacking - Add Sources OPT
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 3 cards from trash as bottom sources", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("WDWA_EX10_036");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] By placing 3 [Mineral] or [Rock] trait cards from your trash as this Digimon's bottom digivolution cards, it unsuspends.";
                }

                bool HasProperTrait(CardSource source)
                {
                    return source.EqualsTraits("Mineral") || source.EqualsTraits("Rock");

                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, HasProperTrait) >= 3;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: HasProperTrait,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        canNoSelect: () => false,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select [Mineral] or [Rock] to place on bottom of digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                        maxCount: 3,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");
                    selectCardEffect.SetUpCustomMessage("Select [Mineral] or [Rock] to place on bottom of digivolution cards.", "The opponent is selecting [Mineral] or [Rock] to place on bottom of digivolution cards.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    bool CanEndSelectCondition(List<CardSource> cardSources)
                    {
                        if (CardEffectCommons.HasNoElement(cardSources))
                        {
                            return false;
                        }

                        return true;
                    }

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (selectedCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));
                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                    }
                }
            }
            #endregion

            #region When Digivolving - Delete/Trash
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 3 sources, Delete 1 digimon and trash top security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By trashing 3 [Mineral] or [Rock] trait cards from any of your Digimon's digivolution cards, delete 1 of your opponent's Digimon and trash their top security card.";
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

                    return totalSourceCount >= 3;
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

                bool CanSelectOpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int trashedCount = 0;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: CanSelectPermanentCondition,
                        cardCondition: HasProperTrait,
                        maxCount: 3,
                        canNoTrash: false,
                        isFromOnly1Permanent: false,
                        activateClass: activateClass,
                        afterSelectionCoroutine: AfterTrashedCards
                    ));

                    IEnumerator AfterTrashedCards(Permanent permanent, List<CardSource> cards)
                    {
                        trashedCount = cards.Count;

                        yield return null;
                    }

                    if (trashedCount == 3)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentsDigimon))
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

                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner.Enemy,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());
                    }
                }
            }
            #endregion

            #region When Attacking - Delete/Trash
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing 3 sources, Delete 1 digimon and trash top security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] By trashing 3 [Mineral] or [Rock] trait cards from any of your Digimon's digivolution cards, delete 1 of your opponent's Digimon and trash their top security card.";
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

                    return totalSourceCount >= 3;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        return permanent.DigivolutionCards.Count(HasProperTrait) >= 1;

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                        return HasProperAmountOfSources();

                    return false;
                }

                bool CanSelectOpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int trashedCount = 0;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: CanSelectPermanentCondition,
                        cardCondition: HasProperTrait,
                        maxCount: 3,
                        canNoTrash: false,
                        isFromOnly1Permanent: false,
                        activateClass: activateClass,
                        afterSelectionCoroutine: AfterTrashedCards
                    ));

                    IEnumerator AfterTrashedCards(Permanent permanent, List<CardSource> cards)
                    {
                        trashedCount = cards.Count;

                        yield return null;
                    }

                    if (trashedCount == 3)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentsDigimon))
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

                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner.Enemy,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());
                    }
                }
            }
            #endregion


            return cardEffects;
        }
    }
}