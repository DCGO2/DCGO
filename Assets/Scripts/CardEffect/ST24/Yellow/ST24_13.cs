using System;
using System.Collections;
using System.Collections.Generic;

// Marcus Damon & Thomas H. Norstein
namespace DCGO.CardEffects.ST24
{
    public class ST24_13 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Name Rule
            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("[Rule] Name: Also treated as [Marcus Damon]/[Thomas H. Norstein].", _ => true, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: ChangeCardNames);
                cardEffects.Add(changeCardNamesClass);
            
                List<string> ChangeCardNames(CardSource cardSource, List<string> cardNames)
                {
                    if (cardSource == card)
                    {
                        cardNames.Add("Marcus Damon");
                        cardNames.Add("Thomas H. Norstein");
                    }
            
                    return cardNames;
                }
            }
            #endregion
            
            #region Shared OP/SOMP
            string SharedEffectName = "Place top card of deck face down under this tamer, then if enemy has Digimon gain 1 memory";

            string SharedEffectDescription(string tag) =>
                $"[{tag}] You may place the top card of your deck face down under this tamer. Then, if your opponent has a Digimon, gain 1 memory.";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && (card.Owner.LibraryCards.Count >= 1
                        || card.Owner.Enemy.GetBattleAreaDigimons().Count >= 1);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.LibraryCards.Count >= 1)
                {
                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                    selectionElements.Add(new(message: $"Yes", value: 1, spriteIndex: 0));
                    selectionElements.Add(new(message: $"No", value: 2, spriteIndex: 1));
                    
                    string selectPlayerMessage = "Will you place the top card from your deck under this Tamer face down?";
                    string notSelectPlayerMessage = "The opponent is choosing whether to place the top card from their deck under their Tamer face down.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool Yes = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                    if (Yes)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                new List<CardSource> { card.Owner.LibraryCards[0] }, activateClass, isFacedown: true));
                    }
                }

                if (card.Owner.Enemy.GetBattleAreaDigimons().Count >= 1)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,
                    hash => SharedActivateCoroutine(hash, activateClass), -1, false,
                    SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }
            #endregion

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,
                    hash => SharedActivateCoroutine(hash, activateClass), -1, false,
                    SharedEffectDescription("Start of Your Main Phase"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnDigivolutionCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend this tamer to give 1 of your [DATA SQUAD] Digimon <Jamming> for the turn", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] When effects trash cards from under this Tamer, by suspending this Tamer, 1 of your [DATA SQUAD] trait Digimon gains <Jamming> for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && //trigger condition goes here;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                bool CanSelectPermanentCondition (Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("DATA SQUAD");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                        new List<Permanent>() { card.PermanentOfThisCard() },
                        CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get Jamming.", "The opponent is selecting 1 Digimon that will get Jamming.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainJamming(
                                targetPermanent: permanent,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
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
