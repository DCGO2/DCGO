using System.Collections;
using System.Collections.Generic;
using System.Linq;

// KingSukamon
namespace DCGO.CardEffects.BT11
{
    public class BT11_043 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Evo
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Sukamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 4));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "1 enemy Digimon's name becomes [Sukamon], color becomes white, and DP becomes 3000";

            string SharedEffectDescription(string tag) => $"[{tag}] If your opponent has 16 or more cards in their trash, or you have 3 or more cards with [Sukamon] in their names in your trash, change 1 of your opponent's Digimon into a white Digimon with 3000 DP and an original name of [Sukamon] until the end of your opponent's turn.";

            CardEffectFactory.ActivateClassesForSharedEffects
               (ref cardEffects, timing, card,
                   SharedEffectName,
                   SharedActivateCoroutine,
                   SharedEffectDescription,
                   optional: false,
                   onPlay: true,
                   whenDigivolving: true,
                   isSkippable: true,
                   additionalUseCondition: AdditionalCanUseCondition);

            bool AdditionalCanUseCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return card.Owner.Enemy.TrashCards.Count >= 16
                    || card.Owner.TrashCards.Count((cardSource) => cardSource.ContainsCardName("Sukamon")) >= 3;
            }

            bool CanSelectOpponentPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Digimon that will get effects.",
                        "The opponent is selecting 1 Digimon that will get effects.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        Permanent selectedPermanent = permanent;

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeBaseDigimonDP(
                                targetPermanent: permanent,
                                changeValue: 3000,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));
                        }

                        if (selectedPermanent != null)
                        {
                            ChangeBaseCardNameClass changeBaseCardNameClass = new ChangeBaseCardNameClass();
                            changeBaseCardNameClass.SetUpICardEffect("Original card name is [Sukamon]", CanUseCondition1, card);
                            changeBaseCardNameClass.SetUpChangeBaseCardNamesClass(changeBaseCardNames: ChangeBaseCardNames);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => changeBaseCardNameClass);

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return selectedPermanent.TopCard != null
                                    && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                            }

                            List<string> ChangeBaseCardNames(CardSource cardSource, List<string> CardNames)
                            {
                                if (cardSource == selectedPermanent.TopCard)
                                {
                                    CardNames = new List<string>() { "Sukamon" };
                                }

                                return CardNames;
                            }
                        }

                        if (selectedPermanent != null)
                        {
                            ChangeBaseCardColorClass changeBaseCardNameClass = new ChangeBaseCardColorClass();
                            changeBaseCardNameClass.SetUpICardEffect("Original card color is white", CanUseCondition1, card);
                            changeBaseCardNameClass.SetUpChangeBaseCardColorClass(ChangeBaseCardColors: ChangeBaseCardColors);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => changeBaseCardNameClass);

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return selectedPermanent.TopCard != null
                                    && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                            }

                            List<CardColor> ChangeBaseCardColors(CardSource cardSource, List<CardColor> CardColors)
                            {
                                if (cardSource == selectedPermanent.TopCard)
                                {
                                    CardColors = new List<CardColor>() { CardColor.White };
                                }

                                return CardColors;
                            }
                        }
                    }
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon gains Security Attack+", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Attacking] For each other Digimon with [Sukamon] in its name in play, this Digimon gains <Security Attack +1> for the turn.";
                }

                int count()
                {
                    return CardEffectCommons.MatchConditionPermanentCount(PermanentCondition);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent)
                        && permanent.IsDigimon
                        && permanent != card.PermanentOfThisCard()
                        && permanent.TopCard.ContainsCardName("Sukamon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && count() >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    int changeValue = count();

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                        targetPermanent: card.PermanentOfThisCard(),
                        changeValue: changeValue,
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Prevent this Digimon from being deleted", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("BT11_043_Inherit");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When this Digimon would be deleted, by deleting 1 other Digimon with [Sukamon] in its name, prevent that deletion.";
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
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {

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

                    activateClass.RemoveUse();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
