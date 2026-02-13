using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT16
{
    public class BT16_090 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Start Turn

            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }

            #endregion

            #region Main Effect

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete [Ukkomon] and breeding area Digimon to play [BigUkkomon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Play_ST16_090");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return
                        "[Main] [Once Per Turn] By deleting 1 of your [Ukkomon] and trashing 1 of your Digimon in the breeding area, you may play 1 [BigUkkomon] in your breeding area from your hand for a cost of 3.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.CardNames.Contains("Ukkomon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectBreedingAreaDigimon(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            return !permanent.ImmuneFromStackTrashing(activateClass);
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.ContainsCardName("Big Ukkomon"))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentCondition) &&
                            card.Owner.GetBreedingAreaPermanents().Count(CanSelectBreedingAreaDigimon) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool ukkomonDeleted = false;
                    bool breedingAreaPermanentDeleted = false;

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.",
                            "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                    targetPermanents: new List<Permanent>() { permanent }, activateClass: activateClass,
                                    successProcess: _ => SuccessProcess(), failureProcess: null));

                            IEnumerator SuccessProcess()
                            {
                                ukkomonDeleted = true;
                                yield return null;
                            }
                        }

                        if (!ukkomonDeleted)
                            yield break;

                        if (card.Owner.GetBreedingAreaPermanents().Count(CanSelectBreedingAreaDigimon) >= 1)
                        {
                            List<Permanent> digitamaPermanents = card.Owner.GetBreedingAreaPermanents()
                                .Filter(permanent => permanent.IsDigimon)
                                .Clone();

                            if (digitamaPermanents.Count >= 1)
                            {
                                foreach (Permanent permanent in digitamaPermanents)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(permanent.DiscardEvoRoots());

                                    CardSource cardSource = permanent.TopCard;

                                    ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                        .ShowCardEffect(new List<CardSource>() { cardSource }, "Cards put to trash", true, true));

                                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveField(permanent));
                                    yield return ContinuousController.instance.StartCoroutine(
                                        CardObjectController.AddTrashCard(cardSource));
                                    breedingAreaPermanentDeleted = true;
                                }
                            }

                            if (ukkomonDeleted && breedingAreaPermanentDeleted)
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

                                    selectHandEffect.SetUpCustomMessage("Select 1 card to play.",
                                        "The opponent is selecting 1 card to play.");
                                    selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                                    yield return StartCoroutine(selectHandEffect.Activate());

                                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                                    {
                                        selectedCards.Add(cardSource);

                                        yield return null;
                                    }

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                        cardSources: selectedCards,
                                        activateClass: activateClass,
                                        payCost: true,
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Hand,
                                        activateETB: true,
                                        isBreedingArea: true,
                                        fixedCost: 3));
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Security Skill

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}