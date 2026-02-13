using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class BT16_095 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Option Skill

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend Digimon or tamers and activate effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Suspend 2 of your opponent's Digimon. Then, return all of your opponent's suspended Digimon with the lowest DP to the bottom of the deck. All of your Digimon get +3000 DP until the end of your opponent's turn.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanSelectPermanentsToBotDeck(Permanent permanent)
                {
                    return CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy, (permanent) => permanent.IsSuspended);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                    {
                        int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(PermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: PermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 2 of your opponent's Digimon to suspend.", "The opponent is selecting 2 Digimon to suspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentsToBotDeck))
                    {
                        List<Permanent> selectedPermanents = new List<Permanent>();

                        foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons())
                        {
                            if (!CanSelectPermanentsToBotDeck(permanent))
                                continue;

                            if (permanent.TopCard.CanNotBeAffected(activateClass))
                                continue;

                            if (permanent.CannotReturnToLibrary(activateClass))
                                continue;

                            selectedPermanents.Add(permanent);
                        }

                        if (selectedPermanents.Count >= 1)
                        {
                            if (selectedPermanents.Count == 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(selectedPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).DeckBounce());
                            }
                            else
                            {
                                List<CardSource> cardSources = selectedPermanents.Map(permanent => permanent.TopCard);

                                List<SkillInfo> skillInfos = cardSources.Map(cardSource =>
                                {
                                    ChangeBaseDPClass cardEffect = new ChangeBaseDPClass();
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
                                    message: "Specify the order to place the card at the bottom of the deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).",
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
                                selectCardEffect.SetNotAddLog();
                                selectCardEffect.SetUpSkillInfos(skillInfos);

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                                {
                                    if (cardSources.Count >= 1)
                                    {
                                        selectedCards = cardSources.Clone();

                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Bottom Cards", true, true));
                                    }
                                }

                                if (selectedCards.Count >= 1)
                                {
                                    List<Permanent> libraryPermanets = selectedCards.Map(cardSource => cardSource.PermanentOfThisCard());

                                    if (libraryPermanets.Count >= 1)
                                    {
                                        DeckBottomBounceClass putLibraryBottomPermanent = new DeckBottomBounceClass(libraryPermanets, CardEffectCommons.CardEffectHashtable(activateClass));

                                        putLibraryBottomPermanent.SetNotShowCards();

                                        yield return ContinuousController.instance.StartCoroutine(putLibraryBottomPermanent.DeckBounce());
                                    }
                                }
                            }
                        }
                    }

                    bool PermanentCondition1(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                    }
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDPPlayerEffect(
                    permanentCondition: PermanentCondition1,
                    changeValue: 3000,
                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                    activateClass: activateClass));
                }
            }

            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Suspend 2 Digimon and activate effects.");
            }

            #endregion

            return cardEffects;
        }
    }
}