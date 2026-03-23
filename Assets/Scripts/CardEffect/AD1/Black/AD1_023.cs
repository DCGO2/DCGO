
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// J.P., Koji, & Koichi
namespace DCGO.CardEffects.AD1
{
    public class AD1_023 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Name Rule
            if (timing == EffectTiming.None)
            {
                ChangeCardNamesClass changeCardNamesClass = new ChangeCardNamesClass();
                changeCardNamesClass.SetUpICardEffect("[Rule] Name: Also treated as [J.P. Shibayama]/[Koji Minamoto]/[Koichi Kimura].", _ => true, card);
                changeCardNamesClass.SetUpChangeCardNamesClass(changeCardNames: ChangeCardNames);
                cardEffects.Add(changeCardNamesClass);

                List<string> ChangeCardNames(CardSource cardSource, List<string> cardNames)
                {
                    if (cardSource == card)
                    {
                        cardNames.Add("J.P. Shibayama");
                        cardNames.Add("Koji Minamoto");
                        cardNames.Add("Koichi Kimura");
                    }

                    return cardNames;
                }
            }
            #endregion

            #region Shared OP/SOMP
            string SharedEffectName = "Place up to 2 [Hybrid] cards with different colors under, if so <Draw 1> and if 4 or more under gain 2 memory";

            string SharedEffectDescription(string tag) =>
                $"[{tag}] You may place up to 2 [Hybrid] trait cards with different colors from your hand or trash under this Tamer. If this effect placed, <Draw 1>. Then, if there are 4 or more [Hybrid] trait cards under this Tamer, gain 2 memory.";

            bool IsHybridCard(CardSource cardSource)
            {
                return cardSource.ContainsTraits("Hybrid")
                    && cardSource.Owner == card.Owner;
            }

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                List<CardSource> selectedCards = new List<CardSource>();
                HashSet<CardColor> selectedColors = new HashSet<CardColor>();

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCards.Add(cardSource);
                    foreach (CardColor color in cardSource.CardColors)
                    {
                        selectedColors.Add(color);
                    }
                    yield return null;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (!IsHybridCard(cardSource)) return false;
                    if (selectedCards.Contains(cardSource)) return false;
                    if (selectedCards.Count > 0)
                    {
                        return cardSource.CardColors.Any(color1 => selectedCards[0].CardColors.Any(color2 => color2 != color1));
                    }
                    return true;
                }

                while (selectedCards.Count < 2)
                {
                    List<CardSource> validHandCards = card.Owner.HandCards.Filter(CanSelectCardCondition).ToList();
                    List<CardSource> validTrashCards = card.Owner.TrashCards.Filter(CanSelectCardCondition).ToList();

                    if (validHandCards.Count == 0 && validTrashCards.Count == 0)
                    {
                        goto END_LOOP;
                    }

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    if (validHandCards.Count > 0)
                    {
                        selectionElements.Add(new(message: "from Hand", value: 1, spriteIndex: 0));
                    }
                    if (validTrashCards.Count > 0)
                    {
                        selectionElements.Add(new(message: "from Trash", value: 2, spriteIndex: 0));
                    }
                    selectionElements.Add(new(message: "Do not place", value: 3, spriteIndex: 1));

                    GManager.instance.userSelectionManager.SetIntSelection(
                        selectionElements: selectionElements,
                        selectPlayer: card.Owner,
                        selectPlayerMessage: "From which area will you select a [Hybrid] card to place under this Tamer?",
                        notSelectPlayerMessage: "The opponent is choosing from which area to select a [Hybrid] card.");

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    switch (GManager.instance.userSelectionManager.SelectedIntValue)
                    {
                        case 1:
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: cardSource => validHandCards.Contains(cardSource),
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

                            selectHandEffect.SetUpCustomMessage(
                                "Select 1 [Hybrid] card to place under this Tamer.",
                                "The opponent is selecting a [Hybrid] card to place under this Tamer.");

                            yield return StartCoroutine(selectHandEffect.Activate());
                            break;
                        }
                        case 2:
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: cardSource => validTrashCards.Contains(cardSource),
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 [Hybrid] card",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage(
                                "Select 1 [Hybrid] card to place under this Tamer.",
                                "The opponent is selecting a [Hybrid] card to place under this Tamer.");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            break;
                        }
                        default:
                            goto END_LOOP;
                    }
                }

                END_LOOP:;

                if (selectedCards.Count > 0)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));

                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(
                        card.Owner,
                        1,
                        activateClass).Draw());
                }

                int hybridCountUnderTamer = card.PermanentOfThisCard().DigivolutionCards.Count(IsHybridCard);
                if (hybridCountUnderTamer >= 4)
                {
                    if (card.Owner.CanAddMemory(activateClass))
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(2, activateClass));
                    }
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

            #region Inherited All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add top security to hand to not leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("AD1_023_ESS_AT");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon with the [Hybrid] or [Ten Warriors] trait would leave the battle area, by adding your top security card to the hand, it doesn't leave.";
                }
              
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && card.Owner.SecurityCards.Count >= 1
                        && card.PermanentOfThisCard().TopCard.HasHybridTenWarriorsTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { card.Owner.SecurityCards[0] }, false, activateClass));

                    card.PermanentOfThisCard().willBeRemoveField = false;

                    card.PermanentOfThisCard().HideDeleteEffect();
                    card.PermanentOfThisCard().HideHandBounceEffect();
                    card.PermanentOfThisCard().HideDeckBounceEffect();
                    card.PermanentOfThisCard().HideWillRemoveFieldEffect();
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
