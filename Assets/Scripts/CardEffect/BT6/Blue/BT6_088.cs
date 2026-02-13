using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT6_088 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnMove)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Memory +1 and Draw 1", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Your Turn] When one of your Digimon with [Gabumon] or [Garurumon] in its name moves from the breeding area to the battle area, gain 1 memory and trigger <Draw 1>. (Draw 1 card from your deck.)";
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent.TopCard.ContainsCardName("Gabumon"))
                    {
                        return true;
                    }

                    if (permanent.TopCard.HasGarurumonName)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerOnMove(hashtable, PermanentCondition))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.CanAddMemory(activateClass))
                    {
                        return true;
                    }

                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
            }
        }

        if (timing == EffectTiming.OnDeclaration)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Digivolve your [Gabumon] into [Gabumon - Bond of Friendship]", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDiscription());
            activateClass.SetHashString("Digivolution_BT6_087");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main][Once Per Turn] Digivolve your [Gabumon] into 1 [Gabumon - Bond of Friendship] in your hand for its digivolution cost, ignoring its level. If you do, trash the top 2 cards of your security stack. Then, if you have 1 or more security cards, delete that Digimon at the end of the turn.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                {
                    if (permanent.TopCard.CardNames.Contains("Gabumon"))
                    {
                        foreach (CardSource cardSource in card.Owner.HandCards)
                        {
                            if (CanSelectCardCondition(cardSource))
                            {
                                if (!cardSource.CanNotEvolve(permanent))
                                {
                                    if (card.Owner.MaxMemoryCost >= cardSource.PayingCost(SelectCardEffect.Root.Hand, new List<Permanent>() { permanent }, checkAvailability: true, ignoreLevel: true))
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

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.EqualsCardName("Gabumon - Bond of Friendship");
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        return true;
                    }
                }

                return false;
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
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 [Gabumon] that will digivolve.", "The opponent is selecting 1 [Gabumon] that will digivolve.");

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
                            payCost: true,
                            reduceCostTuple: null,
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: SuccessProcess(),
                            ignoreRequirements: CardEffectCommons.IgnoreRequirement.Level));

                        IEnumerator SuccessProcess()
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                player: card.Owner,
                                destroySecurityCount: 2,
                                cardEffect: activateClass,
                                fromTop: true).DestroySecurity());

                            if (card.Owner.SecurityCards.Count >= 1)
                            {
                                ActivateClass activateClass1 = new ActivateClass();
                                activateClass1.SetUpICardEffect("Delete the Digimon", CanUseCondition1, card);
                                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, "");
                                activateClass1.SetEffectSourcePermanent(card.PermanentOfThisCard());
                                CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: activateClass1, timing: EffectTiming.OnEndTurn);

                                bool CanUseCondition1(Hashtable hashtable)
                                {
                                    return true;
                                }

                                bool CanActivateCondition1(Hashtable hashtable)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                    {
                                        if (selectedPermanent.CanBeDestroyedBySkill(activateClass1))
                                        {
                                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass1))
                                            {
                                                return true;
                                            }
                                        }
                                    }

                                    return false;
                                }

                                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(new List<Permanent>() { selectedPermanent }, CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                                }
                            }
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
        }

        return cardEffects;
    }
}
