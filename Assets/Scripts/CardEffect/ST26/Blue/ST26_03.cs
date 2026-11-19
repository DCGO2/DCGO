using System.Collections;
using System.Collections.Generic;

// Tenkomon
namespace DCGO.CardEffects.ST26
{
    public class ST26_03 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared OP/WD
            string SharedEffectName = "If you have 1 or fewer Tamers, may play 1 [Valner Dragnogh] from hand for free";

            string SharedEffectDescription(string tag)
                => $"[{tag}] If you have 1 or fewer Tamers, you may play 1 [Valner Dragnogh] from your hand without paying the cost.";

            bool OwnTamerCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card);

            bool ValnerCondition(CardSource cardSource, ICardEffect cardEffect)
                => cardSource.EqualsCardName("Valner Dragnogh")
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, cardEffect);

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.MatchConditionPermanentCount(OwnTamerCondition) <= 1
                    && CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => ValnerCondition(cardSource, activateClass));

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.MatchConditionPermanentCount(OwnTamerCondition) > 1) yield break;

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                    canTargetCondition: cardSource => ValnerCondition(cardSource, activateClass),
                    root: SelectCardEffect.Root.Hand,
                    cardEffect: activateClass,
                    payCost: false));
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                additionalActivateCondition: SharedAdditionalActivateCondition,
                optional: false,
                isSkippable: true,
                onPlay: true,
                whenDigivolving: true);
            #endregion

            #region End of Attack - OPT - Inherited
            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("ST26_03_Inherited");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[End of Attack] [Once Per Turn] Gain 1 memory.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnEndAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.CanAddMemory(activateClass)) yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
