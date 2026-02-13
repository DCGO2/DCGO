using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT19
{
    public class BT19_087 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }

            #region All Turns
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Can select DigiXros cards from Tamer's digivolution cards and from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("CanSelectDigiXrosFromTamer_EX4_062");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When any of your [Composite] or [Twilight] trait Digimon with DigiXros requirements would be played, by suspending this Tamer, 1 card under your Tamers and 1 card in your trash can also be placed for their DigiXros.";
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource.Owner == card.Owner)
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.HasDigiXros)
                            {
                                if (cardSource.EqualsTraits("Composite"))
                                {
                                    return true;
                                }

                                if (cardSource.CardTraits.Contains("Twilight"))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition))
                        {
                            if (CardEffectCommons.IsOnly1CardPlayed(hashtable))
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
                    if (isExistOnField(card))
                    {
                        if (!card.PermanentOfThisCard().IsSuspended && card.PermanentOfThisCard().CanSuspend)
                        {
                            Hashtable hashtable = new Hashtable();
                            hashtable.Add("CardEffect", activateClass);

                            yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, hashtable).Tap());

                            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                            AddMaxUnderTamerCountDigiXrosClass addMaxTamerCountDigiXrosClass = new AddMaxUnderTamerCountDigiXrosClass();
                            addMaxTamerCountDigiXrosClass.SetUpICardEffect("Can select DigiXros cards from Tamer's digivolution cards", CanUseCondition1, card);
                            addMaxTamerCountDigiXrosClass.SetUpAddMaxUnderTamerCountDigiXrosClass(getMaxUnderTamerCount: GetCount);
                            card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => addMaxTamerCountDigiXrosClass);

                            AddMaxTrashCountDigiXrosClass addMaxTrashCountDigiXrosClass = new AddMaxTrashCountDigiXrosClass();
                            addMaxTrashCountDigiXrosClass.SetUpICardEffect("Can select DigiXros cards from trash", CanUseCondition1, card);
                            addMaxTrashCountDigiXrosClass.SetUpAddMaxTrashCountDigiXrosClass(getMaxTrashCount: GetCount);
                            card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => addMaxTrashCountDigiXrosClass);

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                return true;
                            }

                            int GetCount(CardSource cardSource)
                            {
                                if (CardSourceCondition(cardSource))
                                {
                                    return 1;
                                }

                                return 0;
                            }

                            bool CardSourceCondition(CardSource cardSource)
                            {
                                if (cardSource != null)
                                {
                                    if (cardSource.Owner == card.Owner)
                                    {
                                        if (cardSource.HasDigiXros)
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }                            
                        }
                    }
                }
            }
            #endregion

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            return cardEffects;
        }
    }
}