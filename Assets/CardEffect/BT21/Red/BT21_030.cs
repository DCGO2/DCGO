using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Shoutmon X7: Superior Mode
namespace DCGO.CardEffects.BT21
{
    public class BT21_030 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.Level == 6 && targetPermanent.TopCard.EqualsTraits("Hero"))
                    {
                        return true;
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost:5,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region DigiXros

            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros", CanUseCondition, card);
                addDigiXrosConditionClass.SetUpAddDigiXrosConditionClass(getDigiXrosCondition: GetDigiXros);
                addDigiXrosConditionClass.SetNotShowUI(true);
                cardEffects.Add(addDigiXrosConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                DigiXrosCondition GetDigiXros(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        DigiXrosConditionElement element = new DigiXrosConditionElement(CanSelectCardCondition, "1 Digimon card with [Xros Heart] or [Blue Flare] trait");

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    if (cardSource.IsDigimon)
                                    {
                                        if (cardSource.EqualsTraits("Xros Heart"))
                                        {
                                            return true;
                                        }

                                        if (cardSource.EqualsTraits("Blue Flare"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>();

                        for (int i = 0; i < 50; i++)
                        {
                            elements.Add(element);
                        }

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            List<string> cardIDs = new List<string>();

                            foreach (CardSource cardSource1 in cardSources)
                            {
                                if (!cardIDs.Contains(cardSource1.CardID))
                                {
                                    cardIDs.Add(cardSource1.CardID);
                                }
                            }

                            if (cardIDs.Contains(cardSource.CardID))
                            {
                                return false;
                            }

                            return true;
                        }

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, CanTargetCondition_ByPreSelecetedList, 1);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region DigiXros Special

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play Cost -1 and select trash cards for a DigiXros", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("PlayCost-1_BT12_112");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When you would play this card from your hand, by placing 1 of your [Shoutmon] as a digivolution card under this Digimon, reduce its play cost by 1 and place the cards in your trash as digivolution cards for a DigiXros.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.CardNames.Contains("Shoutmon"))
                        {
                            if (permanent.CanSelectBySkill(activateClass))
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    if (!permanent.IsToken)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = 1;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 [Shoutmon].", "The opponent is selecting 1 [Shoutmon].");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            yield return null;

                            if (permanents.Count >= 1)
                            {
                                Permanent selectedPermanent = permanents[0];

                                if (selectedPermanent != null)
                                {
                                    if (selectedPermanent.TopCard != null)
                                    {
                                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            GManager.instance.GetComponent<SelectDigiXrosClass>().AddDigivolutionCardInfos(new AddDigivolutionCardsInfo(activateClass, new List<CardSource>() { selectedPermanent.TopCard }));

                                            if (card.Owner.CanReduceCost(null, card))
                                            {
                                                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                                            }

                                            ChangeCostClass changeCostClass = new ChangeCostClass();
                                            changeCostClass.SetUpICardEffect("Play Cost -1", CanUseCondition1, card);
                                            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                                            card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                                            bool CanUseCondition1(Hashtable hashtable)
                                            {
                                                return true;
                                            }

                                            int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                            {
                                                if (CardSourceCondition(cardSource))
                                                {
                                                    if (RootCondition(root))
                                                    {
                                                        if (PermanentsCondition(targetPermanents))
                                                        {
                                                            Cost -= 1;
                                                        }
                                                    }
                                                }

                                                return Cost;
                                            }

                                            bool PermanentsCondition(List<Permanent> targetPermanents)
                                            {
                                                if (targetPermanents == null)
                                                {
                                                    return true;
                                                }
                                                else
                                                {
                                                    if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                                                    {
                                                        return true;
                                                    }
                                                }

                                                return false;
                                            }

                                            bool CardSourceCondition(CardSource cardSource)
                                            {
                                                return cardSource == card;
                                            }

                                            bool RootCondition(SelectCardEffect.Root root)
                                            {
                                                return true;
                                            }

                                            bool isUpDown()
                                            {
                                                return true;
                                            }

                                            #region can select digixros cards from trash

                                            AddMaxTrashCountDigiXrosClass addMaxTrashCountDigiXrosClass = new AddMaxTrashCountDigiXrosClass();
                                            addMaxTrashCountDigiXrosClass.SetUpICardEffect("Can select DigiXros cards from trash", CanUseCondition1, card);
                                            addMaxTrashCountDigiXrosClass.SetUpAddMaxTrashCountDigiXrosClass(getMaxTrashCount: GetCount);
                                            Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                                            card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                                            ICardEffect GetCardEffect(EffectTiming _timing)
                                            {
                                                if (_timing == EffectTiming.None)
                                                {
                                                    return addMaxTrashCountDigiXrosClass;
                                                }

                                                return null;
                                            }

                                            int GetCount(CardSource cardSource)
                                            {
                                                return 100;
                                            }

                                            #endregion
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region On Play / When Digivolving Shared

            bool SharedCanSelectPermanent(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    return true;
                }
                return false;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(SharedCanSelectPermanent))
                {
                    Permanent selectedPermanent = null;
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(SharedCanSelectPermanent));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SharedCanSelectPermanent,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon.", "The opponent is selecting 1 Digimon.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new ITrashStack(selectedPermanent, 10, activateClass).TrashStack());
                    }
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash top 10 stack cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Trash the top 10 stacked cards of 1 of your opponent's Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPlay(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(SharedCanSelectPermanent))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash top 10 stack cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Trash the top 10 stacked cards of 1 of your opponent's Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(SharedCanSelectPermanent))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Bottom deck digimon with no digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BottomDeck_BT21-030");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] You may return 1 of your opponent's Digimon with no digivolution cards to the bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnAttack(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.HasNoDigivolutionCards)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon.", "The opponent is selecting 1 Digimon.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(new List<Permanent>() { selectedPermanent }, CardEffectCommons.CardEffectHashtable(activateClass)).DeckBounce());
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}