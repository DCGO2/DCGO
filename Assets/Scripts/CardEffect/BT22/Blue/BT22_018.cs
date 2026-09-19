using System.Collections;
using System.Collections.Generic;

//BT22 Sangomon
namespace DCGO.CardEffects.BT22
{
    public class BT22_018 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place under to give blocker and battle immunity", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[On Play] By placing this Digimon as the bottom digivolution card of any of your other Digimon with [Aqua] or [Sea Animal] in any of their traits, until your opponent's turn ends, that Digimon gains <Blocker> and can't be deleted in battle.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPlay(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent != card.PermanentOfThisCard())
                        {
                            if (!permanent.TopCard.Equals(card))
                                return permanent.TopCard.ContainsTraits("Aqua") || permanent.TopCard.ContainsTraits("Sea Animal");
                        }
                    }

                    return false;
                }
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectPermanentEffect selectPermanentEffect =
                        GManager.instance.GetComponent<SelectPermanentEffect>();

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

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Digimon to place this Digimon beneath.",
                        "The opponent is selecting 1 Digimon to place this Digimon beneath.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedCards.Add(permanent.TopCard);

                        yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(
                            new List<Permanent[]>() { new Permanent[] { card.PermanentOfThisCard(), permanent } },
                            false,
                            activateClass).PlacePermanentToDigivolutionCards());

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(targetPermanent: permanent, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBeDeletedByBattle(
                            targetPermanent: permanent,
                            canNotBeDestroyedByBattleCondition: CanNotBeDestroyedByBattleCondition,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            effectName: "Can't be deleted in battle"));

                        bool CanNotBeDestroyedByBattleCondition(Permanent permanent, Permanent attackingPermanent,
                            Permanent defendingPermanent, CardSource defendingCard)
                        {
                            return permanent == attackingPermanent || permanent == defendingPermanent;
                        }
                    }

                }
            }
            #endregion

            #region ESS
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: null));
            }
            #endregion  

            return cardEffects;
        }
    }
}