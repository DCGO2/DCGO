using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT9_109 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
            ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
            ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
            cardEffects.Add(ignoreColorConditionClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return card.Owner.GetBattleAreaDigimons().Count >= 1;
            }

            bool CardCondition(CardSource cardSource)
            {
                return cardSource == card;
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect($"Memory +1 and add this card to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] Gain 1 memory, and add this card to its owner's hand. ";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
            }
        }

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] Place this card under 1 of your Digimon without [X Antibody] in its digivolution cards as its bottom digivolution card.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent.DigivolutionCards.Count((cardSource) => cardSource.CardNames.Contains("X Antibody") || cardSource.CardNames.Contains("XAntibody")) == 0)
                    {
                        return true;
                    }
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
                    Permanent selectedPermanent = null;

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get a digivolution card.", "The opponent is selecting 1 Digimon that will get a digivolution card.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;   
                    }

                    if (selectedPermanent != null && !selectedPermanent.IsToken)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.CloseBrainstrorm(card));

                        yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { card }, activateClass));
                    }
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            CanNotTrashFromDigivolutionCardsClass canNotTrashFromDigivolutionCardsClass = new CanNotTrashFromDigivolutionCardsClass();
            canNotTrashFromDigivolutionCardsClass.SetUpICardEffect("[X Antibody] under this Digimon can't be trashed", CanUseCondition, card);
            canNotTrashFromDigivolutionCardsClass.SetUpCanNotTrashFromDigivolutionCardsClass(CardCondition: CardCondition, CardEffectCondition: CardEffectCondition);
            canNotTrashFromDigivolutionCardsClass.SetIsInheritedEffect(true);
            cardEffects.Add(canNotTrashFromDigivolutionCardsClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return true;
                }

                return false;
            }

            bool CardCondition(CardSource cardSource)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (cardSource.CardNames.Contains("X Antibody") || cardSource.CardNames.Contains("XAntibody"))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CardEffectCondition(ICardEffect cardEffect)
            {
                return cardEffect != null;
            }
        }

        if (timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Digivolve this Digimon into [X Antibody] Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            activateClass.SetIsInheritedEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Attacking] This Digimon can digivolve into a Digimon card with [X Antibody] in its traits in your hand for its digivolution cost.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.HasXAntibodyTraits)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            if (cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, true, activateClass, root: SelectCardEffect.Root.Hand))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
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

                    selectHandEffect.SetUpCustomMessage("Select 1 card to digivolve.", "The opponent is selecting 1 card to digivolve.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (selectedCards.Count >= 1)
                    {
                        CardSource selectedCard = selectedCards[0];

                        if (CardEffectCommons.IsExistOnBattleArea(card))
                        {
                            yield return ContinuousController.instance.StartCoroutine(new PlayCardClass(
                                cardSources: new List<CardSource>() { selectedCard },
                                hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                payCost: true,
                                targetPermanent: card.PermanentOfThisCard(),
                                isTapped: false,
                                root: SelectCardEffect.Root.Hand,
                                activateETB: true).PlayCard());
                        }
                    }
                }
            }
        }

        return cardEffects;
    }
}
