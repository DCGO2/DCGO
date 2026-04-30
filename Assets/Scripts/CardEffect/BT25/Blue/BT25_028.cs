using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

// Dianamon
namespace DCGO.CardEffects.BT25
{
    public class BT25_028 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, true, card, null, level: 5));
            }
            #endregion

            #region Reduce Play Cost
            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -5", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionPermanent(Level6EnemyDigimon);
                }

                bool Level6EnemyDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) 
                        && permanent.TopCard.HasLevel
                        && permanent.Level >= 6;
                }

                int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root,
                        List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource) &&
                        RootCondition(root) &&
                        PermanentsCondition(targetPermanents))
                    {
                        cost -= 5;
                    }

                    return cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    return targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0;
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
            #endregion

            #region Shared OP / WD

            string SharedEffectName = "None of your opponent's Digimon with 1 or less Digivolution cards can suspend untiol end of their turn. Delete 1 of their unsuspended Digimon";

            string SharedEffectDescription(string tag)
                => $"[{tag}] None of your opponent's Digimon with 1 or fewer digivolution cards can suspend until their turn ends. Then, delete 1 of your opponent's unsuspended Digimon.";

            bool CanDeleteCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && !permanent.IsSuspended;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCondition1, card);
                canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                card.Owner.UntilOpponentTurnEndEffects.Add((_timing) => canNotSuspendClass);

                bool CanUseCondition1(Hashtable hashtable)
                {
                    return true;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.DigivolutionCards.Count <= 1
                        && !permanent.TopCard.CanNotBeAffected(activateClass);
                }

                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanDeleteCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanDeleteCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May trash 4 digivolution cards and then DNA into GraceNovamon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("BT25_028_AT");
                cardEffects.Add(activateClass);

                string EffectDescription() 
                    => "[All Turns] [Once Per Turn] When any Digimon are played or digivolve, you may trash any 4 digivolution cards from your opponent's Digimon, Then, 2 of your Digimon may DNA digivolve into [GraceNovamon] in the hand.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, IsDigimonCondition)
                            || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, IsDigimonCondition));
                }

                bool IsDigimonCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);

                bool IsOpponentsDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanSelectDNACardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.CanPlayJogress(true)
                        && cardSource.EqualsCardName("GraceNovamon");
                }

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleArea(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool executed = false;
                    
                    if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                            permanentCondition: IsOpponentsDigimon,
                            cardCondition: _ => true,
                            maxCount: 4,
                            canNoTrash: true,
                            isFromOnly1Permanent: false,
                            activateClass: activateClass,
                            afterSelectionCoroutine: AfterSelectCoroutine
                        ));

                        IEnumerator AfterSelectCoroutine(Permanent permanent, List<CardSource> cardSources)
                        {
                            if (cardSources.Count > 0)
                                executed = true;
                            yield return null;
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                            CanSelectDNACardCondition,
                            payCost: true,
                            isHand: true,
                            activateClass,
                            successProcess: SuccessProcess
                        ));

                    IEnumerator SuccessProcess(CardSource cardSource)
                    {
                        executed = true;
                        yield return null;
                    }

                    if (!executed) activateClass.RemoveUse();
                }
            }
            #endregion

            #region When Attacking ESS
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 of your opponent's Digimon or Tamers can't suspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT25_028_WA_ESS");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] [Once Per Turn] 1 of your opponent's Digimon or Tamers can't suspend until their turn ends.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanSelectOpponentPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectOpponentPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition_ByPreSelecetedList: null,
                            canTargetCondition: CanSelectOpponentPermanentCondition,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon/Tamer that will be unable to suspend.",
                            "The opponent is selecting 1 Digimon/Tamer that will be unable to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        Permanent selectedPermanent = permanent;
                        CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                        canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCanNotSuspendCondition, card);
                        canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCanNotSuspendCondition);
                        selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => canNotSuspendClass);

                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance
                                .GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                        }

                        bool CanUseCanNotSuspendCondition(Hashtable hashtableCanNotSuspend)
                        {
                            return selectedPermanent.TopCard != null
                                && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                        }

                        bool PermanentCanNotSuspendCondition(Permanent permanentCanNotSuspend)
                        {
                            return permanentCanNotSuspend == selectedPermanent;
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
