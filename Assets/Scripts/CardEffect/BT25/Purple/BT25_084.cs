using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Titamon
namespace DCGO.CardEffects.BT25
{
    public class BT25_084 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool Condition(Permanent permanent)
                {
                    return permanent.TopCard.EqualsCardName("Titamon")
                        && permanent.TopCard.CardColors.Distinct().Count() != 3;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: Condition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                bool Condition(Permanent permanent)
                {
                    return permanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 5, permanentCondition: Condition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Shared OP / WD / WA

            string SharedHashString = "BT25_084_OP_WD_WA";

            string SharedEffectName = "By trashing 1 card in your hand, delete all opponent's highest DP digimon. If entering by effect, trash their top security.";

            string SharedEffectDescription(string tag) 
                => $"[{tag}] [Once Per Turn] By trashing 1 card in your hand, delete all of your opponent's highest DP Digimon. After, if played or digivolved by an effect, trash their top security card.";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    additionalActivateCondition: AdditionalActivateCondition,
                    maxCountPerTurn: 1,
                    hashValue: SharedHashString,
                    optional: false,
                    isSkippable: true,
                    onPlay: true,
                    whenDigivolving: true,
                    whenAttacking: true);

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass) => card.Owner.HandCards.Count() > 0;

            bool EnteredByEffect(Hashtable hashtable)
            {
                return (CardEffectCommons.CanTriggerOnPlay(hashtable, card)
                        || CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                    && CardEffectCommons.IsByEffect(hashtable, null);
            }

            bool StrongestEnemyDigimon(Permanent permanent) => CardEffectCommons.IsMaxDP(permanent, card.Owner.Enemy, null);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool discarded = false;

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: _ => true,
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
                    if (cardSources.Count() >= 1)
                    {
                        discarded = true;

                        yield return null;
                    }
                }

                if (discarded)
                {
                    List<Permanent> targetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(StrongestEnemyDigimon);
                    yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(targetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());

                    if (EnteredByEffect(hashtable))
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner.Enemy,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());
                    }
                }
                else
                {
                    activateClass.RemoveUse();
                }
            }

            #endregion

            #region All Turns - When would leave
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 2 cards from hand to prevent this Digimon from leaving play", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT25_084_AT");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon would leave the battle area, by trashing 2 cards in your hand, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && card.Owner.HandCards.Count() >= 2;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool discarded = false;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: _ => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 2,
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
                        if (cardSources.Count() >= 2)
                        {
                            discarded = true;

                            yield return null;
                        }
                    }

                    if (discarded)
                    {
                        Permanent thisPermanent = card.PermanentOfThisCard();

                        thisPermanent.willBeRemoveField = false;

                        thisPermanent.HideDeleteEffect();
                        thisPermanent.HideHandBounceEffect();
                        thisPermanent.HideDeckBounceEffect();
                        thisPermanent.HideWillRemoveFieldEffect();
                    }
                    else
                    {
                        activateClass.RemoveUse();
                    }
                }
            }
            #endregion

            #region All Turns, when hand trashed
            if (timing == EffectTiming.OnDiscardHand)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 of your opponent's lowest DP Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When your hand is trashed from, delete 1 of your opponent's lowest DP Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnTrashHand(hashtable, null, cardSource => cardSource.Owner == card.Owner);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool IsWeakestEnemyDigimon(Permanent permanent) => CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy, null);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsWeakestEnemyDigimon))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsWeakestEnemyDigimon,
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

            return cardEffects;
        }
    }
}
