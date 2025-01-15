using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class RB1_023 : CEntity_Effect
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

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Suspend 1 Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Suspend 1 of your opponent's Digimon with DP less than or equal to this Digimon's DP. Digimon suspended by this effect donÅft unsuspend until the end of your opponentÅfturn.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (permanent.DP <= card.PermanentOfThisCard().DP)
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
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                {
                    int maxCount = 1;

                    if (card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) < maxCount)
                    {
                        maxCount = card.Owner.Enemy.GetBattleAreaPermanents().Count(CanSelectPermanentCondition);
                    }

                    foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaPermanents())
                    {
                        permanent.oldIsTapped_playCard = permanent.IsSuspended;
                    }

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
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        foreach (Permanent selectedPermanent in permanents)
                        {
                            if (selectedPermanent != null)
                            {
                                if (selectedPermanent.IsSuspended)
                                {
                                    if (!selectedPermanent.oldIsTapped_playCard)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendUntilOpponentTurnEnd(
                                            targetPermanent: selectedPermanent,
                                            activateClass: activateClass
                                        ));
                                    }
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
