using System.Collections;
using System.Collections.Generic;

//bt21-064 guilmon
namespace DCGO.CardEffects.BT21
{
    public class BT21_064 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();


            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Gigimon") || (targetPermanent.Level == 2 && targetPermanent.TopCard.EqualsTraits("Hero"));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 0,
                    ignoreDigivolutionRequirement: false,
                    card: card, condition: null)
                );
            }
            #endregion

            #region on play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 [Hero] / gallant line, <Draw 2>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] By trashing 1 card with [Guilmon], [Growlmon], [Gallantmon] or [Megidramon] in its name or the [Hero] trait from your hand, <Draw 2>.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool HasNameOrTrait(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Hero")
                        || cardSource.ContainsCardName("Guilmon")
                        || cardSource.ContainsCardName("Growlmon")
                        || cardSource.ContainsCardName("Gallantmon")
                        || cardSource.ContainsCardName("Megidramon");
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionOwnersHand(card, HasNameOrTrait);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                    bool trashed = false;
                    selectHandEffect.SetUp(
                        canTargetCondition: HasNameOrTrait,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectHandEffect.Mode.Discard,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectHandEffect.SetUpCustomMessage("Select 1 card with the [Hero] trait or guilmon/growlmon/gallantmon/megidramon in name to discard.",
                        "The opponent is selecting 1 card with the [Hero] trait or guilmon/growlmon/gallantmon/megidramon in name to discard.");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        trashed = true;
                        yield return null;
                    }

                    if (trashed)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            new DrawClass(card.Owner, 2, activateClass).Draw());
                    }
                }
            }
            #endregion

            #region On Deletion inherit
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Deletion] Gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletionInherited(hashtable, card))
                    {
                        if (card.Owner.CanAddMemory(activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}