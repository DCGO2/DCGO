using System.Collections;
using System.Collections.Generic;

// Darkdramon
namespace DCGO.CardEffects.BT27
{
    public class BT27_071 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - [Thunderbirdmon] w/[Bio] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.EqualsCardName("Thunderbirdmon")
                        && targetPermanent.TopCard.EqualsTraits("Bio");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Alternate Digivolution Requirement - Tamer w/[Kurata's Army] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.IsTamer
                        && targetPermanent.TopCard.EqualsTraits("Kurata's Army");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 7, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Armor Purge
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.ArmorPurgeEffect(card: card));
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon isn't affected by opponent's Digimon effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Digivolving] Your opponent's Digimon effects don't affect this Digimon until their turn ends.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    thisPermanent.UntilOpponentTurnEndEffects.Add(_timing => PermanentEffectFactory.DigimonEffectImmunity(thisPermanent));

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(thisPermanent));
                }
            }
            #endregion

            #region Shared When Attacking / Counter - OPT

            string SharedEffectName = "<De-Digivolve 2> 1 opponent's Digimon";

            string SharedEffectDescription(string tag)
                => $"[{tag}] [Once Per Turn] <De-Digivolve 2> 1 of your opponent's Digimon. (Trash up to 2 cards from the top. You can't trash past level 3 cards.)";

            bool CanSelectPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition)) yield break;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Degenerate,
                    cardEffect: activateClass);
                
                selectPermanentEffect.SetDegenerationCount(2);
                
                selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's Digimon to De-Digivolve", "The opponent is selecting 1 Digimon to De-Digivolve");
                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: "BT27_071_WA_C",
                whenAttacking: true,
                counter: true);

            #endregion

            #region All Turns - Can't Digivolve
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                    => permanent == card.PermanentOfThisCard();

                cardEffects.Add(CardEffectFactory.CanNotDigivolveStaticEffect(
                    permanentCondition: PermanentCondition,
                    cardCondition: cardSource => true,
                    isInheritedEffect: false,
                    card: card,
                    condition: () => CardEffectCommons.IsExistOnBattleAreaDigimon(card),
                    effectName: "This Digimon can't digivolve"));
            }
            #endregion

            return cardEffects;
        }
    }
}
