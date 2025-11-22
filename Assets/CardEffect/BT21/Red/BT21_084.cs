using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR;

//Haru Shinkai
namespace DCGO.CardEffects.BT21
{
    public class BT21_084 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Turn

            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }

            #endregion

            #region Your Turn - When Linked
            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1, then you may app fuse", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When your Digimon get linked, by suspending this Tamer, <Draw 1> (Draw 1 card from your deck). Then, 1 of your Digimon may app fuse into a Digimon card in the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenLinked(hashtable, LinkPermanentCondition, null))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                        {
                            if (CardEffectCommons.IsOwnerTurn(card))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool LinkPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanSelectPermanent(Permanent permanent)
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
                bool CanSelectCard(CardSource card, Permanent permanent)
                {
                    if (CardEffectCommons.IsExistOnHand(card))
                    {
                        if (card.appFusionCondition != null)
                        {
                            if (card.CanAppFusionFromTargetPermanent(permanent, true))
                                return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanent))
                    {
                        Permanent selectedPermanent = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, CanSelectPermanent));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanent,
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
                            int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, handCard => CanSelectCard(handCard, selectedPermanent)));
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: handCard => CanSelectCard(handCard, selectedPermanent),
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