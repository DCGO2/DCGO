using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Aegiomon
namespace DCGO.CardEffects.BT25
{
    public class BT25_033 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolve Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 2, false, card, null, level: 3));
            }
            #endregion

            #region Barrier
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP / WD

            string SharedEffectName = "By adding your top security card to your hand, -5000 DP to an enemy Digimon.";

            string SharedEffectDescription(string tag) => $"[{tag}] By adding your top security card to the hand, 1 of your opponent's Digimon gets -5000 DP for the turn.";

            bool CanSelectEnemyDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool AdditionCanActivate(Hashtable hashtable, ActivateClass activateClass) => card.Owner.SecurityCards.Any();

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                CardSource topCard = card.Owner.SecurityCards[0];

                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { topCard }, false, activateClass));
                yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(player: card.Owner, refSkillInfos: ref ContinuousController.instance.nullSkillInfos, activateClass).ReduceSecurity());

                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectEnemyDigimon))
                {
                    Permanent selectedPermanent = null;
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectEnemyDigimon,
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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -5000.", "The opponent is selecting 1 Digimon that will get DP -5000.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());


                    if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: selectedPermanent, changeValue: -5000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects
                    (ref cardEffects, timing, card,
                        SharedEffectName,
                        SharedActivateCoroutine,
                        SharedEffectDescription,
                        optional: true,
                        maxCountPerTurn: -1,
                        onPlay: true,
                        whenDigivolving: true,
                        additionalActivateCondition: AdditionCanActivate);
            #endregion

            #region Inherited
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}