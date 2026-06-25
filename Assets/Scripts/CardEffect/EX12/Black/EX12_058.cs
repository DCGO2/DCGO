using System.Collections;
using System.Collections.Generic;

// HiAndromon
namespace DCGO.CardEffects.EX12
{
    public class EX12_058 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("ME");
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
                                && (permanent.TopCard.CardColors.Contains(CardColor.Black)
                                    || permanent.TopCard.CardColors.Contains(CardColor.Purple))
                                && permanent.Levels_ForJogress(card).Contains(5);
                        }

                        bool PermanentCondition2(Permanent permanent)
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                                && (permanent.TopCard.CardColors.Contains(CardColor.Red)
                                    || permanent.TopCard.CardColors.Contains(CardColor.Yellow))
                                && permanent.Levels_ForJogress(card).Contains(5);
                        }

                        JogressConditionElement[] elements = new JogressConditionElement[]
                        {
                        new JogressConditionElement(PermanentCondition1, "a level 5 Black or Purple Digimon"),

                        new JogressConditionElement(PermanentCondition2, "a level 5 Red or Yellow Digimon"),
                        };

                        JogressCondition jogressCondition = new JogressCondition(elements, 0);

                        return jogressCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region Shared OP/WD/WA
            string SharedEffectName = "Reveal 3, may play 1 7 cost or lower [Machine]/[Cyborg]/[ME] among them for free, trash the rest";

            string SharedEffectHash = "EX12_058_OP_WD_WA";

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                maxCountPerTurn: 1,
                hashValue: SharedEffectHash,
                onPlay: true,
                whenDigivolving: true,
                whenAttacking: true);

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] [Once Per Turn] Reveal the top 3 cards of your deck. You may play 1 play cost 7 or lower [Machine], [Cyborg] or [ME] trait card among them without paying the cost. Trash the rest.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.HasPlayCost
                    && cardSource.GetCostItself <= 7
                    && (cardSource.EqualsTraits("Machine")
                        || cardSource.EqualsTraits("Cyborg")
                        || cardSource.EqualsTraits("ME"));
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                CardSource selectedCard = null;

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 3,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "May select 1 7 cost or less [Machine]/[Cyborg]/[ME] card to play for free.",
                            mode: SelectCardEffect.Mode.Custom,
                            maxCount: 1,
                            selectCardCoroutine: SelectCardCoroutine),
                    },
                    remainingCardsPlace: RemainingCardsPlace.Trash,
                    activateClass: activateClass,
                    canNoSelect: true
                ));

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCard = cardSource;

                    if (selectedCard != null)
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(selectedCard, false, activateClass, SelectCardEffect.Root.Library))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: new List<CardSource> { selectedCard },
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Library,
                                activateETB: true));
                        }
                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(new ITrashDeckCards(new List<CardSource> { selectedCard }, activateClass).TrashDeckCards());
                        }
                    }
                }          
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("ME");
                }

                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                cardEffects.Add(CardEffectFactory.AllianceStaticEffect(PermanentCondition, false, card, Condition));
                cardEffects.Add(CardEffectFactory.RebootStaticEffect(PermanentCondition, false, card, Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}
