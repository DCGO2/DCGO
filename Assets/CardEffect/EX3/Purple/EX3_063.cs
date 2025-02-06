using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX3
{
    public class EX3_063 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool PermanentCondition1(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                            {
                                                if (permanent.TopCard.CardColors.Contains(CardColor.Purple))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(5))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                            {
                                                if (permanent.TopCard.CardColors.Contains(CardColor.Red))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(5))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 5 purple Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 5 red Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete Digimons and Blitz", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If DNA digivolving, your opponent chooses 1 of their Digimon. Delete all of their other Digimon. Then, <Blitz>.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsJogress(hashtable))
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                return true;
                            }
                        }

                        if (CardEffectCommons.CanActivateBlitz(card, activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsJogress(_hashtable))
                    {
                        Permanent selectedPermanent = null;

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner.Enemy,
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

                            selectPermanentEffect.SetUpCustomMessage("Select your 1 Digimon.", "The opponent is selecting 1 Digimon.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }
                        }

                        bool PermanentCondition(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    if (permanent.CanBeDestroyedBySkill(activateClass))
                                    {
                                        if (permanent != selectedPermanent)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
                        }

                        List<Permanent> destroyedPermanetns = card.Owner.Enemy.GetBattleAreaDigimons().Filter(PermanentCondition);
                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyedPermanetns, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.BlitzProcess(card, activateClass));
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DP +2000 and this Digimon digivolves", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("DP+2000_EX3_063");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking][Once Per Turn] This Digimon gets +2000 DP for the turn. Then, this Digimon may digivolve into [Imperialdramon: Fighter Mode] in your hand for the digivolution cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.CardNames.Contains("Imperialdramon: Fighter Mode") || cardSource.CardNames.Contains("Imperialdramon:FighterMode");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
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
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: card.PermanentOfThisCard(), changeValue: 2000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: CanSelectCardCondition,
                        payCost: true,
                        reduceCostTuple: null,
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true,
                        activateClass: activateClass,
                        successProcess: null));
                }
            }

            return cardEffects;
        }
    }
}