using System.Collections;
using System.Collections.Generic;

//Craniamon
namespace DCGO.CardEffects.BT23
{
    public class BT23_058 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolve Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel5 &&
                           targetPermanent.TopCard.HasCSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Prevent Digimon from leaving play", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When any of your Digimon or Tamers would leave the battle area by your opponent's effects, by suspending this Digimon, 1 of those Digimon or Tamers doesn't leave.";
                }

                bool RemovedPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card) && (permanent.IsDigimon || permanent.IsTamer);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, RemovedPermanent)
                        && CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOpponentEffect(cardEffect, card));

                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { selectedPermanent }, _hashtable).Tap());

                    if (selectedPermanent.IsSuspended)
                    {
                        List<Permanent> protectedPermanents = CardEffectCommons.GetPermanentsFromHashtable(_hashtable)
                                    .Filter(RemovedPermanent);

                        bool CanSelectRemovedPermanent(Permanent permanent)
                        {
                            return protectedPermanents.Contains(permanent);
                        }

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectRemovedPermanent,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            permanent.willBeRemoveField = false;
                            permanent.HideDeleteEffect();
                            permanent.HideHandBounceEffect();
                            permanent.HideDeckBounceEffect();

                            yield return null;
                        }
                    }
                }
            }
            #endregion

            #region All Turns - OPT
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription1());
                activateClass.SetHashString("AT_BT23-058");
                cardEffects.Add(activateClass);

                string EffectDiscription1()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon suspends, delete all of your opponent's Digimon with the lowest play cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenSelfPermanentSuspends(hashtable, card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool PermanentCondition(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                            && permanent.TopCard.HasPlayCost
                            && CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, true);
                    }

                    List<Permanent> destroyTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(PermanentCondition);
                    yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}