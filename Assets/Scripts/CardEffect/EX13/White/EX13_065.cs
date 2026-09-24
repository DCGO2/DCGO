using System.Collections;
using System.Collections.Generic;

// Sistermon Blanc (Awakened) // Divine Pierce (Awakened)
namespace DCGO.CardEffects.EX13
{
    public class EX13_065 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects

            #region Alternate Digivolution Requirement - [Sistermon Blanc]
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Sistermon Blanc");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Alternate Digivolution Requirement - Lv.2 w/[Huckmon] in text
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasText("Huckmon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 2));
            }
            #endregion

            #region Decode
            if (timing == EffectTiming.WhenRemoveField)
            {
                static bool SourceCondition(CardSource source)
                    => source.EqualsCardName("Sistermon Blanc");

                string[] decodeStrings = { "([Sistermon Blanc])", "[Sistermon Blanc]" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: false, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }
            #endregion

            #region Guard
            if (timing == EffectTiming.WhenRemoveField)
            {
                cardEffects.Add(CardEffectFactory.GuardSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #endregion

            #region Option Effects

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May play 1 cost 4 or lower [Sistermon] in name card from hand or trash, then 1 opponent's Digimon gets -3000 DP for each of your Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] You may play 1 play cost 4 or lower card with [Sistermon] in its name from your hand or trash without paying the cost. Then, to 1 of your opponent's Digimon, give -3000 DP for the turn for each of your Digimon.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanPlaySistermon(CardSource cardSource, SelectCardEffect.Root root)
                    => cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 4
                        && cardSource.ContainsCardName("Sistermon")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass, root: root);

                bool IsOpponentDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    #region May play [Sistermon]
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanPlaySistermon(cardSource, SelectCardEffect.Root.Hand));
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, cardSource => CanPlaySistermon(cardSource, SelectCardEffect.Root.Trash));

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

                    #region -3000 DP for each of your Digimon
                    int dpMinus = 3000 * card.Owner.GetBattleAreaDigimons().Count;

                    if (dpMinus > 0 && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentDigimon))
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
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage($"Select 1 Digimon that will get -{dpMinus} DP.", $"The opponent is selecting 1 Digimon that will get -{dpMinus} DP.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: permanent,
                                changeValue: -dpMinus,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                        }
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
