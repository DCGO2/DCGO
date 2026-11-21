using System.Collections;
using System.Collections.Generic;

// Guardromon
namespace DCGO.CardEffects.BT27
{
    public class BT27_037 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - Lv.3 w/[Machine]/[TS] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsTraits("Machine")
                        || targetPermanent.TopCard.HasTSTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Detach - [TS] trait
            if (timing == EffectTiming.WhenRemoveField)
            {
                static bool CanSelectLinkCardCondition(CardSource cardSource)
                    => cardSource.HasTSTraits;

                foreach (bool isInheritedEffect in new[] { false, true })
                {
                    cardEffects.Add(CardEffectFactory.DetachSelfEffect(
                        isInheritedEffect: isInheritedEffect,
                        card: card,
                        condition: null,
                        conditionString: "[TS] trait",
                        cardCondition: CanSelectLinkCardCondition));
                }
            }
            #endregion

            #region All Turns - OPT
            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<Draw 1>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT27_037_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When any of your Digimon get linked, <Draw 1>.";

                bool LinkedDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenLinked(hashtable, LinkedDigimonCondition, null);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
