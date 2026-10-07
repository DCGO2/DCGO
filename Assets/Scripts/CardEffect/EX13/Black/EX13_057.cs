using System.Collections;
using System.Collections.Generic;

// Grademon
namespace DCGO.CardEffects.EX13
{
    public class EX13_057 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - [Raptordramon]
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Raptordramon");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Alternate Digivolution Requirement - Lv.4 w/[Chronicle] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.HasChronicleTraits;

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null,
                    level: 4));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName = "1 [X Antibody]/[Chronicle] Digimon gains <Reboot> and <Blocker>, if during an attack, immunity and DP +5000";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Until your opponent's turn ends, 1 of your [X Antibody] or [Chronicle] trait Digimon gains <Reboot> and <Blocker>. If during an attack, it also isn't affected by their Digimon effects and gets +5000 DP.";

            bool CanSelectPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && (permanent.TopCard.HasXAntibodyTraits || permanent.TopCard.HasChronicleTraits);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition)) yield break;

                Permanent selectedPermanent = null;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

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
                    "Select 1 Digimon that will gain <Reboot> and <Blocker>. If during an attack, Digimon effect immunity and +5K DP.",
                    "The opponent is selecting 1 Digimon that will gain <Reboot> and <Blocker>. If during an attack, Digimon effect immunity and +5K DP.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;

                    yield return null;
                }

                if (selectedPermanent == null) yield break;

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainReboot(
                    targetPermanent: selectedPermanent,
                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                    activateClass: activateClass));

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(
                    targetPermanent: selectedPermanent,
                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                    activateClass: activateClass));

                if (GManager.instance.attackProcess.IsAttacking)
                {
                    #region Give Digimon Effect Immunity
                    selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => PermanentEffectFactory.DigimonEffectImmunity(selectedPermanent));
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));
                    #endregion

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                        targetPermanent: selectedPermanent,
                        changeValue: 5000,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass));
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            #endregion

            #region End of Attack - OPT
            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into a [Chronicle] trait Digimon card from hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EX13_057_EOA");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[End of Attack] [Once Per Turn] This Digimon may digivolve into a Digimon card with the [Chronicle] trait in the hand or trash.";

                bool DigivolveCardCondition(CardSource cardSource)
                    => cardSource.IsDigimon
                        && cardSource.HasChronicleTraits;

                bool CanDigivolveFromHandCondition(CardSource cardSource)
                    => DigivolveCardCondition(cardSource)
                        && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, true, activateClass, SelectCardEffect.Root.Hand);

                bool CanDigivolveFromTrashCondition(CardSource cardSource)
                    => DigivolveCardCondition(cardSource)
                        && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, true, activateClass, SelectCardEffect.Root.Trash);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnEndAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanDigivolveFromHandCondition)
                            || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanDigivolveFromTrashCondition));

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanDigivolveFromHandCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanDigivolveFromTrashCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        bool isHand = canSelectHand;
                        bool doDigivolve = true;

                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                            {
                                new(message: "From hand", value: 1, spriteIndex: 0),
                                new(message: "From trash", value: 2, spriteIndex: 0),
                                new(message: "Don't digivolve", value: 3, spriteIndex: 1),
                            };

                            GManager.instance.userSelectionManager.SetIntSelection(
                                selectionElements: selectionElements,
                                selectPlayer: card.Owner,
                                selectPlayerMessage: "From which area will you digivolve?",
                                notSelectPlayerMessage: "The opponent is choosing from which area to select a card.");

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            doDigivolve = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                            isHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;
                        }

                        if (doDigivolve)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: card.PermanentOfThisCard(),
                                cardCondition: DigivolveCardCondition,
                                payCost: true,
                                reduceCostTuple: null,
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: isHand,
                                activateClass: activateClass,
                                successProcess: null));
                        }
                    }
                }
            }
            #endregion

            #region All Turns - OPT - ESS
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By trashing your top security card, [Chronicle] Digimon don't leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("EX13_057_ESS_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When any of your [Chronicle] trait Digimon would leave the battle area, by trashing your top security card, they don't leave.";

                bool PermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasChronicleTraits;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.Owner.SecurityCards.Count >= 1;

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashSecurityAndProcessAccordingToResult(
                        player: card.Owner,
                        trashAmount: 1,
                        activateClass: activateClass,
                        fromTop: true,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<CardSource> _cardSources)
                    {
                        List<Permanent> protectedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable)
                            .Filter(PermanentCondition);

                        foreach (Permanent permanent in protectedPermanents)
                        {
                            permanent.willBeRemoveField = false;
                            permanent.HideDeleteEffect();
                            permanent.HideHandBounceEffect();
                            permanent.HideDeckBounceEffect();
                            permanent.HideWillRemoveFieldEffect();
                        }

                        yield return null;
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
