using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Machinedramon Ace
namespace DCGO.CardEffects.EX12
{
    public class EX12_059 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Cyborg")
                        || targetPermanent.TopCard.EqualsTraits("ME");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 5)
                );
            }
            #endregion

            #region Blast Digivolve
            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Fragment
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                string EffectDescription()
                {
                    return "<Fragment <2>> (When this Digimon would be deleted, by trashing any 2 of its digivolution cards, it isn’t deleted.)";
                }

                cardEffects.Add(CardEffectFactory.FragmentSelfEffect(isInheritedEffect: false, card: card, condition: null, trashValue: 2, effectName: "Fragment <2>", effectDiscription: EffectDescription()));
            }
            #endregion

            #region Shared OP/WD/WA
            string SharedEffectName = "<De-Digivolve 3> 1 of enemy Digimon, then may place 2 lvl 5 or lower [Machine]/[Cyborg]/[ME] from hand or trash under to be immune to trashing until end of enemy turn";

            string SharedEffectHash = "EX12_059_OP_WD_WA";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: SharedEffectHash,
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] [Once Per Turn] <De-Digivolve 3> 1 of your opponent's Digimon. Then, by placing 2 level 5 or lower [Machine], [Cyborg] or [ME] trait cards from your hand or trash as this Digimon's bottom digivolution cards, your opponent's effects can't trash any of your Digimon's stacked cards until their turn ends.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.HasLevel
                    && cardSource.Level <= 5
                    && (cardSource.EqualsTraits("Machine")
                        || cardSource.EqualsTraits("Cyborg")
                        || cardSource.EqualsTraits("ME"));
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
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
                        mode: SelectPermanentEffect.Mode.Degenerate,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetDegenerationCount(3);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's Digimon to De-Digivolve", "The opponent is selecting 1 Digimon to De-Digivolve");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                if(card.Owner.HandCards.Count(CanSelectCardCondition) + card.Owner.TrashCards.Count(CanSelectCardCondition) >= 2)
                {
                    int toSelect = 2;
                    List<CardSource> selectedCards = new List<CardSource>();

                    while (toSelect > 0)
                    {
                        bool CanSelectFilteredCardCondtion(CardSource cardSource)
                        {
                            return CanSelectCardCondition(cardSource)
                                && !selectedCards.Contains(cardSource);
                        }

                        int validHandCardCount = card.Owner.HandCards.Count(CanSelectFilteredCardCondtion);
                        int validTrashCardCount = card.Owner.TrashCards.Count(CanSelectFilteredCardCondtion);

                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                        if (validHandCardCount > 0)
                        {
                            selectionElements.Add(new(message: "from Hand", value: 1, spriteIndex: 0));
                        }
                        if (validTrashCardCount > 0)
                        {
                            selectionElements.Add(new(message: "from Trash", value: 2, spriteIndex: 0));
                        }
                        selectionElements.Add(new(message: "Do not place", value: 3, spriteIndex: 1));

                        GManager.instance.userSelectionManager.SetIntSelection(
                            selectionElements: selectionElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "From which area will you select a card?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to select card.");

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedIntValue == 3)
                        {
                            break;
                        }
                        if (GManager.instance.userSelectionManager.SelectedIntValue == 1)
                        {
                            int maxCount = Math.Min(toSelect, card.Owner.HandCards.Count(CanSelectCardCondition));
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectFilteredCardCondtion,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: true,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            string messagePluralize = maxCount > 1 ? "Select one or more cards to place under this Digimon. You will be able to select a second card if you only choose 1." : "Select a card to place under this Digimon.";

                            selectHandEffect.SetUpCustomMessage(
                                messagePluralize,
                                $"The opponent is selecting cards to place.");

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }
                        else
                        {
                            int maxCount = Math.Min(toSelect, card.Owner.TrashCards.Count(CanSelectCardCondition));
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectFilteredCardCondtion,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select card(s) to place",
                                maxCount: maxCount,
                                canEndNotMax: true,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            string messagePluralize = maxCount > 1 ? "Select one or more cards to place under this Digimon. You will be able to select a second card if you only choose 1." : "Select a card to place under this Digimon.";

                            selectCardEffect.SetUpCustomMessage(
                                messagePluralize,
                                $"The opponent is selecting cards to place.");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                selectedCards.Add(cardSource);
                                toSelect--;
                            }

                            yield return null;
                        }
                    }

                    if (selectedCards.Count == 2)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(selectedCards, "Digivolution Cards", true, true));
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));

                        bool DigimonCondition(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && !permanent.TopCard.CanNotBeAffected(activateClass);
                        }

                        ImmuneStackTrashingClass immuneFromStackTrashingClass = new ImmuneStackTrashingClass();
                        immuneFromStackTrashingClass.SetUpICardEffect("Isn't affected by trashing any stacked card", hashtable => true, card);
                        immuneFromStackTrashingClass.SetUpImmuneFromStackTrashingClass(PermanentCondition: DigimonCondition, EffectCondition: EffectCondition);
                        card.Owner.UntilOpponentTurnEndEffects.Add((_timing) => immuneFromStackTrashingClass);
                        card.Owner.UntilOpponentTurnEndEffects.Add((_timing) => PermanentEffectFactory.StaticAddDetailClass(_ => true, DigimonCondition, "Opponent's effects cannot trash stacked cards", false, activateClass));

                        bool EffectCondition(ICardEffect cardEffect) => CardEffectCommons.IsOpponentEffect(cardEffect, card);
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
