using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// Meat
namespace DCGO.CardEffects.EX9
{
    public class EX9_070 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region ignoring colours

            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return card.Owner.GetFieldPermanents().Some(permanet =>
                        permanet.TopCard.HasDMTraits &&
                        !permanet.TopCard.IsOption);
                }

                bool CardCondition(CardSource cardSource)
                    => cardSource == card;
            }

            #endregion

            #region Main/Secuirty Shared

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.LibraryCards.Count >= 1) yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
            }

            #endregion

            #region Main

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1, then place in battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] <Draw 1>. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }
            }

            #endregion

            #region Delay

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 face down in sources, digivolve into [DM] trait Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] <Delay> (After this card is placed, by trashing it the next turn or later, activate the effect below). By placing 1 card from your hand face down as any of your [DM] trait Digimon's bottom digivolution card, it may digivolve into a [DM] trait Digimon card in the hand with the digivolution cost reduced by 2.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanDeclareOptionDelayEffect(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasDMTraits)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        CardSource selectedCard = null;

                        if (card.Owner.HandCards.Count > 0)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: (cardSource) => true,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: false,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectHandEffect.Activate());

                            selectHandEffect.SetUpCustomMessage("Select a card to add face down as a source", "Opponent is selecting a card to add face down as a source");

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCard = cardSource;
                                yield return null;
                            }
                        }

                        if (selectedCard != null)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentCondition))
                            {
                                Permanent selectedPermanent = null;

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
                                    cardEffect: activateClass
                                );

                                selectPermanentEffect.SetUpCustomMessage("Select a Digimon to add source to", "Opponent is selecting a Digimon to add source to");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    selectedPermanent = permanent;
                                    yield return null;
                                }

                                if (selectedPermanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddExecutingCard(selectedCard));
                                    yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass, isFacedown: true));

                                    bool CanSelectDigivovleCondition(CardSource card)
                                    {
                                        return card.HasDMTraits &&
                                               card.CanPlayCardTargetFrame(selectedPermanent.PermanentFrame, true, activateClass);
                                    }

                                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectDigivovleCondition))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                            targetPermanent: selectedPermanent,
                                            cardCondition: CanSelectDigivovleCondition,
                                            payCost: true,
                                            reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
                                            fixedCostTuple: null,
                                            ignoreDigivolutionRequirementFixedCost: -1,
                                            isHand: true,
                                            activateClass: activateClass,
                                            successProcess: null));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1, then place in battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] <Draw 1>. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }
            }

            #endregion

            return cardEffects;
        }
    }
}