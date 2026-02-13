using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class RB1_019 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool PermanentCondition(Permanent targetPermanent)
            {
                if (targetPermanent.TopCard.ContainsCardName("Numemon") || targetPermanent.TopCard.ContainsCardName("Monzaemon"))
                {
                    if (targetPermanent.TopCard.HasLevel)
                    {
                        if (targetPermanent.Level == 5)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Return Digimon to the bottom of Security, and opponent's Digimon gains DP -3000 and Security Attack -1", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Place all level 3 Digimon face down on top of their owners' security stacks in any order. Then, all of your opponentÅf level 4 or higher Digimon get -3000 DP and gain <Security Attack -1> until the end of their turn.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                {
                    if (permanent.IsDigimon)
                    {
                        if (permanent.Level == 3)
                        {
                            if (permanent.TopCard.HasLevel)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            bool PermanentCondition1(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.Level >= 4)
                    {
                        if (permanent.TopCard.HasLevel)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                {
                    foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer.Clone())
                    {
                        List<Permanent> libraryPermanents = player.GetBattleAreaDigimons().Filter(PermanentCondition);

                        if (libraryPermanents.Count >= 1)
                        {
                            if (libraryPermanents.Count == 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(libraryPermanents[0], CardEffectCommons.CardEffectHashtable(activateClass), toTop: true).PutSecurity());
                            }

                            else
                            {
                                List<CardSource> cardSources = libraryPermanents
                                    .Map(permanent => permanent.TopCard);

                                List<SkillInfo> skillInfos = cardSources
                                    .Map(cardSource =>
                                    {
                                        ICardEffect cardEffect = new ChangeBaseDPClass();
                                        cardEffect.SetUpICardEffect(" ", null, cardSource);

                                        return new SkillInfo(cardEffect, null, EffectTiming.None);
                                    });

                                List<CardSource> selectedCards = new List<CardSource>();

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: (cardSource) => true,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => false,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                                    message: "Specify the order to place the card at the top of the security\n(cards will be placed back to the top of the security so that cards with lower numbers are on top).",
                                    maxCount: cardSources.Count,
                                    canEndNotMax: false,
                                    isShowOpponent: false,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: cardSources,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetNotShowCard();
                                selectCardEffect.SetUpSkillInfos(skillInfos);

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                                {
                                    if (cardSources.Count >= 1)
                                    {
                                        selectedCards = cardSources.Clone();

                                        if (player.isYou)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(cardSources, "Security Top Cards", true, true));
                                        }

                                        else
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Security Top Cards", true, true));
                                        }
                                    }
                                }

                                if (selectedCards.Count >= 1)
                                {
                                    List<Permanent> securityPermanets = selectedCards
                                        .Map(cardSource => cardSource.PermanentOfThisCard());

                                    if (securityPermanets.Count >= 1)
                                    {
                                        securityPermanets.Reverse();

                                        foreach (Permanent selectedPermanent in securityPermanets)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(selectedPermanent, CardEffectCommons.CardEffectHashtable(activateClass), toTop: true).PutSecurity());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDPPlayerEffect(
                    permanentCondition: PermanentCondition1,
                    changeValue: -3000,
                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                    activateClass: activateClass));

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttackPlayerEffect(
                    permanentCondition: PermanentCondition1,
                    changeValue: -1,
                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                    activateClass: activateClass));
            }
        }

        if (timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Return 1 digivolution card to place 1 Digimon into the bottom of Security", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Attacking] By trashing 1 card with [Numemon] in its name in this Digimon's digivolution cards, place 1 of your opponentÅf Digimon face down at the bottom of their security stack.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.ContainsCardName("Numemon"))
                {
                    if (!cardSource.CanNotTrashFromDigivolutionCards(activateClass))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (card.Owner.Enemy.CanAddSecurity(activateClass))
                    {
                        return true;
                    }
                }

                return false;
            }


            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    bool trashed = false;

                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 digivolution card to discard.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: null);

                    selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to discard.", "The opponent is selecting 1 digivolution card to discard.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (selectedCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(card.PermanentOfThisCard(), selectedCards, activateClass).TrashDigivolutionCards());

                        trashed = true;
                    }

                    if (trashed)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place to security.", "The opponent is selecting 1 Digimon to place to security.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                Permanent selectedPermanent = permanent;

                                if (selectedPermanent != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(selectedPermanent, CardEffectCommons.CardEffectHashtable(activateClass), toTop: false).PutSecurity());
                                }
                            }
                        }
                    }
                }
            }
        }

        return cardEffects;
    }
}