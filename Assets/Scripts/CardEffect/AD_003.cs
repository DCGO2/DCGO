using System.Collections;
using System.Collections.Generic;

// Wargrowlmon
namespace DCGO.CardEffects.AD1
{
    public class AD1_003 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4 && 
                           (targetPermanent.TopCard.ContainsCardName("Growlmon")
                            || targetPermanent.TopCard.EqualsTraits("Hero"));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP / WD
            string SharedEffectName = "Play 1 [Takato Matsuki] from hand or trash for free, then may delete 1 opponent's Digimon w/ 6000 DP or less";

            string SharedEffectDescription(string tag) => $"[{tag}] You may play 1 [Takato Matsuki] from your hand or trash without paying the cost. Then, you may delete 1 of your opponent's Digimon with 6000 DP or less.";

            bool SharedCanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && (CardEffectCommons.HasMatchConditionOwnersHand(card, cardSource => Takato(cardSource, activateClass))
                        || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, cardSource => Takato(cardSource, activateClass))
                        || CardEffectCommons.HasMatchConditionPermanent(permanent => CanSelectPermanentCondition(permanent, activateClass)));
            }

            bool Takato(CardSource cardSource, ActivateClass activateClass)
            {
                return cardSource.EqualsCardName("Takato Matsuki")
                    && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
            }

            bool CanSelectPermanentCondition(Permanent permanent, ActivateClass activateClass)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.DP <= card.Owner.MaxDP_DeleteEffect(6000, activateClass);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                return null;
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
