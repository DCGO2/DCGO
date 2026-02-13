using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class ST10_12 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Trash 1 card from hand to reveal the top 3 cards of deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] You may trash 1 card in your hand to reveal the top 3 cards of your deck. Add 1 yellow and 1 purple card with [Angel], [Archangel], or [Fallen Angel] in their traits among them to your hand. Place the remaining cards at the bottom of your deck in any order.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.CardColors.Contains(CardColor.Yellow))
                {
                    if (cardSource.CardTraits.Contains("Angel"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("Archangel"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("Fallen Angel"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("FallenAngel"))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                if (cardSource.CardColors.Contains(CardColor.Purple))
                {
                    if (cardSource.CardTraits.Contains("Angel"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("Archangel"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("Fallen Angel"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("FallenAngel"))
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
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.HandCards.Count >= 1)
                {
                    bool discarded = false;

                    int discardCount = 1;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: (cardSource) => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: discardCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count >= 1)
                        {
                            discarded = true;

                            yield return null;
                        }
                    }

                    if (discarded)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                            revealCount: 3,
                            simplifiedSelectCardConditions:
                            new SimplifiedSelectCardConditionClass[]
                            {
                                new SimplifiedSelectCardConditionClass(
                                    canTargetCondition:CanSelectCardCondition,
                                    message:  "Select 1 yellow card with [Angel], [Archangel], or [Fallen Angel] in its traits.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null),
                                new SimplifiedSelectCardConditionClass(
                                    canTargetCondition:CanSelectCardCondition1,
                                    message: "Select 1 purple card with [Angel], [Archangel], or [Fallen Angel] in its traits.",
                                    mode: SelectCardEffect.Mode.AddHand,
                                    maxCount: 1,
                                    selectCardCoroutine: null),
                            },
                            remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                            activateClass: activateClass
                            ));
                    }
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            AddSkillClass addSkillClass = new AddSkillClass();
            addSkillClass.SetUpICardEffect("Your yellow Digimons gain Retaliation", CanUseCondition, card);
            addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
            addSkillClass.SetIsInheritedEffect(true);
            cardEffects.Add(addSkillClass);

            bool CanUseCondition(Hashtable hashtable)
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

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    return true;
                }

                return false;
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                if (PermanentCondition(cardSource.PermanentOfThisCard()))
                {
                    if (cardSource == cardSource.PermanentOfThisCard().TopCard)
                    {
                        return true;
                    }
                }

                return false;
            }

            List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
            {
                if (_timing == EffectTiming.OnDestroyedAnyone)
                {
                    bool Condition()
                    {
                        if (CardSourceCondition(cardSource))
                        {
                            if (cardSource.PermanentOfThisCard().TopCard.CardColors.Contains(CardColor.Yellow))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(isInheritedEffect: false, card: cardSource, condition: Condition));
                }

                return cardEffects;
            }
        }

        return cardEffects;
    }
}
