using System.Collections;
using System.Collections.Generic;

// Oblivimon
namespace DCGO.CardEffects.EX11
{
    public class EX11_041 : CEntity_Effect
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

            string SharedEffectName = "Flip security card face up, Dedigivolve 1. Then Digivolve on their turn";

            string SharedEffectDescription(string tag) => $"[{tag}] Flip your opponent's top face-down security card face up and <De-Digivolve 1> 1 of their Digimon. Then, if it's their turn, this Digimon may digivolve into [Invisimon] in the hand without paying the cost.";

            bool SharedCanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleArea(card);

            bool OpponentsDigimonCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            bool IsInvisimonCardCondition(CardSource cardSource) => cardSource.EqualsCardName("Invisimon");

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
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.", "The opponent is selecting 1 Digimon to De-Digivolve.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        Permanent selectedPermanent = permanent;

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IDegeneration(selectedPermanent, 1, activateClass).Degeneration());
                        }

                        yield return null;
                    }
                }
            
                if (CardEffectCommons.IsOpponentTurn(card))
                {
                    yield return ContinuousController.instance.StartCoroutine(
                    CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: IsInvisimonCardCondition,
                        payCost: false,
                        null,
                        null,
                        -1,
                        isHand: true,
                        activateClass,
                        null
                    ));
                }

                yield return null;
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

            #region Your Turn - ESS

            if (timing == EffectTiming.None)
            {
                CanNotSwitchAttackTargetClass canNotSwitchAttackTargetClass = new CanNotSwitchAttackTargetClass();
                canNotSwitchAttackTargetClass.SetUpICardEffect("This Digimon's attack target can't be switched.", CanUseCondition, card);
                canNotSwitchAttackTargetClass.SetUpCanNotSwitchAttackTargetClass(PermanentCondition: PermanentCondition);
                canNotSwitchAttackTargetClass.SetIsInheritedEffect(true);
                cardEffects.Add(canNotSwitchAttackTargetClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.IsOwnerTurn(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent != null && permanent.TopCard && permanent == card.PermanentOfThisCard();
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
