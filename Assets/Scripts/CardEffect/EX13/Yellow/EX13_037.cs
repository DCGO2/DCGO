using System.Collections;
using System.Collections.Generic;

// Dynasmon
namespace DCGO.CardEffects.EX13
{
    public class EX13_037 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Witchelny");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 5));
            }
            #endregion

            #region Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD/WA
            string SharedEffectName = "Trash your top sec, this Digimon gets +10K DP, if 3 sec or less trash enemy top sec";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    maxCountPerTurn: 1,
                    hashValue: "EX13_037_OP_WD_WA",
                    onPlay: true,
                    whenDigivolving: true,
                    whenAttacking: true);

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] trash your top security card and this Digimon gets +10000 DP until your opponent's turn ends. Then, if you have 3 or fewer security cards, trash their top security card.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                    player: card.Owner,
                    destroySecurityCount: 1,
                    cardEffect: activateClass,
                    fromTop: true).DestroySecurity());

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                    targetPermanent: card.PermanentOfThisCard(),
                    changeValue: 10000,
                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                    activateClass: activateClass));

                if (card.Owner.SecurityCards.Count <= 3)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Give an enemy Digimon -12K DP, if 3 sec or less <Recovery +1>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX13_037_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When security stacks are removed from, 1 of your opponent's Digimon gets -12000 DP until their turn ends. Then, if you have 3 or fewer security cards, <Recovery +1>";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, _ => true);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                bool IsOpponentsDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -12000.",
                            "The opponent is selecting 1 Digimon that will get DP -12000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: permanent,
                                changeValue: -12000,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));
                        }
                    }

                    if (card.Owner.SecurityCards.Count <= 3)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
