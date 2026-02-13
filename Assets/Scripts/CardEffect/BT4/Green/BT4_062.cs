using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT4_062 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Suspend Digimons and return Digimons to deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] <Digi-Burst 4> (Trash 4 of this Digimon's digivolution cards to activate the effect below.) - Suspend all of your opponent's Digimon with 5000 DP or less. Then, place all of your opponent's suspended Digimon at the bottom of their owners' decks in any order. Trash all of the digivolution cards of those Digimon.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.DP <= 5000)
                    {
                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool PermanentCondition1(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (permanent.IsSuspended)
                    {
                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                        {
                            if (!permanent.CannotReturnToLibrary(activateClass))
                            {
                                return true;
                            }
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
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (new IDigiBurst(card.PermanentOfThisCard(), 4, activateClass).CanDigiBurst())
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(new IDigiBurst(card.PermanentOfThisCard(), 4, activateClass).DigiBurst());

                List<Permanent> suspendTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(PermanentCondition);
                yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(suspendTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                List<Permanent> libraryPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(PermanentCondition1);

                if (libraryPermanents.Count >= 1)
                {
                    if (libraryPermanents.Count == 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(libraryPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).DeckBounce());
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
                            List<Permanent> libraryPermanets = selectedCards
                                .Map(cardSource => cardSource.PermanentOfThisCard());

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
        }

        return cardEffects;
    }
}
