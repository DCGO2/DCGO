using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects
{
    public class ImperialdramonPaladinModeAce_BT17_077 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Digivolving
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return true;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DigivolutionCards.Count((cardSource) => !cardSource.CanNotTrashFromDigivolutionCards(activateClass)) >= 1)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    foreach (Permanent selectedPermanent in card.Owner.Enemy.GetBattleAreaDigimons())
                    {
                        if (CanSelectPermanentCondition(selectedPermanent))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: 2, isFromTop: true, activateClass: activateClass));
                        }
                    }
                     if (card.Owner.Enemy.TrashCards.Count >= 1)
                    {
                        if (card.Owner.Enemy.TrashCards.Count == 1)
                        {
                            List<CardSource> libraryCards = card.Owner.Enemy.TrashCards.Clone();

                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(libraryCards));

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(libraryCards, "Deck Bottom Cards", true, true));
                        }

                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                            message: "Select cards to place at the bottom of the deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).",
                            maxCount: -1,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: card.Owner.Enemy.TrashCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                            selectCardEffect.SetNotShowCard();
                            selectCardEffect.SetNotAddLog();

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(cardSources));

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(cardSources, "Deck Bottom Cards", true, true));
                            }
                        }
                    }
                    yield return null;
                }
            }
            #endregion
            
            return cardEffects;
        }
    }
}