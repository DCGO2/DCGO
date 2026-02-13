using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX8
{
    public class EX8_050 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reveal top 3 from deck, Play 1 5 cost or less, [Mineral] or [Rock] trait Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] Reveal the top 3 cards of your deck. You may play 1 [Mineral] or [Rock] trait Digimon card with a play cost of 5 or less from among them without paying the cost. Trash the rest.";
                }

                bool PlayableMineralorRock(CardSource source)
                {
                    return CardEffectCommons.CanPlayAsNewPermanent(cardSource: source, payCost: false, cardEffect: activateClass) &&
                           source.IsDigimon &&
                           source.HasPlayCost && source.GetCostItself <= 5 &&
                           (source.ContainsTraits("Mineral") || source.ContainsTraits("Rock"));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.RevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    selectCardConditions:
                    new SelectCardConditionClass[]
                    {
                        new SelectCardConditionClass(
                            canTargetCondition:PlayableMineralorRock,
                            canTargetCondition_ByPreSelecetedList:null,
                            canEndSelectCondition:null,
                            canNoSelect:true,
                            selectCardCoroutine: CardToPlay,
                            message: "Select 1 [Mineral] or [Rock] trait digimon with 5 cost or less to play",
                            maxCount: 1,
                            canEndNotMax:false,
                            mode: SelectCardEffect.Mode.Custom
                            )
                    },
                    remainingCardsPlace: RemainingCardsPlace.Trash,
                    activateClass: activateClass,
                    canNoAction: false));

                    IEnumerator CardToPlay(CardSource source)
                    {
                        selectedCards.Add(source);

                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Library, activateETB: true));
                }
            }
            #endregion

            #region Redirect - ESS
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may change the attack target to this Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Redirect_BT19-072");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn] [Once Per Turn] When any of your opponent's Digimon attack, you may change the attack target to this Digimon.";
                }

                bool IsOpponentDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool IsThisDigimon(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOpponentTurn(card) &&
                           CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, IsOpponentDigimon);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsThisDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to attack.",
                        "The opponent is selecting 1 Digimon to attack.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(
                            activateClass,
                            false,
                            permanent));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}