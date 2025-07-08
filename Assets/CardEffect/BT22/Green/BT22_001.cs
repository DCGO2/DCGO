using System.Collections;
using System.Collections.Generic;

// Puyoyomon
namespace DCGO.CardEffects.BT22
{
    public class BT22_001 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnAddDigivolutionCards)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("BT22_001_Draw1");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When effects place Digimon cards with [Aqua] or [Sea Animal] in any of their traits in this Digimon's digivolution cards, <Draw 1> (Draw 1 card from your deck.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAddDigivolutionCard(hashtable, permanent => PermanentCondition(permanent), null, cardSource => CardCondition(cardSource));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnField(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && card.TopCardPermanent() == permanent;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && (cardSource.HasAquaTraits || cardSource.HasSeaAnimalTraits);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }

            return cardEffects;
        }
    }
}