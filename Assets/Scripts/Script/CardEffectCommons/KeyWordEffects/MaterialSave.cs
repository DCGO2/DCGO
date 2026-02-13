using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can activate [Material Save]
    public static bool CanActivateMaterialSave(CardSource card, Func<CardSource, bool> CanSelectCardCondition, Func<Permanent, bool> CanSelectPermanentCondition)
    {
        if (IsExistOnBattleArea(card))
        {
            if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1)
            {
                if (HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Material Save]
    public static IEnumerator MaterialSaveProcess(Hashtable hashtable, ICardEffect activateClass, CardSource card, Func<CardSource, bool> CanSelectCardCondition, Func<Permanent, bool> CanSelectPermanentCondition, int materialSaveCount)
    {
        if (IsExistOnBattleArea(card))
        {
            if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1)
            {
                Permanent selectedPermanent = null;

                int maxCount = Math.Min(1, MatchConditionPermanentCount(CanSelectPermanentCondition));

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage(customMessageArray: customPermanentMessageArrayTemplate(customText: "that will get a digivolution card", maxCount: 1, CanSelectDigimon: false, CanSelectTamer: true));

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;

                    yield return null;
                }

                if (selectedPermanent != null)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    maxCount = Math.Min(materialSaveCount, card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: CanEndSelectCondition,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select cards to place on bottom of digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                    selectCardEffect.SetNotShowCard();
                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    bool CanEndSelectCondition(List<CardSource> cardSources)
                    {
                        if (HasNoElement(cardSources))
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
                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(selectedCards, "Digivolution Cards", true, true));
                            yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(selectedCards, activateClass));
                        }
                    }
                }
            }
        }
    }
    #endregion
}