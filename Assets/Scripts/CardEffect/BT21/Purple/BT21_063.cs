using System.Collections;
using System.Collections.Generic;

//bt21_063 Gumdramon
namespace DCGO.CardEffects.BT21
{
    public class BT21_063 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution condition
            if(timing == EffectTiming.None)
            {
                bool Condition(Permanent permanent)
                {
                    return (permanent.TopCard.HasSaveText || permanent.TopCard.EqualsTraits("Hero")) && permanent.Level == 2;
                }
                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(Condition, 0, false, card, null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 [Hero] / Save, <Draw 2>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] By trashing 1 card with <Save> in its text or the [Hero] trait from your hand, <Draw 2>.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool HasNameOrTrait(CardSource cardSource)
                {
                    return cardSource.EqualsTraits("Hero") || cardSource.HasSaveText;
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

                    selectHandEffect.SetUpCustomMessage("Select 1 card with the [Hero] trait or <Save> in text to discard.",
                        "The opponent is selecting 1 card with the [Hero] trait or <Save> in text to discard.");

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

            #region On-deletion save
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.SaveEffect(card: card));
            }
            #endregion

            #region inherit your turn +2k
            if (timing == EffectTiming.None)
            {
                bool condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) && CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 2000, isInheritedEffect: true, card: card, condition: condition));
            }
            #endregion

            return cardEffects;
        }
    }
}