using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Vulcanusmon
namespace DCGO.CardEffects.BT25
{
    public class BT25_075 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.EqualsTraits("TS");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, true, card, null, level: 5));
            }
            #endregion

            #region Reduce Play Cost
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return card.Owner.GetBattleAreaDigimons().Count < card.Owner.Enemy.GetBattleAreaDigimons().Count;
                }

                cardEffects.Add(CardEffectFactory.MandatorySelfPlayCostReduction(5, card, Condition));
            }
            #endregion

            #region Shared OP / WD

            string SharedEffectName = "May link up to 2 cards from hand/trash to your digimon for free. Then <De-Digivolve 1> all enemy Digimon per your link card";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may link up to 2 cards from your hand or trash to any of your Digimon without paying the cost. Then, for each of your link cards, <De-Digivolve 1> all of your opponent's Digimon.";

            bool CanLinkCardCondition(CardSource cardSource) => cardSource.CanLink(false);

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    whenDigivolving: true,
                    onPlay: true);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Link 2 cards
                int toLink = Math.Min(2, card.Owner.HandCards.Count(CanLinkCardCondition)+card.Owner.TrashCards.Count(CanLinkCardCondition));
                while (toLink > 0)
                {
                    int validHandCardCount = card.Owner.HandCards.Count(CanLinkCardCondition);
                    int validTrashCardCount = card.Owner.TrashCards.Count(CanLinkCardCondition);

                    if (validHandCardCount > 0 && validTrashCardCount > 0)
                    {
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                        {
                            new(message: "from Hand", value: 1, spriteIndex: 0),
                            new(message: "from Trash", value: 2, spriteIndex: 0),
                            new(message: "Do not link", value: 3, spriteIndex: 1)
                        };

                        GManager.instance.userSelectionManager.SetIntSelection(
                            selectionElements: selectionElements,
                            selectPlayer: card.Owner,
                            selectPlayerMessage: "From which area will you link a card?",
                            notSelectPlayerMessage: "The opponent is choosing from which area to link card.");
                    }
                    else
                    {
                        GManager.instance.userSelectionManager.SetInt(validHandCardCount > 0 ? 1 : 2);
                    }

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    if (GManager.instance.userSelectionManager.SelectedIntValue == 3)
                    {
                        break;
                    }
                    if (GManager.instance.userSelectionManager.SelectedIntValue == 1)
                    {
                        int maxCount = Math.Min(toLink, validHandCardCount);
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanLinkCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: true,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        string messagePluralize = maxCount > 1 ? "Select one or more cards to link to 1 Digimon. You will be able to select a second link card and second Digimon target if you only select 1 card now." : "Select a card to link to 1 Digimon.";

                        selectHandEffect.SetUpCustomMessage(
                            messagePluralize,
                            $"The opponent is selecting cards to link.");

                        yield return StartCoroutine(selectHandEffect.Activate());
                    }
                    else
                    {
                        int maxCount = Math.Min(toLink, validTrashCardCount);
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanLinkCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Select link card(s)",
                            maxCount: maxCount,
                            canEndNotMax: true,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        string messagePluralize = maxCount > 1 ? "Select one or more cards to link to 1 Digimon. You will be able to select a second link card and second Digimon target if you only select 1 card now." : "Select a card to link to 1 Digimon.";

                        selectCardEffect.SetUpCustomMessage(
                            messagePluralize,
                            $"The opponent is selecting cards to link.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                    }

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count == 0)
                        {
                            toLink = 0;
                        }
                        else
                        {
                            bool CanLinkPermanentCondition(Permanent permanent)
                            {
                                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                    && cardSources.All(cardSource => cardSource.CanLinkToTargetPermanent(permanent, false));
                            }
                            
                            if (CardEffectCommons.HasMatchConditionPermanent(CanLinkPermanentCondition))
                            {
                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanLinkPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                string choicePluralize = cardSources.Count > 1 ? "cards" : "card";

                                selectPermanentEffect.SetUpCustomMessage($"Select 1 Digimon to link the chosen {choicePluralize}.", "The opponent is selecting 1 Digimon to link.");
                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    foreach (CardSource cardSource in cardSources)
                                        yield return ContinuousController.instance.StartCoroutine(new ILinkCard(false, cardSource, permanent, activateClass).LinkCard());

                                    toLink -= cardSources.Count;
                                }
                            }
                            else
                            {
                                List<SelectionElement<int>> selectionElements1 = new List<SelectionElement<int>>()
                                {
                                    new(message: "Ok", value: 1, spriteIndex: 1)
                                };

                                GManager.instance.userSelectionManager.SetIntSelection(
                                    selectionElements: selectionElements1,
                                    selectPlayer: card.Owner,
                                    selectPlayerMessage: "The cards you chose do not have a valid digimon which could link both. Try choosing 1 at a time.",
                                    notSelectPlayerMessage: "The opponent is selecting cards to link.");

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                            }
                        }
                    }
                }
                #endregion

                #region De-Digivolve
                int degenerationCount = card.Owner.GetBattleAreaDigimons().Map(permanent => permanent.LinkedCards).Flat().Count();
                for (int i = 0; i < degenerationCount; i++)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IMassDegeneration(card.Owner.Enemy.GetBattleAreaDigimons(), 1, activateClass).Degeneration());
                }
                #endregion
            }

            #endregion

            #region All Turns
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasTSTraits;
                }

                bool Condition() => CardEffectCommons.IsExistOnBattleArea(card);

                cardEffects.Add(CardEffectFactory.RushStaticEffect(PermanentCondition, false, card, Condition));

                cardEffects.Add(CardEffectFactory.ChangeLinkMaxStaticEffect(PermanentCondition, 1, false, card, Condition));
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Linked Digimon may Attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetEffectTargets(TargetablePermanents);

                cardEffects.Add(activateClass);

                string EffectDescription() => "[Your Turn] When your Digimon get linked, one of them may attack.";

                bool CanSelectPermanentCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass) && 
                        CardEffectCommons.IsOwnerTurn(card) && 
                        CardEffectCommons.CanTriggerWhenLinked(hashtable, CanSelectPermanentCondition, null);

                List<Permanent> TargetablePermanents(Hashtable hashtable) => CardEffectCommons.GetPermanentsFromHashtable(hashtable).Where(permanent => permanent.CanAttack(activateClass)).ToList();

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
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
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Attack,
                            cardEffect: activateClass);
                    
                        
                        selectPermanentEffect.SetUpCustomMessage("Select 1 linked Digimon to attack.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
