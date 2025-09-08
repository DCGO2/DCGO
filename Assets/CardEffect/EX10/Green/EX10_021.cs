using System.Collections;
using System.Collections.Generic;
using System;

// Belphemon: Sleep Mode
namespace DCGO.CardEffects.EX10
{
    public class EX10_021 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Requirements

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Belphemon: Rage Mode");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 card from trash to digivolution cards so that this Digimon gets effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By placing 1 [Belphemon: Rage Mode] from your trash as this Digimon's top digivolution card, until the end of your opponent's turn, this Digimon can't attack and isn't affected by your opponent's effects.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardNames.Contains("Belphemon: Rage Mode"))
                    {
                        return true;
                    }

                    if (cardSource.CardNames.Contains("Belphemon:RageMode"))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (!card.PermanentOfThisCard().IsToken)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        bool added = false;

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to place on bottom of digivolution cards.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");
                            selectCardEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsTop(selectedCards, activateClass));

                                added = true;
                            }
                        }

                        if (added)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotAttack(
                                targetPermanent: card.PermanentOfThisCard(),
                                defenderCondition: null,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                effectName: "Can't Attack"));

                            if (CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                Permanent selectedPermanent = card.PermanentOfThisCard();

                                if (selectedPermanent != null)
                                {
                                    CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                                    canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's effect", CanUseCondition1, card);
                                    canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                                    selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => canNotAffectedClass);

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));

                                    bool CanUseCondition1(Hashtable hashtable)
                                    {
                                        if (selectedPermanent.TopCard != null)
                                        {
                                            return true;
                                        }

                                        return false;
                                    }

                                    bool CardCondition(CardSource cardSource)
                                    {
                                        if (selectedPermanent.TopCard != null)
                                        {
                                            if (selectedPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(selectedPermanent))
                                            {
                                                if (cardSource == selectedPermanent.TopCard)
                                                {
                                                    return true;
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    bool SkillCondition(ICardEffect cardEffect)
                                    {
                                        if (cardEffect != null)
                                        {
                                            if (cardEffect.EffectSourceCard != null)
                                            {
                                                if (cardEffect.EffectSourceCard.Owner == card.Owner.Enemy)
                                                {
                                                    return true;
                                                }
                                            }
                                        }

                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 card from trash to digivolution cards so that this Digimon gets effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By placing 1 [Belphemon: Rage Mode] from your trash as this Digimon's top digivolution card, until the end of your opponent's turn, this Digimon can't attack and isn't affected by your opponent's effects.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardNames.Contains("Belphemon: Rage Mode"))
                    {
                        return true;
                    }

                    if (cardSource.CardNames.Contains("Belphemon:RageMode"))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (!card.PermanentOfThisCard().IsToken)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        bool added = false;

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to place on bottom of digivolution cards.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");
                            selectCardEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsTop(selectedCards, activateClass));

                                added = true;
                            }
                        }

                        if (added)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotAttack(
                                targetPermanent: card.PermanentOfThisCard(),
                                defenderCondition: null,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                effectName: "Can't Attack"));

                            if (CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                Permanent selectedPermanent = card.PermanentOfThisCard();

                                if (selectedPermanent != null)
                                {
                                    CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                                    canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's effect", CanUseCondition1, card);
                                    canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                                    selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => canNotAffectedClass);

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));

                                    bool CanUseCondition1(Hashtable hashtable)
                                    {
                                        if (selectedPermanent.TopCard != null)
                                        {
                                            return true;
                                        }

                                        return false;
                                    }

                                    bool CardCondition(CardSource cardSource)
                                    {
                                        if (selectedPermanent.TopCard != null)
                                        {
                                            if (selectedPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(selectedPermanent))
                                            {
                                                if (cardSource == selectedPermanent.TopCard)
                                                {
                                                    return true;
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    bool SkillCondition(ICardEffect cardEffect)
                                    {
                                        if (cardEffect != null)
                                        {
                                            if (cardEffect.EffectSourceCard != null)
                                            {
                                                if (cardEffect.EffectSourceCard.Owner == card.Owner.Enemy)
                                                {
                                                    return true;
                                                }
                                            }
                                        }

                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Opponent's Turn - OPT

            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 2, Suspend 2", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("AT_EX10-021");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn] [Once Per Turn] When any of your opponent's Digimon suspend, by trashing 2 cards in your hand, suspend 2 of their Digimon or Tamers.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOpponentTurn(card) &&
                           CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, OpponentsDigimon);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.Owner.HandCards.Count >= 2;
                }

                bool OpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card) &&
                           (permanent.IsDigimon || permanent.IsTamer);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int discardCount = 2;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: (cardSource) => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: discardCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}