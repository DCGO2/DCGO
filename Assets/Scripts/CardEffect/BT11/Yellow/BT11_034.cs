using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT11
{
    public class BT11_034 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return (targetPermanent.TopCard.CardTraits.Contains("Xros Heart") || targetPermanent.TopCard.CardTraits.Contains("XrosHeart")) && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 2;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place cards to digivolution cards from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Place 1 Digimon card with [Xros Heart] in its traits from your trash under 1 of your Tamers. If you have a Digimon with [Dorulumon] in its name or with [Dorulumon] in its digivolution cards, place up to 2 Digimon cards with [Xros Heart] in their traits from your trash under 1 of your Tamers instead.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (permanent != null)
                    {
                        if (permanent.TopCard != null)
                        {
                            if (permanent.IsTamer)
                            {
                                if (permanent.TopCard.Owner == card.Owner)
                                {
                                    if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                    {
                                        if (!permanent.IsToken)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.Owner == card.Owner)
                            {
                                if (cardSource.CardTraits.Contains("Xros Heart"))
                                {
                                    return true;
                                }

                                if (cardSource.CardTraits.Contains("XrosHeart"))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                                {
                                    List<CardSource> selectedCards = new List<CardSource>();
                                    List<CardSource> digivolutionCards = new List<CardSource>();

                                    int maxCount = 1;

                                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && (permanent.TopCard.ContainsCardName("Dorulumon") || permanent.DigivolutionCards.Count((cardSource) => cardSource.CardNames.Contains("Dorulumon")) >= 1)))
                                    {
                                        maxCount = 2;
                                    }

                                    if (maxCount == 1)
                                    {
                                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                        selectCardEffect.SetUp(
                                                    canTargetCondition: CanSelectCardCondition,
                                                    canTargetCondition_ByPreSelecetedList: null,
                                                    canEndSelectCondition: null,
                                                    canNoSelect: () => false,
                                                    selectCardCoroutine: SelectCardCoroutine,
                                                    afterSelectCardCoroutine: null,
                                                    message: "Select 1 Digimon card with [Xros Heart] in its traits from trash.",
                                                    maxCount: maxCount,
                                                    canEndNotMax: false,
                                                    isShowOpponent: true,
                                                    mode: SelectCardEffect.Mode.Custom,
                                                    root: SelectCardEffect.Root.Trash,
                                                    customRootCardList: null,
                                                    canLookReverseCard: true,
                                                    selectPlayer: card.Owner,
                                                    cardEffect: activateClass);

                                        selectCardEffect.SetUpCustomMessage("Select 1 Digimon card with [Xros Heart] in its traits from trash.", "The opponent is selecting 1 Digimon card with [Xros Heart] in its traits from trash.");

                                        yield return StartCoroutine(selectCardEffect.Activate());

                                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                                        {
                                            selectedCards.Add(cardSource);

                                            yield return null;
                                        }
                                    }
                                    else if (maxCount == 2)
                                    {
                                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                        selectCardEffect.SetUp(
                                                    canTargetCondition: CanSelectCardCondition,
                                                    canTargetCondition_ByPreSelecetedList: null,
                                                    canEndSelectCondition: CanEndSelectCondition,
                                                    canNoSelect: () => false,
                                                    selectCardCoroutine: SelectCardCoroutine,
                                                    afterSelectCardCoroutine: null,
                                                    message: "Select up to 2 Digimon cards with [Xros Heart] in its traits from trash\n(cards will be placed so that cards with lower numbers are on top).",
                                                    maxCount: maxCount,
                                                    canEndNotMax: true,
                                                    isShowOpponent: true,
                                                    mode: SelectCardEffect.Mode.Custom,
                                                    root: SelectCardEffect.Root.Trash,
                                                    customRootCardList: null,
                                                    canLookReverseCard: true,
                                                    selectPlayer: card.Owner,
                                                    cardEffect: activateClass);

                                        selectCardEffect.SetUpCustomMessage("Select up to 2 Digimon cards with [Xros Heart] in its traits from trash.", "The opponent is selecting up to 2 Digimon cards with [Xros Heart] in its traits from trash.");

                                        yield return StartCoroutine(selectCardEffect.Activate());

                                        bool CanEndSelectCondition(List<CardSource> cardSources)
                                        {
                                            if (maxCount >= 1)
                                            {
                                                if (cardSources.Count <= 0)
                                                {
                                                    return false;
                                                }
                                            }

                                            return true;
                                        }

                                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                                        {
                                            selectedCards.Add(cardSource);

                                            yield return null;
                                        }
                                    }

                                    if (selectedCards.Count >= 1)
                                    {
                                        foreach (CardSource cardSource in selectedCards)
                                        {
                                            digivolutionCards.Add(cardSource);
                                        }

                                        if (digivolutionCards.Count >= 1)
                                        {
                                            maxCount = 1;

                                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                            selectPermanentEffect.SetUp(
                                                selectPlayer: card.Owner,
                                                canTargetCondition: CanSelectPermanentCondition,
                                                canTargetCondition_ByPreSelecetedList: null,
                                                canEndSelectCondition: null,
                                                maxCount: maxCount,
                                                canNoSelect: false,
                                                canEndNotMax: false,
                                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                                afterSelectPermanentCoroutine: null,
                                                mode: SelectPermanentEffect.Mode.Custom,
                                                cardEffect: activateClass);

                                            selectPermanentEffect.SetUpCustomMessage($"Select 1 Tamer that will get digivolution cards from trash.", $"The opponent is selecting 1 Tamer that will get a digivolution cards from trash.");

                                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                            {
                                                Permanent selectedPermanent = permanent;

                                                if (selectedPermanent != null)
                                                {
                                                    yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(digivolutionCards, activateClass));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}