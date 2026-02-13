using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Rei Katsura
namespace DCGO.CardEffects.BT24
{
    public class BT24_087 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.Gain1MemoryTamerOpponentDigimonEffect(card));
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1, Trash 1, then you may app fuse", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When any of your Digimon get linked, by suspending this Tamer, <Draw 1> and trash 1 card in your hand. Then, 1 of your Digimon may app fuse into a Digimon card with the [System], [Life] or [Transmutation] trait in the trash.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenLinked(hashtable, LinkPermanentCondition, null)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool LinkPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanSelectPermanent(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        foreach (CardSource trash in card.Owner.TrashCards)
                        {
                            if (trash.appFusionCondition != null)
                            {
                                if (trash.CanAppFusionFromTargetPermanent(permanent, true, SelectCardEffect.Root.Trash))
                                    return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanSelectCard(CardSource cardSource, Permanent permanent)
                {
                    return cardSource.appFusionCondition != null
                        && cardSource.CanAppFusionFromTargetPermanent(permanent, true, SelectCardEffect.Root.Trash)
                        && (cardSource.EqualsTraits("System")
                            || cardSource.EqualsTraits("Life")
                            || cardSource.EqualsTraits("Transmutation"));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent> { card.PermanentOfThisCard() }, hashtable).Tap());

                    if (card.PermanentOfThisCard().IsSuspended)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                        if (card.Owner.HandCards.Count > 0)
                        {
                            int discardCount = 1;

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: (cardSource) => true,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: discardCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Discard,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }

                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanent))
                        {
                            Permanent selectedPermanent = null;

                            #region Select App Fuse Permament Target

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

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;
                                yield return null;
                            }

                            selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to app fuse", "The opponent is selecting 1 digimon to app fuse");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            #endregion

                            if (selectedPermanent != null)
                            {
                                CardSource selectedCard = null;

                                #region Select App Fusion Hand Target

                                int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, trashCard => CanSelectCard(trashCard, selectedPermanent)));
                                List<CardSource> selectedCards = new List<CardSource>();

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                            canTargetCondition: trashCard => CanSelectCard(trashCard, selectedPermanent),
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: null,
                                            canNoSelect: () => false,
                                            selectCardCoroutine: SelectCardCoroutine,
                                            afterSelectCardCoroutine: null,
                                            message: "Select 1 digivolution card to play.",
                                            maxCount: maxCount1,
                                            canEndNotMax: false,
                                            isShowOpponent: true,
                                            mode: SelectCardEffect.Mode.Custom,
                                            root: SelectCardEffect.Root.Trash,
                                            customRootCardList: null,
                                            canLookReverseCard: true,
                                            selectPlayer: card.Owner,
                                            cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage("Select 1 card to digivolve into.", "The opponent is selecting 1 card to digivolve into.");

                                yield return StartCoroutine(selectCardEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCard = cardSource;

                                    yield return null;
                                }

                                selectCardEffect.SetUpCustomMessage("Select digimon to app fuse into.", "The opponent is selecting digimon to app fuse into.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Selected digimon");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                #endregion

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