using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT5_085 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.BeforePayCost)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Delete 1 [Diaboromon] to get Play Cost -12", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetHashString("PlayCost-12_BT5_085");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "When playing this card from your hand, you may delete 1 of your [Diaboromon] to reduce this card's play cost by 12.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                {
                    if (permanent.TopCard.CardNames.Contains("Diaboromon"))
                    {
                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            if (permanent.CanBeDestroyedBySkill(activateClass))
                            {
                                return true;
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
                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentCondition))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentCondition))
                {
                    bool CanNoSelect = true;

                    CardSource Card = CardEffectCommons.GetCardFromHashtable(_hashtable);

                    if (Card != null)
                    {
                        if (Card.PayingCost(SelectCardEffect.Root.Hand, null, checkAvailability: false) > Card.Owner.MaxMemoryCost)
                        {
                            CanNoSelect = false;
                        }
                    }

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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { permanent }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                        IEnumerator SuccessProcess()
                        {
                            if (card.Owner.CanReduceCost(null, card))
                            {
                                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                            }

                            ChangeCostClass changeCostClass = new ChangeCostClass();
                            changeCostClass.SetUpICardEffect("Play Cost -12", CanUseCondition1, card);
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
                                            Cost -= 12;
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
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            ChangeCostClass changeCostClass = new ChangeCostClass();
            changeCostClass.SetUpICardEffect("Play Cost -12", CanUseCondition, card);
            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
            changeCostClass.SetNotShowUI(true);
            cardEffects.Add(changeCostClass);

            bool CanSelectPermanentCondition(Permanent permanent, ICardEffect activateClass)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                {
                    if (permanent.TopCard.CardNames.Contains("Diaboromon"))
                    {
                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            if (permanent.CanBeDestroyedBySkill(activateClass))
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
                if (card.Owner.HandCards.Contains(card))
                {
                    ICardEffect activateClass = card.EffectList(EffectTiming.BeforePayCost).Find(cardEffect => cardEffect.EffectName == "Delete 1 [Diaboromon] to get Play Cost -12");

                    if (activateClass != null)
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && CanSelectPermanentCondition(permanent, activateClass)))
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
                            Cost -= 12;
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

        if (timing == EffectTiming.None)
        {
            cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
        }

        if (timing == EffectTiming.None)
        {
            DisableEffectClass invalidationClass = new DisableEffectClass();
            invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", CanUseCondition, card);
            invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);

            cardEffects.Add(invalidationClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return true;
            }

            bool InvalidateCondition(ICardEffect cardEffect)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (cardEffect != null)
                    {
                        if (cardEffect is ActivateICardEffect)
                        {
                            if (cardEffect.EffectSourceCard != null)
                            {
                                if (CardEffectCommons.IsExistOnBattleArea(cardEffect.EffectSourceCard))
                                {
                                    if (cardEffect.EffectSourceCard.PermanentOfThisCard().Level == 7 && cardEffect.EffectSourceCard.HasLevel)
                                    {
                                        if (!cardEffect.EffectSourceCard.PermanentOfThisCard().TopCard.CanNotBeAffected(invalidationClass))
                                        {
                                            if (cardEffect.IsWhenDigivolving)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }
        }

        return cardEffects;
    }
}
