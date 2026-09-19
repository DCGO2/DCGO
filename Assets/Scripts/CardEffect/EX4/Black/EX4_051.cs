using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.EX4
{
    public class EX4_051 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("MetalGreymon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Choose 1 effect", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Activate 1 of the effects below. - <De-Digivolve 1> 3 of your opponent's Digimon. - 1 of your other Digimon digivolves into a level 6 or lower Digimon card with [Garurumon] in its name in your hand without paying the cost. -This Digimon and one of your other Digimon may DNA digivolve into a Digimon card in your hand for the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
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
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                            {
                                new SelectionElement<int>(message: $"De-Digivolve", value : 0, spriteIndex: 0),
                                new SelectionElement<int>(message: $"Your other 1 Digimon digivolves", value : 1, spriteIndex: 0),
                                new SelectionElement<int>(message: $"DNA Digivolution", value : 2, spriteIndex: 0),
                            };

                            string selectPlayerMessage = "Which effect will you activate?";
                            string notSelectPlayerMessage = "The opponent is choosing which effect to activate.";

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            int actionID = GManager.instance.userSelectionManager.SelectedIntValue;

                            switch (actionID)
                            {
                                case 0:
                                    bool CanSelectPermanentCondition(Permanent permanent)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                                        {
                                            if (permanent.CanSelectBySkill(activateClass))
                                            {
                                                return true;
                                            }
                                        }

                                        return false;
                                    }

                                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                                    {
                                        int maxCount = Math.Min(3, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                        selectPermanentEffect.SetUp(
                                            selectPlayer: card.Owner,
                                            canTargetCondition: CanSelectPermanentCondition,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: CanEndSelectCondition,
                                            maxCount: maxCount,
                                            canNoSelect: false,
                                            canEndNotMax: false,
                                            selectPermanentCoroutine: SelectPermanentCoroutine,
                                            afterSelectPermanentCoroutine: null,
                                            mode: SelectPermanentEffect.Mode.Custom,
                                            cardEffect: activateClass);

                                        selectPermanentEffect.SetUpCustomMessage("Select Digimon to De-Digivolve.", "The opponent is selecting Digimon to De-Digivolve.");

                                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                        bool CanEndSelectCondition(List<Permanent> permanents)
                                        {
                                            if (permanents.Count <= 0)
                                            {
                                                return false;
                                            }

                                            return true;

                                        }

                                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                        {
                                            Permanent selectedPermanent = permanent;

                                            if (selectedPermanent != null)
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(new IDegeneration(selectedPermanent, 1, activateClass).Degeneration());
                                            }

                                            yield return null;
                                        }
                                    }
                                    break;

                                case 1:
                                    bool CanSelectPermanentCondition1(Permanent permanent)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                                        {
                                            if (permanent != card.PermanentOfThisCard())
                                            {
                                                foreach (CardSource cardSource in card.Owner.HandCards)
                                                {
                                                    if (CanSelectCardCondition(cardSource))
                                                    {
                                                        if (cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass))
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    bool CanSelectCardCondition(CardSource cardSource)
                                    {
                                        return cardSource.IsDigimon && cardSource.HasGarurumonName && cardSource.Level <= 6 && cardSource.HasLevel;
                                    }

                                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                                    {
                                        Permanent selectedPermanent = null;

                                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                        selectPermanentEffect.SetUp(
                                            selectPlayer: card.Owner,
                                            canTargetCondition: CanSelectPermanentCondition1,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: null,
                                            maxCount: maxCount,
                                            canNoSelect: true,
                                            canEndNotMax: false,
                                            selectPermanentCoroutine: SelectPermanentCoroutine,
                                            afterSelectPermanentCoroutine: null,
                                            mode: SelectPermanentEffect.Mode.Custom,
                                            cardEffect: activateClass);

                                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will digivolve.", "The opponent is selecting 1 Digimon that will digivolve.");

                                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                        {
                                            selectedPermanent = permanent;

                                            yield return null;
                                        }

                                        if (selectedPermanent != null)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                                targetPermanent: selectedPermanent,
                                                cardCondition: CanSelectCardCondition,
                                                payCost: false,
                                                reduceCostTuple: null,
                                                fixedCostTuple: null,
                                                ignoreDigivolutionRequirementFixedCost: -1,
                                                isHand: true,
                                                activateClass: activateClass,
                                                successProcess: null));
                                        }
                                    }
                                    break;

                                case 2:
                                    bool CanSelectCardCondition1(CardSource cardSource)
                                    {
                                        if (cardSource != null)
                                        {
                                            if (cardSource.IsDigimon)
                                            {
                                                if (cardSource.Owner == card.Owner)
                                                {
                                                    if (cardSource.CanPlayJogress(true))
                                                    {
                                                        if (isExistOnField(card))
                                                        {
                                                            if (cardSource.CanJogressFromTargetPermanent(card.PermanentOfThisCard(), true))
                                                            {
                                                                return true;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    if (card.Owner.HandCards.Count(CanSelectCardCondition1) >= 1)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(
                                                                 CardEffectCommons.DNADigivolvePermanentsIntoHandOrTrashCard(
                                                                 CanSelectCardCondition1,
                                                                 payCost: true,
                                                                 isHand: true,
                                                                 activateClass,
                                                                 permanentConditions: new Func<Permanent, bool>[] { (permanent) => permanent == card.PermanentOfThisCard() }));
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash the top card of opponent's security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("TrashSecurity_EX4_051");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking][Once Per Turn] If this Digimon has [Omnimon] in its name, trash the top card of your opponent's security stack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.ContainsCardName("Omnimon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }

            return cardEffects;
        }
    }
}