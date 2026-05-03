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
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -5", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return card.Owner.GetBattleAreaDigimons().Count < card.Owner.GetBattleAreaDigimons().Count;
                }

                int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root,
                        List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource) &&
                        RootCondition(root) &&
                        PermanentsCondition(targetPermanents))
                    {
                        cost -= 5;
                    }

                    return cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    return targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                bool isUpDown()
                {
                    return true;
                }
            }
            #endregion

            #region Shared OP / WD

            string SharedEffectName = "May link up to 2 cards from your hand to any of your digimon for free. Then <De-Digivolve 1> all enemy Digimon per your link card";

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
                int toLink = Math.Min(2, card.Owner.HandCards.Count(CanLinkCardCondition));
                while (toLink > 0)
                {
                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanLinkCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: toLink,
                        canNoSelect: true,
                        canEndNotMax: true,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);

                    string messagePluralize = toLink > 1 ? "Select one or more cards to link to 1 Digimon. You will be able to select a second link card and second Digimon target if you only select 1 card now." : "Select a card to link to 1 Digimon.";

                    selectHandEffect.SetUpCustomMessage(
                        messagePluralize,
                        $"The opponent is selecting cards to link.");

                    yield return StartCoroutine(selectHandEffect.Activate());

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
                activateClass.SetUpICardEffect("Linked digimon may Attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] When your Digimon get linked, one of them may attack.";
                }

                bool PermanentCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) && permanent.CanAttack(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerWhenLinked(hashtable, PermanentCondition, null);
                }

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleArea(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent attacker = CardEffectCommons.GetPermanentFromHashtable(hashtable);
                    if (attacker != null && attacker.TopCard != null)
                    {
                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: attacker,
                            canAttackPlayerCondition: () => true,
                            defenderCondition: (permanent) => true,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
