using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Gizmon: XT — BT27-079
namespace DCGO.CardEffects.BT27
{
    public class BT27_079 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            #region Play Cost Reduction Shown
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete your 1 level 4 Digimon to get Play Cost -6", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetHashString("PlayCost-6_BT27_079");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "When you would play this card, by deleting 1 of your level 4 Digimon, reduce the play cost by 6.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.IsLevel4
                        && !permanent.TopCard.CanNotBeAffected(activateClass)
                        && permanent.CanBeDestroyedBySkill(activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, cardSource => cardSource == card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool CanNoSelect =
                    CardEffectCommons.GetPlayCardClassFromHashtable(_hashtable) != null &&
                        card.PayingCost(
                            CardEffectCommons.GetPlayCardClassFromHashtable(_hashtable).Root,
                            null,
                            checkAvailability: false)
                        <= card.Owner.MaxMemoryCost;

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
                            changeCostClass.SetUpICardEffect("Play Cost -6", CanUseCondition1, card);
                            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                            card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return true;
                            }

                            int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                            {
                                if (CardSourceCondition(cardSource)
                                && RootCondition(root)
                                && PermanentsCondition(targetPermanents))
                                {
                                    Cost -= 6;
                                }

                                return Cost;
                            }

                            bool PermanentsCondition(List<Permanent> targetPermanents)
                            {
                                return targetPermanents == null
                                        || targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0;
                            }

                            bool CardSourceCondition(CardSource cardSource)
                            {
                                return cardSource != null
                                    && cardSource == card;
                            }

                            bool RootCondition(SelectCardEffect.Root root)
                            {
                                return true;
                            }

                            bool isUpDown()
                            {
                                return true;
                            }

                            yield return null;
                        }
                    }
                }
            }
            #endregion

            #region Play Cost Reduction Not Shown
            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -6", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanSelectPermanentCondition(Permanent permanent, ICardEffect activateClass)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.IsLevel4
                        && !permanent.TopCard.CanNotBeAffected(activateClass)
                        && permanent.CanBeDestroyedBySkill(activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (card.PermanentOfThisCard() == null)
                    {
                        ICardEffect activateClass = card.EffectList(EffectTiming.BeforePayCost).Find(cardEffect => cardEffect.EffectName == "Delete your 1 level 4 Digimon to get Play Cost -6");

                        if (activateClass != null)
                        {
                            return activateClass != null
                            && CardEffectCommons.HasMatchConditionPermanent(permanent => CanSelectPermanentCondition(permanent, activateClass));
                        }
                    }

                    return false;
                }

                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource)
                    && RootCondition(root)
                    && PermanentsCondition(targetPermanents))
                    {
                        Cost -= 6;
                    }

                    return Cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    return targetPermanents == null
                            || targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource != null
                        && cardSource == card;
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
            #endregion

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
                cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(isInheritedEffect: false, card: card, condition: null));
                cardEffects.Add(CardEffectFactory.CanNotDigivolveStaticSelfEffect(
                    cardCondition: null, isInheritedEffect: false, card: card,
                    condition: () => CardEffectCommons.IsExistOnBattleArea(card),
                    effectName: "Can't digivolve"));
            }
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass effect = new ActivateClass();
                effect.SetUpICardEffect("Play a cost 5 or lower Kurata's Army card", CanUse, card);
                effect.SetUpActivateClass(CanActivate, Resolve, -1, false,
                    "[On Deletion] You may play 1 play cost 5 or lower [Kurata's Army] trait card from your hand or trash without paying the cost.");
                effect.SetIsSkippable(true);
                cardEffects.Add(effect);

                bool Eligible(CardSource candidate)
                {
                    return candidate.EqualsTraits("Kurata's Army")
                        && candidate.GetCostItself <= 5
                        && CardEffectCommons.CanPlayAsNewPermanent(candidate, false, effect);
                }
                bool CanUse(Hashtable data) => CardEffectCommons.CanTriggerOnDeletion(data, card, effect);
                bool CanActivate(Hashtable data) => CardEffectCommons.CanActivateOnDeletion(card, effect)
                    && (card.Owner.TrashCards.Exists(Eligible) || card.Owner.HandCards.Exists(Eligible));
                IEnumerator Resolve(Hashtable data)
                {
                    // Integration note: this prioritizes trash if both zones contain eligible cards.
                    // A combined hand/trash choice requires the project's zone-choice UI integration.
                    SelectCardEffect.Root root = card.Owner.TrashCards.Exists(Eligible)
                        ? SelectCardEffect.Root.Trash : SelectCardEffect.Root.Hand;
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: Eligible, root: root, cardEffect: effect, payCost: false));
                }
            }
            return cardEffects;
        }
    }
}
