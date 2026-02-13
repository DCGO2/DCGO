using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT20
{
    public class BT20_053 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return (targetPermanent.TopCard.EqualsCardName("Raptordramon")) ||
                           (targetPermanent.TopCard.IsLevel4 && targetPermanent.TopCard.EqualsTraits("Chronicle"));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Digimon from your hand to your empty breeding area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] You may play 1 [Dorumon]/[Ryudamon] from your hand to your empty breeding area without paying the cost. Then if during an attack until the end of your opponent's turn, 1 of your Digimon isn't affected by your opponent's Digimon's effects and gets +5000 DP.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool IsCorrectRookieCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           (cardSource.EqualsCardName("Dorumon") || cardSource.EqualsCardName("Ryudamon")) &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass,
                               isBreedingArea: true);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool PlayBreedingAreaCondition()
                {
                    return CardEffectCommons.HasMatchConditionOwnersHand(card, IsCorrectRookieCardCondition) &&
                           card.Owner.GetBreedingAreaPermanents().Count == 0;
                }

                bool IsAttackingCondition()
                {
                    return GManager.instance.attackProcess.IsAttacking;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           (PlayBreedingAreaCondition() || IsAttackingCondition());
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (PlayBreedingAreaCondition())
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsCorrectRookieCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

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
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true,
                            isBreedingArea: true));
                    }

                    if (IsAttackingCondition())
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

                        selectPermanentEffect.SetUpCustomMessage(
                            "Select 1 Digimon that will get immunity to opponent's Digimon effects and DP +5000.",
                            "The opponent is selecting 1 Digimon that will get immunity to opponent's Digimon effects and DP +5000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                            canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon's effects",
                                CanUseConditionImmunity, card);
                            canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition,
                                SkillCondition: SkillCondition);
                            selectedPermanent.UntilOpponentTurnEndEffects.Add(GetCardEffect);

                            bool CanUseConditionImmunity(Hashtable hashtableImmunity)
                            {
                                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(selectedPermanent, card);
                            }

                            bool CardCondition(CardSource cardSource)
                            {
                                return CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent) &&
                                       cardSource == selectedPermanent.TopCard;
                            }

                            bool SkillCondition(ICardEffect cardEffect)
                            {
                                return CardEffectCommons.IsOpponentEffect(cardEffect, card) &&
                                       cardEffect.IsDigimonEffect;
                            }

                            ICardEffect GetCardEffect(EffectTiming timingImmunity)
                            {
                                return timingImmunity == EffectTiming.None ? canNotAffectedClass : null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: selectedPermanent,
                                changeValue: 5000,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Digimon from your hand to your empty breeding area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] You may play 1 [Dorumon]/[Ryudamon] from your hand to your empty breeding area without paying the cost. Then if during an attack until the end of your opponent's turn, 1 of your Digimon isn't affected by your opponent's Digimon's effects and gets +5000 DP.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool IsCorrectRookieCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                           (cardSource.EqualsCardName("Dorumon") || cardSource.EqualsCardName("Ryudamon")) &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass,
                               isBreedingArea: true);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool PlayBreedingAreaCondition()
                {
                    return CardEffectCommons.HasMatchConditionOwnersHand(card, IsCorrectRookieCardCondition) &&
                           card.Owner.GetBreedingAreaPermanents().Count == 0;
                }

                bool IsAttackingCondition()
                {
                    return GManager.instance.attackProcess.IsAttacking;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           (PlayBreedingAreaCondition() || IsAttackingCondition());
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (PlayBreedingAreaCondition())
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsCorrectRookieCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

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
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true,
                            isBreedingArea: true));
                    }

                    if (IsAttackingCondition())
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

                        selectPermanentEffect.SetUpCustomMessage(
                            "Select 1 Digimon that will get immunity to opponent's Digimon effects and DP +5000.",
                            "The opponent is selecting 1 Digimon that will get immunity to opponent's Digimon effects and DP +5000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                            canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon's effects",
                                CanUseConditionImmunity, card);
                            canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition,
                                SkillCondition: SkillCondition);
                            selectedPermanent.UntilOpponentTurnEndEffects.Add(GetCardEffect);

                            bool CanUseConditionImmunity(Hashtable hashtableImmunity)
                            {
                                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(selectedPermanent, card);
                            }

                            bool CardCondition(CardSource cardSource)
                            {
                                return CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent) &&
                                       cardSource == selectedPermanent.TopCard;
                            }

                            bool SkillCondition(ICardEffect cardEffect)
                            {
                                return CardEffectCommons.IsOpponentEffect(cardEffect, card) &&
                                       cardEffect.IsDigimonEffect;
                            }

                            ICardEffect GetCardEffect(EffectTiming timingImmunity)
                            {
                                return timingImmunity == EffectTiming.None ? canNotAffectedClass : null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: selectedPermanent,
                                changeValue: 5000,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));
                        }
                    }
                }
            }

            #endregion

            #region Opponent's Turn - ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Switch attack target to this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("AttackSwitch_BT20_053");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Opponent's Turn] [Once Per Turn] When one of your opponent's Digimon attacks, you may switch the attack target to this Digimon.";
                }

                bool AttackingPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsOpponentPermanent(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOpponentTurn(card) &&
                           CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, AttackingPermanent);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           GManager.instance.attackProcess.AttackingPermanent != null &&
                           GManager.instance.attackProcess.AttackingPermanent.CanSwitchAttackTarget;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        GManager.instance.attackProcess.SwitchDefender(activateClass, false, card.PermanentOfThisCard()));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}