using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.P
{
    public class P_161 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Color Requirements
            
            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);

                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return !CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.TopCard.EqualsCardName("Bishop Device"));
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }
            }
            
            #endregion

            #region Shared When Trashed/Main

            bool CanSelectPermanentToNoSuspendCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                {
                    if (permanent.IsDigimon || permanent.IsTamer)
                    {
                        return true;
                    }
                }

                return false;
            }
            
            #endregion
            
            #region When Trashed from battle area
            
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 of your opponent's digimon or tamers can't suspend.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "When this card is trashed from the battle area, until the end of your opponent's turn, 1 of their Digimon or Tamers can't suspend.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentToNoSuspendCondition))
                    {
                        int maxCount = Math.Min(1, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentToNoSuspendCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentToNoSuspendCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer that will get unable to suspend.",
                            "The opponent is selecting 1 Digimon or Tamer that will get unable to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                                canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCondition, card);
                                canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                                selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => canNotSuspendClass);

                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                        .CreateDebuffEffect(selectedPermanent));
                                }

                                bool CanUseCondition(Hashtable hashtable)
                                {
                                    if (selectedPermanent.TopCard != null)
                                    {
                                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                bool PermanentCondition(Permanent permanent)
                                {
                                    return permanent == selectedPermanent;
                                }
                            }
                        }
                    }
                }
            }
            
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 of your opponent's digimon or tamers can't suspend, then place in battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] Until the end of your opponent's turn, 1 of their Digimon or Tamers can't suspend. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentToNoSuspendCondition))
                    {
                        int maxCount = Math.Min(1, card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentToNoSuspendCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentToNoSuspendCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer that will get unable to suspend.",
                            "The opponent is selecting 1 Digimon or Tamer that will get unable to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                                canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCondition, card);
                                canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                                selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => canNotSuspendClass);

                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                        .CreateDebuffEffect(selectedPermanent));
                                }

                                bool CanUseCondition(Hashtable hashtable)
                                {
                                    if (selectedPermanent.TopCard != null)
                                    {
                                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                bool PermanentCondition(Permanent permanent)
                                {
                                    return permanent == selectedPermanent;
                                }
                            }
                        }
                    }
                    
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            #endregion
            
            #region Security Effect
            
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 opponent's level 5 or lower Digimon to the bottom of the deck, then add this to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Security] Return 1 of your opponent's level 5 or lower Digimon to the bottom of the deck. Then, add this card to the hand.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.Level <= 5)
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
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }
            
            #endregion

            return cardEffects;
        }
    }
}