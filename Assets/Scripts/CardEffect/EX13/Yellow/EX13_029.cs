using System.Collections;
using System.Collections.Generic;

// FlameWizardmon
namespace DCGO.CardEffects.EX13
{
    public class EX13_029 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Witchelny");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 3));
            }
            #endregion

            #region Name Rule
            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as [Wizardmon]", CanUseCondition, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: changeCardNames);
                cardEffects.Add(changeCardNamesClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                List<string> changeCardNames(CardSource cardSource, List<string> CardNames)
                {
                    if (cardSource == card)
                    {
                        CardNames.Add("Wizardmon");
                    }

                    return CardNames;
                }
            }
            #endregion

            #region Armor Purge
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.ArmorPurgeEffect(card: card));
            }
            #endregion

            #region Shared WD/WA
            string SharedEffectName = "Trash top sec to -4K DP 1 enemy Digimon for turn, if 3 sec or less delete 1 4K DP or lower enemy Digimon";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    additionalActivateCondition: (hashtable, activateClass) => card.Owner.SecurityCards.Count >= 1,
                    optional: true,
                    maxCountPerTurn: 1,
                    hashValue: "EX13_029_WD_WA",
                    whenDigivolving: true,
                    whenAttacking: true);

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] By trashing your top security card, 1 of your opponent's Digimon gets -4000 DP for the turn. After, if you have 3 or fewer security cards, delete 1 of your opponent's Digimon with 4000 DP or less.";

            bool CanSelectDPMinusPermanentCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool CanSelectDeletionPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.DP <= card.Owner.MaxDP_DeleteEffect(4000, activateClass);

                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDPMinusPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDPMinusPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -4000.", "The opponent is selecting 1 Digimon that will get DP -4000.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -4000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                    }
                }

                if (card.Owner.SecurityCards.Count <= 3
                && CardEffectCommons.HasMatchConditionPermanent(CanSelectDeletionPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDeletionPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            #region All Turns - Inherited
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 security to stop this Digimon from leaving Battle Area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("EX13_029_Inherited");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon with [Dynasmon] or [Witchelny] in its text would leave the battle area by your opponent's effects, by trashing your top security card, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card)
                        && CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOpponentEffect(cardEffect, card));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && (card.PermanentOfThisCard().TopCard.EqualsCardName("Dynasmon")
                            || card.PermanentOfThisCard().TopCard.HasText("Witchelny"))
                        && card.Owner.SecurityCards.Count >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());


                    Permanent thisCardPermanent = card.PermanentOfThisCard();

                    thisCardPermanent.willBeRemoveField = false;

                    thisCardPermanent.HideDeleteEffect();
                    thisCardPermanent.HideHandBounceEffect();
                    thisCardPermanent.HideDeckBounceEffect();
                    thisCardPermanent.HideWillRemoveFieldEffect();

                    yield return null;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
