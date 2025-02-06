using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX7
{
    public class EX7_067 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return
                        "[Main] trash the top 2 digivolution cards of all of your opponent's Digimon. If this effect didn't trash, you may play 1 level 4 or lower Digimon card with the [Ice-Snow] trait from your hand without paying the cost. Then, none of their Digimon with no digivolution cards can attack until the end of their turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DigivolutionCards.Count((cardSource) =>
                                !cardSource.CanNotTrashFromDigivolutionCards(activateClass)) >= 1)
                        {
                            if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                return true;
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.HasNoDigivolutionCards)
                        {
                            if (permanent.CanSelectBySkill(activateClass))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
                
                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon && cardSource.EqualsTraits("Ice-Snow") && cardSource.Level <= 4)
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        foreach (Permanent selectedPermanent in card.Owner.Enemy.GetBattleAreaDigimons())
                        {
                            if (CanSelectPermanentCondition(selectedPermanent))
                            {
                                if (selectedPermanent != null)
                                {
                                    if (selectedPermanent.DigivolutionCards.Count >= 1)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(
                                            CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                                                targetPermanent: selectedPermanent, trashCount: 2, isFromTop: true,
                                                activateClass: activateClass));
                                    }
                                }
                            }
                        }
                    }

                    else
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
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

                            selectHandEffect.SetUpCustomMessage("Select a digimon to play.", "The opponent is selecting a card to play.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Played Cards");

                            yield return StartCoroutine(selectHandEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);

                                yield return null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: selectedCards,
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Hand,
                                activateETB: true));
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.GainCanNotAttackPlayerEffect(
                            attackerCondition: CanSelectPermanentCondition1,
                            defenderCondition: null,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            effectName: "Can't Attack"));

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.GainCanNotBlockPlayerEffect(
                            attackerCondition: CanSelectPermanentCondition1,
                            defenderCondition: null,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            effectName: "Can't Block"));
                }
            }
            
            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Trash top two digivolution cards or play a digimon, and opponent's digimon can't attack or block");
            }
            
            return cardEffects;
        }
    }
}