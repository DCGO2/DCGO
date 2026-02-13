using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class ST7_03 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                if (CardEffectCommons.IsOwnerTurn(card) && CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, (permanent) => permanent.IsDigimon && permanent.Level >= 6 && permanent.TopCard.HasLevel))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent == card.PermanentOfThisCard();
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource.Owner.HandCards.Contains(cardSource))
                {
                    if (cardSource.CardNames.Contains("Gallantmon"))
                    {
                        return true;
                    }
                }

                return false;
            }

            string effectName = $"Can digivolve to [Gallantmon]";

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: true, card: card, condition: Condition, effectName: effectName, cardCondition: CardCondition));
        }

        if (timing == EffectTiming.OnDestroyedAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
            activateClass.SetIsInheritedEffect(true);
            activateClass.SetHashString("Draw1_ST7_03");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Your Turn][Once Per Turn] When an opponent's Digimon is deleted, trigger <Draw 1>. (Draw 1 card from your deck.)";
            }

            bool PermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
            }
        }

        return cardEffects;
    }
}
