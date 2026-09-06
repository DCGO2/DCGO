using System.Collections;
using System.Collections.Generic;

// MetalGarurumon
namespace DCGO.CardEffects.EX12
{
    public class EX12_035 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Garurumon")
                        || targetPermanent.TopCard.EqualsTraits("ME")
                        || targetPermanent.TopCard.EqualsTraits("VB");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null,
                    level: 5)
                );
            }
            #endregion

            #region DNA Digivolution
            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect("DNA Digivolution", CanUseCondition, card);
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
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && (permanent.TopCard.CardColors.Contains(CardColor.Blue)
                                    || permanent.TopCard.CardColors.Contains(CardColor.Black))
                                && permanent.Levels_ForJogress(card).Contains(5);
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && (permanent.TopCard.CardColors.Contains(CardColor.Purple)
                                    || permanent.TopCard.CardColors.Contains(CardColor.Yellow))
                                && permanent.Levels_ForJogress(card).Contains(5);
                        }

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 5 Blue or Black Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 5 Purple or Yellow Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Evade
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.EvadeSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Decode
            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon
                        && cardSource.HasLevel
                        && cardSource.Level <= 5
                        && (cardSource.ContainsCardName("Gabumon")
                            || cardSource.ContainsCardName("Garurumon")
                            || cardSource.EqualsTraits("ME")
                            || cardSource.EqualsTraits("VB"));
                }

                string[] decodeStrings = { "(Lv.5 or lower w/[Gabumon]/[Garurumon] in name or w/[ME]/[VB] trait)", "Level 5 or lower Digimon card with [Gabumon]/[Garurumon] in name or [ME]/[VB] trait" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: false, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "Trash 4 enemy Digimon sources, then bot deck 1 enemy Digimon with less sources than this";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] Trash any 4 digivolution cards from your opponent's Digimon. Then, return 1 of your opponent's Digimon with as many or fewer digivolution cards as this Digimon to the bottom of the deck.";
            }

            bool CanSelectTrashPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && !permanent.HasNoDigivolutionCards;
            }

            bool CanSelectSpinPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionCards.Count <= card.PermanentOfThisCard().DigivolutionCards.Count;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                    permanentCondition: CanSelectTrashPermanentCondition,
                    cardCondition: _ => true,
                    maxCount: 4,
                    canNoTrash: false,
                    isFromOnly1Permanent: false,
                    activateClass: activateClass
                ));

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSpinPermanentCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectSpinPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Stun 1 enemy Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("EX12_035_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When any Digimon are played or digivolve, 1 of your opponent's Digimon can't suspend until their turn ends.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, IsDigimonCondition)
                            || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, IsDigimonCondition));
                }

                bool IsDigimonCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent);

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                bool CanSelectPermanentCondition(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect1.SetUp(
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

                        selectPermanentEffect1.SetUpCustomMessage("Select 1 Digimon to gain can't suspend until end of opponent's turn.", "The opponent is selecting 1 Digimon that will gain can't suspend until end of opponent's turn.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                #region Can't Suspend
                                Permanent selectedPermanent = permanent;

                                CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                                canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCondition1, card);
                                canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                                selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => canNotSuspendClass);

                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                                }

                                bool CanUseCondition1(Hashtable hashtable)
                                {
                                    return selectedPermanent.TopCard != null
                                        && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                                }

                                bool PermanentCondition(Permanent permanent)
                                {
                                    return permanent == selectedPermanent;
                                }
                                #endregion
                            }

                            yield return null;
                        }
                    }
                }
            }
            #endregion

            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect("Assembly", CanUseCondition, card);
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
                        AssemblyConditionElement element1 = new AssemblyConditionElement(CanSelectCardCondition1, elementCount: 1);
                        AssemblyConditionElement element2 = new AssemblyConditionElement(CanSelectCardCondition2, elementCount: 1);
                        AssemblyConditionElement element3 = new AssemblyConditionElement(CanSelectCardCondition3, elementCount: 1);

                        bool CanSelectCardCondition1(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.IsLevel5
                                && (cardSource.ContainsCardName("Gabumon")
                                    || cardSource.ContainsCardName("Garurumon")
                                    || cardSource.EqualsTraits("ME")
                                    || cardSource.EqualsTraits("VB"));
                        }

                        bool CanSelectCardCondition2(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.IsLevel4
                                && (cardSource.ContainsCardName("Gabumon")
                                    || cardSource.ContainsCardName("Garurumon")
                                    || cardSource.EqualsTraits("ME")
                                    || cardSource.EqualsTraits("VB"));
                        }

                        bool CanSelectCardCondition3(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.IsLevel3
                                && (cardSource.ContainsCardName("Gabumon")
                                    || cardSource.ContainsCardName("Garurumon")
                                    || cardSource.EqualsTraits("ME")
                                    || cardSource.EqualsTraits("VB"));
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            elements: new List<AssemblyConditionElement>() { element1, element2, element3 },
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
