using System.Collections;
using System.Collections.Generic;

//Viximon
namespace DCGO.CardEffects.ST22
{
    public class ST22_01 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnUseOption)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Evo for reduced cost of 3", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("YT_ST22-001");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When you use Option cards with the [Onmyōjutsu] or [Plug-In] trait, this Digimon may digivolve into a Digimon card with [Kyubimon], [Taomon] or [Sakuyamon] in its name in the hand with the digivolution cost reduced by 3.";
                }

                bool OptionUsed(CardSource source)
                {
                    return source.IsOption &&
                           (source.EqualsTraits("Onmyōjutsu") || source.EqualsTraits("Plug-In"));
                }

                bool DigivolutionTarget(CardSource source)
                {
                    return (source.ContainsCardName("Kyubimon") || source.ContainsCardName("Taomon") || source.ContainsCardName("Sakuyamon")) &&
                           source.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, true, activateClass);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.CanTriggerWhenOwnerUseOption(hashtable,OptionUsed,null,card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: card.PermanentOfThisCard(),
                                cardCondition: DigivolutionTarget,
                                payCost: true,
                                reduceCostTuple: (reduceCost: 3, reduceCostCardCondition: null),
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null,
                                isOptional:false));
                }
            }

            return cardEffects;
        }
    }
}
