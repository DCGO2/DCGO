using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// arisa Kinosaki
namespace DCGO.CardEffects.EX11
{
    public class EX11_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Main
            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ();
                activateClass.SetUpICardEffect("By suspending this, Draw 1. If deleted by Overclock, you may play 1 level 4 or lower Puppet Digimon from hand.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() => "[All Turns] When any of your Tokens or [Puppet] trait Digimon are deleted, by suspending this Tamer, <Draw 1>. If deleted by <Overclock>, you may play 1 level 4 or lower [Puppet] trait Digimon card from your hand without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.IsToken || permanent.TopCard.EqualsTraits("Puppet"));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool CardSelectCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel
                        && cardSource.Level <= 4
                        && cardSource.EqualsTraits("Puppet");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    
                    ICardEffect triggeringEffect = CardEffectCommons.GetCardEffectFromHashtable(hashtable);

                    if (triggeringEffect != null 
                        && triggeringEffect.EffectName == "Overclock" 
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CardSelectCondition))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CardSelectCondition,
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

                        selectHandEffect.SetUpCustomMessage("Select 1 digimon to play", "The opponent is selecting 1 digimon to play");

                        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        if (selectedCards.Count > 0) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true));
                    }
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
