using System;
using System.Collections;
using System.Collections.Generic;

// Shakkoumon
namespace DCGO.CardEffects.BT23
{
    public class BT23_032 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.IsLevel4
                        && targetPermanent.TopCard.HasCSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region DNA Digivolution - Yellow Lv.4 + Black/Blue Lv.4: Cost 0

            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
                addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
                addJogressConditionClass.SetNotShowUI(true);
                cardEffects.Add(addJogressConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                JogressCondition GetJogress(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool PermanentCondition1(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                            {
                                if (permanent.TopCard.CardColors.Contains(CardColor.Yellow))
                                {
                                    if (permanent.Levels_ForJogress(card).Contains(4))
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                            {
                                if (permanent.TopCard.CardColors.Contains(CardColor.Black) || permanent.TopCard.CardColors.Contains(CardColor.Blue))
                                {
                                    if (permanent.Levels_ForJogress(card).Contains(4))
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 4 Yellow Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 4 Black or Blue Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 digimon gains [This digimon attacks at start of main phase]. then if DNA digivolved, <De-Digivolve 1>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Until your opponent's turn ends, give 1 of their Digimon '[Start of Your Main Phase] This Digimon attacks.'. Then, if DNA digivolving, <De-Digivolve 1> 1 of your opponent's Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanSelectPermamentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermamentCondition))
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermamentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get effects.",
                            "The opponent is selecting 1 Digimon that will get effects.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            ActivateClass activateClassDebuff = new ActivateClass();
                            activateClassDebuff.SetUpICardEffect("Attack with this Digimon", CanUseConditionDebuff,
                                selectedPermanent.TopCard);
                            activateClassDebuff.SetUpActivateClass(CanActivateConditionDebuff, ActivateCoroutineDebuff, -1, false,
                                EffectDescriptionDebuff());
                            activateClassDebuff.SetEffectSourcePermanent(selectedPermanent);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add(GetCardEffect);

                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                                    .CreateDebuffEffect(selectedPermanent));
                            }

                            string EffectDescriptionDebuff()
                            {
                                return "[Start of Your Main Phase] Attack with this Digimon.";
                            }

                            bool CanUseConditionDebuff(Hashtable hashtable1)
                            {
                                return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(selectedPermanent) &&
                                       selectedPermanent.TopCard.Owner.GetBattleAreaDigimons().Contains(selectedPermanent) &&
                                       GManager.instance.turnStateMachine.gameContext.TurnPlayer == selectedPermanent.TopCard.Owner;
                            }

                            bool CanActivateConditionDebuff(Hashtable hashtable1)
                            {
                                return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(selectedPermanent) &&
                                       !selectedPermanent.TopCard.CanNotBeAffected(activateClass) &&
                                       selectedPermanent.CanAttack(activateClassDebuff);
                            }

                            IEnumerator ActivateCoroutineDebuff(Hashtable hashtableDebuff)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent) &&
                                    selectedPermanent.CanAttack(activateClassDebuff))
                                {
                                    SelectAttackEffect selectAttackEffect =
                                        GManager.instance.GetComponent<SelectAttackEffect>();

                                    selectAttackEffect.SetUp(
                                        attacker: selectedPermanent,
                                        canAttackPlayerCondition: () => true,
                                        defenderCondition: _ => true,
                                        cardEffect: activateClassDebuff);

                                    selectAttackEffect.SetCanNotSelectNotAttack();

                                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                                }
                            }

                            ICardEffect GetCardEffect(EffectTiming timingDebuff)
                            {
                                return timingDebuff == EffectTiming.OnStartMainPhase ? activateClassDebuff : null;
                            }
                        }
                    }

                    if (CardEffectCommons.IsJogress(hashtable) && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermamentCondition))
                    {
                        Permanent selectedPermanent = null;
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermamentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to gain <De-digivolve 1>.", "The opponent is selecting 1 Digimon to gain <De-digivolve 1>.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null) yield return ContinuousController.instance.StartCoroutine(new IDegeneration(
                            permanent: selectedPermanent,
                            DegenerationCount: 1,
                            cardEffect: activateClass).Degeneration());
                    }
                }
            }

            #endregion

            #region All Turns Shared

            string SharedEffectName() => "Play 1 level 4- [Yellow]/[Black]/[CS] trait digimon from digivolution sources";

            string SharedEffectDiscription(string tag) => $"[{tag}] [Once Per Turn] When this Digimon would leave the battle area other than by your effects, you may play 1 level 4 or lower yellow, black or [CS] trait Digimon card from its digivolution cards without paying the cost.";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass, bool IsTopCard)
            {
                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel && cardSource.Level <= 4
                        && (cardSource.CardColors.Contains(CardColor.Yellow) || cardSource.CardColors.Contains(CardColor.Black) || cardSource.HasCSTraits)
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, false, activateClass);
                }

                var thisPermament = card.PermanentOfThisCard();

                if (thisPermament.StackCards.Exists(CanSelectCardCondition))
                {
                    CardSource selectedCard = null;
                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                    int maxCount = Math.Min(1, thisPermament.StackCards.Filter(CanSelectCardCondition).Count);

                    selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 digivolution card to play.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: null);

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCard = cardSource;
                        yield return null;
                    }

                    selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to play.", "The opponent is selecting 1 digivolution card to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Selected card");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: new List<CardSource>() { selectedCard },
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.DigivolutionCards,
                        activateETB: true));
                }
            }

            #endregion

            #region All Turns - OPT

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass, true), 1, true, SharedEffectDiscription("All Turns"));
                activateClass.SetHashString("BT23_032_AT");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card)
                        && !CardEffectCommons.IsByEffect(hashtable, effect => effect.EffectSourceCard.Owner == card.Owner);
                }
            }

            #endregion

            #region All Turns - OPT - ESS

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                 activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass, false), 1, true, SharedEffectDiscription("Inherited All Turns"));
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT23_032_AT_ESS");
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnField(card)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermamentCondition)
                        && !CardEffectCommons.IsByEffect(hashtable, effect => effect.EffectSourceCard.Owner == card.Owner);
                }

                bool PermamentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent == card.PermanentOfThisCard().TopCard.PermanentOfThisCard();
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
