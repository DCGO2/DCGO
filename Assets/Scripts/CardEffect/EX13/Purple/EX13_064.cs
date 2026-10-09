using System.Collections;
using System.Collections.Generic;

// LordKnightmon
namespace DCGO.CardEffects.EX13
{
    public class EX13_064 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - Lv.5 w/[Knightmon] in text
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasText("Knightmon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null,
                    level: 5));
            }
            #endregion

            #region Alternate Digivolution Requirement - [Rie Kishibe]
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Rie Kishibe");

                bool Condition()
                    => card.Owner.SecurityCards.Count <= 3;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: Condition));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May play or use 1 cost 8 or lower [Knightmon] text card from hand or trash for free", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Digivolving] You may play or use 1 play or use cost 8 or lower [Knightmon] text card from your hand or trash without paying the cost.";

                bool IsKnightmonTextCard(CardSource cardSource)
                    => cardSource.HasText("Knightmon")
                        && cardSource.GetCostItself <= 8;

                bool CanSelectHandCardCondition(CardSource cardSource)
                    => IsKnightmonTextCard(cardSource)
                        && CardEffectCommons.CanPlayOrUse(cardSource, activateClass, SelectCardEffect.Root.Hand);

                bool CanSelectTrashCardCondition(CardSource cardSource)
                    => IsKnightmonTextCard(cardSource)
                        && CardEffectCommons.CanPlayOrUse(cardSource, activateClass, SelectCardEffect.Root.Trash);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCardCondition)
                            || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectTrashCardCondition));

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCardCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectTrashCardCondition);
                    bool fromHand = canSelectHand;
                    CardSource selectedCard = null;

                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new SelectionElement<int>(message: "From hand", value: 1, spriteIndex: 0),
                            new SelectionElement<int>(message: "From trash", value: 2, spriteIndex: 0),
                            new SelectionElement<int>(message: "Don't play or use", value: 3, spriteIndex: 1),
                        };

                        GManager.instance.userSelectionManager.SetIntSelection(
                            selectionElements: selectionElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "From which area do you play or use a card?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to play or use a card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 3) yield break;

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
                            canTargetCondition: CanSelectHandCardCondition,
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
                    else if (!fromHand && canSelectTrash)
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectTrashCardCondition,
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
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 card to play or use.", "The opponent is selecting 1 card to play or use.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    if (selectedCard == null) yield break;

                    SelectCardEffect.Root root = fromHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;

                    if (selectedCard.IsOption)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                            cardSources: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            payCost: false,
                            root: root));
                    }
                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: root,
                            activateETB: true));
                    }
                }
            }
            #endregion

            #region Your Turn - Alliance / Piercing
            if (timing == EffectTiming.None)
            {
                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("Your [Knightmon] text Digimon gain Alliance and Piercing", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                cardEffects.Add(addSkillClass);

                bool GrantCondition()
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanUseCondition(Hashtable hashtable)
                    => GrantCondition();

                bool PermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasText("Knightmon");

                bool CardSourceCondition(CardSource cardSource)
                    => cardSource.PermanentOfThisCard() != null
                        && PermanentCondition(cardSource.PermanentOfThisCard())
                        && cardSource == cardSource.PermanentOfThisCard().TopCard;

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> effects, EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnAllyAttack)
                    {
                        effects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: cardSource, condition: GrantCondition));
                    }

                    if (_timing == EffectTiming.OnDetermineDoSecurityCheck)
                    {
                        effects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: cardSource, condition: GrantCondition));
                    }

                    return effects;
                }
            }
            #endregion

            #region Your Turn - OPT
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 of your [Knightmon] text Digimon may gain <Rush> and <Collision> and attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("EX13_064_YT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Your Turn] [Once Per Turn] When any of your other Digimon or Tamers are played, 1 of your [Knightmon] text Digimon may gain <Rush> and <Collision> for the turn and attack.";

                bool PlayedPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer)
                        && permanent != card.PermanentOfThisCard();

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasText("Knightmon");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PlayedPermanentCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to gain <Rush> and <Collision> and attack.", "The opponent is selecting 1 Digimon to gain <Rush> and <Collision> and attack.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    if (selectedPermanent == null)
                    {
                        activateClass.RemoveUse();
                        yield break;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(
                        targetPermanent: selectedPermanent,
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCollision(
                        targetPermanent: selectedPermanent,
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));

                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent) && selectedPermanent.CanAttack(activateClass))
                    {
                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: selectedPermanent,
                            canAttackPlayerCondition: () => true,
                            defenderCondition: _ => true,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
