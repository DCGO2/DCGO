using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Bearcatmon
namespace DCGO.CardEffects.BT27
{
    public class BT27_064 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared helpers: "trash the bottom face-down card from under any of your Tamers"
            bool CanSelectTamerWithFaceDown(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Any(c => c.IsFaceDown);
            }

            // Pays the cost. onResult(true) when a face-down card was trashed.
            IEnumerator PayFaceDownCost(ActivateClass activateClass, Action<bool> onResult)
            {
                bool trashed = false;

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerWithFaceDown))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectTamerWithFaceDown,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: true,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Tamer to trash its bottom face-down card.",
                        "The opponent is selecting 1 Tamer to trash its bottom face-down card.");

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                                    targetPermanent: permanents[0],
                                    trashCount: 1,
                                    isFromTop: false,
                                    activateClass: activateClass,
                                    cardCondition: c => c.IsFlipped));

                            trashed = true;
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                onResult(trashed);
            }
            #endregion

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasGlowingDawnTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null, level: 4));
            }
            #endregion

            #region Hand Main - Digivolve from Pristimon
            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Wolvermon] from trash under [Pristimon] to digivolve into this card", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Hand] [Main] If you have a Tamer with [Reina Sakuya] in its name, by placing 1 [Wolvermon] from your trash as any of your [Pristimon]'s bottom digivolution card, that Digimon digivolves into this card, ignoring level.";
                }

                bool IsReinaTamer(Permanent permanent)
                {
                    return permanent.IsTamer && permanent.TopCard.ContainsCardName("Reina Sakuya");
                }

                bool IsPristimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && !permanent.IsToken
                        && permanent.TopCard.EqualsCardName("Pristimon");
                }

                bool IsWolvermon(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Wolvermon");
                }

                bool IsThisCard(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnHand(cardSource) && cardSource == card;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnHand(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsReinaTamer)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsPristimon)
                        && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsWolvermon);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource wolvermon = null;
                    Permanent pristimon = null;

                    #region Select Wolvermon from trash
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: IsWolvermon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => false,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 [Wolvermon] to place as a digivolution card.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 [Wolvermon] to place as a digivolution card.", "The opponent is selecting 1 [Wolvermon] to place as a digivolution card.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        wolvermon = cardSource;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    #endregion

                    #region Select Pristimon
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsPristimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 [Pristimon] to digivolve.", "The opponent is selecting 1 [Pristimon] to digivolve.");

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        pristimon = permanent;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    #endregion

                    yield return ContinuousController.instance.StartCoroutine(
                        pristimon.AddDigivolutionCardsBottom(new List<CardSource>() { wolvermon }, activateClass));

                    if (pristimon.DigivolutionCards.Contains(wolvermon))
                    {
                        // Normal digivolution cost, ignoring level only.
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: pristimon,
                            cardCondition: IsThisCard,
                            payCost: true,
                            reduceCostTuple: null,
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null,
                            ignoreSelection: true,
                            ignoreRequirements: CardEffectCommons.IgnoreRequirement.Level,
                            failedProcess: null,
                            isOptional: false));
                    }
                }
            }
            #endregion

            #region On Play / When Digivolving
            string SharedEffectName = "De-Digivolve 1, give opponent's Digimon forced attack";

            string SharedEffectDescription(string tag) =>
                $"[{tag}] <De-Digivolve 1> 1 of your opponent's Digimon. Then, by trashing the bottom face-down card from under any of your Tamers, give 1 of their Digimon \"[Start of Your Main Phase] This Digimon attacks.\" until their turn ends.";

            bool CanSelectOpponentDigimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region De-Digivolve 1
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentDigimon))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Degenerate,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
                #endregion

                #region Then, pay cost and give forced attack

                bool paid = false;
                yield return ContinuousController.instance.StartCoroutine(PayFaceDownCost(activateClass, result => paid = result));

                if (!paid || !CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentDigimon))
                {
                    yield break;
                }

                Permanent selectedPermanent = null;

                SelectPermanentEffect selectTarget = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectTarget.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectOpponentDigimon,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectTargetCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectTarget.SetUpCustomMessage("Select 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");

                IEnumerator SelectTargetCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;
                    yield return null;
                }

                yield return ContinuousController.instance.StartCoroutine(selectTarget.Activate());

                if (selectedPermanent != null)
                {
                    ActivateClass grant = new ActivateClass();
                    grant.SetUpICardEffect("[Start of Your Main Phase] This Digimon attacks.", CanUseGrant, selectedPermanent.TopCard);
                    grant.SetUpActivateClass(CanActivateGrant, ActivateGrant, -1, false, "[Start of Your Main Phase] This Digimon attacks.");
                    grant.SetEffectSourcePermanent(selectedPermanent);
                    selectedPermanent.UntilOwnerTurnEndEffects.Add(GetGrantEffect);

                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                    }

                    bool CanUseGrant(Hashtable h)
                    {
                        return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(selectedPermanent.TopCard, activateClass)
                            && selectedPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(selectedPermanent)
                            && GManager.instance.turnStateMachine.gameContext.TurnPlayer == selectedPermanent.TopCard.Owner;
                    }

                    bool CanActivateGrant(Hashtable h)
                    {
                        return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(selectedPermanent.TopCard, activateClass)
                            && !selectedPermanent.TopCard.CanNotBeAffected(activateClass)
                            && selectedPermanent.CanAttack(grant);
                    }

                    ICardEffect GetGrantEffect(EffectTiming _timing)
                    {
                        return _timing == EffectTiming.OnStartMainPhase ? grant : null;
                    }

                    IEnumerator ActivateGrant(Hashtable h)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent)
                            && selectedPermanent.CanAttack(grant))
                        {
                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: selectedPermanent,
                                canAttackPlayerCondition: () => true,
                                defenderCondition: (permanent) => true,
                                cardEffect: grant);

                            selectAttackEffect.SetCanNotSelectNotAttack();

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }
                    }
                }
                #endregion
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

            #region Inherited - All Turns Once Per Turn, attack targets change
            if (timing == EffectTiming.OnAttackTargetChanged)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash opponent's top security card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT27_064_AT");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When attack targets change, if this Digimon has the [Glowing Dawn] trait, by trashing the bottom face-down card from under any of your Tamers, trash your opponent's top security card.";
                }

                bool HasGlowingDawn()
                {
                    Permanent permanent = card.PermanentOfThisCard();
                    return permanent != null && permanent.TopCard != null && permanent.TopCard.HasGlowingDawnTraits;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPermanentAttackTargetSwitch(hashtable, permanent => true);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && HasGlowingDawn()
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerWithFaceDown);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool paid = false;
                    yield return ContinuousController.instance.StartCoroutine(PayFaceDownCost(activateClass, result => paid = result));

                    if (paid && card.Owner.Enemy.SecurityCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            new IDestroySecurity(
                                player: card.Owner.Enemy,
                                destroySecurityCount: 1,
                                cardEffect: activateClass,
                                fromTop: true).DestroySecurity());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
