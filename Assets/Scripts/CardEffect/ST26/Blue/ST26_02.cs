using System.Collections;
using System.Collections.Generic;

// Renamon
namespace DCGO.CardEffects.ST26
{
    public class ST26_02 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared WM/OP
            string SharedEffectName = "Trash the bottom source of 1 opponent's Digimon, then if they have no Digimon with sources, <Draw 1>";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Trash the bottom digivolution card of 1 of your opponent's Digimon. Then, if your opponent has no Digimon with digivolution cards, <Draw 1>.";

            bool OpponentDigimonCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool OpponentDigimonWithSourcesCondition(Permanent permanent)
                => OpponentDigimonCondition(permanent)
                    && permanent.DigivolutionCards.Count >= 1;

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(OpponentDigimonCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: OpponentDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to trash its bottom digivolution card.", "The opponent is selecting 1 Digimon to trash its bottom digivolution card.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                            targetPermanent: permanent, trashCount: 1, isFromTop: false, activateClass: activateClass));
                    }
                }

                if (!CardEffectCommons.HasMatchConditionPermanent(OpponentDigimonWithSourcesCondition))
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                whenMoving: true,
                onPlay: true);
            #endregion

            #region When Attacking - Inherited
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Change the attack target to 1 opponent's unsuspended Digimon with the fewest digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] If this Digimon has the [Ice-Snow] trait, you may change the attack target to 1 of your opponent's unsuspended Digimon with the fewest digivolution cards.";

                bool NewTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsMinDigivolutionCards(permanent, card.Owner.Enemy, target => !target.IsSuspended)
                        && permanent != GManager.instance.attackProcess.DefendingPermanent;

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && card.PermanentOfThisCard().TopCard.EqualsTraits("Ice-Snow")
                        && card.PermanentOfThisCard().CanSwitchAttackTarget
                        && CardEffectCommons.HasMatchConditionPermanent(NewTargetCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: NewTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to become the new attack target.", "The opponent is selecting 1 Digimon to become the new attack target.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(activateClass, false, permanent));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
