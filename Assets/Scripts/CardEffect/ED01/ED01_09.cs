using System.Collections;
using System.Collections.Generic;

//Bond of Rubies
namespace DCGO.CardEffects.ED01
{
    public class ED01_09 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 red [Gemmon]/[Kanata Hondo] from hand for free, then place in battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] You may play 1 red [Gemmon] or [Kanata Hondo] from your hand without paying the cost. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.CardColors.Contains(CardColor.Red)
                        && (cardSource.EqualsCardName("Gemmon")
                            || cardSource.EqualsCardName("Kanata Hondo"))
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                    { 
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                            canTargetCondition: CanSelectCardCondition,
                            SelectCardEffect.Root.Hand,
                            activateClass,
                            payCost: false
                        ));
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            #endregion

            #region Your turn - Delay
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into a Digimon w/[Vemmon] in text for 3 less.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] When one of your red [ALYSION] trait Digimon attacks, <Delay>. (By trashing this card after the placing turn, activate the effect.)\r\n• That Digimon gains <Security A. +1> and +3000 DP for the turn. (This Digimon checks 1 additional security card.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanDeclareOptionDelayEffect(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, TriggerPermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool TriggerPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.CardColors.Contains(CardColor.Red)
                        && permanent.TopCard.HasAlysianTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        Permanent selectedPermanent = CardEffectCommons.GetAttackerFromHashtable(hashtable);

                        if (selectedPermanent == null) yield break;

                        yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.ChangeDigimonSAttack(
                            targetPermanent: selectedPermanent,
                            changeValue: 1,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                string EffectDescription() => "[Security] Activate this card's [Main] effect.";

                CardEffectCommons.AddActivateMainOptionSecurityEffect(card, ref cardEffects, "Play 1 red [Gemmon]/[Kanata Hondo] from hand for free, then place in battle area", EffectDescription());
            }
            #endregion

            return cardEffects;
        }
    }
}
