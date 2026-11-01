using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Bagramon
namespace DCGO.CardEffects.BT11
{
    public class BT11_088 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared OP/WD
            string SharedEffectName = "Depending on enemy Digimon count, look at opponent's hand and trash 1 or place 1 enemy Digimon under 1 other Digimon";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] If your opponent has 1 or fewer Digimon in play, look at your opponent's hand and trash 1 card in it. If your opponent has 2 or more Digimon in play, place 1 of your opponent's Digimon under 1 of your opponent's other Digimon as its bottom digivolution card.";

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.Enemy.GetBattleAreaDigimons().Count <= 1
                && card.Owner.Enemy.HandCards.Count >= 1)
                {
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: (cardSource) => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => false,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 card to discard from opponent's hand.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Discard,
                        root: SelectCardEffect.Root.Custom,
                        customRootCardList: card.Owner.Enemy.HandCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: null);

                    selectCardEffect.SetUpCustomMessage("Select 1 card to discard from opponent's hand.", "The opponent is selecting 1 card to discard from your hand.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                }

                if (card.Owner.Enemy.GetBattleAreaDigimons().Count >= 2)
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that place under other Digimon's digivolution cards.", "The opponent is selecting 1 Digimon that place under other Digimon's digivolution cards.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        bool CanSelectPermanentCondition1(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                                && permanent != selectedPermanent
                                && !permanent.IsToken;
                        }

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition1,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine1,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get digivolution cards.", "The opponent is selecting 1 Digimon that will get digivolution cards.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { selectedPermanent, permanent } }, false, activateClass).PlacePermanentToDigivolutionCards());
                        }
                    }
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnAddDigivolutionCards || timing == EffectTiming.OnEnterFieldAnyone)
            {
                #region Shared All Turns
                string SharedEffectName2() => "Trash 1 of this Digimon's sources to trash enemy top sec";

                string SharedEffectDescription2() => "[All Turns][Once Per Turn] When an opponent's Digimon digivolves or an effect adds cards to the digivolution cards of an opponent's Digimon, by trashing 1 card in this Digimon's digivolution cards, trash the top card of your opponent's security stack.";

                string SharedHashString2 = "BT11_088_AT";

                bool CanSelectCardCondition2(CardSource cardSource, ActivateClass activateClass)
                {
                    return !cardSource.CanNotTrashFromDigivolutionCards(activateClass)
                        || cardSource.IsFaceDown;
                }

                IEnumerator SharedActivateCoroutine2(Hashtable hashtable, ActivateClass activateClass)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: (cardSource) => CanSelectCardCondition2(cardSource, activateClass),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => false,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 digivolution card to discard.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.DigivolutionCards,
                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: null);

                    selectCardEffect.SetUseFaceDown();
                    selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to discard.", "The opponent is selecting 1 digivolution card to discard.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (selectedCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(card.PermanentOfThisCard(), selectedCards, activateClass).TrashDigivolutionCards());

                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner.Enemy,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());
                    }
                }
                #endregion


                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName2(), CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hash => SharedActivateCoroutine2(hash, activateClass), 1, true, SharedEffectDescription2());
                activateClass.SetHashString(SharedHashString2);
                cardEffects.Add(activateClass);

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                    && card.PermanentOfThisCard().DigivolutionCards.Count(cardSource => CanSelectCardCondition2(cardSource, activateClass)) >= 1;

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass) && 
                        (timing == EffectTiming.OnAddDigivolutionCards && CardEffectCommons.CanTriggerOnAddDigivolutionCard(
                            hashtable: hashtable,
                            permanentCondition: PermanentCondition,
                            cardEffectCondition: cardEffect => cardEffect != null,
                            cardCondition: null)) ||
                        (timing == EffectTiming.OnEnterFieldAnyone && CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
