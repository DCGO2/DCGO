using System.Collections;
using System.Collections.Generic;

// Gawappamon — BT27-025.
namespace DCGO.CardEffects.BT27
{
    public class BT27_025 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.EqualsTraits("DATA SQUAD");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    level: 3,
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving
            string SharedEffectName = "Give 1 of your Digimon battle-deletion protection; if played by an effect, also give it Blocker";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] 1 of your Digimon can't be deleted in battle until your opponent's turn ends.If played by effects, it also gains<Blocker>.";

            bool IsOwnDigimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool playedByEffect = CardEffectCommons.CanTriggerOnPlay(hashtable, card)
                    && CardEffectCommons.IsByEffect(hashtable, null);

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                Permanent selectedPermanent = null;

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: IsOwnDigimon,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage(
                    $"Select 1 of your Digimon to protect from battle deletion{(playedByEffect ? "" : " and gain <Blocker>")}.",
                    $"The opponent is selecting a Digimon to protect from battle deletion{(playedByEffect ? "" : " and gain <Blocker>")}.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                if (selectedPermanent == null) yield break;

                yield return ContinuousController.instance.StartCoroutine(
                    CardEffectCommons.GainCanNotBeDeletedByBattle(
                        targetPermanent: selectedPermanent,
                        canNotBeDestroyedByBattleCondition: CanNotBeDestroyedByBattleCondition,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass,
                        effectName: "Can't be deleted in battle"));

                if (playedByEffect)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.GainBlocker(
                            targetPermanent: selectedPermanent,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));
                }

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;
                    yield break;
                }

                bool CanNotBeDestroyedByBattleCondition(Permanent permanent,
                    Permanent attackingPermanent, Permanent defendingPermanent, CardSource defendingCard)
                {
                    return permanent == selectedPermanent
                        && (permanent == attackingPermanent || permanent == defendingPermanent);
                }
            }   
            #endregion

            #region Inherited Evade
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.EvadeSelfEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
