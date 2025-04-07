using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects
{
    public class BT21_020 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Reducded Digivolution Cost

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce the digivolution cost by 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "When any of your Digimon with [Agunimon]/[BurningGreymon] in their digivolution cards would digivolve into this card in the hand, reduce the digivolution cost by 1.";

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.cardSources.Find(source => source.EqualsCardName("Agunimon") || source.EqualsCardName("BurningGreymon")) != null)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CardCondition(CardSource source)
                {
                    if (source == card) return true;
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnHand(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, PermanentCondition))
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentCondition, CardCondition))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnHand(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnHand(card))
                    {
                        Hashtable hashtable = new Hashtable();
                        hashtable.Add("CardEffect", activateClass);

                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return true;
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
                            if (targetPermanents != null)
                            {
                                if (targetPermanents.Exists(PermanentCondition))
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        bool PermanentCondition(Permanent targetPermanent)
                        {
                            if (targetPermanent.TopCard != null)
                            {
                                return PermanentCondition(targetPermanent);
                            }

                            return false;
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                return CardCondition(cardSource);
                            }

                            return false;
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
                }
            }

            #endregion

            #region Sec +1

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(
                    changeValue: 1,
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #endregion

            #region On Deletion / ESS Shared

            ActivateClass SharedActivateClass()
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] You may play 1 red Tamer card with inherited effects from your hand or trash without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnDeletion(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCard) || CardEffectCommons.HasMatchConditionOpponentsCardInTrash(card, CanSelectCard))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectCard(CardSource source)
                {
                    if (source.IsTamer)
                    {
                        if (source.CardColors.Contains(CardColor.Red))
                        {
                            if (source.HasInheritedEffect)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return null;
                }

                return activateClass;
            }

            #endregion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = SharedActivateClass();
                cardEffects.Add(activateClass);
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = SharedActivateClass();
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);
            }

            return cardEffects;
        }
    }
}