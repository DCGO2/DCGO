using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX3
{
    public class EX3_033 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Trial of the Four Great Dragons] in battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If you don't have a [Trial of the Four Great Dragons] in play, you may place 1 [Trial of the Four Great Dragons] from your hand in your battle area.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardNames.Contains("Trial of the Four Great Dragons") || cardSource.CardNames.Contains("TrialoftheFourGreatDragons"))
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass, isPlayOption: true))
                        {
                            return true;
                        }
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
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            if (card.Owner.GetBattleAreaPermanents().Count((permanent) =>
                            permanent.TopCard.CardNames.Contains("Trial of the Four Great Dragons")
                            || permanent.TopCard.CardNames.Contains("TrialoftheFourGreatDragons")) == 0)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 card to place in battle area.", "The opponent is selecting 1 card to place in battle area.");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        if (selectedCards.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(
                                            card: selectedCards[0],
                                            cardEffect: activateClass,
                                            root: SelectCardEffect.Root.Hand));
                        }
                    }
                }
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && (permanent.TopCard.CardTraits.Contains("Four Great Dragons") || permanent.TopCard.CardTraits.Contains("FourGreatDragons"))))
                            {
                                return true;
                            }

                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.TopCard.CardNames.Contains("Trial of the Four Great Dragons") || permanent.TopCard.CardNames.Contains("TrialoftheFourGreatDragons")))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: Condition));
            }

            if (timing == EffectTiming.None)
            {
                bool CanUseCondition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardTraits.Contains("Four Great Dragons"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardTraits.Contains("FourGreatDragons"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.BlockerStaticEffect(permanentCondition: PermanentCondition, isInheritedEffect: true, card: card, condition: CanUseCondition));
            }

            return cardEffects;
        }
    }
}