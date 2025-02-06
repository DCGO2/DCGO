using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT11
{
    public class BT11_031 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("MetalGreymon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon and gain Memory +2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Unsuspend this Digimon. Then, if a card with [Blue Flare] or [Xros Heart] in its traits is in this Digimon's digivolution cards and your opponent has 2 or more Digimon in play, gain 2 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            if (CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard()))
                            {
                                return true;
                            }

                            if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardTraits.Contains("Blue Flare") || cardSource.CardTraits.Contains("BlueFlare") || cardSource.CardTraits.Contains("Xros Heart") || cardSource.CardTraits.Contains("XrosHeart")) >= 1)
                            {
                                if (card.Owner.Enemy.GetBattleAreaDigimons().Count >= 2)
                                {
                                    if (card.Owner.CanAddMemory(activateClass))
                                    {
                                        return true;
                                    }
                                }
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
                            Permanent selectedPermanent = card.PermanentOfThisCard();

                            yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());

                            if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardTraits.Contains("Blue Flare") || cardSource.CardTraits.Contains("BlueFlare") || cardSource.CardTraits.Contains("Xros Heart") || cardSource.CardTraits.Contains("XrosHeart")) >= 1)
                            {
                                if (card.Owner.Enemy.GetBattleAreaDigimons().Count >= 2)
                                {
                                    if (card.Owner.CanAddMemory(activateClass))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(2, activateClass));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Save and place cards to digivolution cards from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] <Save>. (You may place this card under one of your Tamers.) Then, place 1 blue [Greymon] and 1 blue [MailBirdramon] from your trash under 1 of your Tamers with [General] in its traits.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.IsTamer)
                        {
                            if (!permanent.IsToken)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.CardColors.Contains(CardColor.Blue))
                        {
                            if (cardSource.Owner == card.Owner)
                            {
                                if (cardSource.CardNames.Contains("Greymon"))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.CardColors.Contains(CardColor.Blue))
                        {
                            if (cardSource.Owner == card.Owner)
                            {
                                if (cardSource.CardNames.Contains("MailBirdramon"))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.IsTamer)
                        {
                            if (!permanent.IsToken)
                            {
                                if (permanent.TopCard.CardTraits.Contains("General"))
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
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateSave(hashtable, CanSelectPermanentCondition))
                    {
                        return true;
                    }

                    if (CardEffectCommons.CanActivateOnDeletion(card))
                    {
                        if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition1) >= 1)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                            {
                                return true;
                            }

                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition1))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SaveProcess(_hashtable, activateClass, card, CanSelectPermanentCondition));

                    if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition1) >= 1)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();
                        List<CardSource> digivolutionCards = new List<CardSource>();

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            int maxCount = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => false,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 blue [Greymon] from trash.",
                                        maxCount: maxCount,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 blue [Greymon] from trash.", "The opponent is selecting 1 blue [Greymon] from trash.");

                            yield return StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }
                        }

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition1))
                        {
                            int maxCount = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition1,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => false,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 blue [MailBirdramon] from trash.",
                                        maxCount: maxCount,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 blue [MailBirdramon] from trash.", "The opponent is selecting 1 blue [MailBirdramon] from trash.");

                            yield return StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }
                        }

                        if (selectedCards.Count >= 1)
                        {
                            if (selectedCards.Count == 1)
                            {
                                foreach (CardSource cardSource in selectedCards)
                                {
                                    digivolutionCards.Add(cardSource);
                                }
                            }
                            else
                            {
                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: (cardSource) => true,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                                    message: "Specify the order to place the cards in the digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                                    maxCount: selectedCards.Count,
                                    canEndNotMax: false,
                                    isShowOpponent: false,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Cards");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                                {
                                    foreach (CardSource cardSource in cardSources)
                                    {
                                        digivolutionCards.Add(cardSource);
                                    }

                                    yield return null;
                                }
                            }

                            if (digivolutionCards.Count >= 1)
                            {
                                int maxCount = 1;

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentCondition1,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage($"Select 1 Tamer with [General] traits that will get digivolution cards from trash.", $"The opponent is selecting 1 Tamer with [General] traits that will get a digivolution cards from trash.");

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

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Blue Flare"))
                            {
                                return true;
                            }

                            if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("BlueFlare"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: true, card: card, condition: Condition));
            }

            return cardEffects;
        }
    }
}