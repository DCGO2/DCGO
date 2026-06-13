using System.Collections;
using System.Collections.Generic;

//Canoweissmon
namespace DCGO.CardEffects.EX12
{
    public class EX12_014 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Gammamon")
                        || targetPermanent.TopCard.EqualsTraits("VB");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 4)
                );
            }
            #endregion

            #region Decode
            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel
                        && cardSource.Level <= 4
                        && (cardSource.HasText("Gammamon")
                            || cardSource.EqualsTraits("VB"));
                }

                string[] decodeStrings = { "(Lv.4 or lower w/[Gammamon] in text or w/[VB] trait)", "Level 4 or lower Digimon card with [Gammamon] in text or [VB] Trait" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: false, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "May place 1 lvl 5 or lower [Gammamon] in text or [VB] trait from hand or trash under, then 1 of your Digimon may attack";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] You may place 1 level 5 or lower Digimon card with [Gammamon] in its text or the [VB] trait from your hand or trash as this Digimon's bottom digivolution card. Then, 1 of your Digimon may attack.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.IsDigimon
                    && cardSource.HasLevel
                    && cardSource.Level <= 5
                    && (cardSource.HasText("Gammamon")
                        || cardSource.EqualsTraits("VB"));
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region May place under
                bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                if (canSelectHand || canSelectTrash)
                {
                    #region Setup Location Selection
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    if (canSelectHand)
                    {
                        selectionElements.Add(new(message: $"From hand", value: 1, spriteIndex: 0));
                    }
                    if (canSelectTrash)
                    {
                        selectionElements.Add(new(message: $"From trash", value: 2, spriteIndex: 0));
                    }
                    selectionElements.Add(new(message: $"Do not select", value: 3, spriteIndex: 1));

                    string selectPlayerMessage = "From which area do you select a card?";
                    string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                    #endregion

                    bool doSelect = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                    bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                    if (doSelect)
                    {
                        #region Hand/Trash Card Selection
                        List<CardSource> selectedCards = new List<CardSource>();

                        if (fromHand)
                        {
                            SelectHandEffect selectHandEffect2 = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect2.SetUp(
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

                            yield return StartCoroutine(selectHandEffect2.Activate());
                        }
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to play.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectCardEffect.Activate());
                        }
                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);                            
                            yield return null;
                        }
                        #endregion

                        if (selectedCards.Count > 0)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));
                        }
                    }
                }
                #endregion

                #region May Attack
                bool IsYourDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.CanAttack(activateClass);
                }

                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsYourDigimon))
                {
                    SelectPermanentEffect selectPermanentEffect =
                        GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsYourDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Attack,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will attack.",
                        "The opponent is selecting 1 Digimon that will attack.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
                #endregion
            }
            #endregion

            #region Inherited
            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel
                        && cardSource.Level <= 4
                        && (cardSource.HasText("Gammamon")
                            || cardSource.EqualsTraits("VB"));
                }

                string[] decodeStrings = { "(Lv.4 or lower w/[Gammamon] in text or w/[VB] trait)", "Level 4 or lower Digimon card with [Gammamon] in text or [VB] Trait" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: true, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
