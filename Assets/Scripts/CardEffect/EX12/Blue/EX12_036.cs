using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Ryugumon
namespace DCGO.CardEffects.EX12
{
    public class EX12_036 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition [Digivolve] Lv.5 w/[Aquatic]/[Shambala] trait: Cost 3
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Aquatic")
                        || targetPermanent.TopCard.EqualsTraits("Shambala");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 5)
                );
            }
            #endregion

            #region Barrier
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Evade
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.EvadeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Decode <Lv.5 or lower w/[Aqua]/[Sea Animal] in any trait or w/[TB] trait>
            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel
                        && cardSource.Level <= 5
                        && (cardSource.ContainsTraits("Aqua")
                            || cardSource.ContainsTraits("Sea Animal")
                            || cardSource.EqualsTraits("TB"));
                }

                string[] decodeStrings = { "(Lv.5 or lower w/[Aqua]/[Sea Animal] in any trait or w/[TB] trait)", "Level 5 or lower Digimon card with [Aqua]/[Sea Animal] in any trait or [TB] trait" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: false, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }
            #endregion

            #region Shared OP/WD/WA — Place 1 Lv.6 or lower [Aqua]/[Sea Animal]/[TB] from hand as bottom source, then unsuspend 1 of your Digimon

            string SharedHashString = "EX12_036_OP_WD_WA";

            string SharedEffectName = "Place 1 Lv.6 or lower [Aqua]/[Sea Animal]/[TB] card from hand as bottom source, unsuspend 1 of your Digimon";

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: true,
                maxCountPerTurn: 1,
                hashValue: SharedHashString,
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] By placing 1 level 6 or lower card with [Aqua] or [Sea Animal] in any of its traits or the [TB] trait from your hand as this Digimon's bottom digivolution card, 1 of your Digimon may unsuspend.";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectHandCardCondition);
            }

            bool CanSelectHandCardCondition(CardSource cardSource)
            {
                return cardSource.HasLevel
                    && cardSource.Level <= 6
                    && (cardSource.ContainsTraits("Aqua")
                        || cardSource.ContainsTraits("Sea Animal")
                        || cardSource.EqualsTraits("TB"));
            }

            bool IsYourDigimonCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (SharedCanActivateCondition(hashtable))
                {
                    CardSource selectedCard = null;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectHandCardCondition,
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

                    selectHandEffect.SetUpCustomMessage("Select 1 card to place as the bottom digivolution card.", "The opponent is selecting 1 card to place as the bottom digivolution card.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    if (selectedCard != null)
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { selectedCard }, activateClass));

                            if (CardEffectCommons.HasMatchConditionPermanent(IsYourDigimonCondition))
                            {
                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: IsYourDigimonCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: null,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.UnTap,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            }
                        }
                    }
                }
            }

            #endregion

            #region [All Turns]
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 opponent's Digimon can't activate [When Digivolving] effects or suspend until their turn ends", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX12_036_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When any of your Digimon are played or digivolve, 1 of your opponent's Digimon can't activate [When Digivolving] effects or suspend until their turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, YourDigimonPlayedCondition)
                            || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, YourDigimonPlayedCondition));
                }

                bool YourDigimonPlayedCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon);
                }

                bool IsOpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon))
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsOpponentsDigimon));

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectedPermanent,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectedPermanent(Permanent target)
                        {
                            selectedPermanent = target;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage(
                            "Select 1 Digimon that cant activate [When Digivolving] effects or suspend.",
                            "The opponent is selecting 1 Digimon that will gain cant activate [When Digivolving] effects or suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermanent != null)
                        {
                            // Can't activate [When Digivolving] effects
                            DisableEffectClass invalidationClass = new DisableEffectClass();
                            invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", CanUseConditionDebuff, card);
                            invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => invalidationClass);

                            bool CanUseConditionDebuff(Hashtable hashtableDebuff)
                            {
                                return selectedPermanent.TopCard != null
                                    && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                            }

                            bool InvalidateCondition(ICardEffect cardEffect)
                            {
                                if (selectedPermanent.TopCard != null)
                                {
                                    if (cardEffect != null)
                                    {
                                        if (cardEffect.EffectSourceCard != null)
                                        {
                                            if (isExistOnField(cardEffect.EffectSourceCard))
                                            {
                                                if (cardEffect.EffectSourceCard.PermanentOfThisCard() == selectedPermanent)
                                                {
                                                    if (cardEffect.IsWhenDigivolving)
                                                    {
                                                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                return false;
                            }

                            // Can't suspend
                            CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                            canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseConditionSuspend, card);
                            canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCanNotSuspendCondition);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => canNotSuspendClass);

                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                            }

                            bool CanUseConditionSuspend(Hashtable hashtableSuspend)
                            {
                                return selectedPermanent.TopCard != null
                                    && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                            }

                            bool PermanentCanNotSuspendCondition(Permanent permanentTarget)
                            {
                                return permanentTarget == selectedPermanent;
                            }
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
