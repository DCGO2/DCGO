using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

//Paradise Lost
namespace DCGO.CardEffects.EX10
{
    public class EX10_071 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Color Requirement

            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionPermanent((permanent) =>
                           CardEffectCommons.IsOwnerPermanent(permanent, card) &&
                           permanent.TopCard.ContainsCardName("Lucemon"), true);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }
            }

            #endregion

            #region Main Effect

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 Digimon gains <Raid>, <Piercing>, <Blocker>, +3000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] Until your opponent's turn ends, 1 of your Digimon with [Lucemon] in its name in its name gains <Raid>, <Piercing>, <Blocker> and +3000 DP.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool IsLucemon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.ContainsCardName("Lucemon");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(IsLucemon))
                    {
                        Permanent selectedPermanent = null;
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsLucemon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will gain effects.",
                            "The opponent is selecting 1 Digimon that will gain effects.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRaid(
                                targetPermanent: selectedPermanent,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(
                                targetPermanent: selectedPermanent,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(
                                targetPermanent: selectedPermanent,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: selectedPermanent,
                                changeValue: 3000,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));
                        }
                    }
                }
            }

            #endregion

            #region End Of Your Turn

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash top security, Then one of your Digimon may attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Trash] [End of Your Turn] If you have a Digimon with [Lucemon] in its name, by returning this card to the bottom of the deck, trash your top security card and 1 of your Digimon attacks without suspending.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.IsExistOnTrash(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(IsLucemon);
                }

                bool IsLucemon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.TopCard.ContainsCardName("Lucemon");
                }

                bool AttackWithDigimn(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           permanent.CanAttack(activateClass, true);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(new List<CardSource>() { card }));

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { card }, "Deck Bottom Cards", true, true));

                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                    player: card.Owner,
                                    destroySecurityCount: 1,
                                    cardEffect: activateClass,
                                    fromTop: true).DestroySecurity());

                    if (CardEffectCommons.HasMatchConditionPermanent(AttackWithDigimn))
                    {
                        Permanent selectedPermanent = null;
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: AttackWithDigimn,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will attack.",
                            "The opponent is selecting 1 Digimon that will attack.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            if (selectedPermanent.CanAttack(activateClass, withoutTap: true))
                            {
                                SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                                selectAttackEffect.SetUp(
                                    attacker: selectedPermanent,
                                    canAttackPlayerCondition: () => true,
                                    defenderCondition: (permanent) => true,
                                    cardEffect: activateClass);

                                selectAttackEffect.SetCanNotSelectNotAttack();
                                selectAttackEffect.SetWithoutTap();

                                yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                            }
                        }
                    }
                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Lucemon] from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] You may play 1 [Lucemon] from your trash without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                if (cardSource.EqualsCardName("Lucemon"))
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
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource)))
                    {
                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 [Lucemon] to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 digimon to play.", "The opponent is selecting 1 digimon to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Digimon");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Trash, activateETB: true));
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}