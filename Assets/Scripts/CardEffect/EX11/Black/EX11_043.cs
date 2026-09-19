using System.Collections;
using System.Collections.Generic;

// Invisimon
namespace DCGO.CardEffects.EX11
{
    public class EX11_043 : CEntity_Effect
    {

        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel5 &&
                           (targetPermanent.TopCard.EqualsTraits("Cyborg") || targetPermanent.TopCard.EqualsTraits("Machine"));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Security - End of Opponents Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play this card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "(Security) [End of Opponent's Turn] Play this card without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOpponentTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistInSecurity(card) &&
                           CardEffectCommons.CanPlayAsNewPermanent(card, false, activateClass,SelectCardEffect.Root.Security);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: new List<CardSource>() { card }, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Security, activateETB: true));
                }
            }
            #endregion

            #region Shared OP / WD

            string SharedEffectName = "Flip security card face up, Deck Bounce lowest Cost. Then gain Sec +1";

            string SharedEffectDescription(string tag) => $"[{tag}] Flip your opponent's top face-down security card face up and return 1 of their lowest play cost Digimon to the bottom of the deck. Then, this Digimon gains <Security A. +1> until your turn ends.";

            bool SharedCanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleArea(card);

            bool OpponentsDigimonCondition(Permanent permanent) => CardEffectCommons.IsMinCost(permanent, card.Owner.Enemy, true);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                foreach (CardSource source in card.Owner.Enemy.SecurityCards)
                {
                    if (!source.IsFlipped)
                        continue;

                    yield return ContinuousController.instance.StartCoroutine(new IFlipSecurity(source).FlipFaceUp());

                    break;
                }

                if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimonCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: OpponentsDigimonCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(targetPermanent: card.PermanentOfThisCard(), changeValue: 1, effectDuration: EffectDuration.UntilOwnerTurnEnd, activateClass: activateClass));
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnSecurityCheck)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place top card face up as bottom security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When your Digimon check face-up security cards, you may place this Digimon's top stacked card face up as the bottom security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if(CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (!CardEffectCommons.GetCardFromHashtable(hashtable).IsFlipped)
                            return true;
                    }

                    return false;
                }


                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().DigivolutionCards.Count > 0;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    // Place this card face up as the bottom security card
                    if (card.Owner.CanAddSecurity(activateClass))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(
                            card, toTop: false, faceUp: true));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}