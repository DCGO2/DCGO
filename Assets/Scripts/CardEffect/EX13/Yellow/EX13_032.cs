using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Chirinmon
namespace DCGO.CardEffects.EX13
{
    public class EX13_032 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("DATA SQUAD");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region Shared WD/WA
            string SharedEffectName()
                => "Trash 1 from sec or Tamers to unsuspend, then prevent 1 enemy Digimon's [When Digivolving] until their turn ends";

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                additionalActivateCondition: AdditionalActivateCondition,
                maxCountPerTurn: 1,
                hashValue: "EX13_032_WD_WA",
                optional: false,
                isSkippable: true,
                whenDigivolving: true,
                whenAttacking: true);

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] By trashing your top security card or the bottom face-down card from under any of your Tamers, this Digimon unsuspends. After, 1 of your opponent's Digimon can't activate [When Digivolving] effects until their turn ends.";

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return card.Owner.SecurityCards.Any()
                    || CardEffectCommons.HasMatchConditionPermanent(IsTamerWithFaceDownCard);
            }

            bool IsTamerWithFaceDownCard(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Count(cs => cs.IsFaceDown) >= 1;

            bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

            bool CanSelectPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool isUsed = false;
                bool hasPaidCost = false;
                bool canAddSecurityToHand = card.Owner.SecurityCards.Any();
                bool canTrashBottomFaceDownCard = CardEffectCommons.HasMatchConditionPermanent(IsTamerWithFaceDownCard);

                #region Select to pay Cost
                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                if (canAddSecurityToHand) selectionElements.Add(new SelectionElement<int>(message: "Trash top security card", value: 1, spriteIndex: 0));
                if (canTrashBottomFaceDownCard) selectionElements.Add(new SelectionElement<int>(message: "Trash bottom face down card from 1 tamer", value: 2, spriteIndex: 0));
                selectionElements.Add(new SelectionElement<int>(message: "Don't pay the cost", value: 3, spriteIndex: 1));

                string selectPlayerMessage = "Will you pay the cost?";
                string notSelectPlayerMessage = "The opponent is choosing to pay the cost.";

                GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                bool payCost = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                bool isSecurity = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                #endregion

                #region Pay Cost
                if (payCost)
                {
                    if (isSecurity)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());

                        hasPaidCost = true;
                        isUsed = true;
                    }
                    else
                    {
                        SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect1.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsTamerWithFaceDownCard,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);


                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 1, isFromTop: false, activateClass: activateClass, FaceDownCards));
                            hasPaidCost = true;
                            isUsed = true;
                        }

                        selectPermanentEffect1.SetUpCustomMessage("Select 1 Tamer to trash 1 bottom face-down card from", "The opponent is selecting 1 Tamer to trash 1 bottom face-down card from");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());
                    }
                }

                if (hasPaidCost)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());

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
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that can't activate [When Digivolving] effects.", "The opponent is selecting 1 Digimon that can't activate [When Digivolving] effects.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent selectedPermanent)
                        {
                            DisableEffectClass invalidationClass = new DisableEffectClass();
                            invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", CanUseConditionDebuff, card);
                            invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => invalidationClass);

                            bool CanUseConditionDebuff(Hashtable hashtableDebuff)
                                => selectedPermanent.TopCard != null;

                            bool InvalidateCondition(ICardEffect cardEffect)
                                => selectedPermanent.TopCard != null
                                    && cardEffect != null
                                    && cardEffect.EffectSourceCard != null
                                    && isExistOnField(cardEffect.EffectSourceCard)
                                    && cardEffect.EffectSourceCard.PermanentOfThisCard() == selectedPermanent
                                    && cardEffect.IsWhenDigivolving
                                    && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);

                            yield return null;
                        }
                    }
                }
                #endregion

                if (!isUsed) activateClass.RemoveUse();
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing top stacked card as top security, doesn't leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When this Digimon would leave the battle area, by placing its top stacked card as the top security card, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.PermanentOfThisCard().DigivolutionCards.Count > 0
                        && card.Owner.CanAddSecurity(activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent protectedPermanent = card.PermanentOfThisCard();

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(card, card.PermanentOfThisCard()));

                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(card, toTop: true, faceUp: false));

                    protectedPermanent.willBeRemoveField = false;

                    protectedPermanent.HideDeleteEffect();
                    protectedPermanent.HideHandBounceEffect();
                    protectedPermanent.HideDeckBounceEffect();
                    protectedPermanent.HideWillRemoveFieldEffect();

                    yield return null;
                }
            }
            #endregion

            #region Inherited All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing top stacked card as top security, doesn't leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EX13_032_Inherited");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon with [Kentaurosmon] in its name would leave the battle area, by placing its top stacked card as the top security card, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.PermanentOfThisCard().TopCard.ContainsCardName("Kentaurosmon")
                        && card.PermanentOfThisCard().DigivolutionCards.Count > 0
                        && card.Owner.CanAddSecurity(activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(card.PermanentOfThisCard().TopCard, card.PermanentOfThisCard()));

                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(card.PermanentOfThisCard().TopCard, toTop: true, faceUp: false));

                    card.PermanentOfThisCard().willBeRemoveField = false;

                    card.PermanentOfThisCard().HideDeleteEffect();
                    card.PermanentOfThisCard().HideHandBounceEffect();
                    card.PermanentOfThisCard().HideDeckBounceEffect();
                    card.PermanentOfThisCard().HideWillRemoveFieldEffect();

                    yield return null;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
