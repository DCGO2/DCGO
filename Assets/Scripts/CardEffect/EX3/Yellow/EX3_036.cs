using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX3
{
    public class EX3_036 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Opponent's Digimons reduce Security Attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] All of your opponent's Digimon gain <Security Attack -1> until the end of your opponent's turn. (This Digimon checks 1 fewer security cards.) If this card was played by [Trial of the Four Great Dragons]'s effect, all of your opponent's Digimon gain <Security Attack -2> until the end of your opponent's turn instead.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    int minusPoint = 1;

                    bool CardEffectCondition(ICardEffect cardEffect)
                    {
                        if (cardEffect != null)
                        {
                            if (cardEffect.EffectSourceCard != null)
                            {
                                if (cardEffect.EffectSourceCard.CardNames.Contains("Trial of the Four Great Dragons")
                                || cardEffect.EffectSourceCard.CardNames.Contains("TrialoftheFourGreatDragons"))
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }

                    if (CardEffectCommons.IsByEffect(_hashtable, CardEffectCondition))
                    {
                        minusPoint = 2;
                    }

                    bool PermanentCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                        {
                            if (!permanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttackPlayerEffect(
                permanentCondition: PermanentCondition,
                changeValue: -minusPoint,
                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                activateClass: activateClass));
                }
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Trial of the Four Great Dragons] in battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] If you don't have a [Trial of the Four Great Dragons] in play, you may place 1 [Trial of the Four Great Dragons] from your hand in your battle area.";
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
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnTrash(card))
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

            return cardEffects;
        }
    }
}