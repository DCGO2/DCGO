using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Goldramon
namespace DCGO.CardEffects.BT14
{
    public class BT14_018 : CEntity_Effect
    {
        private static WaitForSeconds _waitForSeconds0_4 = new WaitForSeconds(0.4f);

        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            #region Shared OP/WD

            string SharedEffectName = "Play tokens";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag)
                => $"[{tag}] Play 1 [Amon of Crimson Flame] (Digimon/Red/6000 DP/<Rush>) Token and 1 [Umon of Blue Thunder] (Digimon/Yellow/6000 DP/<Blocker>) Token.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayAmonAndUmonToken(activateClass));
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.BeforePayCost || timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete tokens and Recovery +1 (Deck)", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetHashString("BT14_018_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When this Digimon would digivolve or leave the battle area, delete all of your [Amon of Crimson Flame] and [Umon of Blue Thunder]. If this effect deletes, <Recovery +1 (Deck)>.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && (permanent.TopCard.EqualsCardName("Amon of Crimson Flame")
                            || permanent.TopCard.EqualsCardName("Umon of Blue Thunder"));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && (timing == EffectTiming.BeforePayCost
                                && CardEffectCommons.CanTriggerWhenPermanentWouldDigivolveOfCard(hashtable, null, card)
                            || timing == EffectTiming.WhenRemoveField
                                && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(PermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<Permanent> destroyTargetPermanents = card.Owner.GetBattleAreaPermanents()
                    .Filter(permanent => PermanentCondition(permanent));

                    DestroyPermanentsClass destroyPermanentsClass = new DestroyPermanentsClass(
                        destroyTargetPermanents,
                        CardEffectCommons.CardEffectHashtable(activateClass));

                    yield return ContinuousController.instance.StartCoroutine(destroyPermanentsClass.Destroy());

                    if (destroyPermanentsClass.DestroyedPermanents.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());

                        yield return _waitForSeconds0_4;
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}