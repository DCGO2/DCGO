using System.Collections;
using System.Collections.Generic;

// LoaderLeomon
namespace DCGO.CardEffects.BT27
{
    public class BT27_051 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - Lv.4 w/[Leomon] in name or [Holy Beast]/[ACCEL] trait
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                    => targetPermanent.TopCard.ContainsCardName("Leomon")
                        || targetPermanent.TopCard.HasHolyBeastTraits
                        || targetPermanent.TopCard.EqualsTraits("ACCEL");

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 4));
            }
            #endregion

            #region Rule - Trait: Has [Holy Beast] Type
            // Rule traits normally come from the card data; this keeps the trait even if the data omits it.
            if (timing == EffectTiming.None)
            {
                ChangeTraitsClass changeTraitsClass = new ChangeTraitsClass();
                changeTraitsClass.SetUpICardEffect("Has the [Holy Beast] trait", CanUseCondition, card);
                changeTraitsClass.SetUpChangeTraitsClass(changeeTraits: ChangeTraits);
                cardEffects.Add(changeTraitsClass);

                bool CanUseCondition(Hashtable hashtable)
                    => true;

                List<string> ChangeTraits(CardSource cardSource, List<string> cardTraits)
                {
                    if (cardSource == card && !cardTraits.Contains("Holy Beast"))
                    {
                        cardTraits.Add("Holy Beast");
                    }

                    return cardTraits;
                }
            }
            #endregion

            #region Fortitude
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.FortitudeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName = "This Digimon may battle 1 of your opponent's Digimon";

            string SharedEffectDescription(string tag)
                => $"[{tag}] This Digimon may battle 1 of your opponent's Digimon.";

            bool IsDefenderCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool SharedCanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
                => CardEffectCommons.HasMatchConditionPermanent(IsDefenderCondition);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent selectedDefender = null;

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: IsDefenderCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectDefenderCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 of your opponent's Digimon to battle.", "The opponent is selecting 1 Digimon to battle.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectDefenderCoroutine(Permanent permanent)
                {
                    selectedDefender = permanent;
                    yield return null;
                }

                Permanent thisPermanent = card.PermanentOfThisCard();

                if (selectedDefender != null && thisPermanent != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IBattle(thisPermanent, selectedDefender, null, true).Battle());
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                isSkippable: true,
                additionalActivateCondition: SharedCanActivateCondition,
                onPlay: true,
                whenDigivolving: true);

            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Recovery +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Deletion] <Recovery +1>.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.CanActivateOnDeletion(card, activateClass)
                        && card.Owner.LibraryCards.Count >= 1
                        && card.Owner.CanAddSecurity(activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                }
            }
            #endregion

            #region On Deletion - ESS
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Recovery +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[On Deletion] <Recovery +1>.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.CanActivateOnDeletion(card, activateClass)
                        && card.Owner.LibraryCards.Count >= 1
                        && card.Owner.CanAddSecurity(activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
