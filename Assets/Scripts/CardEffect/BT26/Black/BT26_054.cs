using System;
using System.Collections;
using System.Collections.Generic;

// Andromon
namespace DCGO.CardEffects.BT26
{
    public class BT26_054 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("CS");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName()
                => "May play 1 [CS] Tamer from hand free (not sharing a name with your Tamers)";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may play 1 Tamer card with the [CS] trait from your hand without paying the cost. This effect can't play cards with the same name as any of your Tamers.";

            bool HasSameNameAsOwnedTamer(CardSource cardSource)
                => CardEffectCommons.HasMatchConditionOwnersPermanent(card, permanent =>
                    permanent.IsTamer && permanent.TopCard.CardNames.Exists(name => cardSource.CardNames.Contains(name)));

            bool CanSelectHandCardCondition(CardSource cardSource, ICardEffect activateClass)
                => cardSource.IsTamer
                    && cardSource.EqualsTraits("CS")
                    && !HasSameNameAsOwnedTamer(cardSource)
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);

            bool SharedAdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => CanSelectHandCardCondition(cardSource, activateClass));

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanSelectHandCardConditionBound(CardSource cardSource) => CanSelectHandCardCondition(cardSource, activateClass);

                if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCardConditionBound))
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: CanSelectHandCardConditionBound,
                        root: SelectCardEffect.Root.Hand,
                        cardEffect: activateClass,
                        payCost: false));
                }
            }

            #endregion

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                additionalActivateCondition: SharedAdditionalActivateCondition,
                onPlay: true,
                whenDigivolving: true);

            #region All Turns
            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [CS] card in hand free", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetHashString("BT26_054_AllTurns");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When effects place [CS] trait Digimon cards in this Digimon's digivolution cards, this Digimon may digivolve into a [CS] trait Digimon card in the hand without paying the cost.";

                bool PermanentCondition(Permanent permanent) => permanent == card.PermanentOfThisCard();

                bool AddedCardCondition(CardSource cardSource) => cardSource.IsDigimon && cardSource.EqualsTraits("CS");

                bool CardCondition(CardSource cardSource) => cardSource.IsDigimon && cardSource.EqualsTraits("CS");

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAddDigivolutionCard(hashtable, PermanentCondition, null, AddedCardCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool isUsed = false;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: CardCondition,
                        payCost: false,
                        reduceCostTuple: null,
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true,
                        activateClass: activateClass,
                        successProcess: SuccessProcess(),
                        isOptional: true));

                    IEnumerator SuccessProcess()
                    {
                        isUsed = true;
                        yield return null;
                    }

                    if (!isUsed) activateClass.RemoveUse();
                }
            }
            #endregion

            #region Inherit - Redirect Attack
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May change the attack target to this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT26_054_Inherit");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Opponent's Turn] [Once Per Turn] When one of your opponent's Digimon attacks, you may change the attack target to this Digimon.";

                bool IsOpponentDigimon(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool IsThisDigimon(Permanent permanent)
                    => permanent == card.PermanentOfThisCard();

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsOpponentTurn(card)
                        && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, IsOpponentDigimon);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsThisDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to attack.", "The opponent is selecting 1 Digimon to attack.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(activateClass, false, permanent));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
