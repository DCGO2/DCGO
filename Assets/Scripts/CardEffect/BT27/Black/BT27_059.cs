using System.Collections;
using System.Collections.Generic;

// Clockmon
namespace DCGO.CardEffects.BT27
{
    public class BT27_059 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "Make 1 of your Digimon immune to enemy <De-Digivolve> effects until their turn ends, if their turn, <De-Digivolve 1> 1 of their Digimon";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag)
                => $"[{tag}] Your opponent's <De-Digivolve> effects don't affect 1 of your Digimon until their turn ends. Then, if it's their turn, <De-Digivolve 1> 1 of their Digimon.";

            bool CanSelectOwnerPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }

            bool CanSelectOpponentPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnerPermanentCondition))
                {
                    Permanent selectedPermanent = null;

                    IEnumerator AfterSelectPermanent(List<Permanent> permanents)
                    {
                        if (permanents.Count >= 1)
                            selectedPermanent = permanents[0];

                        yield return null;
                    }

                    void ActivateDeDigivolveProtection()
                    {
                        ImmuneFromDeDigivolveClass immuneFromDeDigivolveClass = new ImmuneFromDeDigivolveClass();
                        immuneFromDeDigivolveClass.SetUpICardEffect("Isn't affected by <De-Digivolve>", CanUseDeDigivolveCondition, selectedPermanent.TopCard);
                        immuneFromDeDigivolveClass.SetUpImmuneFromDeDigivolveClass(PermanentCondition: PermanentDeDigivolveCondition);
                        selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => immuneFromDeDigivolveClass);
                    }

                    bool CanUseDeDigivolveCondition(Hashtable hashtable1)
                    {
                        return selectedPermanent.TopCard != null;
                    }

                    bool PermanentDeDigivolveCondition(Permanent permanent)
                    {
                        return permanent == selectedPermanent;
                    }

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOwnerPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanent,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select Digimon that will get immunity to opponent's <De-Digivolve> effects.",
                        "The opponent is selecting Digimon that will get immunity to your <De-Digivolve> effects.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    if (selectedPermanent != null)
                    {
                        ActivateDeDigivolveProtection();
                    }
                }

                if (CardEffectCommons.IsOpponentTurn(card)
                && CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Degenerate,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's Digimon to <De-Digivolve 1>", "The opponent is selecting 1 Digimon to <De-Digivolve 1>");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            #region Opponents Turn - ESS
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT27_059_Inherited");
                cardEffects.Add(activateClass);

                string EffectDescription() => "[Opponent's Turn] [Once Per Turn] When any of your Digimon suspend, this Digimon may unsuspend.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOpponentTurn(card)
                        && CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, PermanentCondition);
                }

                bool PermanentCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool CanActivateCondition(Hashtable hashtable) 
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && card.PermanentOfThisCard().IsSuspended
                        && CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard());
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
