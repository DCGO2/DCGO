using System.Collections;
using System.Collections.Generic;

// Takato Matsuki
namespace DCGO.CardEffects.EX13
{
    public class EX13_068 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Turn
            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }
            #endregion

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                cardEffects.Add(CardEffectFactory.StartOfYourMainPhaseClass(
                    card,
                    "By returning this Tamer to the bottom of the deck, play 1 [Takato Matsuki] from hand, then may play 1 [Guilmon] from trash",
                    ActivateCoroutine,
                    EffectDescription(),
                    optional: true));

                string EffectDescription()
                    => "[Start of Your Main Phase] By returning this Tamer to the bottom of the deck, you may play 1 [Takato Matsuki] from your hand without paying the cost. After, if you don't have a Digimon, you may play 1 [Guilmon] from your trash without paying the cost.";

                IEnumerator ActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
                {
                    bool IsTakatoMatsuki(CardSource cardSource)
                        => cardSource.EqualsCardName("Takato Matsuki")
                            && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);

                    bool IsGuilmon(CardSource cardSource)
                        => cardSource.EqualsCardName("Guilmon")
                            && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass, root: SelectCardEffect.Root.Trash);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeckBouncePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() },
                        activateClass: activateClass,
                        successProcess: SuccessProcess(),
                        failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, IsTakatoMatsuki))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                canTargetCondition: IsTakatoMatsuki,
                                root: SelectCardEffect.Root.Hand,
                                cardEffect: activateClass,
                                payCost: false));
                        }

                        if (card.Owner.GetBattleAreaDigimons().Count == 0
                            && CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsGuilmon))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                                canTargetCondition: IsGuilmon,
                                root: SelectCardEffect.Root.Trash,
                                cardEffect: activateClass,
                                payCost: false));
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
