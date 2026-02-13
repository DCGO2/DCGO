using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Vortexdramon
namespace DCGO.CardEffects.EX11
{
    public class EX11_074 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Alternate Digivolution
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasShotoKazama);
                }

                bool HasShotoKazama(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card) &&
                           targetPermanent.TopCard.EqualsCardName("Shoto Kazama");
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(targetPermanent, card)
                        && targetPermanent.TopCard.EqualsCardName("GrandGalemon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 6,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: Condition)
                );
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Vortex
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.VortexSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #endregion

            #region Shared WD / WA

            string SharedEffectName = "Suspend 1 Digimon. If yours, this becomes immune to opponent's Digimon effects and gains +6K DP until end of their turn.";

            string SharedEffectDescription(string tag) => $"[{tag}] You may suspend 1 Digimon. If this effect suspended your Digimon, until your opponent's turn ends, their Digimon's effects don't affect this Digimon and gets +6000 DP.";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent selectedPermanent = null;
                bool ownDigimon = false;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to suspend.", "The opponent is selecting 1 Digimon to suspend.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;

                    yield return null;
                }

                if (selectedPermanent != null &&
                    selectedPermanent.TopCard != null &&
                    !selectedPermanent.TopCard.CanNotBeAffected(activateClass) &&
                    !selectedPermanent.IsSuspended && selectedPermanent.CanSuspend)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new SuspendPermanentsClass(new List<Permanent>() { selectedPermanent },
                            CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    ownDigimon = selectedPermanent.IsSuspended &&
                                    CardEffectCommons.IsOwnerPermanent(selectedPermanent, card);
                }

                if (ownDigimon)
                {
                    #region Give Digimon Effect Immunity

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                    }

                    bool CardCondition(CardSource cardSource)
                    {
                        return cardSource == card
                            && CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                    }

                    bool SkillCondition(ICardEffect cardEffect)
                    {
                        return CardEffectCommons.IsOpponentEffect(cardEffect, card)
                            && cardEffect.IsDigimonEffect;
                    }

                    CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                    canNotAffectedClass.SetUpICardEffect("Not affected by opponent's Digimon's effects", CanUseCondition1, card);
                    canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                    card.PermanentOfThisCard().UntilOpponentTurnEndEffects.Add((_timing) => canNotAffectedClass);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(card.PermanentOfThisCard()));

                    #endregion

                    #region Gain +6000 DP

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP
                    (
                        card.PermanentOfThisCard(),
                        6000,
                        EffectDuration.UntilOpponentTurnEnd,
                        activateClass
                    ));

                    #endregion
                }       
            }

            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }
            #endregion

            #region When Attacking
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Attacking"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
            }
            #endregion

            #region All turns
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ();
                activateClass.SetUpICardEffect("This Digimon may unsuspend. Then may battle 1 opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EX11_074_AT_Battle");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When any Digimon suspend, this Digimon may unsuspend. Then, this Digimon may battle 1 of your opponent's Digimon.";
                
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon);
                }

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleArea(card);

                bool CanSelectPermanentCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.PermanentOfThisCard().IsSuspended && card.PermanentOfThisCard().CanUnsuspend)
                    {
                        List<SelectionElement<bool>> selectionElements1 = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage1 = "Will you unsuspend this Digimon?";
                        string notSelectPlayerMessage1 = "The opponent is choosing if they will unsuspend.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements1, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage1, notSelectPlayerMessage: notSelectPlayerMessage1);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        var selectedOption = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (selectedOption)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() },  activateClass).Unsuspend()
                            );
                        }
                    }

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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to battle.", "The opponent is selecting 1 Digimon to battle.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IBattle(card.PermanentOfThisCard(), selectedPermanent, null, true).Battle());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
