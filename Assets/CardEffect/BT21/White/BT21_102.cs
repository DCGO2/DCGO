using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Tai Kamiya
namespace DCGO.CardEffects.BT21
{
    public class BT21_102 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Turn

            if (timing == EffectTiming.OnStartTurn) cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));

            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend to draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "When one of your Digimon attacks, by suspending this Tamer, <Draw 1>";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                    && CardEffectCommons.IsOwnerTurn(card)
                    && CardEffectCommons.CanActivateSuspendCostEffect(card)
                    && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition);

                bool PermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }
                }
            }

            #endregion

            #region Main

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play Adventure/Hero card for reduced cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Play_BT21_102");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Main] [Once Per Turn] You may play 1 [ADVENTURE]/[Hero] trait card with a play cost of 2 or less from your hand without paying the cost. For each of your Tamers' colors, add 1 to this effect's play cost maximum. Then, return this Tamer to the bottom of the deck.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card)
                    && CardEffectCommons.IsOwnerTurn(card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleArea(card);

                bool CanSelectCard(CardSource source)
                    => source.HasPlayCost &&
                    source.BasePlayCostFromEntity <= AdjustedPlayCostMax(source)
                    && (source.HasAdventureTraits || source.HasHeroTraits);

                int AdjustedPlayCostMax(CardSource cardSource)
                {
                    var tamers = cardSource.Owner.GetBattleAreaPermanents().Filter(x => x.IsTamer).Select(x => x.TopCard).ToList();
                    return 2 + Combinations.GetDifferenetColorCardCount(tamers);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCard,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
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
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource>() { cardSource },
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true,
                            isBreedingArea: false
                        ));
                    }

                    var selectedPermanents = new List<Permanent>() { card.PermanentOfThisCard() };

                    DeckBottomBounceClass putLibraryBottomPermanent = new DeckBottomBounceClass(selectedPermanents, hashtable);
                    putLibraryBottomPermanent.SetNotShowCards();
                    yield return ContinuousController.instance.StartCoroutine(putLibraryBottomPermanent.DeckBounce());
                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill) cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));

            #endregion

            return cardEffects;
        }
    }
}