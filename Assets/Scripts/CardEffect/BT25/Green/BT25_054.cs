using System.Collections;
using System.Collections.Generic;

// GreatGrizzlymon
namespace DCGO.CardEffects.BT25
{
    public class BT25_054 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolve Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null, level: 4));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP / WD
            string SharedEffectName = "Taunt 1 enemy digimon";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] Give 1 of your opponent's Digimon \"[Start of Your Main Phase] This Digimon attacks.\" until their turn ends.";

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to taunt.",
                        "The opponent is selecting 1 Digimon to taunt.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        ActivateClass activateClassDebuff = new ActivateClass();
                        activateClassDebuff.SetUpICardEffect("This Digimon attacks", CanUseConditionDebuff,
                            selectedPermanent.TopCard);
                        activateClassDebuff.SetUpActivateClass(CanActivateConditionDebuff, ActivateCoroutineDebuff, -1, false,
                            EffectDescriptionDebuff());
                        activateClassDebuff.SetEffectSourcePermanent(selectedPermanent);
                        selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);
                        selectedPermanent.UntilOwnerTurnEndEffects.Add(GetDetailEffect);

                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                .CreateDebuffEffect(selectedPermanent));
                        }

                        string EffectDescriptionDebuff()
                        {
                            return "[Start of Your Main Phase] This Digimon attacks.";
                        }

                        bool CanUseConditionDebuff(Hashtable hashtable1)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(selectedPermanent, card) &&
                                   CardEffectCommons.IsOpponentTurn(card) &&
                                   !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                        }

                        bool CanActivateConditionDebuff(Hashtable hashtable1)
                        {
                            return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(selectedPermanent) &&
                                   !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                        }

                        IEnumerator ActivateCoroutineDebuff(Hashtable hashtableDebuff)
                        {
                            if (selectedPermanent.CanAttack(activateClassDebuff))
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

                        bool CanShowDebuffCondition(Hashtable hashtable1)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(selectedPermanent, card) &&
                                   !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                        }
                        
                        ICardEffect GetDetailEffect(EffectTiming timing)
                        {
                            if (timing == EffectTiming.None)
                            {
                                return CardEffectFactory.AddDetailClass(CanShowDebuffCondition, permanent => permanent == selectedPermanent, "[Start of Your Main Phase] This Digimon attacks.", true, card);
                            }
                            return null;
                        }
                    }
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnEndBattle)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [Callismon]/[Marsmon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When this Digimon wins a battle, it may digivolve into [Callismon] or [Marsmon] in the hand without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    bool WinnerCondition(Permanent permanent) => permanent.cardSources.Contains(card);

                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenWinBattle(hashtable: hashtable, winnerCondition: WinnerCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return (cardSource.EqualsCardName("Callismon")
                            || cardSource.EqualsCardName("Marsmon"))
                        && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        card.PermanentOfThisCard(),
                        digivolvingCard => CanSelectCardCondition(digivolvingCard),
                        payCost: false,
                        reduceCostTuple: null,
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true,
                        activateClass: activateClass,
                        successProcess: null
                    ));
                }
            }

            #endregion

            #region All Turns Inherited
            if (timing == EffectTiming.OnEndBattle)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash top card of enemy security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT25_054_Inherited");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns][Once Per Turn] When this Digimon deletes your opponent's Digimon in battle, trash their top card security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    bool WinnerCondition(Permanent permanent) => permanent.cardSources.Contains(card);
                    bool LoserCondition(Permanent permanent) => CardEffectCommons.IsOpponentPermanent(permanent, card);

                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenDeleteOpponentDigimonByBattle(hashtable: hashtable, winnerCondition: WinnerCondition, loserCondition: LoserCondition, isOnlyWinnerSurvive: false);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
