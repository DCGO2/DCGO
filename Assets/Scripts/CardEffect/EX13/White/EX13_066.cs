using System.Collections;
using System.Collections.Generic;

// Sistermon Noir (Awakened) // Mickey Bullet (Awakened)
namespace DCGO.CardEffects.EX13
{
    public class EX13_066 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Rule - Name
            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("[Rule] Name: Also treated as [Sistermon Ciel (Awakened)].", _ => true, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: ChangeCardNames);
                cardEffects.Add(changeCardNamesClass);

                List<string> ChangeCardNames(CardSource cardSource, List<string> cardNames)
                {
                    if (cardSource == card) cardNames.Add("Sistermon Ciel (Awakened)");

                    return cardNames;
                }
            }
            #endregion

            #region Rule - Trait
            if (timing == EffectTiming.None)
            {
                ChangeTraitsClass changeTraitsClass = new ChangeTraitsClass();
                changeTraitsClass.SetUpICardEffect("[Rule] Trait: Also treated as [Data] Attribute.", _ => true, card);
                changeTraitsClass.SetUpChangeTraitsClass(changeeTraits: ChangeTraits);
                cardEffects.Add(changeTraitsClass);

                List<string> ChangeTraits(CardSource cardSource, List<string> cardTraits)
                {
                    if (cardSource == card && !cardTraits.Contains("Data")) cardTraits.Add("Data");

                    return cardTraits;
                }
            }
            #endregion

            #region Digimon Effects

            #region Alternate Digivolution Requirement - [Sistermon Noir]/[Sistermon Ciel]
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Sistermon Noir")
                        || targetPermanent.TopCard.EqualsCardName("Sistermon Ciel");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 1,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Alternate Digivolution Requirement - Lv.3 w/[Huckmon] in text
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasText("Huckmon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 3));
            }
            #endregion

            #region Decode
            if (timing == EffectTiming.WhenRemoveField)
            {
                static bool SourceCondition(CardSource source)
                    => source.EqualsCardName("Sistermon Noir")
                        || source.EqualsCardName("Sistermon Ciel");

                string[] decodeStrings = { "([Sistermon Noir]/[Sistermon Ciel])", "[Sistermon Noir] or [Sistermon Ciel]" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(
                    card: card,
                    isInheritedEffect: false,
                    decodeStrings: decodeStrings,
                    sourceCondition: SourceCondition,
                    condition: null));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 opponent's Digimon with play cost 4 or less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Digivolving] Delete 1 of your opponent's Digimon with a play cost of 4 or less.";

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasPlayCost
                        && permanent.TopCard.GetCostItself <= 4;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentCondition);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
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
            #endregion

            #endregion

            #region Option Effects

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May play 1 cost 4 or lower [Sistermon] in name card from hand or trash, then <De-Digivolve 1> 1 opponent's Digimon for each of your Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] You may play 1 play cost 4 or lower card with [Sistermon] in its name from your hand or trash without paying the cost. Then, to 1 of your opponent's Digimon, <De-Digivolve 1> for each of your Digimon.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanPlaySistermon(CardSource cardSource, SelectCardEffect.Root root)
                    => cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 4
                        && cardSource.ContainsCardName("Sistermon")
                        && CardEffectCommons.CanPlayAsNewPermanent(
                            cardSource: cardSource,
                            payCost: false,
                            cardEffect: activateClass,
                            root: root);

                bool IsOpponentDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region May play [Sistermon]
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(
                        card,
                        cardSource => CanPlaySistermon(cardSource, SelectCardEffect.Root.Hand));
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(
                        card,
                        cardSource => CanPlaySistermon(cardSource, SelectCardEffect.Root.Trash));

                    if (canSelectHand || canSelectTrash)
                    {
                        if (canSelectHand && canSelectTrash)
                        {
                            GManager.instance.userSelectionManager.SetBoolSelection(
                                selectionElements: new List<SelectionElement<bool>>()
                                {
                                    new SelectionElement<bool>(message: "From hand", value: true, spriteIndex: 0),
                                    new SelectionElement<bool>(message: "From trash", value: false, spriteIndex: 1),
                                },
                                selectPlayer: card.Owner,
                                selectPlayerMessage: "From which area will you play a card?",
                                notSelectPlayerMessage: "The opponent is choosing from which area to play a card.");
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectHand);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        SelectCardEffect.Root root = GManager.instance.userSelectionManager.SelectedBoolValue
                            ? SelectCardEffect.Root.Hand
                            : SelectCardEffect.Root.Trash;

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                            canTargetCondition: cardSource => CanPlaySistermon(cardSource, root),
                            root: root,
                            cardEffect: activateClass,
                            payCost: false));
                    }
                    #endregion

                    #region <De-Digivolve 1> for each of your Digimon
                    int degenerationCount = card.Owner.GetBattleAreaDigimons().Count;

                    if (degenerationCount > 0 && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentDigimon))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOpponentDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Degenerate,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetDegenerationCount(degenerationCount);
                        selectPermanentEffect.SetUpCustomMessage($"Select 1 Digimon to De-Digivolve {degenerationCount}.", $"The opponent is selecting 1 Digimon to De-Digivolve {degenerationCount}.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                    #endregion
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
