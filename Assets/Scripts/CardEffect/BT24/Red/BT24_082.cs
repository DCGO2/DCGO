using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Owen Dreadnought
namespace DCGO.CardEffects.BT24
{
    public class BT24_082 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start Of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By bottom decking, Play 1 [Owen Dreadnought] from hand. Then if you have no digimon, Play 1 [Elizamon] from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By returning this Tamer to the bottom of the deck, you may play 1 [Owen Dreadnought] from your hand without paying the cost. Then, if you don't have a Digimon, you may play 1 [Elizamon] from your trash without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool IsOwenDreadnought(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnHand(cardSource)
                        && cardSource.EqualsCardName("Owen Dreadnought")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                bool IsElizamon(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnTrash(cardSource)
                        && cardSource.EqualsCardName("Elizamon")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass, SelectCardEffect.Root.Trash);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.DeckBouncePeremanentAndProcessAccordingToResult(
                            targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                            activateClass: activateClass,
                            successProcess: SuccessProcess(),
                            failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, IsOwenDreadnought))
                        {
                            CardSource selectedHandCard = null;

                            #region Select Hand card

                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, IsOwenDreadnought));
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsOwenDreadnought,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedHandCard = cardSource;
                                yield return null;
                            }

                            selectHandEffect.SetUpCustomMessage("Select 1 [Owen Dreadnought] to play", "The opponent is selecting 1 [Owen Dreadnought] to play");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Selected card");
                            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                            #endregion

                            if (selectedHandCard != null) yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.PlayPermanentCards(
                                    cardSources: new List<CardSource>() { selectedHandCard },
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Hand,
                                    activateETB: true));
                        }

                        if (!card.Owner.GetBattleAreaDigimons().Any() && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsElizamon))
                        {
                            CardSource selectedTrashCard = null;

                            #region Select Trash card

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: IsElizamon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 [Elizamon] to play",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedTrashCard = cardSource;
                                yield return null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            #endregion

                            if (selectedTrashCard != null) yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.PlayPermanentCards(
                                    cardSources: new List<CardSource>() { selectedTrashCard },
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Trash,
                                    activateETB: true));
                        }
                    }
                }
            }

            #endregion

            #region Your turns

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("+3k and may attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] When any of your Digimon digivolve into a [Reptile] or [Dragonkin] trait Digimon, by suspending this Tamer, that Digimon gets +3000 DP for the turn. Then, it may attack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.EqualsTraits("Reptile") || permanent.TopCard.EqualsTraits("Dragonkin"));
                }
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                        new List<Permanent>() { card.PermanentOfThisCard() },CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    Permanent digivolvedPermanent = CardEffectCommons.GetPlayedPermanentsFromEnterFieldHashtable(hashtable, null)[0];

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                        targetPermanent: digivolvedPermanent, 
                        changeValue: 3000, 
                        effectDuration: EffectDuration.UntilEachTurnEnd, 
                        activateClass: activateClass));

                    if (digivolvedPermanent.CanAttack(activateClass))
                    {
                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: digivolvedPermanent,
                            canAttackPlayerCondition: () => true,
                            defenderCondition: _ => true,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }
                }
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