using System.Collections;
using System.Collections.Generic;

// AncientMegatheriummon
namespace DCGO.CardEffects.BT18
{
    public class BT18_028 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DigiXros

            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros -2", CanUseCondition, card);
                addDigiXrosConditionClass.SetUpAddDigiXrosConditionClass(getDigiXrosCondition: GetDigiXros);
                addDigiXrosConditionClass.SetNotShowUI(true);
                cardEffects.Add(addDigiXrosConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                DigiXrosCondition GetDigiXros(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        DigiXrosConditionElement elementHuman =
                            new DigiXrosConditionElement(CanSelectCardCondition, "Kumamon");

                        bool CanSelectCardCondition(CardSource conditionCardSource)
                        {
                            return conditionCardSource != null
                                && conditionCardSource.Owner == card.Owner
                                && conditionCardSource.IsDigimon
                                && conditionCardSource.CardNames_DigiXros.Contains("Kumamon");
                        }

                        DigiXrosConditionElement elementBeast =
                            new DigiXrosConditionElement(CanSelectCardCondition1, "Korikakumon");

                        bool CanSelectCardCondition1(CardSource conditionCardSource)
                        {
                            return conditionCardSource != null
                                && conditionCardSource.Owner == card.Owner
                                && conditionCardSource.IsDigimon
                                && conditionCardSource.CardNames_DigiXros.Contains("Korikakumon");
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>()
                            { elementHuman, elementBeast };

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, null, 2);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "Trash 2 bottom sources from all opponent's Digimon. Their sourceless digimon can't suspend";

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
                return $"[{tag}] Trash the bottom 2 digivolution cards of all of your opponent's Digimon. None of their Digimon with no digivolution cards can suspend until the end of their turn.";
            }

            bool PermanentCanNotSuspendCondition(Permanent permanentCanNotSuspend)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanentCanNotSuspend, card)
                    && permanentCanNotSuspend.HasNoDigivolutionCards;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                foreach (Permanent selectedPermanent in card.Owner.Enemy.GetBattleAreaDigimons())
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                        targetPermanent: selectedPermanent,
                        trashCount: 2,
                        isFromTop: false,
                        activateClass: activateClass));
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotSuspendPlayerEffect(
                    permanentCondition: PermanentCanNotSuspendCondition,
                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                    activateClass: activateClass,
                    isOnlyActivePhase: false,
                    effectName: "Can't Suspend"));
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play a Digimon from digivolution cards.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] When this Digimon would leave the battle area, you may play 1 level 4 or lower Digimon card with the [Mammal]/[Ice-Snow]/[Hybrid] trait from this Digimon's digivolution cards without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.HasLevel && cardSource.Level <= 4 &&
                           (cardSource.ContainsTraits("Mammal") || cardSource.ContainsTraits("Ice-Snow") ||
                            cardSource.ContainsTraits("Hybrid")) &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && card.PermanentOfThisCard().DigivolutionCards.Some(CanSelectCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: CanSelectCardCondition,
                        SelectCardEffect.Root.DigivolutionCards,
                        activateClass,
                        payCost: false,
                        targetPermanent: card.PermanentOfThisCard()
                    ));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}