using System.Collections;
using System.Collections.Generic;

// Sukamon
namespace DCGO.CardEffects.EX13
{
    public class EX13_028 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal 3 from deck, may play 1 3 cost or lower Digimon [Chuumon]/[Sukamon] in name for free, trash rest", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Deletion] Reveal the top 3 cards of your deck. You may play 1 play cost 3 or lower Digimon card with [Chuumon] or [Sukamon] in its name among them without paying the cost. Trash the rest.";
                }

                bool CanPlayCondition(CardSource cardSource)
                {
                    return cardSource.HasPlayCost
                        && cardSource.GetCostItself <= 3
                        && (cardSource.ContainsCardName("Chuumon")
                            || cardSource.ContainsCardName("Sukamon"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                        revealCount: 3,
                        simplifiedSelectCardConditions:
                        new SimplifiedSelectCardConditionClass[]
                        {
                            new SimplifiedSelectCardConditionClass(
                                canTargetCondition:CanPlayCondition,
                                message: "Select 1 Digimon card to Play.",
                                mode: SelectCardEffect.Mode.Custom,
                                maxCount: 1,
                                selectCardCoroutine: SelectCardCoroutine),
                        },
                        remainingCardsPlace: RemainingCardsPlace.Trash,
                        activateClass: activateClass,
                        canNoSelect: true
                    ));

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        new List<CardSource>() { selectedCard },
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Library,
                        activateETB: true));
                }
            }
            #endregion

            #region Inherited
            if (timing == EffectTiming.OnLeaveFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 other [Sukamon] in name to prevent leaving", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("EX13_028_Inherit");
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
