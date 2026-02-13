using System;
using System.Collections;
using System.Collections.Generic;

// Karakurumon
namespace DCGO.CardEffects.EX11
{
    public class EX11_022 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4
                        && targetPermanent.TopCard.EqualsTraits("Puppet")
                        && (targetPermanent.TopCard.CardColors.Contains(CardColor.Yellow)
                            || targetPermanent.TopCard.CardColors.Contains(CardColor.Purple));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Scapegoat

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.ScapegoatSelfEffect(isInheritedEffect: false, card: card, condition: null, effectName: "<Scapegoat>", effectDiscription: null));
            }

            #endregion

            #region OP/WD Shared

            string SharedEffectName() => "Play a 4K DP or less [Puppet] trait Digimon from hand or trash for free. End of turn, delete it.";

            string SharedEffectDescription(string tag) => $"[{tag}] You may play 1 [Puppet] trait Digimon card with 4000 DP or less from your hand or trash without paying the cost. At turn end, delete the Digimon this effect played.";

            bool SharedCanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, cardSource => CanSelectCardCondition(cardSource, activateClass))
                        || CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanSelectCardCondition(cardSource, activateClass)));
            }

            bool CanSelectCardCondition(CardSource cardSource, ActivateClass activateClass)
            {
                return cardSource.IsDigimon
                    && cardSource.HasDP
                    && cardSource.CardDP <= 4000
                    && cardSource.EqualsTraits("Puppet")
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Setup Location Selection

                bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanSelectCardCondition(cardSource, activateClass));
                bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, cardSource => CanSelectCardCondition(cardSource, activateClass));
                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                if (canSelectHand)
                {
                    selectionElements.Add(new(message: "From hand", value: 0, spriteIndex: 0));
                }
                if (canSelectTrash)
                {
                    selectionElements.Add(new(message: "From trash", value: 1, spriteIndex: 0));
                }
                selectionElements.Add(new(message: "Do not play", value: 2, spriteIndex: 1));

                string selectPlayerMessage = "From which area will you play?";
                string notSelectPlayerMessage = "The opponent is choosing if they will activate their effect.";

                GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements,
                    selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                    notSelectPlayerMessage: notSelectPlayerMessage);

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                    .WaitForEndSelect());

                int selection = GManager.instance.userSelectionManager.SelectedIntValue;

                #endregion

                #region Hand/Trash Card Selection & Play

                if (selection != 2)
                {
                    CardSource selectedCard = null;

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selection == 0)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: cardSource => CanSelectCardCondition(cardSource, activateClass),
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

                        selectHandEffect.SetUpCustomMessage("Select 1 Digimon to play.", "The opponent is selecting 1 Digimon to play.");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayPermanentCards(new List<CardSource>() { selectedCard }, activateClass, false, false, SelectCardEffect.Root.Hand, true));
                    }
                    else
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: cardSource => CanSelectCardCondition(cardSource, activateClass),
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 Digimon to play.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 Digimon to play.", "The opponent is selecting 1 Digimon to play.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayPermanentCards(new List<CardSource>() { selectedCard }, activateClass, false, false, SelectCardEffect.Root.Trash, true));
                    }

                    #region Delete Played Digimon

                    Permanent selectedPermanent = selectedCard.PermanentOfThisCard();

                    ActivateClass activateClass1 = new ActivateClass();
                    activateClass1.SetUpICardEffect("Delete this Digimon", CanUseCondition2, card);
                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, "");
                    activateClass1.SetEffectSourcePermanent(selectedPermanent);
                    CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: activateClass1, timing: EffectTiming.OnEndTurn);

                    bool CanUseCondition2(Hashtable hashtable)
                    {
                        return true;
                    }

                    bool CanActivateCondition1(Hashtable hashtable)
                    {
                        return selectedPermanent.TopCard != null
                            && selectedPermanent.CanBeDestroyedBySkill(activateClass1)
                            && !selectedPermanent.TopCard.CanNotBeAffected(activateClass1);
                    }

                    IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(new List<Permanent>() { selectedPermanent }, CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                    }

                    #endregion
                }

                #endregion

            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(hash => SharedCanActivateCondition(hash, activateClass), (hashTable) => SharedActivateCoroutine(hashTable, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(hash => SharedCanActivateCondition(hash, activateClass), (hashTable) => SharedActivateCoroutine(hashTable, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete your 1 other token or [Puppet] Digimon to prevent this Digimon from leaving Battle Area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Substitute_EX11_022");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon would leave the battle area other than by your effects, by deleting 1 of your Tokens or other [Puppet] trait trait Digimon, prevent it from leaving.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent != card.PermanentOfThisCard()
                        && (permanent.IsToken
                            || permanent.TopCard.EqualsTraits("Puppet"));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card)
                        && !CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent thisCardPermanent = card.PermanentOfThisCard();

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
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
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                targetPermanents: new List<Permanent>() { permanent },
                                activateClass: activateClass,
                                successProcess: permanents => SuccessProcess(),
                                failureProcess: null));

                            IEnumerator SuccessProcess()
                            {
                                thisCardPermanent.willBeRemoveField = false;

                                thisCardPermanent.HideDeleteEffect();
                                thisCardPermanent.HideHandBounceEffect();
                                thisCardPermanent.HideDeckBounceEffect();
                                thisCardPermanent.HideWillRemoveFieldEffect();

                                yield return null;
                            }
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
