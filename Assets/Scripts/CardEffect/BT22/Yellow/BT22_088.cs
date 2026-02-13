using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Arisa Kinosaki
namespace DCGO.CardEffects.BT22
{
    public class BT22_088 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start Of Your Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By bottom decking, Play 1 [Arisa Kinosaki] from hand. then if you have no digimon, Play 1 [Shoemon] from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] By returning this Tamer to the bottom of the deck, you may play 1 [Arisa Kinosaki] from your hand without paying the cost. Then, if you don't have a Digimon, you may play 1 [Shoemon] from your trash without paying the cost.";
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

                bool IsArisaKinosaki(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnHand(cardSource)
                        && cardSource.EqualsCardName("Arisa Kinosaki")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                bool IsShoemon(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnTrash(cardSource)
                        && cardSource.EqualsCardName("Shoemon")
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
                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, IsArisaKinosaki))
                        {
                            CardSource selectedHandCard = null;

                            #region Select Hand card

                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersCardCountInHand(card, IsArisaKinosaki));
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsArisaKinosaki,
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

                            selectHandEffect.SetUpCustomMessage("Select 1 [Arisa Kinosaki] to play", "The opponent is selecting 1 [Arisa Kinosaki] to play");
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

                        if (!card.Owner.GetBattleAreaDigimons().Any() && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsShoemon))
                        {
                            CardSource selectedTrashCard = null;

                            #region Select Trash card

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: IsShoemon,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 [Shoemon] to play",
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

            #region All turns

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When any of your Tokens or [Puppet] trait Digimon are played, by suspending this Tamer, <Draw 1> (Draw 1 card from your deck.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.IsToken || permanent.TopCard.HasPuppetTraits);
                }
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent> { card.PermanentOfThisCard() }, hashtable).Tap());
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
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