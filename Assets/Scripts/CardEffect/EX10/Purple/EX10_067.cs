using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Ryoma Mogami
namespace DCGO.CardEffects.EX10
{
    public class EX10_067 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Turn

            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend and place 1 to gain Alliance", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When any of your Digimon digivolve into a Digimon with <Save> in its text, by suspending this Tamer and placing 1 Digimon card with <Save> in its text from under your Tamers as that Digimon's bottom digivolution card, that Digimon gains <Alliance> for the turn.";
                }

                bool HasSaveText(CardSource source)
                {
                    return source.IsDigimon && source.HasSaveText;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        return HasSaveText(permanent.TopCard);

                    return false;
                }

                bool IsTamerHasSource(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card) &&
                           permanent.IsTamer &&
                           permanent.DigivolutionCards.Count(HasSaveText) > 0;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, PermanentCondition, null))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(IsTamerHasSource))
                                return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    Permanent selectedTamerPermanent = null;
                    List<CardSource> selectedSource = new List<CardSource>();

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsTamerHasSource,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to get the placed card from.",
                        "The opponent is selecting 1 Tamer to get the placed card from.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedTamerPermanent = permanent;

                        yield return null;
                    }

                    if (selectedTamerPermanent != null)
                    {
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: HasSaveText,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to place on bottom of digivolution cards.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: selectedTamerPermanent.DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");
                        selectCardEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedSource.Add(cardSource);

                            yield return null;
                        }

                        if (selectedSource.Count > 0)
                        {
                            List<Permanent> permanents = CardEffectCommons.GetPlayedPermanentsFromEnterFieldHashtable(
                                hashtable: hashtable,
                                rootCondition: null);
                            yield return ContinuousController.instance.StartCoroutine(permanents[0].AddDigivolutionCardsBottom(selectedSource, activateClass));
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainAlliance(targetPermanent: permanents[0], effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                        }
                    }
                }
            }

            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}