using System;
using System.Collections;
using System.Collections.Generic;

// UltimateBrachiomon
namespace DCGO.CardEffects.BT25
{
    public class BT25_019 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Alt Digivolution Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasTSTraits || targetPermanent.TopCard.EqualsTraits("Dinosaur");


                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 5, permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #endregion

            #region OP/WD Shared
            string SharedEffectName = "Delete 1 Digimon with highest DP.";
            string SharedEffectDescription(string tag) => $"[{tag}] Delete 1 of your opponent's highest DP Digimon.";

            bool CanSelectPermamentCondition(Permanent permanent)
                => CardEffectCommons.IsMaxDP(permanent, card.Owner.Enemy, null);

            IEnumerator SharedActivateCoroutine(Hashtable _hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermamentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, CanSelectPermamentCondition));

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermamentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 highest DP Digimon to delete.", "Opponent is select 1 highest DP digimon to delete.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: -1,
                onPlay: true,
                whenDigivolving: true);

            #endregion

            #region End of Your Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If opponent is at 5 or more memory, this gains immune to digimon effects, If opponent is at 5 or less memory, this gains immune to option effects.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT25_019_EndOfYourTurn");
                cardEffects.Add(activateClass);

                string EffectDescription() => "[End of Your Turn] [Once Per Turn] If your opponent has 5 or more memory, their Digimon effects don't affect this Digimon until their turn ends. Then, if they have 5 or less, their Option effects don't affect this Digimon until their turn ends.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);


                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card);


                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();
                    var opponentMemory = card.Owner.Enemy.MemoryForPlayer;

                    if (opponentMemory >= 5)
                    {
                        #region Give Digimon Effect Immunity
                        thisPermanent.UntilOpponentTurnEndEffects.Add((_timing) => PermanentEffectFactory.DigimonEffectImmunity(thisPermanent));
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(thisPermanent));
                        #endregion
                    }
                    if (opponentMemory <= 5)
                    {
                        #region Give Option Effect Immunity
                        thisPermanent.UntilOpponentTurnEndEffects.Add((_timing) => PermanentEffectFactory.OptionEffectImmunity(thisPermanent));
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(thisPermanent));
                        #endregion
                    }
                }

            }
            #endregion

            return cardEffects;
        }
    }
}
