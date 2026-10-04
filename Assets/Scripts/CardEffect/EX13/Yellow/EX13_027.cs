using System.Collections;
using System.Collections.Generic;

// Chuumon
namespace DCGO.CardEffects.EX13
{
    public class EX13_027 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared WM/OP
            string SharedEffectName()
                => "Reveal top 3, add 1 [Sukamon]/[Etemon] card to hand, trash 1, bot deck the rest";

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                whenMoving: true,
                onPlay: true);

            string SharedEffectDescription(string tag)
                => $"[{tag}] Reveal the top 3 cards of your deck. Among them, add 1 card with [Sukamon] or [Etemon] in its name to the hand and trash 1 such card. Return the rest to the bottom of the deck.";

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.ContainsCardName("Sukamon")
                    || cardSource.ContainsCardName("Etemon");
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                revealCount: 3,
                simplifiedSelectCardConditions:
                new SimplifiedSelectCardConditionClass[]
                {
                    new SimplifiedSelectCardConditionClass(
                        canTargetCondition:CanSelectCardCondition,
                        message: "Select 1 card with [Sukamon] or [Etemon] in its name to add to hand.",
                        mode: SelectCardEffect.Mode.AddHand,
                        maxCount: 1,
                        selectCardCoroutine: null),
                    new SimplifiedSelectCardConditionClass(
                        canTargetCondition:CanSelectCardCondition,
                        message: "Select 1 card with [Sukamon] or [Etemon] in its name to trash.",
                        mode: SelectCardEffect.Mode.Discard,
                        maxCount: 1,
                        selectCardCoroutine: null),
                },

            remainingCardsPlace: RemainingCardsPlace.DeckBottom,
            activateClass: activateClass));
            }
            #endregion

            #region Inherited
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 other [Sukamon] in name to prevent leaving", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("EX13_027_Inherit");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon would leave the battle area other than by your effects, by deleting 1 other Digimon with [Sukamon] in its name, it doesn't leave.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent)
                        && permanent != card.PermanentOfThisCard()
                        && permanent.TopCard.ContainsCardName("Sukamon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card)
                        && !CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool isUsed = false;

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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        Permanent thisCardPermanent = card.PermanentOfThisCard();
                        isUsed = true;

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { permanent }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                        IEnumerator SuccessProcess()
                        {
                            if (thisCardPermanent.TopCard != null)
                            {
                                thisCardPermanent.willBeRemoveField = false;

                                thisCardPermanent.HideDeleteEffect();
                                thisCardPermanent.HideHandBounceEffect();
                                thisCardPermanent.HideDeckBounceEffect();
                                thisCardPermanent.HideWillRemoveFieldEffect();
                            }

                            yield return null;
                        }
                    }

                    if (!isUsed)
                    {
                        activateClass.RemoveUse();
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
