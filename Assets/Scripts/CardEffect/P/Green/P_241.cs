using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Yujin Ozora
namespace DCGO.CardEffects.P
{
    public class P_241 : CEntity_Effect
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

            #region Your Turn - When Linked
            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 of your [Appmon] gains <Vortex> and +3K DP for turn, then you may app fuse", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] When your Digimon get linked, by suspending this Tamer, 1 of your Digimon with the [Appmon] trait gains <Vortex> and +3000 DP for the turn. Then, 1 of your Digimon may app fuse into a Digimon card in the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerWhenLinked(hashtable, LinkPermanentCondition, null);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool LinkPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanSelectFusePermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        foreach(CardSource hand in card.Owner.HandCards)
                        {
                            if (hand.appFusionCondition != null)
                            {
                                if (hand.CanAppFusionFromTargetPermanent(permanent, true))
                                    return true;
                            }
                        }
                        
                    }
                    return false;
                }
                bool CanSelectCardCondition(CardSource card, Permanent permanent)
                {
                    return CardEffectCommons.IsExistOnHand(card)
                        && card.appFusionCondition != null
                        && card.CanAppFusionFromTargetPermanent(permanent, true);
                }

                bool CanSelectBuffPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("Appmon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    #region Give DP and Vortex
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectBuffPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectBuffPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectBuffPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to give <Vortex> and +3000 DP", "The opponent is selecting 1 Digimon to give <Vortex> and +3000 DP");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainVortex(targetPermanent: permanent, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: 3000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                        }
                    }
                    #endregion

                    #region App Fuse
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectFusePermanentCondition))
                    {
                        Permanent selectedPermanent = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, CanSelectFusePermanentCondition));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectFusePermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to app fuse", "The opponent is selecting 1 digimon to app fuse");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            CardSource selectedCard = null;
                            int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, handCard => CanSelectCardCondition(handCard, selectedPermanent)));
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: handCard => CanSelectCardCondition(handCard, selectedPermanent),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select digimon to app fuse into.", "The opponent is selecting digimon to app fuse into.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Selected digimon");
                            yield return StartCoroutine(selectHandEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCard = cardSource;
                                yield return null;
                            }

                            if (selectedCard != null && selectedCard.CanAppFusionFromTargetPermanent(selectedPermanent, true))
                            {
                                CardSource linkCard = selectedPermanent.LinkedCards.Where(x => selectedCard.appFusionCondition.linkedCondition(selectedPermanent, x)).First();

                                PlayCardClass playCardClass = new PlayCardClass(new List<CardSource> { selectedCard }, hashtable, true, selectedPermanent, false, SelectCardEffect.Root.Hand, true);
                                playCardClass.SetAppFusion(new int[] { selectedPermanent.PermanentFrame.FrameID, selectedPermanent.LinkedCards.IndexOf(linkCard) });

                                yield return ContinuousController.instance.StartCoroutine(playCardClass.PlayCard());
                            }
                        }
                    }
                    #endregion
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
