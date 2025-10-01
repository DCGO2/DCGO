using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Pyramidimon
namespace DCGO.CardEffects.EX10
{
    public class EX10_033 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

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

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place up to 3 cards from trash as bottom sources", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("WDWA_EX10_033");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] [Once Per Turn] You may place up to 3 [Mineral] or [Rock] cards from your trash as this Digimon's bottom digivolution cards.";
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
                           CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, HasProperTrait);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: HasProperTrait,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select [Mineral] or [Rock] to place on bottom of digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                        maxCount: 3,
                        canEndNotMax: true,
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
                    }
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place up to 3 cards from trash as bottom sources", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("WDWA_EX10_033");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] You may place up to 3 [Mineral] or [Rock] cards from your trash as this Digimon's bottom digivolution cards.";
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
                           CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, HasProperTrait);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: HasProperTrait,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select [Mineral] or [Rock] to place on bottom of digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                        maxCount: 3,
                        canEndNotMax: true,
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
                    }
                }
            }
            #endregion

            #region When Digivolving - Reduce Cost
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing up to 3 sources, reduce 1 digimons play cost by 2 for each card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By trashing up to 3 [Mineral] or [Rock] trait cards from any of your Digimon's digivolution cards, to 1 of your opponent's Digimon, reduce the play cost by 2 until their turn ends for each card trashed.";
                }

                bool HasProperTrait(CardSource source)
                {
                    return !source.CanNotTrashFromDigivolutionCards(activateClass) &&
                           (source.EqualsTraits("Mineral") || source.EqualsTraits("Rock"));
                }

                int HasProperAmountOfSources()
                {
                    int totalSourceCount = 0;

                    foreach (Permanent permanent in card.Owner.GetBattleAreaDigimons())
                    {
                        totalSourceCount += permanent.DigivolutionCards.Count(HasProperTrait);
                    }

                    return totalSourceCount;
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
                        return HasProperAmountOfSources() > 0;

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int trashedCount = 0;
                    int maxCount = Mathf.Min(1, HasProperAmountOfSources());

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: CanSelectPermanentCondition,
                        cardCondition: HasProperTrait,
                        maxCount: 3,
                        canNoTrash: true,
                        isFromOnly1Permanent: false,
                        activateClass: activateClass,
                        afterSelectionCoroutine: AfterTrashedCards,
                        canEndNotMax: true
                    ));

                    IEnumerator AfterTrashedCards(Permanent permanent, List<CardSource> cards)
                    {
                        trashedCount = cards.Count;

                        yield return null;
                    }

                    if (trashedCount > 0)
                    {
                        bool PermanentCondition(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                        }

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: PermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanent,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 card to reduce play cost.", "The opponent is selecting 1 card to reduce play cost.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        
                        IEnumerator AfterSelectPermanent(List<Permanent> permanents)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangePlayCostPlayerEffect(
                            permanentCondition: perm => permanents.Contains(perm),
                            changeValue: -trashedCount * 2,
                            setFixedCost: false,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));
                        }
                        
                    }
                }
            }
            #endregion

            #region When Attacking - Reduce Cost
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing up to 3 sources, reduce 1 digimons play cost by 2 for each card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] By trashing up to 3 [Mineral] or [Rock] trait cards from any of your Digimon's digivolution cards, to 1 of your opponent's Digimon, reduce the play cost by 2 until their turn ends for each card trashed.";
                }

                bool HasProperTrait(CardSource source)
                {
                    return !source.CanNotTrashFromDigivolutionCards(activateClass) &&
                           (source.EqualsTraits("Mineral") || source.EqualsTraits("Rock"));
                }

                int HasProperAmountOfSources()
                {
                    int totalSourceCount = 0;

                    foreach (Permanent permanent in card.Owner.GetBattleAreaDigimons())
                    {
                        totalSourceCount += permanent.DigivolutionCards.Count(HasProperTrait);
                    }

                    return totalSourceCount;
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
                        return HasProperAmountOfSources() > 0;

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int trashedCount = 0;
                    int maxCount = Mathf.Min(1, HasProperAmountOfSources());

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: CanSelectPermanentCondition,
                        cardCondition: HasProperTrait,
                        maxCount: 3,
                        canNoTrash: true,
                        isFromOnly1Permanent: false,
                        activateClass: activateClass,
                        afterSelectionCoroutine: AfterTrashedCards,
                        canEndNotMax: true
                    ));

                    IEnumerator AfterTrashedCards(Permanent permanent, List<CardSource> cards)
                    {
                        trashedCount = cards.Count;

                        yield return null;
                    }

                    if (trashedCount > 0)
                    {
                        bool PermanentCondition(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                        }

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: PermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanent,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 card to reduce play cost.", "The opponent is selecting 1 card to reduce play cost.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanent(List<Permanent> permanents)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangePlayCostPlayerEffect(
                            permanentCondition: perm => permanents.Contains(perm),
                            changeValue: -trashedCount * 2,
                            setFixedCost: false,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));
                        }

                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}