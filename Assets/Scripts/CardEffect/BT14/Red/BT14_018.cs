using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.BT14
{
    public class BT14_018 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play tokens", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Play 1 [Amon of Crimson Flame] (Digimon/Red/6000 DP/<Rush>) Token and 1 [Umon of Blue Thunder] (Digimon/Yellow/6000 DP/<Blocker>) Token.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame()) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayAmonToken(activateClass));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayUmonToken(activateClass));
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play tokens", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Play 1 [Amon of Crimson Flame] (Digimon/Red/6000 DP/<Rush>) Token and 1 [Umon of Blue Thunder] (Digimon/Yellow/6000 DP/<Blocker>) Token.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame()) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayAmonToken(activateClass));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayUmonToken(activateClass));
                }
            }

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete tokens and Recovery +1 (Deck)", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("Recovery1_BT14_018");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When this Digimon would digivolve or leave the battle area, delete all of your [Amon of Crimson Flame] and [Umon of Blue Thunder]. If this effect deletes, <Recovery +1 (Deck)>.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.CardNames.Contains("Amon of Crimson Flame"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardNames.Contains("AmonofCrimsonFlame"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool PermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.CardNames.Contains("Umon of Blue Thunder"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardNames.Contains("UmonofBlueThunder"))
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
                        if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolveOfCard(hashtable, null, card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                        {
                            return true;
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition1))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<Permanent> destroyTargetPermanents = card.Owner.GetBattleAreaPermanents()
                    .Filter(permanent => PermanentCondition(permanent) || PermanentCondition1(permanent));

                    DestroyPermanentsClass destroyPermanentsClass = new DestroyPermanentsClass(
                        destroyTargetPermanents,
                        CardEffectCommons.CardEffectHashtable(activateClass));

                    yield return ContinuousController.instance.StartCoroutine(destroyPermanentsClass.Destroy());

                    if (destroyPermanentsClass.DestroyedPermanents.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());

                        yield return new WaitForSeconds(0.4f);
                    }
                }
            }

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete tokens and Recovery +1 (Deck)", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("Recovery1_BT14_018");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When this Digimon would digivolve or leave the battle area, delete all of your [Amon of Crimson Flame] and [Umon of Blue Thunder]. If this effect deletes, <Recovery +1 (Deck)>.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.CardNames.Contains("Amon of Crimson Flame"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardNames.Contains("AmonofCrimsonFlame"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool PermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.CardNames.Contains("Umon of Blue Thunder"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.CardNames.Contains("UmonofBlueThunder"))
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
                        if (CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                        {
                            return true;
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition1))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<Permanent> destroyTargetPermanents = card.Owner.GetBattleAreaPermanents()
                    .Filter(permanent => PermanentCondition(permanent) || PermanentCondition1(permanent));

                    DestroyPermanentsClass destroyPermanentsClass = new DestroyPermanentsClass(
                        destroyTargetPermanents,
                        CardEffectCommons.CardEffectHashtable(activateClass));

                    yield return ContinuousController.instance.StartCoroutine(destroyPermanentsClass.Destroy());

                    if (destroyPermanentsClass.DestroyedPermanents.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());

                        yield return new WaitForSeconds(0.4f);
                    }
                }
            }

            return cardEffects;
        }
    }
}