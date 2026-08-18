using System;
using System.Collections;
using System.Collections.Generic;

// Bearcatmon // Penetrate Blow
namespace DCGO.CardEffects.BT26
{
    public class BT26_057 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsTraits("Glowing Dawn");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing a Tamer's bottom face-down card, not affected by opponent's effects and +3000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Digivolving] By trashing the bottom face-down card from under any of your Tamers, until your opponent's turn ends, their Digimon effects don't affect this Digimon, and it gets +3000 DP.";

                bool IsTamerWithFaceDownCard(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.HasFaceDownDigivolutionCards
                        && !permanent.ImmuneFromStackTrashing(activateClass);

                bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(IsTamerWithFaceDownCard);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedTamer = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsTamerWithFaceDownCard,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedTamer = permanent;
                        yield return null;
                    }

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to trash its bottom face-down card.", "The opponent is selecting 1 Tamer to trash its bottom face-down card.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    if (selectedTamer != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedTamer, trashCount: 1, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));

                        Permanent thisPermanent = card.PermanentOfThisCard();

                        if (thisPermanent != null)
                        {
                            CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                            canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon effects", CanUseCondition1, card);
                            canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition1, SkillCondition: SkillCondition1);
                            thisPermanent.UntilOpponentTurnEndEffects.Add((_timing) => canNotAffectedClass);

                            bool CanUseCondition1(Hashtable _hashtable) => CardEffectCommons.IsPermanentExistsOnBattleArea(thisPermanent);

                            bool CardCondition1(CardSource cardSource) => CardEffectCommons.IsPermanentExistsOnBattleArea(thisPermanent) && cardSource == thisPermanent.TopCard;

                            bool SkillCondition1(ICardEffect cardEffect)
                                => cardEffect != null
                                    && cardEffect.EffectSourceCard != null
                                    && cardEffect.EffectSourceCard.Owner == card.Owner.Enemy
                                    && cardEffect.EffectSourceCard.IsDigimon;

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: thisPermanent, changeValue: 3000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        }
                    }
                }
            }
            #endregion

            #region All Turns - Attack Target Changed
            if (timing == EffectTiming.OnAttackTargetChanged)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT26_057_AllTurns");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When attack targets change or effects trash cards from under your Tamers, this Digimon may unsuspend.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentAttackTargetSwitch(hashtable, permanent => true);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard());

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                }
            }
            #endregion

            #region All Turns - Trash Under Tamer
            if (timing == EffectTiming.OnDigivolutionCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT26_057_AllTurns");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When attack targets change or effects trash cards from under your Tamers, this Digimon may unsuspend.";

                bool OwnersTamerCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnTrashDigivolutionCard(hashtable, OwnersTamerCondition, cardEffect => true, cardSource => true);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard());

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                }
            }
            #endregion

            #endregion

            #region Option Effects

            #region Use Req.
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                    => cardSource.ContainsTraits("Glowing Dawn");
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("De-Digivolve 1 1 opponent's Digimon, then it must attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] <De-Digivolve 1> 1 of your opponent's Digimon. Then, give 1 of their Digimon \"[Start of Your Main Phase] This Digimon attacks.\" until their turn ends.";

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Degenerate,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetDegenerationCount(1);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.", "The opponent is selecting 1 Digimon to De-Digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermanent != null && CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                        {
                            ActivateClass activateClass1 = new ActivateClass();
                            activateClass1.SetUpICardEffect("[Start of Your Main Phase] This Digimon attacks.", CanUseCondition1, selectedPermanent.TopCard);
                            activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDescription1());
                            activateClass1.SetEffectSourcePermanent(selectedPermanent);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                            }

                            string EffectDescription1() => "[Start of Your Main Phase] This Digimon attacks.";

                            bool CanUseCondition1(Hashtable hashtable1)
                                => CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent)
                                    && GManager.instance.turnStateMachine.gameContext.TurnPlayer == selectedPermanent.TopCard.Owner;

                            bool CanActivateCondition1(Hashtable hashtable1)
                                => CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent)
                                    && !selectedPermanent.TopCard.CanNotBeAffected(activateClass)
                                    && selectedPermanent.CanAttack(activateClass1);

                            ICardEffect GetCardEffect(EffectTiming _timing)
                                => _timing == EffectTiming.OnStartMainPhase ? activateClass1 : null;

                            IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent) && selectedPermanent.CanAttack(activateClass1))
                                {
                                    SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                    selectAttackEffect.SetUp(
                                        attacker: selectedPermanent,
                                        canAttackPlayerCondition: () => true,
                                        defenderCondition: (permanent) => true,
                                        cardEffect: activateClass1);

                                    selectAttackEffect.SetCanNotSelectNotAttack();

                                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion

            #endregion

            return cardEffects;
        }
    }
}
