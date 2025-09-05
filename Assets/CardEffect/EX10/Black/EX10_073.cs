using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//EX10 Deusmon
namespace DCGO.CardEffects.EX10
{
    public class EX10_073 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static effects
            #region Link +1

            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.ChangeSelfLinkMaxStaticEffect(1, false, card, null));

            #endregion

            #region Security Atk +1

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region App Fusion (Warudamon & Cometmon)

            if (timing == EffectTiming.None)
            {
                AddAppFusionConditionClass addAppFusionConditionClass = new AddAppFusionConditionClass();
                addAppFusionConditionClass.SetUpICardEffect($"App Fusion", (hashtable) => true, card);
                addAppFusionConditionClass.SetUpAddAppFusionConditionClass(getAppFusionCondition: GetAppFusion);
                addAppFusionConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAppFusionConditionClass);

                AppFusionCondition GetAppFusion(CardSource cardSource)
                {
                    bool linkCondition(Permanent permanent, CardSource source)
                    {
                        if (source != null)
                        {
                            if (source != card)
                            {
                                if (permanent.TopCard.EqualsCardName("Warudamon"))
                                {
                                    if (permanent.LinkedCards.Find(x => x.EqualsCardName("Cometmon")))
                                    {
                                        return true;
                                    }
                                }
                                if (permanent.TopCard.EqualsCardName("Cometmon"))
                                {
                                    if (permanent.LinkedCards.Find(x => x.EqualsCardName("Warudamon")))
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        return false;
                    }
                    bool digimonCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        {
                            if (permanent.TopCard.EqualsCardName("Warudamon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Cometmon")))
                                {
                                    return true;
                                }
                            }
                            if (permanent.TopCard.EqualsCardName("Cometmon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Warudamon")))
                                {
                                    return true;
                                }
                            }
                            return false;
                        }

                        return false;
                    }

                    if (cardSource == card)
                    {
                        AppFusionCondition AppFusionCondition = new AppFusionCondition(
                            linkedCondition: linkCondition,
                            digimonCondition: digimonCondition,
                            cost: 0);

                        return AppFusionCondition;
                    }

                    return null;
                }
            }

            #endregion
            #endregion

            #region When Digivolving/EoOT shared
            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                    (card.Owner.HandCards.Count > 0 || card.PermanentOfThisCard().cardSources.Any(source => source.CanLinkToTargetPermanent(card.PermanentOfThisCard(), false)));
            }

            bool CanSelectLinkCard(CardSource cardSource)
            {
                return cardSource.CanLinkToTargetPermanent(card.PermanentOfThisCard(), false);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool hasInHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectLinkCard);
                bool hasInSources = card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectLinkCard) > 0;
                CardSource selectedCard = null;

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCard = cardSource;
                    yield return null;
                }

                if (hasInHand)
                {
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                    selectCardEffect.SetUp(
                                canTargetCondition: CanSelectLinkCard,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to link.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 card to link", "The opponent is selecting 1 card to link");

                    yield return StartCoroutine(selectCardEffect.Activate());
                }

                if (selectedCard != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddLinkCard(selectedCard, activateClass));
                    selectedCard = null;
                }

                if (hasInSources)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                    selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectLinkCard,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);
                    selectHandEffect.SetUpCustomMessage("Select 1 card to link", "The opponent is selecting 1 card to link");

                    yield return StartCoroutine(selectHandEffect.Activate());
                }

                if (selectedCard != null)
                    yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddLinkCard(selectedCard, activateClass));

            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add new links to tgus digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, (hashtable) => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[When Digivolving] You may link 1 Digimon card from your hand to this Digimon without paying the cost. Then, you may link 1 Digimon card from this Digimon's digivolution cards to this Digimon without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            #endregion

            #region End Of Opponent's Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add new links to this digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, (hashtable) => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[End Of Opponent's Turn] You may link 1 Digimon card from your hand to this Digimon without paying the cost. Then, you may link 1 Digimon card from this Digimon's digivolution cards to this Digimon without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            #endregion

            #region All Turns
            if(timing == EffectTiming.OnLinkCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete when trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX10-073AllTurns");
                cardEffects.Add(activateClass);

                string EffectDescription() => "[All Turns] [Once Per Turn] When effects trash any of this Digimon's link cards, delete 1 of your opponent's Digimon with the lowest play cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && 
                        CardEffectCommons.CanTriggerOnTrashLinkedCard(hashtable, perm => perm == card.PermanentOfThisCard(), cardEffect => cardEffect != null, source => source != null);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentsDigimon);
                }

                bool IsOpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, true, null);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentsDigimon))
                    {
                        #region Delete Lowest Play Cost Digimon

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOpponentsPermanentCount(card, IsOpponentsDigimon));
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

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        #endregion
                    }
                }
            }
            #endregion
            return cardEffects;
        }
    }
}