using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT11
{
    public class BT11_103 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect($"Play Cost -1", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(
                    changeCostFunc: ChangeCost,
                    cardSourceCondition: CardSourceCondition,
                    rootCondition: RootCondition,
                    isUpDown: isUpDown,
                    isCheckAvailability: () => false,
                    isChangePayingCost: () => true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) =>
                    permanent.TopCard.CardColors.Contains(CardColor.Green) && permanent.IsTamer);
                }

                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource))
                    {
                        if (RootCondition(root))
                        {
                            if (PermanentsCondition(targetPermanents))
                            {
                                Cost -= 1;
                            }
                        }
                    }

                    return Cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    return true;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                bool isUpDown()
                {
                    return true;
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
                    return "[Main] Until the end of your opponent's turn, all of their Digimon gain \"[All Turns] When this Digimon becomes suspended, lose 1 memory.\"";
                }
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool PermanentCondition(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                    }

                    foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaPermanents())
                    {
                        if (PermanentCondition(permanent))
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(permanent));
                        }
                    }

                    AddSkillClass addSkillClass = new AddSkillClass();
                    addSkillClass.SetUpICardEffect("Memory -1", CanUseCondition1, card);
                    addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                    card.Owner.UntilOpponentTurnEndEffects.Add((_timing) => addSkillClass);

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
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
                        if (_timing == EffectTiming.OnTappedAnyone)
                        {
                            ActivateClass activateClass1 = new ActivateClass();
                            activateClass1.SetUpICardEffect("Memory -1", CanUseCondition2, cardSource);
                            activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                            cardEffects.Add(activateClass1);

                            if (cardSource.PermanentOfThisCard() != null)
                            {
                                activateClass1.SetEffectSourcePermanent(cardSource.PermanentOfThisCard());
                            }

                            string EffectDiscription1()
                            {
                                return "[All Turns] When this Digimon becomes suspended, lose 1 memory.";
                            }

                            bool CanUseCondition2(Hashtable hashtable)
                            {
                                if (CardSourceCondition(cardSource))
                                {
                                    if (CardEffectCommons.IsExistOnBattleArea(cardSource))
                                    {
                                        if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable,
                                        (permanent) => permanent == cardSource.PermanentOfThisCard()))
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            bool CanActivateCondition1(Hashtable hashtable)
                            {
                                if (CardEffectCommons.IsExistOnBattleArea(cardSource))
                                {
                                    return true;
                                }

                                return false;
                            }

                            IEnumerator ActivateCoroutine1(Hashtable _hashtable)
                            {
                                yield return ContinuousController.instance.StartCoroutine(cardSource.Owner.AddMemory(-1, activateClass1));
                            }
                        }

                        return cardEffects;
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(
                    card: card,
                    cardEffects: ref cardEffects,
                    effectName: $"Opponent's Digimons get effect");
            }

            return cardEffects;
        }
    }
}