using System.Collections;
using System.Collections.Generic;

// Yuji Musya
namespace DCGO.CardEffects.BT27
{
    public class BT27_087 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Start of Your Main Phase] If you have a Digimon with the [Chronicle] trait, gain 1 memory.";

                bool ChronicleDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasChronicleTraits;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(ChronicleDigimonCondition)
                        && card.Owner.CanAddMemory(activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Activate 1 effect: reveal 3 and add 1 [X Antibody]/[Chronicle] card, or play 1 Lv.3 [Chronicle] Digimon to breeding area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Play] Activate 1 of the effects below: - Reveal the top 3 cards of your deck. Add 1 [X Antibody] or [Chronicle] trait card among them to the hand. Trash the rest. - You may play 1 level 3 [Chronicle] trait Digimon card from your hand to your breeding area without paying the cost.";

                bool CanSelectRevealCardCondition(CardSource cardSource)
                    => cardSource.HasXAntibodyTraits || cardSource.HasChronicleTraits;

                bool CanSelectBreedingCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && cardSource.IsLevel3
                        && cardSource.HasChronicleTraits
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Hand, isBreedingArea: true);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                    {
                        new(message: "Reveal 3, add 1 [X Antibody]/[Chronicle] card", value: 1, spriteIndex: 0),
                        new(message: "Play 1 Lv.3 [Chronicle] Digimon to breeding area", value: 2, spriteIndex: 0),
                    };

                    GManager.instance.userSelectionManager.SetIntSelection(
                        selectionElements: selectionElements,
                        selectPlayer: card.Owner,
                        selectPlayerMessage: "Select 1 effect to activate.",
                        notSelectPlayerMessage: "The opponent is selecting 1 effect to activate.");

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    int choice = GManager.instance.userSelectionManager.SelectedIntValue;

                    if (choice == 1)
                    {
                        #region Reveal the top 3 cards
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                            revealCount: 3,
                            simplifiedSelectCardConditions:
                            new SimplifiedSelectCardConditionClass[]
                            {
                                new SimplifiedSelectCardConditionClass(
                                    canTargetCondition: CanSelectRevealCardCondition,
                                    message: "Select 1 card with the [X Antibody] or [Chronicle] trait.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null),
                            },
                            remainingCardsPlace: RemainingCardsPlace.Trash,
                            activateClass: activateClass));
                        #endregion
                    }
                    else if (choice == 2)
                    {
                        #region May play 1 Lv.3 [Chronicle] Digimon to breeding area
                        if (!CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectBreedingCardCondition)) yield break;

                        CardSource selectedCard = null;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectBreedingCardCondition,
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

                        selectHandEffect.SetUpCustomMessage("Select 1 card to play in the breeding area.", "The opponent is selecting 1 card to play in the breeding area.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                        if (selectedCard != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: new List<CardSource>() { selectedCard },
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Hand,
                                activateETB: true,
                                isBreedingArea: true));
                        }
                        #endregion
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
