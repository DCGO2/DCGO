using System.Collections;
using System.Collections.Generic;

// Meramon
namespace DCGO.CardEffects.BT22
{
    public class BT22_010 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel3 && (targetPermanent.TopCard.HasFlameTraits || targetPermanent.TopCard.HasCSTraits);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Main

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Pay 2, Gain raid & piercing. then it may attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("BT22_010_Main");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] [Once Per Turn] By paying 2 cost, this Digimon gains <Raid> (When this Digimon attacks, you may switch the target of attack to 1 of your opponent's unsuspended Digimon with the highest DP) and <Piercing> (When this Digimon attacks and deletes an opponent's Digimon and survives the battle, it performs any security checks it normally would) for the turn. Then, this Digimon may attack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && CardEffectCommons.IsOwnerTurn(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(-2, activateClass));
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRaid(card.PermanentOfThisCard(), EffectDuration.UntilEachTurnEnd, activateClass));
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(card.PermanentOfThisCard(), EffectDuration.UntilEachTurnEnd, activateClass));

                    if (card.PermanentOfThisCard().CanAttack(activateClass))
                    {
                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: card.PermanentOfThisCard(),
                            canAttackPlayerCondition: () => true,
                            defenderCondition: _ => true,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) && CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 2000, isInheritedEffect: true, card: card, condition: Condition));
            }

            #endregion

            return cardEffects;
        }
    }
}