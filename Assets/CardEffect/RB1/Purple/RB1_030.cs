using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class RB1_030 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.TopCard.ContainsCardName("Gammamon") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4;
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
        }

        if (timing == EffectTiming.OnEnterFieldAnyone || timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Trash 1 card from hand and your 1 Digimon gains effects", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
            activateClass.SetHashString("Discard_RB1_030");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                if (timing == EffectTiming.OnEnterFieldAnyone)
                {
                    return "[When Digivolving][Once Per Turn] By trashing 1 card with [Gammamon] in its text in your hand,1 of your Digimon gains \"[On Deletion] Delete 1 of your opponent's Digimon with the lowest level\" until the end of your opponent's turn.";
                }

                else
                {
                    return "[When Attacking][Once Per Turn] By trashing 1 card with [Gammamon] in its text in your hand, 1 of your Digimon gains \"[On Deletion] Delete 1 of your opponent's Digimon with the lowest level\" until the end of your opponent's turn.";
                }
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.HasText("Gammamon");
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (timing == EffectTiming.OnEnterFieldAnyone)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                else
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }
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
                bool discarded = false;

                if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                {
                    int discardCount = 1;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
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
                }

                if (discarded)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = 1;

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

                        selectPermanentEffect.SetUpCustomMessage("Selcect 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            if (selectedPermanent != null)
                            {
                                CardSource _topCard = selectedPermanent.TopCard;

                                ActivateClass activateClass1 = new ActivateClass();
                                activateClass1.SetUpICardEffect("Delete 1 Digimon with the lowest level", CanUseCondition1, selectedPermanent.TopCard);
                                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                                activateClass1.SetEffectSourcePermanent(selectedPermanent);
                                CardEffectCommons.AddEffectToPermanent(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilOpponentTurnEnd, card: card, cardEffect: activateClass1, timing: EffectTiming.OnDestroyedAnyone);

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));

                                string EffectDiscription1()
                                {
                                    return "[On Deletion] Delete 1 of your opponentfDigimon with the lowest level.";
                                }

                                bool CanSelectPermanentCondition1(Permanent permanent)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                                    {
                                        if (CardEffectCommons.IsMinLevel(permanent, _topCard.Owner.Enemy))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                bool CanUseCondition1(Hashtable hashtable1)
                                {
                                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                    {
                                        if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable1, (permanent) => permanent == selectedPermanent))
                                        {
                                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                            {
                                                return true;
                                            }
                                        }
                                    }

                                    return false;
                                }

                                bool CanActivateCondition1(Hashtable hashtable1)
                                {
                                    if (CardEffectCommons.IsTopCardInTrashOnDeletion(hashtable1))
                                    {
                                        if (_topCard.Owner.Enemy.GetBattleAreaDigimons().Count(CanSelectPermanentCondition1) >= 1)
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                {
                                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: _topCard.Owner,
                                        canTargetCondition: CanSelectPermanentCondition1,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: null,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Destroy,
                                        cardEffect: activateClass);

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                                }
                            }
                        }
                    }
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            AddSkillClass addSkillClass = new AddSkillClass();
            addSkillClass.SetUpICardEffect("This Digimon gains all effects of [Gammamon] in digivolution cards", CanUseCondition, card);
            addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);

            cardEffects.Add(addSkillClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            bool PermanentCondition(Permanent permanent)
            {
                return permanent == card.PermanentOfThisCard();
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
                if (CardSourceCondition(cardSource))
                {
                    foreach (CardSource cardSource1 in cardSource.PermanentOfThisCard().DigivolutionCards)
                    {
                        if (cardSource1.ContainsCardName("Gammamon"))
                        {
                            foreach (ICardEffect cardEffect in cardSource1.cEntity_EffectController.GetCardEffects_ExceptAddedEffects(_timing, card))
                            {
                                if (!cardEffect.IsSecurityEffect && !cardEffect.IsInheritedEffect)
                                {
                                    cardEffects.Add(cardEffect);
                                }
                            }
                        }
                    }
                }

                return cardEffects;
            }
        }

        if (timing == EffectTiming.None)
        {
            AddSkillClass addSkillClass = new AddSkillClass();
            addSkillClass.SetUpICardEffect("This Digimon gains all effects of [Gammamon] in digivolution cards", CanUseCondition, card);
            addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
            addSkillClass.SetIsInheritedEffect(true);
            cardEffects.Add(addSkillClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            bool PermanentCondition(Permanent permanent)
            {
                return permanent == card.PermanentOfThisCard();
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
                if (CardSourceCondition(cardSource))
                {
                    foreach (CardSource cardSource1 in cardSource.PermanentOfThisCard().DigivolutionCards)
                    {
                        if (cardSource1.ContainsCardName("Gammamon"))
                        {
                            foreach (ICardEffect cardEffect in cardSource1.cEntity_EffectController.GetCardEffects_ExceptAddedEffects(_timing, card))
                            {
                                if (!cardEffect.IsSecurityEffect && !cardEffect.IsInheritedEffect)
                                {
                                    cardEffects.Add(cardEffect);
                                }
                            }
                        }
                    }
                }

                return cardEffects;
            }
        }

        return cardEffects;
    }
}
