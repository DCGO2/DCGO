using System;
using System.Collections;
using System.Collections.Generic;

// Seventh Graviton
namespace DCGO.CardEffects.BT24
{
    public class BT24_096 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region [Trash] Your Turn

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By returning this card to deck, Activate [Main] effect", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Trash] [Your Turn] When any of your Digimon digivolve into [Creepymon (X Antibody)], by returning this card to the bottom of the deck, activate this card's [Main] effects.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsCardName("Creepymon (X Antibody)");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card)
                        && CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> cardSources = new List<CardSource>() { card };
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(cardSources));
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(cardSources, "Deck Bottom Card", true, true));

                    if (card.Owner.LibraryCards.Contains(card))
                    {
                        ActivateClass mainActivateClass = CardEffectCommons.OptionMainEffect(card);

                        if (mainActivateClass != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(mainActivateClass.Activate(CardEffectCommons.OptionMainCheckHashtable(card)));
                        }
                    }
                }
            }

            #endregion

            #region Main

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 level 6 or higher. If not trash opponent's top 3 deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] Delete 1 of your opponent's level 6 or higher Digimon. If this effect didn't delete, trash the top 3 cards of your opponent's deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectPermament(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasLevel && permanent.TopCard.Level >= 6;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<Permanent> deleteTargetPermanents = new List<Permanent>();

                    bool failedToDelete = true;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermament))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermament,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            deleteTargetPermanents = permanents.Clone();
    
                            yield return null;
                        }
                    }

                    if (deleteTargetPermanents.Count > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: deleteTargetPermanents, activateClass: activateClass, successProcess: SuccessDeleteProcess, failureProcess: null));

                        IEnumerator SuccessDeleteProcess(List<Permanent> permanents)
                        {
                            if (permanents.Count > 0)
                                failedToDelete = false;

                            yield return null;
                        }
                    }

                    if (failedToDelete)
                    {
                        int trashCount = 3;

                        yield return ContinuousController.instance.StartCoroutine(new IAddTrashCardsFromLibraryTop(trashCount, card.Owner.Enemy, activateClass).AddTrashCardsFromLibraryTop());
                    }
                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(
                    card: card,
                    cardEffects: ref cardEffects,
                    effectName: "Delete 1 level 6 or higher. If not trash opponent's top 3 deck");
            }

            #endregion

            return cardEffects;
        }
    }
}
