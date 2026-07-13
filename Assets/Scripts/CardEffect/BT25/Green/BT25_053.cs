using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Aegiochusmon: Green
namespace DCGO.CardEffects.BT25
{
    public class BT25_053 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Aegiomon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null));
            }
            #endregion

            #region Vortex
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.VortexSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Decode <[Aegiomon]>

            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource source)
                {
                    return source.EqualsCardName("Aegiomon");
                }

                string[] decodeStrings = { "(Aegiomon)", "Aegiomon" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: false, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }

            #endregion

            #region Shared OP / WD

            string SharedEffectName = "Suspend 1 opponent digimon or tamer, freeze until opponent turn ends. Then if 3 or less security, this gains Piercing & 5K DP";

            string SharedEffectDescription(string tag) => $"[{tag}] Suspend 1 of your opponent's Digimon or Tamers. It can't unsuspend until their turn ends. Then, if you have 3 or fewer security cards, this Digimon gains <Piercing> and +5000 DP for the turn.";

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer);
                }

                // Suspend 1 opponent digimon or tamer, freeze until opponent turn ends.
                if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                {
                    Permanent selectedPermanent = null;

                    #region Select 1 opponent digimon or tamer

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: PermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or tamer to suspend and freeze until opponent turn ends.", "The opponent is selecting 1 Digimon or tamer to suspend and freeze until your turn ends.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    #endregion

                    if (selectedPermanent != null)
                    {
                        #region Suspend and Freeze
                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { selectedPermanent }, hashtable).Tap());
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspend(selectedPermanent, EffectDuration.UntilOpponentTurnEnd, activateClass, null, "Can not unsuspend"));
                        #endregion
                    }
                }

                // if 3 or less security, this gains Piercing & 5K DP
                if (card.Owner.SecurityCards.Count <= 3)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    #region Gain Piercing & +5K DP
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(thisPermanent, EffectDuration.UntilEachTurnEnd, activateClass));
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(thisPermanent, 5000, EffectDuration.UntilEachTurnEnd, activateClass));
                    #endregion
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

            #region Inherit All Turns - OPT

            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 Opponent Digimon or Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("BT25_053_AT");
                activateClass.SetIsSkippable(true);
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When your security stack is removed from, you may suspend 1 of your opponent's Digimon or Tamers.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, IsYourSecurity);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool IsYourSecurity(Player player)
                {
                    return player == card.Owner;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool hasUsed = false;
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, PermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, PermanentCondition));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: PermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or tamer to suspend.", "The opponent is selecting 1 Digimon or tamer to suspend.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            if (permanents.Any()) hasUsed = true;
                            yield return null;
                        }
                    }

                    if (!hasUsed) activateClass.RemoveUse();
                }
            }

            #endregion

            return cardEffects;
        }
    }
}