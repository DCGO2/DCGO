using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Sunarizamon
namespace DCGO.CardEffects.EX11
{
    public class EX11_038 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared WM/OP

            string SharedEffectName() => "By trashing 1 card, <Draw 1>.";

            string SharedEffectDescription(string tag) => $"[{tag}] By trashing 1 [Mineral] or [Rock] trait card from your hand or your Digimon's digivolution cards, <Draw 1>.";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && (CardEffectCommons.HasMatchConditionOwnersHand(card, HasMineralOrRock)
                        || CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectTrashTargetCondition));
            }

            bool HasMineralOrRock(CardSource source)
            {
                return source.EqualsTraits("Mineral")
                        || source.EqualsTraits("Rock");
            }

            bool CanSelectTrashTargetCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionCards.Count(HasMineralOrRock) >= 1;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Setup Location Selection

                bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, HasMineralOrRock);
                bool canSelectDigivolutionSource = CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectTrashTargetCondition);
                List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();

                if (canSelectHand)
                {
                    selectionElements.Add(new(message: "From hand", value: 0, spriteIndex: 0));
                }
                if (canSelectDigivolutionSource)
                {
                    selectionElements.Add(new(message: "From digivolution cards", value: 1, spriteIndex: 0));
                }
                selectionElements.Add(new(message: "Do not trash", value: 2, spriteIndex: 1));

                string selectPlayerMessage = "From which area will you trash a card?";
                string notSelectPlayerMessage = "The opponent is choosing if they will activate their effect.";

                GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements,
                    selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                    notSelectPlayerMessage: notSelectPlayerMessage);

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                    .WaitForEndSelect());

                int selection = GManager.instance.userSelectionManager.SelectedIntValue;

                #endregion
                if (selection != 2)
                {
                    bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                    bool discarded = false;

                    if (selection == 0)
                    {
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: HasMineralOrRock,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            mode: SelectHandEffect.Mode.Discard,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }
                    }
                    else
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                            permanentCondition: CanSelectTrashTargetCondition,
                            cardCondition: HasMineralOrRock,
                            maxCount: 1,
                            canNoTrash: true,
                            isFromOnly1Permanent: false,
                            activateClass: activateClass,
                            afterSelectionCoroutine: AfterTrashedCards
                        ));

                        IEnumerator AfterTrashedCards(Permanent permanent, List<CardSource> cards)
                        {
                            if (cards.Count >= 1)
                            {
                                discarded = true;

                                yield return null;
                            }
                        }
                    }

                    if (discarded)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    }
                }
            }

            #endregion

            #region When Moving

            if (timing == EffectTiming.OnMove)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, (hashTable) => SharedActivateCoroutine(hashTable, activateClass), -1, false, SharedEffectDescription("When Moving"));
                cardEffects.Add(activateClass);

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnMove(hashtable, PermanentCondition);
                }
            }
            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, (hashTable) => SharedActivateCoroutine(hashTable, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }

            #endregion

            #region Inherited

            if (timing == EffectTiming.OnDigivolutionCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<Draw 1>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "When effects trash this card from a [Mineral] or [Rock] trait Digimon's digivolution cards, <Draw 1>.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    Permanent trashedPermanent = CardEffectCommons.GetPermanentFromHashtable(hashtable);
                    return (trashedPermanent.TopCard.EqualsTraits("Mineral") || trashedPermanent.TopCard.EqualsTraits("Rock")) &&
                           CardEffectCommons.CanTriggerOnTrashSelfDigivolutionCard(hashtable, cardEffect => cardEffect != null, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnTrash(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
