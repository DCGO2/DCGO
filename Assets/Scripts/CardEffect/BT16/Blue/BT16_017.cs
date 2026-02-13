using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class BT16_017 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("DemiVeemon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region Your turn - When enter field

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When one of your other Digimon is played or digivolves, if that Digimon is green or has the [Free] trait, gain 1 memory.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                           && permanent != card.PermanentOfThisCard()
                           && permanent.IsDigimon
                           && (permanent.TopCard.CardColors.Contains(CardColor.Green)
                               || permanent.TopCard.ContainsTraits("Free"));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOwnerTurn(card)
                           && CardEffectCommons.IsExistOnBattleArea(card)
                           && (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition)
                               || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable,
                                   PermanentCondition));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                           && card.Owner.CanAddMemory(activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        card.Owner.AddMemory(1, activateClass));
                }
            }

            #endregion

            #region Inherited Effect

            if (timing == EffectTiming.None)
            {
                bool InheritedEffectCondition()
                {
                    return CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 2000,
                    isInheritedEffect: true,
                    card: card,
                    condition: InheritedEffectCondition));
            }

            #endregion

            return cardEffects;
        }
    }
}