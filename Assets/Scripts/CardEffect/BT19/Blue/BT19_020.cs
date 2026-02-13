using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT19
{
    public class BT19_020 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Rush

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card, condition: null));
            }

            #endregion

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Tamer with [Kiriha Aonuma] in its name from hand, then <Save>", CanUseCondition,
                    card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Deletion] If you have 1 or fewer Tamers, you may play 1 [Kiriha Aonuma] from your hand without paying the cost. Then, <Save>.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool IsTamerCardCondition(CardSource cardSource)
                {
                    return cardSource.IsTamer &&
                           cardSource.EqualsCardName("Kiriha Aonuma") &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                bool CanSelectSaveTamerPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                           && permanent.IsTamer;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card) &&
                           ((CardEffectCommons.HasMatchConditionOwnersHand(card, IsTamerCardCondition) &&
                             card.Owner.GetBattleAreaPermanents().Count(permanent => permanent.IsTamer) <= 1) ||
                            CardEffectCommons.CanActivateSave(hashtable, CanSelectSaveTamerPermanentCondition));
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, IsTamerCardCondition) &&
                        card.Owner.GetBattleAreaPermanents().Count(permanent => permanent.IsTamer) <= 1)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsTamerCardCondition,
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

                        selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false,
                                root: SelectCardEffect.Root.Hand, activateETB: true));
                        }
                    }

                    if (CardEffectCommons.CanActivateSave(hashtable, CanSelectSaveTamerPermanentCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SaveProcess(hashtable, activateClass, card, CanSelectSaveTamerPermanentCondition));
                    }
                }
            }

            #endregion

            #region Reboot - ESS

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: null));
            }

            #endregion

            return cardEffects;
        }
    }
}