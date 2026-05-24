using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Flaremon
namespace DCGO.CardEffects.BT25
{
    public class BT25_017 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 4, permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "May attack, then by trashing 1 from hand, delete 1 enemy digimon with 7000 DP or less";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] This Digimon may attack. Then, by trashing 1 card in your hand, delete 1 of your opponent's Digimon with 7000 DP or less.";
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.HasDP
                        && permanent.DP <= card.Owner.MaxDP_DeleteEffect(7000, activateClass);
                }

                if (card.PermanentOfThisCard().CanAttack(activateClass))
                {
                    SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                    selectAttackEffect.SetUp(
                        attacker: card.PermanentOfThisCard(),
                        canAttackPlayerCondition: () => true,
                        defenderCondition: _ => true,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                }

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: (cardSource) => true,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                    mode: SelectHandEffect.Mode.Discard,
                    cardEffect: activateClass);

                yield return StartCoroutine(selectHandEffect.Activate());

                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                {
                    if (cardSources.Count >= 1 && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
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
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }                               
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [Apollomon] in hand for 2 less", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription() => "[Your Turn] When your Digimon are played or digivolve, if any of them are blue, this Digimon may digivolve into [Apollomon] in the hand with the cost reduced by 2.";
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition)
                            || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition)
                        && HasValidColor(hashtable);
                }

                bool HasValidColor(Hashtable hashtable)
                {
                    List<Hashtable> hashtables = CardEffectCommons.GetHashtablesFromHashtable(hashtable);
                    return hashtables.Map(CardEffectCommons.GetPermanentFromHashtable).Any(HasCorrectColorPermanent);
                }

                bool HasCorrectColorPermanent(Permanent permanent)
                {
                    return PermanentCondition(permanent)
                        && permanent.TopCard.CardColors.Contains(CardColor.Blue);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Apollomon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: CanSelectCardCondition,
                        payCost: true,
                        reduceCostTuple: (reduceCost: 2, reduceCostCardCondition: null),
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true,
                        activateClass: activateClass,
                        successProcess: null));
                }
            }
            #endregion

            #region Inherited Security A. +1
            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: true, card: card, condition: null));
            #endregion

            return cardEffects;
        }
    }
}
