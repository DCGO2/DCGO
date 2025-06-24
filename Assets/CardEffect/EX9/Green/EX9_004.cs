using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Tanemon
namespace DCGO.CardEffects.EX9
{
    public class EX9_004 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Your Turn - ESS

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Gain1Memory_EX9_004");
                cardEffects.Add(activateClass);

                string EffectDescription() =>
                    "[Your Turn] [Once Per Turn] When any of your [Ver.4] trait Digimon are played, by trashing this Digimon's bottom face-down digivolution card, gain 1 memory.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PlayedPermanentCondition);
                }

                bool PlayedPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.EqualsTraits("Ver.4");
                }

                bool CanSelectTrashSourceCardCondition(CardSource cardSource)
                {
                    return cardSource.IsFlipped && !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectTrashSourceCardCondition) >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    CardSource trashTargetCard = card.PermanentOfThisCard().DigivolutionCards.Filter(CanSelectTrashSourceCardCondition)[^1];

                    selectedCards.Add(trashTargetCard);

                    if (selectedCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            new ITrashDigivolutionCards(card.PermanentOfThisCard(), selectedCards, activateClass).TrashDigivolutionCards());

                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}