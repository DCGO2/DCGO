using System;
using System.Collections;
using System.Collections.Generic;

//Defense Plug-In F
namespace DCGO.CardEffects.ST22
{
    public class ST22_11 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Ignore Colour Requirement

            if (timing == EffectTiming.None)
            {

                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsTamer);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

            }

            #endregion

            #region Link Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasLevel && targetPermanent.Level >= 3;
                }

                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 2, card: card));
            }

            #endregion

            #region Link

            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
            }

            #endregion

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("De-Digivolve(2) 1 opponent digimon. Then, add this card to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Security] <De-Digivolve 2> 1 of your opponent's Digimon. Then, add this card to the hand.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }


                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region De-Digivolve(2) 1 opponent digimon

                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(new IDegeneration(
                            permanent: selectedPermanent,
                            DegenerationCount: 2,
                            cardEffect: activateClass).Degeneration());

                    }

                    #endregion

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }

            #endregion

            #region Main

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may link this to 1 digimon. Then, 1 digimon gains <Reboot> and 3K DP until their turn ends", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Main] You may link this card to 1 of your Digimon without paying the cost. Then, until your opponent's turn ends, 1 of your Digimon gains <Reboot> and +3000 DP.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                            card.CanLinkToTargetPermanent(permanent, false);
                }

                bool CanSelectDigimonPermamentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region Select Digimon To Link

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {


                        Permanent selectedPermanent = null;
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to link.", "The opponent is selecting 1 Digimon to link.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }



                        if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddLinkCard(card, activateClass));
                    }

                    #endregion

                    #region Select Digimon to gain <Reboot> & 3K DP

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDigimonPermamentCondition, card))
                    {
                        Permanent selectedPermanent = null;
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectDigimonPermamentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDigimonPermamentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainReboot(
                            targetPermanent: selectedPermanent,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));

                        if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: selectedPermanent,
                            changeValue: 3000,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));

                    }

                    #endregion
                }
            }

            #endregion

            #region Link ESS

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null, isLinkedEffect: true));
            }

            #endregion

            return cardEffects;
        }
    }
}
