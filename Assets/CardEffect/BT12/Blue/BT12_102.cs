using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT12
{
    public class BT12_102 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play Cost -3", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("PlayCost-3_BT12_102");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "If you have this card in your hand, you may place 1 of your blue Digimon under another of your blue Digimon as its bottom digivolution card and reduce the cost by 3.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardColors.Contains(CardColor.Blue))
                        {
                            if (!permanent.IsToken)
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    bool CanSelectPermanentCondition1(Permanent permanent1)
                                    {
                                        if (permanent1 != permanent)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent1, card))
                                            {
                                                if (permanent1.TopCard.CardColors.Contains(CardColor.Blue))
                                                {
                                                    if (!permanent1.IsToken)
                                                    {
                                                        if (!permanent1.TopCard.CanNotBeAffected(activateClass))
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    if (permanent.TopCard.Owner.GetBattleAreaDigimons().Some(CanSelectPermanentCondition1))
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
                    if (cardSource == card)
                    {
                        if (CardEffectCommons.IsExistOnHand(cardSource))
                        {
                            return true;
                        }
                    }

                    return false;
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
                    if (card.Owner.GetBattleAreaDigimons().Count((permanent) => permanent.TopCard.CardColors.Contains(CardColor.Blue) && !permanent.IsToken) >= 2
                    && card.Owner.GetBattleAreaDigimons().Count(CanSelectPermanentCondition) >= 1)
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.GetBattleAreaDigimons().Count((permanent) => permanent.TopCard.CardColors.Contains(CardColor.Blue) && !permanent.IsToken) >= 2
                    && card.Owner.GetBattleAreaDigimons().Count(CanSelectPermanentCondition) >= 1)
                    {
                        bool CanNoSelect =
                        CardEffectCommons.GetPlayCardClassFromHashtable(_hashtable) != null &&
                            card.PayingCost(
                                CardEffectCommons.GetPlayCardClassFromHashtable(_hashtable).Root,
                                null,
                                checkAvailability: false)
                            <= card.Owner.MaxMemoryCost;

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            Permanent digivolutionPermanent = null;

                            int maxCount = 1;

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: CanNoSelect,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that place under other Digimon's digivolution cards.", "The opponent is selecting 1 Digimon that place under other Digimon's digivolution cards.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                digivolutionPermanent = permanent;

                                yield return null;
                            }

                            if (digivolutionPermanent != null)
                            {
                                bool CanSelectPermanentCondition1(Permanent permanent1)
                                {
                                    if (permanent1 != digivolutionPermanent)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent1, card))
                                        {
                                            if (permanent1.TopCard.CardColors.Contains(CardColor.Blue))
                                            {
                                                if (!permanent1.IsToken)
                                                {
                                                    if (!permanent1.TopCard.CanNotBeAffected(activateClass))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    return false;
                                }

                                maxCount = 1;

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentCondition1,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: CanNoSelect,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine1,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get digivolution cards.", "The opponent is selecting 1 Digimon that will get digivolution cards.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { digivolutionPermanent, permanent } }, false, activateClass).PlacePermanentToDigivolutionCards());
                                }

                                if (digivolutionPermanent.TopCard == null && digivolutionPermanent.PlaceOtherPermanentEffect == activateClass)
                                {
                                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                                    ChangeCostClass changeCostClass = new ChangeCostClass();
                                    changeCostClass.SetUpICardEffect("Play Cost -3", CanUseCondition1, card);
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
                                                    Cost -= 3;
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
                                        if (cardSource != null)
                                        {
                                            if (cardSource == card)
                                            {
                                                return true;
                                            }
                                        }

                                        return false;
                                    }

                                    bool RootCondition(SelectCardEffect.Root root)
                                    {
                                        return true;
                                    }

                                    bool isUpDown()
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -3", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanSelectPermanentCondition(Permanent permanent, ICardEffect activateClass)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardColors.Contains(CardColor.Blue))
                        {
                            if (!permanent.IsToken)
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    bool CanSelectPermanentCondition1(Permanent permanent1)
                                    {
                                        if (permanent1 != permanent)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent1, card))
                                            {
                                                if (permanent1.TopCard.CardColors.Contains(CardColor.Blue))
                                                {
                                                    if (!permanent1.IsToken)
                                                    {
                                                        if (!permanent1.TopCard.CanNotBeAffected(activateClass))
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    if (permanent.TopCard.Owner.GetBattleAreaDigimons().Some(CanSelectPermanentCondition1))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Contains(card))
                    {
                        ICardEffect activateClass = card.EffectList(EffectTiming.BeforePayCost).Find(cardEffect => cardEffect.EffectName == "Play Cost -3");

                        if (activateClass != null)
                        {
                            if (card.Owner.GetBattleAreaDigimons().Count((permanent) =>
                                permanent.TopCard.CardColors.Contains(CardColor.Blue) && !permanent.IsToken) >= 2
                            && card.Owner.GetBattleAreaDigimons().Count(permanent => CanSelectPermanentCondition(permanent, activateClass)) >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource))
                    {
                        if (RootCondition(root))
                        {
                            if (PermanentsCondition(targetPermanents))
                            {
                                Cost -= 3;
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
                    if (cardSource != null)
                    {
                        if (cardSource == card)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                bool isUpDown()
                {
                    return true;
                }
            }

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Return 1 of your opponent's Digimon to their owner's deck.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
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
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Return 1 Digimon to the bottom of deck");
            }

            return cardEffects;
        }
    }
}