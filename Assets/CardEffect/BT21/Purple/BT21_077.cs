using System.Collections;
using System.Collections.Generic;
using System.Linq;

//BT21_077 Regulusmon
namespace DCGO.CardEffects.BT21
{
    public class BT21_077 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution condition
            if(timing == EffectTiming.None)
            {
                bool Condition(Permanent permanent)
                {
                    return permanent.Level == 4 && permanent.TopCard.HasText("Gammamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(Condition, 3, false, card, null));
            }
            #endregion

            #region On Play/When Digivolving shared
            bool CanTargetPermanent(Permanent permanent)
            {
                return permanent.IsDigimon && CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return isExistOnField(card);
            }

            bool CanTrashCard(CardSource card)
            {
                return card.HasText("Gammamon");
            }
            #endregion

            #region On Play
            if(timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Force attack and give collision", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] By trashing 1 card with [Gammamon] in its text from your hand, give 1 of your opponent's Digimon <Collision> and \"[Start of Your Main Phase] This Digimon attacks.\" until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Count(CanTrashCard) >= 1)
                    {
                        bool discarded = false;

                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanTrashCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }

                        if (discarded)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanTargetPermanent))
                            {
                                Permanent selectedPermanent = null;

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanTargetPermanent,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get effects.",
                                    "The opponent is selecting 1 Digimon that will get effects.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    selectedPermanent = permanent;

                                    yield return null;
                                }

                                if (selectedPermanent != null)
                                {

                                    CardEffectCommons.GainCollision(selectedPermanent, EffectDuration.UntilOpponentTurnEnd, activateClass);

                                    ActivateClass activateClassDebuff = new ActivateClass();
                                    activateClassDebuff.SetUpICardEffect("Attack with this Digimon", CanUseConditionDebuff,
                                        selectedPermanent.TopCard);
                                    activateClassDebuff.SetUpActivateClass(CanActivateConditionDebuff, ActivateCoroutineDebuff, -1, false,
                                        EffectDescriptionDebuff());
                                    activateClassDebuff.SetEffectSourcePermanent(selectedPermanent);
                                    selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                            .CreateDebuffEffect(selectedPermanent));
                                    }

                                    string EffectDescriptionDebuff()
                                    {
                                        return "[Start of Your Main Phase] Attack with this Digimon.";
                                    }

                                    bool CanUseConditionDebuff(Hashtable hashtable1)
                                    {
                                        return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(selectedPermanent) &&
                                               selectedPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(selectedPermanent) &&
                                               GManager.instance.turnStateMachine.gameContext.TurnPlayer == selectedPermanent.TopCard.Owner;
                                    }

                                    bool CanActivateConditionDebuff(Hashtable hashtable1)
                                    {
                                        return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(selectedPermanent) &&
                                               !selectedPermanent.TopCard.CanNotBeAffected(activateClass) &&
                                               selectedPermanent.CanAttack(activateClassDebuff);
                                    }

                                    IEnumerator ActivateCoroutineDebuff(Hashtable hashtableDebuff)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent) &&
                                            selectedPermanent.CanAttack(activateClassDebuff))
                                        {
                                            SelectAttackEffect selectAttackEffect =
                                                GManager.instance.GetComponent<SelectAttackEffect>();

                                            selectAttackEffect.SetUp(
                                                attacker: selectedPermanent,
                                                canAttackPlayerCondition: () => true,
                                                defenderCondition: _ => true,
                                                cardEffect: activateClassDebuff);

                                            selectAttackEffect.SetCanNotSelectNotAttack();

                                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                        }
                                    }

                                    ICardEffect GetCardEffect(EffectTiming timingDebuff)
                                    {
                                        return timingDebuff == EffectTiming.OnStartMainPhase ? activateClassDebuff : null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Force attack and give collision", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] By trashing 1 card with [Gammamon] in its text from your hand, give 1 of your opponent's Digimon <Collision> and \"[Start of Your Main Phase] This Digimon attacks.\" until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Count(CanTrashCard) >= 1)
                    {
                        bool discarded = false;

                        int discardCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanTrashCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: discardCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }

                        if (discarded)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanTargetPermanent))
                            {
                                Permanent selectedPermanent = null;

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanTargetPermanent,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get effects.",
                                    "The opponent is selecting 1 Digimon that will get effects.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    selectedPermanent = permanent;

                                    yield return null;
                                }

                                if (selectedPermanent != null)
                                {

                                    CardEffectCommons.GainCollision(selectedPermanent, EffectDuration.UntilOpponentTurnEnd, activateClass);

                                    ActivateClass activateClassDebuff = new ActivateClass();
                                    activateClassDebuff.SetUpICardEffect("Attack with this Digimon", CanUseConditionDebuff,
                                        selectedPermanent.TopCard);
                                    activateClassDebuff.SetUpActivateClass(CanActivateConditionDebuff, ActivateCoroutineDebuff, -1, false,
                                        EffectDescriptionDebuff());
                                    activateClassDebuff.SetEffectSourcePermanent(selectedPermanent);
                                    selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                            .CreateDebuffEffect(selectedPermanent));
                                    }

                                    string EffectDescriptionDebuff()
                                    {
                                        return "[Start of Your Main Phase] Attack with this Digimon.";
                                    }

                                    bool CanUseConditionDebuff(Hashtable hashtable1)
                                    {
                                        return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(selectedPermanent) &&
                                               selectedPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(selectedPermanent) &&
                                               GManager.instance.turnStateMachine.gameContext.TurnPlayer == selectedPermanent.TopCard.Owner;
                                    }

                                    bool CanActivateConditionDebuff(Hashtable hashtable1)
                                    {
                                        return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(selectedPermanent) &&
                                               !selectedPermanent.TopCard.CanNotBeAffected(activateClass) &&
                                               selectedPermanent.CanAttack(activateClassDebuff);
                                    }

                                    IEnumerator ActivateCoroutineDebuff(Hashtable hashtableDebuff)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent) &&
                                            selectedPermanent.CanAttack(activateClassDebuff))
                                        {
                                            SelectAttackEffect selectAttackEffect =
                                                GManager.instance.GetComponent<SelectAttackEffect>();

                                            selectAttackEffect.SetUp(
                                                attacker: selectedPermanent,
                                                canAttackPlayerCondition: () => true,
                                                defenderCondition: _ => true,
                                                cardEffect: activateClassDebuff);

                                            selectAttackEffect.SetCanNotSelectNotAttack();

                                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                        }
                                    }

                                    ICardEffect GetCardEffect(EffectTiming timingDebuff)
                                    {
                                        return timingDebuff == EffectTiming.OnStartMainPhase ? activateClassDebuff : null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region On Deletions Shared
            bool CanUseConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
            }
            #endregion

            #region On deletion regular
            if(timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play Cannonweissmon or level 4 or lower gammamon in text digimon", CanUseConditionShared, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Deletion] You may play 1 [Canoweissmon] or 1 level 4 or lower Digimon card with [Gammamon] in its text from your trash without paying the cost.";
                }

                bool CanPlay(CardSource cardSource)
                {
                    return ((cardSource.HasText("Gammamon") && cardSource.HasLevel && cardSource.Level <= 4) || cardSource.EqualsCardName("Cannonweissmon")) &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card) && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlay);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanPlay,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 [Cannonweissmon] or level 4 or lower [Gammamon] in text Digimon to play.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 [Cannonweissmon] or level 4 or lower [Gammamon] in text Digimon to play.",
                        "The opponent is selecting 1 [Cannonweissmon] or level 4 or lower [Gammamon] in text Digimon to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: selectedCards,
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Trash,
                        activateETB: true));
                }
            }
            #endregion

            #region On deletion inherit
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play Cannonweissmon or level 4 or lower gammamon in text digimon", CanUseConditionShared, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Deletion] You may play 1 level 4 or lower Digimon card with [Gammamon] in its text from your trash without paying the cost.";
                }

                bool CanPlay(CardSource cardSource)
                {
                    return cardSource.HasText("Gammamon") && cardSource.HasLevel && cardSource.Level <= 4 &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card) && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanPlay);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanPlay,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 level 4 or lower [Gammamon] in text Digimon to play.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 level 4 or lower [Gammamon] in text Digimon to play.",
                        "The opponent is selecting 1 level 4 or lower [Gammamon] in text Digimon to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: selectedCards,
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Trash,
                        activateETB: true));
                }
            }
            #endregion
            return cardEffects;
        }
    }
}