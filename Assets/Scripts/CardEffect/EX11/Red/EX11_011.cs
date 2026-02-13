using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX11
{
    // Dinomon
    public class EX11_011 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return (targetPermanent.TopCard.EqualsTraits("Dinosaur") || targetPermanent.TopCard.ContainsCardName("Tyrannomon"))
                        && targetPermanent.TopCard.IsLevel5;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Security Attack +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Fortitude
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.FortitudeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region On Play/When Digivolving Shared

            string SharedEffectName = "May suspend 1 Digimon. Delete all but highest play cost for each player.";

            string SharedEffectDescription(string tag) => $"[{tag}] You may suspend 1 Digimon. Then, choose 1 of each player's Digimon with the highest play cost and delete all other Digimon.";

            bool SharedCanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

            bool IsDigimonCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);

            bool IsMaxCostOwnersDigimon(Permanent permanent) => CardEffectCommons.IsMaxCost(permanent, card.Owner, true);

            bool IsMaxCostOpponentsDigimon(Permanent permanent) => CardEffectCommons.IsMaxCost(permanent, card.Owner.Enemy, true);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                SelectPermanentEffect suspendPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                suspendPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: IsDigimonCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Tap,
                    cardEffect: activateClass);

                yield return ContinuousController.instance.StartCoroutine(suspendPermanentEffect.Activate());

                List<Permanent> selectedPermanents = new List<Permanent>();

                IEnumerator selectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanents.Add(permanent);

                    yield return null;
                }

                if (card.Owner.GetBattleAreaDigimons().Count > 1)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsMaxCostOwnersDigimon));

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsMaxCostOwnersDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: selectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 of your highest cost Digimon, the rest will be deleted.", "Opponent is selecting which of their Digimon they will not delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
                else if (card.Owner.GetBattleAreaDigimons().Count == 1)
                {
                    selectedPermanents.Add(card.Owner.GetBattleAreaDigimons()[0]);
                }

                if (card.Owner.Enemy.GetBattleAreaDigimons().Count > 1)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsMaxCostOpponentsDigimon));

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsMaxCostOpponentsDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: selectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 of your opponent's highest cost Digimon, the rest will be deleted.", "Opponent is selecting which of your Digimon they will not delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
                else if (card.Owner.Enemy.GetBattleAreaDigimons().Count == 1)
                {
                    selectedPermanents.Add(card.Owner.Enemy.GetBattleAreaDigimons()[0]);
                }

                List<Permanent> DestroyTargetPermanents = GManager.instance.turnStateMachine.gameContext.Players.Map(player => player.GetBattleAreaDigimons()).Flat().Except(selectedPermanents).ToList();                
                if (DestroyTargetPermanents.Count > 0)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(DestroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                }
            }
            
            #endregion
            
            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }
            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }
            #endregion

            #region Opponent's Turn
            if (timing == EffectTiming.None)
            {
                bool AttackerCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool DefenderCondition(Permanent permanent)
                {
                    return !(CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.IsSuspended);
                }

                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOpponentTurn(card)
                        && card.PermanentOfThisCard().IsSuspended;
                }

                string effectName = "While this Digimon is suspended, all of your opponent's Digimon can only attack suspended Digimon.";

                cardEffects.Add(CardEffectFactory.CanNotAttackStaticEffect(
                    attackerCondition: AttackerCondition,
                    defenderCondition: DefenderCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    effectName: effectName
                ));
            }      
            #endregion

            return cardEffects;
        }
    }
}
