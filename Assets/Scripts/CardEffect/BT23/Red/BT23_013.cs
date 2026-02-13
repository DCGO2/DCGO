using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Jesmon
namespace DCGO.CardEffects.BT23
{
    public class BT23_013 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("SaviorHuckmon")
                        || (targetPermanent.TopCard.HasCSTraits && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.IsLevel5);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Huckmon");
                }

                bool Condition()
                {
                    return card.Owner.Enemy.GetBattleAreaDigimons().Filter(x => x.DP >= 10000).Count >= 1;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 5,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: Condition)
                );
            }

            #endregion

            #region Rush

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Alliance

            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #endregion

            #region WD/WA Shared

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool IsValidSistermon(CardSource cardSource)
                {
                    var digimonOnField = card.Owner.GetBattleAreaDigimons().Select(x => x.TopCard);

                    var fieldNames = digimonOnField
                        .SelectMany(x => x.CardNames)
                        .Select(n => n.ToLowerInvariant())
                        .ToHashSet();

                    var sourceNames = (cardSource.CardNames)
                        .Select(n => n.ToLowerInvariant());

                    return cardSource.IsDigimon
                        && cardSource.ContainsCardName("Sistermon")
                        && !sourceNames.Any(n => fieldNames.Contains(n))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                bool HasTokenInPlay(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.EqualsCardName("Atho, René & Por");
                }

                bool playToken = false;
                bool playSistermon = false;
                bool playFromHand = false;
                bool playFromTrash = false;

                bool canPlayToken = !CardEffectCommons.HasMatchConditionPermanent(HasTokenInPlay);
                bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, IsValidSistermon);
                bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsValidSistermon);

                #region User Selection - Play Token or Select Card Location

                var playOptions = new List<SelectionElement<bool>>();
                if (canPlayToken) playOptions.Add(new SelectionElement<bool>(message: $"Play [Atho, René & Por] Token", value: true, spriteIndex: 0));
                if (canSelectHand || canSelectTrash) playOptions.Add(new SelectionElement<bool>(message: $"Player [Sistermon] in name digimon from hand or trash", value: false, spriteIndex: 1));

                if (playOptions.Count == 1)
                {
                    if (playOptions[0].Message.Contains("Token")) playToken = true;
                    else playSistermon = true;
                }

                if (playOptions.Count == 2)
                {
                    string selectPlayerMessage = "Which effect will you use";
                    string notSelectPlayerMessage = "The opponent is which effect to use.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: playOptions, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                    var selectedOption = GManager.instance.userSelectionManager.SelectedBoolValue;

                    if (selectedOption) playToken = true;
                    else playSistermon = true;
                }

                if (playSistermon)
                {
                    if (canSelectHand && canSelectTrash)
                    {
                        List<SelectionElement<bool>> selectionElements1 = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage1 = "From which area do you select a card?";
                        string notSelectPlayerMessage1 = "The opponent is choosing from which area to select a card.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);
                    }
                    else
                    {
                        GManager.instance.userSelectionManager.SetBool(canSelectHand);
                    }

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    var handOrTrashSelection = GManager.instance.userSelectionManager.SelectedBoolValue;
                    if (handOrTrashSelection) playFromHand = true;
                    else playFromTrash = true;
                }

                #endregion

                CardSource selectedCard = null;
                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCard = cardSource;
                    yield return null;
                }

                if (playToken) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayAthoRenePorToken(activateClass: activateClass));

                if (playFromHand)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsValidSistermon,
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

                    selectHandEffect.SetUpCustomMessage("Select 1 digimon to play", "The opponent is selecting 1 digimon to play");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        new List<CardSource>() { selectedCard },
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Hand,
                        activateETB: true));
                }

                if (playFromTrash)
                {
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: IsValidSistermon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 digimon to play",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 digimon to play", "The opponent is selecting 1 digimon to play");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Selected Digimon");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        new List<CardSource>() { selectedCard },
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Trash,
                        activateETB: true));
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Atho, René & Por] token or 1 [Sistermon] in name digimon from hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may play 1 [Atho, René & Por] Token or, from your hand or trash, 1 Digimon card with [Sistermon] in its name without paying the cost. This effect can't play cards with the same names as any of your Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Atho, René & Por] token or 1 [Sistermon] in name digimon from hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] You may play 1 [Atho, René & Por] Token or, from your hand or trash, 1 Digimon card with [Sistermon] in its name without paying the cost. This effect can't play cards with the same names as any of your Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region Your Turn  - OPT

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This digimon may attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT23_013_YT");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When any of your other Digimon are played, this Digimon may attack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition, null);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent != card.PermanentOfThisCard();
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.PermanentOfThisCard().CanAttack(activateClass))
                    {
                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: card.PermanentOfThisCard(),
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