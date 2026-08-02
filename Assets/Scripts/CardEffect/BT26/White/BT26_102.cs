using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects
{
    public class BT26_102 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Colour Requirement
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.HasSevenCodeTraits;
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing 6 [Seven Code] digimon from battle area. link cards or trash as bottom source of 1 [Seven Code] Digimon, Digivolve into [Dantemon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Main] By placing 6 [Seven Code] trait Digimon cards from your battle area, link cards or trash as 1 of your [Seven Code] trait Digimon's bottom digivolution cards, that Digimon may digivolve into [Dantemon] in the hand, ignoring digivolution requirements and without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card)
                        && GetTotalTargetCount() >= 6;

                int GetTotalTargetCount()
                {
                    int PermanentCount = CardEffectCommons.MatchConditionOwnersPermanentCount(card, perm => CanSelectCardSourceCondition(perm.TopCard));
                    int CardSourceCount = CardEffectCommons.MatchConditionOwnersPermanentCount(card, perm => perm.LinkedCards.Find(CanSelectCardSourceCondition));
                    int TrashCount = CardEffectCommons.MatchConditionOwnersCardCountInHand(card, CanSelectCardSourceCondition);

                    return PermanentCount + CardSourceCount + TrashCount;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.HasSevenCodeTraits;

                bool CanSelectCardSourceCondition(CardSource cardSource)
                    => cardSource.HasSevenCodeTraits;

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> cardSources = null;
                    List<Permanent> permanents = null;
                    Permanent permanent = null;
                    int currentCount = cardSources.Count + permanents.Count;

                    while (currentCount < 6)
                    {

                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}