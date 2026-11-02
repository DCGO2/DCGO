using System.Collections;
using System.Collections.Generic;

// Spencer Damon
namespace DCGO.CardEffects.BT27
{
    public class BT27_090 : CEntity_Effect
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

            #region All Turns
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing this Tamer under a played Digimon, opponent's effects can't return 1 of your Digimon or trash its stacked cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] When any of your Digimon with [Leomon] in their names or the [Boss] or [DATA SQUAD] trait are played, by placing this Tamer as any of those Digimon's bottom digivolution card, your opponent's effects can't return 1 of your Digimon to hands or decks, or trash its stacked cards until their turn ends.";

                bool PlayedDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.ContainsCardName("Leomon")
                            || permanent.TopCard.EqualsTraits("Boss")
                            || permanent.TopCard.HasDataSquadTraits);

                bool CanSelectProtectedCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool OpponentEffectCondition(ICardEffect cardEffect)
                    => CardEffectCommons.IsOpponentEffect(cardEffect, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && card.PermanentOfThisCard().IsTamer
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PlayedDigimonCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.PermanentOfThisCard().IsTamer;

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> playedDigimon = CardEffectCommons.GetPermanentsFromHashtable(hashtable).Filter(PlayedDigimonCondition);

                    if (playedDigimon.Count == 0) yield break;

                    Permanent selectedDigimon = null;

                    SelectPermanentEffect selectPlacementEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPlacementEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: permanent => playedDigimon.Contains(permanent),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPlacementCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPlacementEffect.SetUpCustomMessage("Select 1 Digimon to place this Tamer under.", "The opponent is selecting 1 Digimon to place their Tamer under.");

                    yield return ContinuousController.instance.StartCoroutine(selectPlacementEffect.Activate());

                    IEnumerator SelectPlacementCoroutine(Permanent permanent)
                    {
                        selectedDigimon = permanent;
                        yield return null;
                    }

                    if (selectedDigimon == null) yield break;

                    yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(
                        new List<Permanent[]> { new Permanent[] { card.PermanentOfThisCard(), selectedDigimon } }, false, activateClass).PlacePermanentToDigivolutionCards());

                    if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectProtectedCondition)) yield break;

                    SelectPermanentEffect selectProtectedEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectProtectedEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectProtectedCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectProtectedCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectProtectedEffect.SetUpCustomMessage("Select 1 Digimon to protect.", "The opponent is selecting 1 Digimon to protect.");

                    yield return ContinuousController.instance.StartCoroutine(selectProtectedEffect.Activate());

                    IEnumerator SelectProtectedCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.GainCanNotReturnToHand(permanent, OpponentEffectCondition, EffectDuration.UntilOpponentTurnEnd, activateClass, "Can't be returned to hand"));

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.GainCanNotReturnToDeck(permanent, OpponentEffectCondition, EffectDuration.UntilOpponentTurnEnd, activateClass, "Can't be returned to deck"));

                        ImmuneStackTrashingClass immuneStackTrashingClass = new ImmuneStackTrashingClass();
                        immuneStackTrashingClass.SetUpICardEffect("Opponent's effects can't trash stacked cards", _ => permanent.TopCard != null, permanent.TopCard);
                        immuneStackTrashingClass.SetUpImmuneFromStackTrashingClass(PermanentCondition: target => target == permanent, EffectCondition: OpponentEffectCondition);

                        CardEffectCommons.AddEffectToPermanent(
                            targetPermanent: permanent,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            card: card,
                            cardEffect: immuneStackTrashingClass,
                            timing: EffectTiming.None);
                    }
                }
            }
            #endregion

            #region All Turns - ESS
            if (timing == EffectTiming.None)
            {
                bool Condition()
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && (card.PermanentOfThisCard().TopCard.ContainsCardName("Leomon")
                            || card.PermanentOfThisCard().TopCard.EqualsTraits("Boss")
                            || card.PermanentOfThisCard().TopCard.HasDataSquadTraits);

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 3000, isInheritedEffect: true, card: card, condition: Condition));
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
