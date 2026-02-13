using System.Collections;
using System.Collections.Generic;

//Kyubimon
namespace DCGO.CardEffects.ST22
{
	public class ST22_03 : CEntity_Effect
	{
		public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
		{
			List<ICardEffect> cardEffects = new List<ICardEffect>();

			#region WM/OP Shared
			string EffectDiscriptionShared(string tag)
			{
				return $"[{tag}] Reveal the top 3 cards of your deck. Add 1 card with [Renamon], [Kyubimon], [Taomon], [Sakuyamon] or [Rika Nonaka] in its name or the [Onmyōjutsu] or [Plug-In] trait among them to the hand. Return the rest to the bottom of the deck.";
			}

			bool CanActivateConditionShared(Hashtable hashtable)
			{
				return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
			}

			bool CanSelectCardCondition(CardSource cardSource)
			{
                return cardSource.ContainsCardName("Renamon")
                        || cardSource.ContainsCardName("Kyubimon")
                        || cardSource.ContainsCardName("Taomon")
                        || cardSource.ContainsCardName("Sakuyamon")
                        || cardSource.ContainsCardName("Rika Nonaka")
                        || cardSource.EqualsTraits("Onmyōjutsu")
                        || cardSource.EqualsTraits("Plug-In");
            }

            IEnumerator ActivateCoroutineShared(Hashtable _hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 card with [Renamon]/[Kyubimon]/[Taomon]/[Sakuyamon]/[Rika Nanoka] in its name or the [Onmyōjutsu]/[Plug-In] trait.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                    activateClass: activateClass
                ));
            }
            #endregion

            #region When Moving
            if (timing == EffectTiming.OnMove)
			{
				ActivateClass activateClass = new ActivateClass();
				activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
				activateClass.SetUpActivateClass(CanActivateConditionShared, hashtable => ActivateCoroutineShared(hashtable, activateClass), -1, false, EffectDiscriptionShared("When Moving"));
				cardEffects.Add(activateClass);

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

				bool CanUseCondition(Hashtable hashtable)
				{
					return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
						   CardEffectCommons.CanTriggerOnMove(hashtable, PermanentCondition);
				}
			}
			#endregion

			#region When Digivolving

			if (timing == EffectTiming.OnEnterFieldAnyone)
			{
				ActivateClass activateClass = new ActivateClass();
				activateClass.SetUpICardEffect("Reveal the top 3 cards of deck", CanUseCondition, card);
				activateClass.SetUpActivateClass(CanActivateConditionShared, hashtable => ActivateCoroutineShared(hashtable, activateClass), -1, false, EffectDiscriptionShared("When Digivolving"));
				cardEffects.Add(activateClass);

				bool CanUseCondition(Hashtable hashtable)
				{
					return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
						&& CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
				}
			}

			#endregion

			#region Barrier - ESS

			if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
			{
				cardEffects.Add(CardEffectFactory.BarrierSelfEffect(isInheritedEffect: true, card: card, condition: null));
			}

			#endregion

			return cardEffects;
		}
	}
}
