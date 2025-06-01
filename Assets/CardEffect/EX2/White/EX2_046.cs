using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX2
{
    public class EX2_046 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect($"Play Cost -2", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);

                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Contains(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Count((permanent) => permanent != card.PermanentOfThisCard() && permanent.TopCard.CardNames.Contains("ADR-02Searcher")) == 0)
                        {
                            if (card.Owner.GetBattleAreaDigimons().Count((permanent) => permanent != card.PermanentOfThisCard() && permanent.TopCard.CardNames.Contains("ADR-02 Searcher")) == 0)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }



                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource))
                    {
                        if (RootCondition(root))
                        {
                            if (PermanentsCondition(targetPermanents))
                            {
                                Cost -= 2;
                            }
                        }
                    }

                    return Cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    if (targetPermanents == null)
                    {
                        return true;
                    }

                    else
                    {
                        if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    if (root == SelectCardEffect.Root.Hand)
                    {
                        return true;
                    }

                    return false;
                }

                bool isUpDown()
                {
                    return true;
                }
            }

            if (timing == EffectTiming.None)
            {
                bool DefenderCondition(Permanent permanent)
                {
                    return permanent == null;
                }

                bool Condition()
                {
                    return CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.CanNotAttackSelfStaticEffect(defenderCondition: DefenderCondition, isInheritedEffect: false, card: card, condition: Condition, effectName: "Can't Attack to player"));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Trigger <Draw 1>. (Draw 1 card from your deck.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
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
                }
            }

            /*if (timing == EffectTiming.None)
            {
                bool Condition()
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
                        if (permanent.TopCard.CardTraits.Contains("D-Reaper"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                    permanentCondition: PermanentCondition,
                    changeValue: 1000,
                    isInheritedEffect: true,
                    card: card,
                    condition: Condition,
                    effectName: () => "Your Digimons with [D-Reaper] in their traits gain DP +1000"));
            }*/

            #region All Turns - ESS

            if (timing != EffectTiming.None)
            {
                if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();


                    if (Condition())
                    {
                        foreach(Permanent permanent in card.Owner.GetBattleAreaPermanents().Filter(PermanentCondition))
                            permanent.AddBoost(new Permanent.DPBoost($"EX2_046_{card.GetInstanceID()}", 1000, Condition));
                    }
                    else
                    {
                        foreach (Permanent permanent in card.Owner.GetBattleAreaPermanents().Filter(PermanentCondition))
                            permanent.RemoveBoost($"EX2_046_{card.GetInstanceID()}");
                    }
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardTraits.Contains("D-Reaper"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           card.PermanentOfThisCard().DigivolutionCards.Contains(card);
                }
            }

            #endregion

            return cardEffects;
        }
    }
}