using System.Collections;
using System.Collections.Generic;

// Quetzalmon
namespace DCGO.CardEffects.BT27
{
    public class BT27_038 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - Tamer w/[Kurata's Army] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.IsTamer
                        && targetPermanent.TopCard.EqualsTraits("Kurata's Army");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Armor Purge
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.ArmorPurgeEffect(card: card));
            }
            #endregion

            #region Shared On Play / When Digivolving / When Attacking - OPT
            string SharedEffectName = "1 opponent's Digimon gets Security A. -1, then by deleting 1 of your Digimon, 1 opponent's Digimon gets -6000 DP";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] Give 1 of your opponent's Digimon <Security A. -1> until their turn ends. Then, by choosing 1 of your Digimon, delete the chosen Digimon, and 1 of their Digimon gets -6000 DP until their turn ends.";

            bool CanSelectOpponentDigimonCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool CanSelectOwnDigimonCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region 1 opponent's Digimon gets Security A. -1
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentDigimonCondition))
                {
                    SelectPermanentEffect selectSAttackEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectSAttackEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectSAttackCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectSAttackEffect.SetUpCustomMessage("Select 1 Digimon that will get Security A. -1.", "The opponent is selecting 1 Digimon that will get Security A. -1.");

                    yield return ContinuousController.instance.StartCoroutine(selectSAttackEffect.Activate());

                    IEnumerator SelectSAttackCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                            targetPermanent: permanent,
                            changeValue: -1,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));
                    }
                }
                #endregion

                #region By choosing 1 of your Digimon, delete it and 1 opponent's Digimon can't suspend
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnDigimonCondition))
                {
                    List<Permanent> selectedOwnDigimon = new List<Permanent>();

                    SelectPermanentEffect selectOwnEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectOwnEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOwnDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectOwnCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectOwnEffect.SetUpCustomMessage("Select 1 of your Digimon to delete.", "The opponent is selecting 1 of their Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectOwnEffect.Activate());

                    IEnumerator SelectOwnCoroutine(Permanent permanent)
                    {
                        selectedOwnDigimon.Add(permanent);
                        yield return null;
                    }

                    if (selectedOwnDigimon.Count == 1)
                    {
                        Permanent selectOpponentPermanent = null;

                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentDigimonCondition))
                        {
                            SelectPermanentEffect selectOpponentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectOpponentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectOpponentDigimonCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectOpponentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectOpponentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -6000.", "The opponent is selecting 1 Digimon that will get DP -6000.");

                            yield return ContinuousController.instance.StartCoroutine(selectOpponentEffect.Activate());

                            IEnumerator SelectOpponentCoroutine(Permanent permanent)
                            {
                                selectOpponentPermanent = permanent;
                                yield return null;
                            }
                        }

                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(selectedOwnDigimon, hashtable).Destroy());

                        if (selectOpponentPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: selectOpponentPermanent,
                                changeValue: -6000,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));
                        }
                    }
                }
                #endregion
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: "BT27_038_OP_WD_WA",
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);
            #endregion

            return cardEffects;
        }
    }
}
