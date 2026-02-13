using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.EX11
{
    public class EX11_061 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Turn

            if (timing == EffectTiming.OnStartMainPhase)
            {
                  cardEffects.Add(CardEffectFactory.Gain1MemoryTamerOpponentDigimonEffect(card));
            }
            
            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend tamer, play lvl 3 [Puppet], delete it at end of turns.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When any of your Digimon digivolve into a [Puppet] trait Digimon, by suspending this Tamer, you may play 1 level 3 [Puppet] trait Digimon card from your hand without paying the cost. At turn end, delete the Digimon this effect played.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool PermanentCondition (Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("Puppet");
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool IsLevel3PuppetCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.IsLevel3 
                        && cardSource.ContainsTraits("Puppet") 
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                        new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, IsLevel3PuppetCardCondition))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsLevel3PuppetCardCondition,
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

                        selectHandEffect.SetUpCustomMessage("Select 1 Digimon card to play.", "The opponent is selecting 1 Digimon card to play.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                        if (selectedCards.Count> 0)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass,
                                payCost: false, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));

                            Permanent playedDigimon = null;

                            yield return new WaitForSeconds(0.2f);

                            playedDigimon = selectedCards[0].PermanentOfThisCard();            

                            #region Delete Played Digimon

                            Permanent selectedPermanent = selectedCards[0].PermanentOfThisCard();

                            ActivateClass activateClass1 = new ActivateClass();
                            activateClass1.SetUpICardEffect("Delete this Digimon", CanUseCondition2, card);
                            activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, "");
                            activateClass1.SetEffectSourcePermanent(selectedPermanent);
                            CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: activateClass1, timing: EffectTiming.OnEndTurn);

                            bool CanUseCondition2(Hashtable hashtable)
                            {
                                return true;
                            }

                            bool CanActivateCondition1(Hashtable hashtable)
                            {
                                return selectedPermanent.TopCard != null
                                    && selectedPermanent.CanBeDestroyedBySkill(activateClass1)
                                    && !selectedPermanent.TopCard.CanNotBeAffected(activateClass1);
                            }

                            IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(new List<Permanent>() { selectedPermanent }, CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                            }

                            #endregion

                        }
                    }
                }
            }

            #endregion

            #region Security Skill

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}
