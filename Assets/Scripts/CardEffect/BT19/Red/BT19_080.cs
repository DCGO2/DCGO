using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT19
{
    public class BT19_080 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Turn
            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Give a Digimon Raid and it attacks", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription() => "[Your Turn] When any of your Digimon digivolve into a Digimon with [Growlmon]/[Gallantmon] in its name, by suspending this Tamer, that Digimon gains <Raid> for the turn. Then, that Digimon attacks a player.";

                bool CanSelectPermanentCondition(Permanent permanent) =>
                    CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                    permanent.IsDigimon &&
                    permanent.TopCard.ContainsCardName("Growlmon") ||
                        permanent.IsDigimon && permanent.TopCard.ContainsCardName("Gallantmon");

                bool CanUseCondition(Hashtable hashtable) =>
                    CardEffectCommons.IsExistOnBattleArea(card) &&
                    CardEffectCommons.IsOwnerTurn(card) &&
                    CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, CanSelectPermanentCondition);

                bool CanActivateCondition(Hashtable hashtable) =>
                    CardEffectCommons.IsExistOnBattleArea(card) &&
                    CardEffectCommons.CanActivateSuspendCostEffect(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> digivolvedPermanent = CardEffectCommons.GetPlayedPermanentsFromEnterFieldHashtable(hashtable, null);
                    List<CardSource> selectedCards = new List<CardSource>();

                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRaid(
                            targetPermanent: digivolvedPermanent[0],
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));

                    if (digivolvedPermanent[0].CanAttack(activateClass))
                    {
                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: digivolvedPermanent[0],
                            canAttackPlayerCondition: () => true,
                            defenderCondition: (permanent) => false,
                            cardEffect: activateClass);

                        selectAttackEffect.SetCanNotSelectNotAttack();

                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}