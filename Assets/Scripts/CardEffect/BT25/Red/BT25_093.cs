using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Ignition Flare
namespace DCGO.CardEffects.BT25
{
    public class BT25_093 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Link Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent) => targetPermanent.TopCard.HasTSTraits;
                cardEffects.Add(CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 3, card: card));
            }

            #endregion

            #region Link
            if (timing == EffectTiming.OnDeclaration)
            {
                cardEffects.Add(CardEffectFactory.LinkEffect(card));
            }
            #endregion

            #region Use Requirement
            if (timing == EffectTiming.None)
            {
                bool CardCondition(CardSource cardSource) => cardSource.HasTSTraits;
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));
            }
            #endregion

            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete all opponent digimon with lowest DP, if this didnt delete trash 1 opponent option card in battle area. Then you may link this to 1 of your digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main]  Delete all of your opponent's Digimon with the lowest DP. If this effect didn't delete, trash 1 of your opponent's Option cards in the battle area. Then, you may link this card to 1 of your Digimon on the field without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectOwnerPermamentCondition(Permanent permanent)
                    => (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) || CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card))
                        && card.CanLinkToTargetPermanent(permanent, false, true);

                bool CanSelectEnemyOptionCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && permanent.IsOption;

                bool CanSelectEnemyPermamentCondition(Permanent permanent)
                    => CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy, null);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    IEnumerator DeletionFailureProcess()
                    {
                        if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectEnemyOptionCondition))
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectEnemyOptionCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 option in battle area to trash", "The opponent is selecting 1 option in battle area to trash.");
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        }
                    }

                    // Delete all opponent digimon with lowest DP, if didnt delete, trash 1 opponent option
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectEnemyPermamentCondition))
                    {
                        List<Permanent> targetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().FindAll(CanSelectEnemyPermamentCondition);

                        if (targetPermanents.Any())
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                targetPermanents: targetPermanents,
                                activateClass: activateClass,
                                successProcess: null,
                                failureProcess: DeletionFailureProcess));
                        }
                    }
                    else yield return ContinuousController.instance.StartCoroutine(DeletionFailureProcess());


                    // Link to 1 Owner digimon on the field
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectOwnerPermamentCondition))
                    {
                        Permanent selectedPermament = null;

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionOwnersPermanentCount(card, CanSelectOwnerPermamentCondition));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOwnerPermamentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermament = permanent;
                            yield return null;
                        }

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to link this card to", "The opponent is selecting 1 Digimon to link this card to.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        if (selectedPermament != null) yield return ContinuousController.instance.StartCoroutine(selectedPermament.AddLinkCard(
                            addedLinkCard: card,
                            cardEffect: activateClass));
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                string EffectDiscription() => "[Security] Activate this card's [Main] effect.";

                CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Delete all opponent digimon with lowest DP, if this didnt delete trash 1 opponent option card in battle area. Then you may link this to 1 of your digimon", effectDiscription: EffectDiscription());
            }
            #endregion

            #region Link ESS
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 enemy digimon with equal or less DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("BT25-093-Linked-WA");
                activateClass.SetIsLinkedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription() => "[When Attacking] [Once Per Turn] Delete 1 of your opponent's Digimon with as much DP as this Digimon or less.";


                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool ValidOpponentDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.HasDP
                        && permanent.DP <= card.PermanentOfThisCard().DP;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(ValidOpponentDigimon))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(ValidOpponentDigimon));

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: ValidOpponentDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 opponent digimon with as much DP as this Digimon or less to delete", "The opponent is selecting 1 opponent digimon with as much DP as this Digimon or less to delete.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}