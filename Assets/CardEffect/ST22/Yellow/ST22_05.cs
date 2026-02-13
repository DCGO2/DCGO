using System;
using System.Collections;
using System.Collections.Generic;

// Sakuyamon ACE
namespace DCGO.CardEffects.ST22
{
    public class ST22_05 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Alt Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.EqualsCardName("Sakuyamon: Maid Mode");

                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 1, true, card, null));
            }

            #endregion

            #region Blast Digivolve

            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }
            #endregion

            #region Alliance

            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #endregion

            #region OP/WD/WA Shared

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Play Pipe Fox Token

                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                {
                    new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                    new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                };

                string selectPlayerMessage = "Will you play a [Pipe Fox] token?";
                string notSelectPlayerMessage = "The opponent is choosing to play a token";
                GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                bool summonToken = GManager.instance.userSelectionManager.SelectedBoolValue;
                if (summonToken) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPipeFox(activateClass));


                #endregion

                #region Use Option Card

                bool SelectSourceCard(CardSource source)
                {
                    return source.IsOption
                        && !source.CanNotPlayThisOption
                        && (source.EqualsTraits("Onmyōjutsu") || source.EqualsTraits("Plug-In"));
                }

                bool CanSelectPermanent(Permanent permanent)
                {
                    return permanent.IsTamer
                        && permanent.DigivolutionCards.Exists(SelectSourceCard);
                }

                bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, SelectSourceCard);
                bool canSelectTamer = CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanent);

                if (canSelectHand || canSelectTamer)
                {
                    if (canSelectHand && canSelectTamer)
                    {
                        List<SelectionElement<bool>> selectionElements1 = new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                        new SelectionElement<bool>(message: $"From tamer", value : false, spriteIndex: 1),
                    };

                        string selectPlayerMessage1 = "From which area do you select a card?";
                        string notSelectPlayerMessage1 = "The opponent is choosing from which area to select a card.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);
                    }
                    else
                    {
                        GManager.instance.userSelectionManager.SetBool(canSelectHand);
                    }
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                    bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                    List<CardSource> selectedCards = new List<CardSource>();
                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        yield return null;
                    }

                    Permanent selectedPermament = null;
                    IEnumerator SelectPermamentCoroutine(Permanent permanent)
                    {
                        selectedPermament = permanent;
                        yield return null;
                    }

                    if (fromHand)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: SelectSourceCard,
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

                        selectHandEffect.SetUpCustomMessage("Select 1 option to use.", "The opponent is selecting 1 option to use.");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
                        if (selectedCards.Count > 0) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            root: SelectCardEffect.Root.Hand));

                    }
                    else
                    {

                        var selectablePermanents = CardEffectCommons.MatchConditionOwnersPermanentCount(card, CanSelectPermanent);
                        if (selectablePermanents == 1) selectedPermament = card.Owner.GetBattleAreaPermanents().Find(CanSelectPermanent);
                        else
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, CanSelectPermanent));
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanent,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermamentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }

                        if (selectedPermament != null)
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: SelectSourceCard,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 option card to use",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.DigivolutionCards,
                                customRootCardList: selectedPermament.DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 option card to use.", "The opponent is selecting 1 option card to use.");
                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            if (selectedCards.Count > 0) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                cardSources: selectedCards,
                                activateClass: activateClass,
                                payCost: false,
                                root: SelectCardEffect.Root.DigivolutionCards));
                        }

                    }
                }

                #endregion
            }

            #endregion

            #region On Play - OPT

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may then 1 [Pipe Fox] Token, then you may use 1 [Onmyōjutsu] or [Plug-In] trait in hand or under a tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), 1, true, EffectDiscription());
                activateClass.SetHashString("ST22-005");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] [Once Per Turn] You may play 1 [Pipe Fox] Token. Then, you may use 1 Option card with the [Onmyōjutsu] or [Plug-In] trait from your hand or under your Tamers without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region When Digivolving - OPT

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may then 1 [Pipe Fox] Token, then you may use 1 [Onmyōjutsu] or [Plug-In] trait in hand or under a tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), 1, true, EffectDiscription());
                activateClass.SetHashString("ST22-005");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] [Once Per Turn] You may play 1 [Pipe Fox] Token. Then, you may use 1 Option card with the [Onmyōjutsu] or [Plug-In] trait from your hand or under your Tamers without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region When Attacking - OPT

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may then 1 [Pipe Fox] Token, then you may use 1 [Onmyōjutsu] or [Plug-In] trait in hand or under a tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), 1, true, EffectDiscription());
                activateClass.SetHashString("ST22-005");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] You may play 1 [Pipe Fox] Token. Then, you may use 1 Option card with the [Onmyōjutsu] or [Plug-In] trait from your hand or under your Tamers without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            return cardEffects;
        }
    }
}