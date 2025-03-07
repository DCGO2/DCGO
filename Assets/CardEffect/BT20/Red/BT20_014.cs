using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT20
{
    public class BT20_014 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                {
                    #region On Play
                    ActivateClass activateClass = new ActivateClass();
                    activateClass.SetUpICardEffect("", CanUseCondition, card);
                    activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                    cardEffects.Add(activateClass);

                    string EffectDiscription()
                    {
                        return "[On Play] Delete 1 of your opponent's Digimon with 5000DP or less.";
                    }

                    bool CanUseCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                    }

                    bool CanActivateCondition(Hashtable hashtable)
                    {
                        return card.Owner.Enemy.GetBattleAreaDigimons().Count(CanSelectPermanent) >= 1;
                    }

                    bool CanSelectPermanent(Permanent permanent)
                    {
                        return permanent.DP <= 5000;
                    }

                    IEnumerator ActivateCoroutine(Hashtable hashtable)
                    {
                        List<Permanent> selectedPermanents = new List<Permanent>();

                        int maxCount = 1;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner.Enemy,
                            canTargetCondition: CanSelectPermanent,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass
                            );

                        selectPermanentEffect.SetUpCustomMessage("Select 1 card to delete.", "The opponent is selecting 1 card to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                    #endregion
                }
                {
                    #region On Digivolve
                    ActivateClass activateClass = new ActivateClass();
                    activateClass.SetUpICardEffect("", CanUseCondition, card);
                    activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                    cardEffects.Add(activateClass);

                    string EffectDiscription()
                    {
                        return "[On Digivolve] Delete 1 of your opponent's Digimon with 5000DP or less.";
                    }

                    bool CanUseCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                    }

                    bool CanActivateCondition(Hashtable hashtable)
                    {
                        return card.Owner.Enemy.GetBattleAreaDigimons().Count(CanSelectPermanent) >= 1;
                    }

                    bool CanSelectPermanent(Permanent permanent)
                    {
                        return permanent.DP <= 5000;
                    }

                    IEnumerator ActivateCoroutine(Hashtable hashtable)
                    {
                        List<Permanent> selectedPermanents = new List<Permanent>();

                        int maxCount = 1;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner.Enemy,
                            canTargetCondition: CanSelectPermanent,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass
                            );

                        selectPermanentEffect.SetUpCustomMessage("Select 1 card to delete.", "The opponent is selecting 1 card to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    #endregion
                }
            }

            if (timing == EffectTiming.OnEndTurn)
            {
                #region End of Turn
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] By suspending 1 of your other Digimon, this Digiomon may digivolve into a Digimon card with [Jesmon] in its name in the hand without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Count(CanSelectPlayCardCondition) >= 1)
                    {
                        return true;
                    }
                    return false;
                }

                bool CanSelectPlayCardCondition (CardSource cardSource)
                {
                    if (cardSource.ContainsCardName("Jesmon"))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanSelectSuspendPermanent (Permanent permanent)
                {
                    if (permanent.CanSuspend)
                    {
                        if (permanent.TopCard.Level == 5)
                        {
                            return true;
                        }    
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Count(CanSelectPlayCardCondition) >= 1)
                    {
                        if (card.Owner.GetBattleAreaDigimons().Count(CanSelectSuspendPermanent) >= 1)
                        {

                            #region select card to suspend
                            List<Permanent> selectedPermanents = new List<Permanent>();

                            int maxCount = 1;

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectSuspendPermanent,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: selectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Tap,
                                cardEffect: activateClass
                                );
                            selectPermanentEffect.SetUpCustomMessage("Select 1 card to suspend.", "The opponent is selecting 1 card to suspend.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator selectPermanentCoroutine (Permanent permanent)
                            {
                                selectedPermanents.Add( permanent );
                                yield return null;
                            }
                            #endregion

                            if (selectedPermanents.Count > 0)
                            {
                                #region select card to play
                                List<CardSource> selectedCards = new List<CardSource>();

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPlayCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    message: "Select 1 card to play.",
                                    canNoSelect: () => true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Hand,
                                    customRootCardList: null,
                                    canLookReverseCard: false,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);

                                    yield return null;
                                }

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: selectedCards,
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Hand,
                                    activateETB: true));
                                #endregion

                            }
                        }
                    }
                }
                #endregion
            }

            if (timing == EffectTiming.None)
            {
                #region inherited
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(false, card, null));

                string EffectDiscription()
                {
                    return "[Your Turn] This Digimon with the [Royal Knight] trait gains <Alliance>";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           card.PermanentOfThisCard().TopCard.HasRoyalKnightTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return null;
                }
                #endregion
            }

            return cardEffects;
        }
    }
}