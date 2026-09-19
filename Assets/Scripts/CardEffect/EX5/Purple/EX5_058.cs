using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class EX5_058 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Play 1 [Fujitsumon] token", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] If there are 4 or more total Digimon, play 1 [Fujitsumon] Token (Digimon/Purple/3000 DP/[All Turns] This Digimon doesn't unsuspend./[On Deletion] Trash 1 card in your hand.) suspended to your battle area. If there are 3 or fewer, play it suspended to your opponent's battle area.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    Player player = card.Owner.GetBattleAreaDigimons().Count + card.Owner.Enemy.GetBattleAreaDigimons().Count >= 4
                    ? card.Owner : card.Owner.Enemy;

                    if (player.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame()) >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                bool isOwnerPermanent = card.Owner.GetBattleAreaDigimons().Count + card.Owner.Enemy.GetBattleAreaDigimons().Count >= 4;

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayFujitsumonToken(activateClass, isOwnerPermanent));
            }
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Play 1 [Fujitsumon] token", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] If there are 4 or more total Digimon, play 1 [Fujitsumon] Token (Digimon/Purple/3000 DP/[All Turns] This Digimon doesn't unsuspend./[On Deletion] Trash 1 card in your hand.) suspended to your battle area. If there are 3 or fewer, play it suspended to your opponent's battle area.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    Player player = card.Owner.GetBattleAreaDigimons().Count + card.Owner.Enemy.GetBattleAreaDigimons().Count >= 4
                    ? card.Owner : card.Owner.Enemy;

                    if (player.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame()) >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                bool isOwnerPermanent = card.Owner.GetBattleAreaDigimons().Count + card.Owner.Enemy.GetBattleAreaDigimons().Count >= 4;

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayFujitsumonToken(activateClass, isOwnerPermanent));
            }
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
            activateClass.SetHashString("Memory1_EX5_058");
            activateClass.SetIsInheritedEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[All Turns] [Once Per Turn] When an effect plays an opponent's Digimon, gain 1 memory.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition))
                    {
                        if (CardEffectCommons.IsByEffect(hashtable, null))
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
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
            }
        }

        return cardEffects;
    }
}
