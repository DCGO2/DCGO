using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT11
{
    public class BT11_093 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("The digivolved Digimon gets DP +2000 and effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When one of your Digimon digivolves into a Digimon with [Greymon] in its name, by suspending this Tamer, that Digimon gets +2000 DP until the end of your opponent's turn. If it digivolved into a Digimon with the same level, that Digimon isn't affected by your opponent's Option cards until the end of your opponent's turn.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasGreymonName)
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
                            if (CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition))
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
                        if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    List<Permanent> permanents = CardEffectCommons.GetPlayedPermanentsFromEnterFieldHashtable(
                        hashtable: _hashtable,
                        rootCondition: null);

                    if (permanents != null)
                    {
                        foreach (Permanent selectedPermanent in permanents)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: selectedPermanent, changeValue: 2000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));

                                if (CardEffectCommons.IsDigivolvedFromSameLevelFromEnterFieldHashtable(_hashtable, selectedPermanent))
                                {
                                    CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                                    canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's option", CanUseCondition1, card);
                                    canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                                    selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => canNotAffectedClass);

                                    bool CanUseCondition1(Hashtable hashtable)
                                    {
                                        if (selectedPermanent.TopCard != null)
                                        {
                                            return true;
                                        }

                                        return false;
                                    }

                                    bool CardCondition(CardSource cardSource)
                                    {
                                        if (selectedPermanent.TopCard != null)
                                        {
                                            if (selectedPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(selectedPermanent))
                                            {
                                                if (cardSource == selectedPermanent.TopCard)
                                                {
                                                    return true;
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    bool SkillCondition(ICardEffect cardEffect)
                                    {
                                        if (cardEffect != null)
                                        {
                                            if (cardEffect.EffectSourceCard != null)
                                            {
                                                if (cardEffect.EffectSourceCard.Owner == card.Owner.Enemy)
                                                {
                                                    if (cardEffect.EffectSourceCard.IsOption)
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }

                                        return false;
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
}