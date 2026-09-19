using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT19
{
    public class BT19_003 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherit
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 option card to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("ReturnOptionFromTrash_BT19_003");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[End of Your Turn][Once Per Turn] Return 1 Option card with [Plug-In] in its name from your trash to the hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsOption)
                    {
                        if (cardSource.ContainsCardName("Plug-In"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                     int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanSelectCardCondition));
                   
                     SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                   
                     selectCardEffect.SetUp(
                         canTargetCondition: CanSelectCardCondition,
                         canTargetCondition_ByPreSelecetedList: null,
                         canEndSelectCondition: null,
                         canNoSelect: () => false,
                         selectCardCoroutine: null,
                         afterSelectCardCoroutine: null,
                         message: "Select 1 card to add to your hand.",
                         maxCount: maxCount,
                         canEndNotMax: false,
                         isShowOpponent: true,
                         mode: SelectCardEffect.Mode.AddHand,
                         root: SelectCardEffect.Root.Trash,
                         customRootCardList: null,
                         canLookReverseCard: true,
                         selectPlayer: card.Owner,
                         cardEffect: activateClass);
                   
                     yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                }             
            }
            #endregion

            return cardEffects;
        }
    }
}