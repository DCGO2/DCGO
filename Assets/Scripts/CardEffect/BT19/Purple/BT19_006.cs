using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT19
{
    public class BT19_006 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 level 3 purple Digimon card from your trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] If deleted other than by battle, return 1 level 3 purple Digimon card from your trash to the hand.";
                }

                bool PurpleLevelThree(CardSource source)
                {
                    return source.CardColors.Contains(CardColor.Purple) &&
                           source.IsLevel3;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerOnDeletion(hashtable, card))
                        return !CardEffectCommons.IsByBattle(hashtable);

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletionInherited(hashtable, card))
                        return CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, PurpleLevelThree);
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: PurpleLevelThree,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => false,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 purple level 3 digimon card to return to hand.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.AddHand,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 purple level 3 digimon card to return to hand.", "The opponent is select 1 purple level 3 digimon card to return to hand.");

                    yield return StartCoroutine(selectCardEffect.Activate());
                }
            }

            return cardEffects;
        }
    }
}