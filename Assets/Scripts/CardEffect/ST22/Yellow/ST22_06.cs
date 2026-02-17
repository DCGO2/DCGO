using System;
using System.Collections;
using System.Collections.Generic;

// Sakuyamon: Maid Mode
namespace DCGO.CardEffects.ST22
{
    public class ST22_06 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.EqualsCardName("Sakuyamon");

                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 1, true, card, null));
            }

            #endregion

            #region OP/WD Shared

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
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
                        List<SelectionElement<bool>> selectionElements2 = new List<SelectionElement<bool>>()
                    {
                        new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                        new SelectionElement<bool>(message: $"From tamer", value : false, spriteIndex: 1),
                    };

                        string selectPlayerMessage2 = "From which area do you select a card?";
                        string notSelectPlayerMessage2 = "The opponent is choosing from which area to select a card.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements2, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage2, notSelectPlayerMessage: notSelectPlayerMessage2);
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

                        selectHandEffect.SetUpCustomMessage("Select 1 option card to use.", "The opponent is selecting 1 option card to use.");

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
                activateClass.SetUpICardEffect("You may use 1 [Onmyōjutsu] or [Plug-In] trait in hand or under a tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] You may use 1 Option card with the [Onmyōjutsu] or [Plug-In] trait from your hand or under your Tamers without paying the cost.";
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
                activateClass.SetUpICardEffect("You may use 1 [Onmyōjutsu] or [Plug-In] trait in hand or under a tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may use 1 Option card with the [Onmyōjutsu] or [Plug-In] trait from your hand or under your Tamers without paying the cost.";
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

            #region All Turns - On Lose Security / On Use Option - OPT

            if (timing == EffectTiming.OnLoseSecurity || timing == EffectTiming.OnUseOption)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 1 Digimon in Security, trash top Security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("ST22-06_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] [Once Per Turn] When you use Option cards or your security stack is removed from, by placing 1 of your opponent's Digimon with the lowest DP as the bottom security card, trash their top security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card)
                     && (CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => player == card.Owner)
                     || CardEffectCommons.CanTriggerWhenOwnerUseOption(hashtable, null, null, card)))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                     && CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition) >= 1)
                    {
                        return true;
                    }
                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            if (!permanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                if (CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;
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

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place in security", "The opponent is selecting 1 Digimon place in security.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            if (selectedPermanent.TopCard.Owner.CanAddSecurity(activateClass))
                            {
                                CardSource topCard = selectedPermanent.TopCard;

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlacePermanentInSecurityAndProcessAccordingToResult(
                                    targetPermanent: selectedPermanent,
                                    activateClass: activateClass,
                                    toTop: false,
                                    SuccessProcess));

                                IEnumerator SuccessProcess(CardSource cardSource)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                        player: card.Owner.Enemy,
                                        destroySecurityCount: 1,
                                        cardEffect: activateClass,
                                        fromTop: true).DestroySecurity());
                                }
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
