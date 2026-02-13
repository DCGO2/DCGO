using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT18
{
    public class BT18_006 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Deletion - ESS

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash cards from the top of your deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Deletion] Trash the top card of your deck for each color of your opponent's Digimon and Tamers.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                int OpponentColorCount()
                {
                    List<CardColor> cardColors = new List<CardColor>();

                    foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaPermanents()
                                 .Where(permanent => permanent.IsDigimon || permanent.IsTamer))
                    {
                        cardColors.AddRange(permanent.TopCard.CardColors);
                    }

                    cardColors = cardColors.Distinct().ToList();

                    return cardColors.Count;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletionInherited(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new IAddTrashCardsFromLibraryTop(OpponentColorCount(), card.Owner, activateClass).AddTrashCardsFromLibraryTop());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}