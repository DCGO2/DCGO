using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class P_073 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.TopCard.CardNames.Contains("WereGarurumon");
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
        }

        if (timing == EffectTiming.None)
        {
            ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
            changeCardNamesClass.SetUpICardEffect("Also treated as [WereGarurumon]", CanUseCondition, card);
            changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);

            cardEffects.Add(changeCardNamesClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return true;
            }



            List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
            {
                if (cardSource == card)
                {
                    CardNames.Add("WereGarurumon");
                }

                return CardNames;
            }
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Return level 3 Digimons to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] If you have a Tamer in play, return 2 of your opponentÅfs level 3 Digimon to their ownersÅfands.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.TopCard.HasLevel)
                    {
                        if (permanent.Level == 3)
                        {
                            return true;
                        }
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
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsTamer))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (isExistOnField(card))
                {
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                    {
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
                                mode: SelectPermanentEffect.Mode.Bounce,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Prevent this Digimon from being deleted", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            activateClass.SetIsInheritedEffect(true);
            activateClass.SetHashString("Substitute_P_073");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[All Turns] If this Digimon has [Garurumon] or [Omnimon] in its name and would be deleted in battle, you may trash 2 cards of the same level from this DigimonÅf digivolution cards to prevent the deletion.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource != null)
                {
                    if (cardSource.Owner == card.Owner)
                    {
                        if (!cardSource.CanNotTrashFromDigivolutionCards(activateClass))
                        {
                            if (isExistOnField(card))
                            {
                                if (card.PermanentOfThisCard().DigivolutionCards.Contains(card))
                                {
                                    foreach (CardSource cardSource1 in card.PermanentOfThisCard().DigivolutionCards)
                                    {
                                        if (cardSource != cardSource1)
                                        {
                                            if (cardSource.Level == cardSource1.Level)
                                            {
                                                if (!cardSource1.CanNotTrashFromDigivolutionCards(activateClass))
                                                {
                                                    if (cardSource.HasLevel && cardSource1.HasLevel)
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (isExistOnField(card))
                {
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                    {
                        if (hashtable != null)
                        {
                            if (hashtable.ContainsKey("Permanents"))
                            {
                                if (hashtable["Permanents"] is List<Permanent>)
                                {
                                    List<Permanent> permanents = (List<Permanent>)hashtable["Permanents"];

                                    if (permanents != null)
                                    {
                                        if (permanents.Count((permanent) => permanent == card.PermanentOfThisCard()) >= 1)
                                        {
                                            if (hashtable.ContainsKey("battle"))
                                            {
                                                if (hashtable["battle"] is IBattle)
                                                {
                                                    IBattle battle = (IBattle)hashtable["battle"];

                                                    if (battle != null)
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (isExistOnField(card))
                {
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                    {
                        if (card.PermanentOfThisCard().TopCard.HasGarurumonName || card.PermanentOfThisCard().TopCard.ContainsCardName("Omnimon"))
                        {
                            if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 2)
                            {
                                List<CardSource> canSelectCards = new List<CardSource>();

                                foreach (CardSource cardSource in card.PermanentOfThisCard().DigivolutionCards)
                                {
                                    canSelectCards.Add(cardSource);
                                }

                                if (canSelectCards.Count >= 2)
                                {
                                    List<CardSource[]> cardsList = ParameterComparer.Enumerate(canSelectCards, 2).ToList();

                                    foreach (CardSource[] cardSources in cardsList)
                                    {
                                        if (cardSources.Length == 2)
                                        {
                                            if (cardSources[0].Level == cardSources[1].Level)
                                            {
                                                if (cardSources[0].HasLevel && cardSources[1].HasLevel)
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (isExistOnField(card))
                {
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 2)
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = 2;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition,
                                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                        canEndSelectCondition: CanEndSelectCondition,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select cards to discard.",
                                        maxCount: maxCount,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Custom,
                                        customRootCardList: selectedPermanent.DigivolutionCards,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            selectCardEffect.SetNotShowCard();
                            yield return StartCoroutine(selectCardEffect.Activate());

                            bool CanEndSelectCondition(List<CardSource> cardSources)
                            {
                                if (maxCount >= 1)
                                {
                                    if (cardSources.Count <= 0)
                                    {
                                        return false;
                                    }
                                }

                                List<int> levels = new List<int>();

                                foreach (CardSource cardSource1 in cardSources)
                                {
                                    if (!levels.Contains(cardSource1.Level))
                                    {
                                        levels.Add(cardSource1.Level);
                                    }
                                }

                                levels = levels.Distinct().ToList();

                                if (levels.Count > 1)
                                {
                                    return false;
                                }

                                return true;
                            }

                            bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                            {
                                List<int> levels = new List<int>();

                                foreach (CardSource cardSource1 in cardSources)
                                {
                                    if (!levels.Contains(cardSource1.Level))
                                    {
                                        levels.Add(cardSource1.Level);
                                    }
                                }

                                if (!levels.Contains(cardSource.Level))
                                {
                                    levels.Add(cardSource.Level);
                                }

                                levels = levels.Distinct().ToList();

                                if (levels.Count > 1)
                                {
                                    return false;
                                }

                                return true;
                            }

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);
                                yield return null;
                            }

                            if (selectedCards.Count >= 1)
                            {
                                if (selectedCards.Count == 2)
                                {
                                    selectedPermanent.willBeRemoveField = false;
                                }

                                yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(selectedPermanent, selectedCards, activateClass).TrashDigivolutionCards());
                            }
                        }
                    }
                }
            }
        }

        return cardEffects;
    }
}
