using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT19
{
    public class BT19_036 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Wizardmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add top security card to hand and place 1 Option card as bottom security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Add your top security card to the hand. Then, if [Wizardmon]/[X Antibody] is in this Digimon's digivolution cards, you may place 1 yellow or purple Option card with cost of 5 or less from your hand as your bottom security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsOption)
                    {
                        if (cardSource.CardColors.Contains(CardColor.Yellow) || cardSource.CardColors.Contains(CardColor.Purple))
                        {
                            if(cardSource.HasUseCost && cardSource.GetCostItself <= 5)
                            {
                                return true;
                            }                      
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        CardSource topCard = card.Owner.SecurityCards[0];

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { topCard }, false, activateClass));

                        yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                            player: card.Owner,
                            refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
                    }                        

                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.EqualsCardName("Wizardmon") || cardSource.EqualsCardName("X Antibody")) >= 1)
                    {
                        if (card.Owner.CanAddSecurity(activateClass))
                        {
                            if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                            {
                                List<CardSource> selectedCards = new List<CardSource>();

                                int maxCount = 1;

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectHandEffect.SetUpCustomMessage(
                                    "Select 1 card to place at the bottom of security.",
                                    "The opponent is selecting 1 card to place at the bottom of security.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Security Bottom Card");

                                yield return StartCoroutine(selectHandEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);

                                    yield return null;
                                }

                                foreach (CardSource cardSource in selectedCards)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(cardSource, toTop: false));
                                }
                            }
                        }
                    }
                }

            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add top security card to hand and place 1 Option card as bottom security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Add your top security card to the hand. Then, if [Wizardmon]/[X Antibody] is in this Digimon's digivolution cards, you may place 1 yellow or purple Option card with cost of 5 or less from your hand as your bottom security card.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsOption)
                    {
                        if (cardSource.CardColors.Contains(CardColor.Yellow) || cardSource.CardColors.Contains(CardColor.Purple))
                        {
                            if (cardSource.HasUseCost && cardSource.GetCostItself <= 5)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        CardSource topCard = card.Owner.SecurityCards[0];

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { topCard }, false, activateClass));

                        yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                            player: card.Owner,
                            refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
                    }

                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.EqualsCardName("Wizardmon") || cardSource.EqualsCardName("X Antibody")) >= 1)
                    {
                        if (card.Owner.CanAddSecurity(activateClass))
                        {
                            if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                            {
                                List<CardSource> selectedCards = new List<CardSource>();

                                int maxCount = 1;

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectHandEffect.SetUpCustomMessage(
                                    "Select 1 card to place at the bottom of security.",
                                    "The opponent is selecting 1 card to place at the bottom of security.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Security Bottom Card");

                                yield return StartCoroutine(selectHandEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);

                                    yield return null;
                                }

                                foreach (CardSource cardSource in selectedCards)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(cardSource, toTop: false));
                                }
                            }
                        }
                    }
                }

            }
            #endregion

            #region Inherit
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 security to prevent this Digimon from leaving Battle Area", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("TrashSecurityToStay_WizardmonXAntibody_BT19_036");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns][Once Per Turn] When this yellow Digimon with the [Data]/[Witchelny] trait would leave the battle area by your opponent's effects, by trashing your top security card, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card))
                        {
                            if (CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOpponentEffect(cardEffect, card)))
                                return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        Permanent thisPermanent = card.PermanentOfThisCard();
                        if (thisPermanent.TopCard.CardColors.Contains(CardColor.Yellow) && (thisPermanent.TopCard.ContainsTraits("Data") || thisPermanent.TopCard.ContainsTraits("Witchelny")))
                        {
                            if (card.Owner.SecurityCards.Count >= 1)
                                return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card) || card.Owner.SecurityCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                            player: card.Owner,
                            destroySecurityCount: 1,
                            cardEffect: activateClass,
                            fromTop: true).DestroySecurity());


                        Permanent thisCardPermanent = card.PermanentOfThisCard();

                        thisCardPermanent.willBeRemoveField = false;

                        thisCardPermanent.HideDeleteEffect();
                        thisCardPermanent.HideHandBounceEffect();
                        thisCardPermanent.HideDeckBounceEffect();
                        thisCardPermanent.HideWillRemoveFieldEffect();

                        yield return null;
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}