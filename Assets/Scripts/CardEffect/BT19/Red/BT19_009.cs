using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT19
{
    public class BT19_009 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Tamer with [Takato Matsuki] in its name from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[When Digivolving] If you have 1 or fewer Tamers, you may play 1 [Takato Matsuki] from your hand without paying the cost.";
                }

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

                bool IsTakatoCardCondition(CardSource cardSource)
                {
                    return cardSource.IsTamer && (cardSource.EqualsCardName("Takato Matsuki") && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) && CardEffectCommons.HasMatchConditionOwnersHand(card, IsTakatoCardCondition) && card.Owner.GetBattleAreaPermanents().Count(permanent => permanent.IsTamer) <= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                     List<CardSource> selectedCards = new List<CardSource>();

                     SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                     selectHandEffect.SetUp(
                         selectPlayer: card.Owner,
                         canTargetCondition: IsTakatoCardCondition,
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

                         yield return null;
                     }

                     yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));                 
                }
            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.None)
            {
                ChangeDPDeleteEffectMaxDPClass changeDPDeleteEffectMaxDPClass = new ChangeDPDeleteEffectMaxDPClass();
                changeDPDeleteEffectMaxDPClass.SetUpICardEffect("Maximum DP of DP-based deletion effects gets +2000 DP if 0 or less memory", CanUseCondition, card);
                changeDPDeleteEffectMaxDPClass.SetUpChangeDPDeleteEffectMaxDPClass(changeMaxDP: ChangeMaxDP);
                changeDPDeleteEffectMaxDPClass.SetIsInheritedEffect(true);
                cardEffects.Add(changeDPDeleteEffectMaxDPClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.MemoryForPlayer <= 0)
                        {
                            if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                int ChangeMaxDP(int maxDP, ICardEffect cardEffect)
                {
                    if (cardEffect != null)
                    {
                        if (cardEffect.EffectSourceCard != null)
                        {
                            if (cardEffect.EffectSourceCard.Owner == card.Owner)
                            {
                                if (cardEffect.EffectSourceCard.PermanentOfThisCard() == card.PermanentOfThisCard()) maxDP += 2000;
                            }
                        }
                    }

                    return maxDP;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}