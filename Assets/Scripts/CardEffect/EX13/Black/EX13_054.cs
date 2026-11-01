using System.Collections;
using System.Collections.Generic;

// Nanimon
namespace DCGO.CardEffects.EX13
{
    public class EX13_054 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfDigimonAfterBattleSecurityEffect(card: card));
            }
            #endregion

            #region Shared On Play / On Deletion

            string SharedEffectName = "1 of your opponent's Digimon can't attack players";

            string SharedEffectDescription(string tag)
                => $"[{tag}] 1 of your opponent's Digimon can't attack players until their turn ends.";

            bool CanSelectPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that can't attack players.", "The opponent is selecting 1 Digimon that can't attack players.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        bool DefenderCondition(Permanent defender)
                            => defender == null;

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotAttack(
                            targetPermanent: permanent,
                            defenderCondition: DefenderCondition,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            effectName: "Can't Attack to player"));
                    }
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                onDeletion: true);

            #endregion

            #region All Turns - ESS
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 1000,
                    isInheritedEffect: true,
                    card: card,
                    condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
