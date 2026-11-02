using System.Collections;
using System.Collections.Generic;

// Thundermon (X Antibody)
namespace DCGO.CardEffects.BT27
{
    public class BT27_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Thundermon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Name Treated As Including [Mamemon]
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.NameTreatedAsIncluding(card, "Mamemon"));
            }
            #endregion

            #region Shared On Play / When Digivolving / On Deletion

            string SharedEffectName = "1 of your [Mamemon] in name Digimon gains <Blocker> and can't be returned to hand or deck";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Until your opponent's turn ends, 1 of your Digimon with [Mamemon] in its name gains <Blocker> and their effects can't return it to hands or decks.";

            bool CanSelectPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.ContainsCardName("Mamemon");

            bool OpponentEffectCondition(ICardEffect cardEffect)
                => CardEffectCommons.IsOpponentEffect(cardEffect, card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition)) yield break;

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

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to gain <Blocker> and return protection.", "The opponent is selecting 1 Digimon to gain <Blocker> and return protection.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(permanent, EffectDuration.UntilOpponentTurnEnd, activateClass));

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.GainCanNotReturnToHand(permanent, OpponentEffectCondition, EffectDuration.UntilOpponentTurnEnd, activateClass, "Can't be returned to hand"));

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.GainCanNotReturnToDeck(permanent, OpponentEffectCondition, EffectDuration.UntilOpponentTurnEnd, activateClass, "Can't be returned to deck"));
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true,
                onDeletion: true);

            #endregion

            #region Blocker - ESS
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: true, card: card, condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
