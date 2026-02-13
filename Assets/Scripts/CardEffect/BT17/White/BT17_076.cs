using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCGO.CardEffects.BT17
{
    public class BT17_076 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5)
                        return targetPermanent.TopCard.EqualsCardName("Eosmon");

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 4,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Level 5 or lower Eosmon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Play_BT17_076");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] [Once Per Turn] You may play 1 level 5 or lower [Eosmon] from your hand without paying the cost.";
                }

                bool HasProperEosmon(CardSource source)
                {
                    if (source.HasLevel && source.Level <= 5)
                    {
                        if (source.EqualsCardName("Eosmon"))
                        {
                            return CardEffectCommons.CanPlayAsNewPermanent(source, false, activateClass);
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, HasProperEosmon))
                    {
                        int maxCount = Math.Min(1, card.Owner.HandCards.Count(HasProperEosmon));

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: HasProperEosmon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: SelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select [Eosmon] to play.", "The opponent is selecting an [Eosmon] to play.");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(List<CardSource> selectedCards)
                        {
                            if (selectedCards.Count > 0)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: selectedCards,
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Hand,
                                    activateETB: true));
                            }
                        }
                    }
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Level 5 or lower Eosmon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Play_BT17_076");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] [Once Per Turn] You may play 1 level 5 or lower [Eosmon] from your hand without paying the cost.";
                }

                bool HasProperEosmon(CardSource source)
                {
                    if (source.HasLevel && source.Level <= 5)
                    {
                        if (source.EqualsCardName("Eosmon"))
                        {
                            return CardEffectCommons.CanPlayAsNewPermanent(source, false, activateClass);
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, HasProperEosmon))
                    {
                        int maxCount = Math.Min(1, card.Owner.HandCards.Count(HasProperEosmon));

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: HasProperEosmon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: SelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select [Eosmon] to play.", "The opponent is selecting an [Eosmon] to play.");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(List<CardSource> selectedCards)
                        {
                            if (selectedCards.Count > 0)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: selectedCards,
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Hand,
                                    activateETB: true));
                            }
                        }
                    }
                }
            }
            #endregion

            #region All Turns - When [Eosmon] is played
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                List<Permanent> playedPermanents = null;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete opponents Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("Delete_BT17_076");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When one of your [Eosmon] is played, delete 1 of your opponent's Digimon with as much or less DP as that Digimon.";
                }

                bool IsEosmon(Permanent permanent)
                {
                    if (CardEffectCommons.IsOwnerPermanent(permanent, card))
                        return permanent.TopCard.EqualsCardName("Eosmon");

                    return false;
                }

                bool IsOpponentsDigimon(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        foreach(Permanent played in playedPermanents)
                        {
                            if(played.DP >= permanent.DP)
                                return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, IsEosmon);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    playedPermanents = CardEffectCommons.GetPlayedPermanentsFromEnterFieldHashtable(hashtable, null);

                    if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsOpponentsDigimon));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to destory.", "The opponent is selecting 1 Digimon to destory.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            #region All Turns - +1000DP
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if(CardEffectCommons.IsOwnerTurn(card))
                        return CardEffectCommons.IsExistOnBattleAreaDigimon(card);

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.EqualsCardName("Eosmon"))
                            return true;
                    }

                    return false;
                }

                bool IsTamer(Permanent permanent)
                {
                    return permanent.IsTamer;
                }

                int DPIncrease()
                {
                    return CardEffectCommons.MatchConditionPermanentCount(IsTamer) * 1000;
                }

                cardEffects.Add(CardEffectFactory.ChangeDPStaticEffect(
                    permanentCondition: PermanentCondition,
                    changeValue: DPIncrease(),
                    isInheritedEffect: false,
                    card: card,
                    condition: Condition,
                    effectName: () => "[Your Turn] For each Tamer, all of your [Eosmon] get +1000 DP."));
            }
            #endregion

            return cardEffects;
        }
    }
}