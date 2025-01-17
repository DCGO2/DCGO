using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT17
{
    public class BT17_089 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Your Turn - When Effect Suspends
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend this Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your turn] When an effect suspends one of your Digimon, you may suspend this Tamer.";
                }

                bool SuspendedCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, SuspendedCondition))
                            {
                                if (CardEffectCommons.IsByEffect(hashtable, null))
                                {
                                    return !card.PermanentOfThisCard().IsSuspended;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());
                }
            }
            #endregion

            #region Your Turn - When becomes suspended
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1, Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("SuspendSelf_BT17_089");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When this Tamer becomes suspended, gain 1 memory. Then, if you have [Argomon] or a yellow Digimon with [Agumon] or [Greymon] in its name, <Draw 1>.";
                }

                bool SuspendedCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool HasDigimonCondition(Permanent permanent)
                {
                    if (permanent.IsDigimon)
                    {
                        if (permanent.TopCard.EqualsCardName("Argomon"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardColors.Contains(CardColor.Yellow))
                        {
                            if(permanent.TopCard.ContainsCardName("Agumon") || permanent.TopCard.HasGreymonName)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, SuspendedCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasDigimonCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}