using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT17
{
    public class BT17_091 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Security Skill

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            #region Start of Your Turn

            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }

            #endregion

            #region Main - Mind Link

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Mind Link", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Main] <Mind Link> with 1 of your Digimon with the [Dark Animal] or [SoC] trait. (Place this Tamer as that Digimon's bottom digivolution card if there are no Tamer cards in its digivolution cards.)";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           (permanent.TopCard.CardTraits.Contains("Dark Animal") ||
                            permanent.TopCard.CardTraits.Contains("DarkAnimal") ||
                            permanent.TopCard.CardTraits.Contains("SoC"));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new MindLinkClass(
                            tamer: card.PermanentOfThisCard(),
                            digimonCondition: CanSelectPermanentCondition,
                            activateClass: activateClass
                        ).MindLink()
                    );
                }
            }

            #endregion

            #region Rule Text - Name

            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("Also treated as [Eiji Nagasumi]", _ => true, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: ChangeCardNames);
                cardEffects.Add(changeCardNamesClass);

                List<string> ChangeCardNames(CardSource cardSource, List<string> cardNames)
                {
                    if (cardSource == card)
                    {
                        cardNames.Add("Eiji Nagasumi");
                        cardNames.Add("EijiNagasumi");
                    }

                    return cardNames;
                }
            }

            #endregion

            #region All Turns - ESS

            bool DarkAnimalSocCondition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("DarkAnimal"))
                    {
                        return true;
                    }

                    if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Dark Animal"))
                    {
                        return true;
                    }

                    if (card.PermanentOfThisCard().TopCard.CardTraits.Contains("SoC"))
                    {
                        return true;
                    }
                }

                return CardEffectCommons.IsExistOnBattleArea(card) &&
                       (card.PermanentOfThisCard().TopCard.CardTraits.Contains("Dark Animal") ||
                        card.PermanentOfThisCard().TopCard.CardTraits.Contains("DarkAnimal") ||
                        card.PermanentOfThisCard().TopCard.CardTraits.Contains("SoC"));
            }

            // Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: true, card: card,
                    condition: DarkAnimalSocCondition));
            }

            // Alliance
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.AllianceSelfEffect(isInheritedEffect: true, card: card,
                    condition: DarkAnimalSocCondition));
            }

            #endregion

            #region End of All Turns

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Eiji Nagasumi] from this Digimon's digivolution cards", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[End of All Turns] You may play 1 [Eiji Nagasumi] from this Digimon's digivolution cards without paying the cost.";
                }

                bool IsEijiCardCondition(CardSource cardSource)
                {
                    return CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass) &&
                           (cardSource.EqualsCardName("Eiji Nagasumi") ||
                            cardSource.EqualsCardName("EijiNagasumi"));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           card.PermanentOfThisCard().DigivolutionCards.Some(IsEijiCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        if (selectedPermanent != null)
                        {
                            if (selectedPermanent.DigivolutionCards.Some(IsEijiCardCondition))
                            {
                                int maxCount = 1;

                                List<CardSource> selectedCards = new List<CardSource>();

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: IsEijiCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 digivolution card to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedPermanent.DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to play.",
                                    "The opponent is selecting 1 digivolution card to play.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                                yield return StartCoroutine(selectCardEffect.Activate());

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);
                                    yield return null;
                                }

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: selectedCards,
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.DigivolutionCards,
                                    activateETB: true));
                            }
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}