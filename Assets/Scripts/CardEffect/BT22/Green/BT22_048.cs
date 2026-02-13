using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Togemon
namespace DCGO.CardEffects.BT22
{
    public class BT22_048 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel3 && targetPermanent.TopCard.HasCSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Give 1 digimon +3k DP, then if there is 2 same level digimon in sources, gain Raid & pierce", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] 1 of your Digimon gets +3000 DP for the turn. Then, if this Digimon's stack has 2 or more same-level cards, this Digimon gains <Raid> (When this Digimon attacks, you may switch the target of attack to 1 of your opponent's unsuspended Digimon with the highest DP) and <Piercing> (When this Digimon attacks and deletes an opponent's Digimon and survives the battle, it performs any security checks it normally would) for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Give 1 digimon +3k DP, then if there is 2 same level digimon in sources, gain Raid & pierce", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] 1 of your Digimon gets +3000 DP for the turn. Then, if this Digimon's stack has 2 or more same-level cards, this Digimon gains <Raid> (When this Digimon attacks, you may switch the target of attack to 1 of your opponent's unsuspended Digimon with the highest DP) and <Piercing> (When this Digimon attacks and deletes an opponent's Digimon and survives the battle, it performs any security checks it normally would) for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }

            #endregion

            #region OP/WD Shared

            bool CanSelectPermanent(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanent))
                {
                    Permanent selectedPermanent = null;
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanent));
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanent,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to give +3k DP.", "The opponent is selecting 1 Digimon to give +3k DP.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(selectedPermanent, 3000, EffectDuration.UntilEachTurnEnd, activateClass));
                }

                bool levelMatch = card.PermanentOfThisCard().StackCards
                    .Filter(x => !x.IsFlipped)
                    .GroupBy(x => x.Level)
                    .Any(g => g.Count() >= 2);

                if (levelMatch)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRaid(card.PermanentOfThisCard(), EffectDuration.UntilEachTurnEnd, activateClass));
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(card.PermanentOfThisCard(), EffectDuration.UntilEachTurnEnd, activateClass));
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