using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Lamiamon
namespace DCGO.CardEffects.BT21
{
    public class BT21_025 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Progress

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ProgressSelfStaticEffect(false, card, null));
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnAttackTargetChanged)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("trash top security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("YT_BT21-025");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Your Turn] [Once Per Turn] When any of your [Reptile]/[Dragonkin] trait Digimon's attack targets change, trash your opponent's top security card.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentAttackTargetSwitch(hashtable, PermanentCondition))
                        {
                            if (CardEffectCommons.IsOwnerTurn(card))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }
                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.EqualsTraits("Reptile") || permanent.TopCard.EqualsTraits("Dragonkin"))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }

            #endregion

            #region All Turns (ESS)

            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Reptile] or [Dragonkin] from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("PlayDigimon_BT21_025");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[All Turns] [Once Per Turn] When your opponent's security is removed from, you may play 1 [Reptile]/[Dragonkin] trait Digimon card with 5000 DP or less from your hand without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, PlayerCondition))
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }
                    return false;
                }

                bool PlayerCondition(Player player)
                {
                    if (player == card.Owner.Enemy)
                    {
                        return true;
                    }
                    return false;
                }

                bool CanSelectCardCondition(CardSource source)
                {
                    if (source.IsDigimon)
                    {
                        if (source.EqualsTraits("Reptile") || source.EqualsTraits("Dragonkin"))
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: source, payCost: false, cardEffect: activateClass))
                            {
                                if (source.CardDP <= 5000)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
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
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selectedCard != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: new List<CardSource>() { selectedCard }, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}