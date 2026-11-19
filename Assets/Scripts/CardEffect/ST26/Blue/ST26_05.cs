using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Yukinamon / Ice Beast Caricatures
namespace DCGO.CardEffects.ST26
{
    public class ST26_05 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Side
            #region Iceclad
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.IcecladSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash any 3 opponent's sources, then if they have no Digimon with sources, 1 [Ice-Snow] Digimon may attack without suspending", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Digivolving] Trash any 3 digivolution cards from your opponent's Digimon. Then, if your opponent has no Digimon with digivolution cards, 1 of your [Ice-Snow] trait Digimon may attack without suspending.";

                bool OpponentDigimonWithSourcesCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.DigivolutionCards.Count >= 1;

                bool IceSnowAttackerCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("Ice-Snow")
                        && permanent.CanAttack(activateClass, true);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: OpponentDigimonWithSourcesCondition,
                        cardCondition: null,
                        maxCount: 3,
                        canNoTrash: false,
                        isFromOnly1Permanent: false,
                        activateClass: activateClass));

                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentDigimonWithSourcesCondition)
                        || !CardEffectCommons.HasMatchConditionPermanent(IceSnowAttackerCondition)) yield break;

                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IceSnowAttackerCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 [Ice-Snow] Digimon that will attack without suspending.", "The opponent is selecting 1 Digimon that will attack.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    if (selectedPermanent == null || !selectedPermanent.CanAttack(activateClass, true)) yield break;

                    SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                    selectAttackEffect.SetUp(
                        attacker: selectedPermanent,
                        canAttackPlayerCondition: () => true,
                        defenderCondition: _ => true,
                        cardEffect: activateClass);

                    selectAttackEffect.SetCanNotSelectNotAttack();
                    selectAttackEffect.SetWithoutTap();

                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May return 1 opponent's Digimon with no sources to the bottom of the deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] You may return 1 of your opponent's Digimon with no digivolution cards to the bottom of the deck.";

                bool SourcelessOpponentDigimonCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.DigivolutionCards.Count == 0;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(SourcelessOpponentDigimonCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: SourcelessOpponentDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to return to the bottom of the deck.", "The opponent is selecting 1 Digimon to return to the bottom of the deck.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion
            #endregion

            #region Option Side
            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash all cards under 1 opponent's Digimon or Tamer, then 2 of their Digimon or Tamers can't suspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] Trash all cards from under 1 of your opponent's Digimon or Tamers. Then, 2 of their Digimon or Tamers can't suspend until their turn ends.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool OpponentDigimonOrTamerCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region Trash all cards from under 1 of the opponent's Digimon or Tamers
                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentDigimonOrTamerCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentDigimonOrTamerCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon or Tamer to trash all cards from under it.", "The opponent is selecting 1 Digimon or Tamer to trash all cards from under it.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            if (permanent.DigivolutionCards.Count == 0) yield break;

                            yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(
                                permanent,
                                permanent.DigivolutionCards.ToList(),
                                activateClass).TrashDigivolutionCards());
                        }
                    }
                    #endregion

                    #region 2 of their Digimon or Tamers can't suspend
                    int canNotSuspendCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(OpponentDigimonOrTamerCondition));

                    if (canNotSuspendCount == 0) yield break;

                    SelectPermanentEffect canNotSuspendSelectEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    canNotSuspendSelectEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: OpponentDigimonOrTamerCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: canNotSuspendCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: CanNotSuspendCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    canNotSuspendSelectEffect.SetUpCustomMessage($"Select {canNotSuspendCount} Digimon or Tamers that can't suspend.", "The opponent is selecting Digimon or Tamers that can't suspend.");

                    yield return ContinuousController.instance.StartCoroutine(canNotSuspendSelectEffect.Activate());

                    IEnumerator CanNotSuspendCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotSuspend(
                            targetPermanent: permanent,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            condition: null,
                            effectName: "Can't suspend"));
                    }
                    #endregion
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion
            #endregion

            return cardEffects;
        }
    }
}
