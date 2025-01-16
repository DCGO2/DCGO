using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX4
{
    public class EX4_073 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Omnimon") && targetPermanent.TopCard.HasLevel && targetPermanent.Level == 7;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("De-Digivolve 3 on 1 Digimon and delete Digimons so that their play cost total is up to 6", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] <De-Digivolve 3> 1 of your opponent's Digimon. Then, choose any number of your opponent's Digimon so that their play cost total is up to 6 and delete them.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.GetCostItself <= 6)
                        {
                            if (permanent.TopCard.HasPlayCost)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.", "The opponent is selecting 1 Digimon to De-Digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        bool CanEndSelectCondition(List<Permanent> permanents)
                        {
                            if (permanents.Count <= 0)
                            {
                                return false;
                            }

                            return true;

                        }

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IDegeneration(selectedPermanent, 3, activateClass).Degeneration());
                            }

                            yield return null;
                        }
                    }

                    if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition1) >= 1)
                    {
                        List<Permanent> selectedPermanents = new List<Permanent>();

                        int maxCount = card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition1);

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition1,
                            canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: true,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        bool CanEndSelectCondition(List<Permanent> permanents)
                        {
                            if (permanents.Count <= 0)
                            {
                                return false;
                            }

                            int sumCost = 0;

                            foreach (Permanent permanent1 in permanents)
                            {
                                sumCost += permanent1.TopCard.GetCostItself;
                            }

                            if (sumCost > 6)
                            {
                                return false;
                            }

                            return true;
                        }

                        bool CanTargetCondition_ByPreSelecetedList(List<Permanent> permanents, Permanent permanent)
                        {
                            int sumCost = 0;

                            foreach (Permanent permanent1 in permanents)
                            {
                                sumCost += permanent1.TopCard.GetCostItself;
                            }

                            sumCost += permanent.TopCard.GetCostItself;

                            if (sumCost > 6)
                            {
                                return false;
                            }

                            return true;
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash digivolution cards to delete Digimon or Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] By trashing up to 3 level 6 or higher cards in this Digimon's digivolution cards, among your opponent's Digimon and Tamers, delete 1 with the lowest play cost for each card trashed. If you trashed 3 cards, trash the top 2 cards of your opponent's security stack.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (!cardSource.CanNotTrashFromDigivolutionCards(activateClass))
                    {
                        if (cardSource.Level >= 6)
                        {
                            if (cardSource.HasLevel)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, false);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        int trashCount = 0;

                        if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = Math.Min(3, selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition));

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: CanEndSelectCondition,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select digivolution cards to discard.",
                                        maxCount: maxCount,
                                        canEndNotMax: true,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Custom,
                                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: null);

                            selectCardEffect.SetUpCustomMessage("Select digivolution cards to discard.", "The opponent is selecting digivolution cards to discard.");

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
                                yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(card.PermanentOfThisCard(), selectedCards, activateClass).TrashDigivolutionCards());

                                trashCount = selectedCards.Count;
                            }
                        }

                        if (trashCount >= 1)
                        {
                            if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                            {
                                for (int i = 0; i < trashCount; i++)
                                {
                                    int maxCount = Math.Min(1, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectPermanentCondition,
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

                            if (trashCount == 3)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                    player: card.Owner.Enemy,
                                    destroySecurityCount: 2,
                                    cardEffect: activateClass,
                                    fromTop: true).DestroySecurity());
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}