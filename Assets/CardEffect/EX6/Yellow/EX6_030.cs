using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCGO.CardEffects.EX6
{
    public class EX6_030 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Play 1 level 5 or lower Digimon from security, then 1 of your opponent's Digimon gets -7000 DP until the end of the turn",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] Search your security stack. You may play 1 level 5 or lower Digimon card with the [Angel]/[Archangel] trait among them without paying the cost. Then, shuffle your security stack, and 1 of your opponent's Digimon gets -7000 DP until the end of the turn.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.Level <= 5)
                        {
                            if (cardSource.CardTraits.Contains("Angel") || cardSource.CardTraits.Contains("Archangel"))
                            {
                                if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false,
                                        cardEffect: activateClass))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        int maxCount = Math.Min(1, card.Owner.SecurityCards.Count(CanSelectCardCondition));

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Select 1 Digimon to play.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Security,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                    player: card.Owner,
                                    refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
                            }

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayPermanentCards(cardSources: selectedCards,
                                activateClass: activateClass, payCost: false, isTapped: false,
                                root: SelectCardEffect.Root.Security, activateETB: true));
                    }

                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        ContinuousController.instance.PlaySE(GManager.instance.ShuffleSE);

                        card.Owner.SecurityCards = RandomUtility.ShuffledDeckCards(card.Owner.SecurityCards);
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1,
                            CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -7000.",
                            "The opponent is selecting 1 Digimon that will get DP -7000.");

                        yield return ContinuousController.instance.StartCoroutine(
                            selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent,
                                    changeValue: -7000, effectDuration: EffectDuration.UntilEachTurnEnd,
                                    activateClass: activateClass));
                        }
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "By trashing the top card of your security stack, prevent one Digimon from leaving the battle area",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] When one of your Digimon with the [Angel]/[Archangel]/[Three Great Angels] trait would leave the battle area other than in battle, by trashing the top card of your security stack, prevent it from leaving.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasAngelTraitRestrictive)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition))
                        {
                            if (!CardEffectCommons.IsByBattle(hashtable) &&
                                !CardEffectCommons.IsByEffect(hashtable,
                                    cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card)))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.SecurityCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        List<Permanent> removedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable);

                        removedPermanents = removedPermanents.Filter(PermanentCondition);
                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                    player: card.Owner,
                                    destroySecurityCount: 1,
                                    cardEffect: activateClass,
                                    fromTop: true).DestroySecurity());

                            foreach(Permanent permanent in removedPermanents)
                            {
                                permanent.willBeRemoveField = false;
                                permanent.HideDeleteEffect();
                                permanent.HideHandBounceEffect();
                                permanent.HideDeckBounceEffect();
                                permanent.HideWillRemoveFieldEffect();
                            }
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
