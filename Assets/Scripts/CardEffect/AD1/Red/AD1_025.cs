using System.Collections;
using System.Collections.Generic;
using System;

// Omnimon
namespace DCGO.CardEffects.AD1
{
    public class AD1_025 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region DNA Condition
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.GetJogressConditionClass(
                    PermanentCondition1, 
                    "Lv.6 w/[Greymon] in name", 
                    PermanentCondition2, 
                    "Lv.6 w/[Garurumon] in name", 
                    card
                ));

                bool PermanentCondition1(Permanent permanent)
                {
                    return permanent.TopCard.ContainsCardName("Greymon")
                        && permanent.Levels_ForJogress(card).Contains(6);
                }

                bool PermanentCondition2(Permanent permanent)
                {
                    return permanent.TopCard.ContainsCardName("Garurumon")
                        && permanent.Levels_ForJogress(card).Contains(6);
                }
            }
            #endregion

            #region Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Partition
            List<PartitionCondition> partitionConditions = new List<PartitionCondition>
            {
                new PartitionCondition("WarGreymon"),
                new PartitionCondition("MetalGarurumon")
            };

            if (timing == EffectTiming.WhenRemoveField)
            {
                cardEffects.Add(CardEffectFactory.PartitionSelfEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null,
                    cardSourceConditions: partitionConditions));
            }
            #endregion

            #region Common Methods

            bool IsOpponentsDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            #endregion

            #region Shared OP / WD

            string SharedEffectName = "Bottom deck all enemy Digimon with as many or fewer sources as this, then delete 1 enemy Digimon";

            string SharedEffectDescription(string tag) 
                => $"[{tag}] Return all of your opponent's Digimon with as many or fewer digivolution cards as this Digimon to the bottom of the deck. Then, delete 1 of your opponent's Digimon.";

            bool SharedCanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

            bool IsEnemyWithLessSources(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionCards.Count <= card.PermanentOfThisCard().DigivolutionCards.Count;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                List<Permanent> bottomDeckTargets = card.Owner.Enemy.GetBattleAreaDigimons().Filter(IsEnemyWithLessSources);

                yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(bottomDeckTargets, CardEffectCommons.CardEffectHashtable(activateClass)).DeckBounce());

                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsOpponentsDigimon))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsOpponentsDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnLeaveFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 enemy option card in the battle area and 1 card from their security.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("AD1-025_AT");
                cardEffects.Add(activateClass);

                string EffectDescription() 
                    => "[All Turns] [Once Per Turn] When any of your opponent's Digimon leave the battle area, trash 1 of their Option cards in the battle area and trash their top security card.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, IsOpponentsDigimon);
                }

                bool IsEnemyOptionPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                        && permanent.IsOption;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, IsEnemyOptionPermanent))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: IsEnemyOptionPermanent,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select option to trash.", "The opponent is selecting 1 option to trash.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                }
            }
            #endregion

            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect($"Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element1 = new AssemblyConditionElement(CanSelectCardCondition1, selectMessage: "[WarGreymon]", elementCount: 1);
                        AssemblyConditionElement element2 = new AssemblyConditionElement(CanSelectCardCondition2, selectMessage: "[MetalGarurumon]", elementCount: 1);

                        bool CanSelectCardCondition1(CardSource cardSource)
                        {
                            return cardSource != null && 
                                cardSource.Owner == card.Owner && 
                                cardSource.IsDigimon && 
                                cardSource.EqualsCardName("WarGreymon");
                        }

                        bool CanSelectCardCondition2(CardSource cardSource)
                        {
                            return cardSource != null && 
                                cardSource.Owner == card.Owner && 
                                cardSource.IsDigimon && 
                                cardSource.EqualsCardName("MetalGarurumon");
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            elements:new List<AssemblyConditionElement>() { element1, element2 },
                            reduceCost: 6);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}