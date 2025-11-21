using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Eater Bit
namespace DCGO.CardEffects.BT23
{
    public class BT23_073 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 level 3 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Delete 1 of your opponent's level 3 Digimon.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.Level == 3)
                        {
                            if (permanent.TopCard.HasLevel)
                            {
                                return true;
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
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
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
            }
            #endregion

            #region All Turns

            if (timing == EffectTiming.WhenRemoveField)
            {
                List<Permanent> wouldBeRemovedPermaments = new List<Permanent>();

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Prevent a Digimon from leaving battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Substitute_BT23_073");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When any of your other Digimon with the [Eater] or [Hudie] trait would leave the battle area, by deleting this Digimon or placing it as the bottom digivolution card of your [Mother Eater] in the breeding area, 1 of those Digimon doesn't leave.";
                }

                bool RemovedFromPlayPermanent(Permanent permanent)
                {
                    wouldBeRemovedPermaments.Add(permanent);
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.HasEaterTraits || permanent.TopCard.HasHudieTraits)
                        && permanent != card.PermanentOfThisCard())
                    {
                        wouldBeRemovedPermaments.Add(permanent);
                        return true;
                    }
                    return false;

                }

                bool HasMother(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card) &&
                           permanent.TopCard.EqualsCardName("Mother Eater");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, RemovedFromPlayPermanent);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canAddSource = CardEffectCommons.HasMatchConditionOwnersBreedingPermanent(card, HasMother);
                    bool canDestroy = card.PermanentOfThisCard().CanBeDestroyed();

                    if (canAddSource || canDestroy)
                    {
                        bool willDestroy = false;

                        if (canAddSource && canDestroy)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: $"Delete", value : true, spriteIndex: 0),
                                new SelectionElement<bool>(message: $"Add to Sources", value : false, spriteIndex: 1),
                            };

                            string selectPlayerMessage = "Will you delete or add to sources?";
                            string notSelectPlayerMessage = "The opponent is choosing whether to delete or add to sources.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                            willDestroy = GManager.instance.userSelectionManager.SelectedBoolValue;
                        }
                        else
                        {
                            willDestroy = canDestroy;
                        }

                        if (willDestroy)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                                activateClass: activateClass,
                                successProcess: permanents => SuccessProcess(),
                                failureProcess: null));
                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { card.PermanentOfThisCard(), card.Owner.GetBreedingAreaPermanents()[0] } }, false, activateClass).PlacePermanentToDigivolutionCards());
                            yield return ContinuousController.instance.StartCoroutine(SuccessProcess());
                        }

                        IEnumerator SuccessProcess()
                        {
                            Permanent selectedPermanent = wouldBeRemovedPermaments[0];
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(perm => wouldBeRemovedPermaments.Contains(perm)));


                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: perm => wouldBeRemovedPermaments.Contains(perm),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;
                                yield return null;
                            }

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will not be removed from battle area.", "The opponent is selecting 1 Digimon that will not be removed from battle area.");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            if (selectedPermanent != null)
                            {
                                selectedPermanent.willBeRemoveField = false;

                                selectedPermanent.HideDeleteEffect();
                                selectedPermanent.HideHandBounceEffect();
                                selectedPermanent.HideDeckBounceEffect();
                                selectedPermanent.HideWillRemoveFieldEffect();
                                yield return null;
                            }
                        }
                    }
                }
            }

            #endregion

            #region ESS

            #region Reduce Cost Effect

            ActivateClass activateClass2 = new ActivateClass();
            activateClass2.SetUpICardEffect("Reduce play cost", CanUseCondition2, card);
            activateClass2.SetUpActivateClass(CanActivateCondition2, ActivateCoroutine2, 1, true, EffectDiscription2());
            activateClass2.SetHashString("Reduce_BT22_079");
            activateClass2.SetIsInheritedEffect(true);

            string EffectDiscription2()
            {
                return "[Breeding] [Your Turn] [Once Per Turn] When any of your Digimon cards with the [Eater] trait would be played, you may reduce the play costs by 1.";
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource.Owner == card.Owner)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasEaterTraits)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition2(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBreedingArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition2(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBreedingArea(card))
                {
                    PlayCardClass playCardClass = CardEffectCommons.GetPlayCardClassFromHashtable(hashtable);

                    if (playCardClass != null)
                    {
                        if (playCardClass.PayCost)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine2(Hashtable _hashtable)
            {
                if (CardEffectCommons.IsExistOnBreedingArea(card))
                {
                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect($"Play Cost -1", CanUseCondition1, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                    yield return new WaitForSeconds(0.4f);
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
                        CardSource Card = CardEffectCommons.GetCardFromHashtable(_hashtable);

                        if (Card != null)
                        {
                            if (cardSource == Card)
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

            #endregion

            #region Before Pay Cost

            if (timing == EffectTiming.BeforePayCost)
            {
                cardEffects.Add(activateClass2);
            }

            #endregion

            #region Before Pay Cost (Not Shown)

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -1", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (!card.Owner.isYou && GManager.instance.IsAI)
                    {
                        return false;
                    }

                    if (CardEffectCommons.IsExistOnBreedingArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (activateClass2 != null)
                            {
                                if (!card.cEntity_EffectController.isOverMaxCountPerTurn(activateClass2, activateClass2.MaxCountPerTurn))
                                {
                                    return true;
                                }
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
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.HasEaterTraits)
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

            #endregion

            #endregion

            return cardEffects;
        }
    }
}