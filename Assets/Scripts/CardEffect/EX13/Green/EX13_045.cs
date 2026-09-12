using System.Collections;
using System.Collections.Generic;

// Examon
namespace DCGO.CardEffects.EX13
{
    public class EX13_045 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DNA Digivolution
            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect("DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                    => true;

                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource != card) return null;

                    bool GreenLevel6Condition(Permanent permanent)
                        => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                            && permanent.TopCard.CardColors.Contains(CardColor.Green)
                            && permanent.Levels_ForJogress(card).Contains(6);

                    bool BlueLevel6Condition(Permanent permanent)
                        => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                            && permanent.TopCard.CardColors.Contains(CardColor.Blue)
                            && permanent.Levels_ForJogress(card).Contains(6);

                    JogressConditionElement[] elements =
                    {
                        new JogressConditionElement(GreenLevel6Condition, "a level 6 Green Digimon"),
                        new JogressConditionElement(BlueLevel6Condition, "a level 6 Blue Digimon"),
                    };

                    return new JogressCondition(elements, 0);
                }
            }
            #endregion

            #region Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Security Attack +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Evade
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.EvadeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If DNA digivolving, attack, all your Digimon +10000 DP, then may battle", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Digivolving] If DNA digivolving, this Digimon attacks and all of your Digimon get +10000 DP until your opponent's turn ends. Then, this Digimon may battle 1 of your opponent's Digimon.";

                bool IsOwnerDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool IsOpponentDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    if (CardEffectCommons.IsJogress(hashtable))
                    {
                        if (thisPermanent != null && thisPermanent.CanAttack(activateClass))
                        {
                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: thisPermanent,
                                canAttackPlayerCondition: () => true,
                                defenderCondition: _ => true,
                                cardEffect: activateClass);

                            selectAttackEffect.SetCanNotSelectNotAttack();

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDPPlayerEffect(
                            permanentCondition: IsOwnerDigimon,
                            changeValue: 10000,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));
                    }

                    thisPermanent = card.PermanentOfThisCard();

                    if (thisPermanent != null && CardEffectCommons.HasMatchConditionPermanent(IsOpponentDigimon))
                    {
                        Permanent selectedDefender = null;

                        SelectPermanentEffect defenderSelectEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        defenderSelectEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOpponentDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectDefenderCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        defenderSelectEffect.SetUpCustomMessage("Select 1 of your opponent's Digimon to battle.", "The opponent is selecting 1 Digimon to battle.");

                        yield return ContinuousController.instance.StartCoroutine(defenderSelectEffect.Activate());

                        IEnumerator SelectDefenderCoroutine(Permanent permanent)
                        {
                            selectedDefender = permanent;
                            yield return null;
                        }

                        if (selectedDefender != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IBattle(thisPermanent, selectedDefender, null, true).Battle());
                        }
                    }
                }
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnEndBattle)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play/use 1 cost 12 or lower [Dracomon]/[Examon] text card from hand or sources for free", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("EX13_045_YT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] [Once Per Turn] When this Digimon wins a battle, you may play or use 1 play or use cost 12 or lower [Dracomon] or [Examon] text card from your hand or its digivolution cards without paying the cost.";

                // Battle hashtables hold copies of the winners with their card order reversed, so match on containment rather than TopCard
                bool WinnerCondition(Permanent permanent)
                    => permanent.cardSources.Contains(card);

                bool CanSelectCardCondition(CardSource cardSource)
                    => (cardSource.HasText("Dracomon") || cardSource.HasText("Examon"))
                        && cardSource.GetCostItself <= 12
                        && CardEffectCommons.CanPlayOrUse(cardSource, activateClass);

                bool HasSelectableSourceCard()
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.PermanentOfThisCard().DigivolutionCards.Some(CanSelectCardCondition);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerWhenWinBattle(hashtable, WinnerCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition) || HasSelectableSourceCard());

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canSelectSources = HasSelectableSourceCard();
                    bool fromHand = canSelectHand;
                    CardSource selectedCard = null;

                    if (canSelectHand && canSelectSources)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new SelectionElement<int>(message: "From hand", value: 1, spriteIndex: 0),
                            new SelectionElement<int>(message: "From digivolution cards", value: 2, spriteIndex: 0),
                            new SelectionElement<int>(message: "Don't play or use", value: 3, spriteIndex: 1),
                        };

                        GManager.instance.userSelectionManager.SetIntSelection(
                            selectionElements: selectionElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "From which area do you play or use a card?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to play or use a card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 3)
                        {
                            yield break;
                        }

                        fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                    }

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (fromHand && canSelectHand)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
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

                        selectHandEffect.SetUpCustomMessage("Select 1 card to play or use.", "The opponent is selecting 1 card to play or use.");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                    }
                    else if (!fromHand && canSelectSources)
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to play or use.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to play or use.", "The opponent is selecting 1 card to play or use.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    if (selectedCard == null)
                    {
                        activateClass.RemoveUse();
                    }
                    else if (selectedCard.IsOption)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                            cardSources: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            payCost: false,
                            root: SelectCardEffect.Root.Hand));
                    }
                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: fromHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.DigivolutionCards,
                            activateETB: true));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
