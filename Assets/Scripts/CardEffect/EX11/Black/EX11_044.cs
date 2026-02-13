using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Pyramidimon
namespace DCGO.CardEffects.EX11
{
    public class EX11_044 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Reboot

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Fragment

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                string EffectDiscription()
                {
                    return "<Fragment <3>> (When this Digimon would be deleted, by trashing any 3 of its digivolution cards, it isn’t deleted.)";
                }

                cardEffects.Add(CardEffectFactory.FragmentSelfEffect(isInheritedEffect: false, card: card, condition: null, trashValue: 3, effectName: "Fragment <3>", effectDiscription: EffectDiscription()));
            }

            #endregion

            #region Shared OP/WD/WA

            string SharedEffectName()
            {
                return "By trashing 3 [Mineral]/[Rock] cards from Digimon digivolution cards delete opponent's highest play cost Digimon/Tamer.";
            }

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] [Once Per Turn] By trashing any 3 [Mineral] or [Rock] trait cards from your Digimon's digivolution cards, delete 1 of your opponent's highest play cost Digimon or Tamers.";
            }

            string SharedHashString = "EX11_044_OP_WD_WA";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card)
                    && HasProperAmountOfSources();
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionCards.Count(HasProperTrait) >= 1;
            }

            bool CanSelectPermanentCondition1(Permanent permanent)
            {
                return CardEffectCommons.IsMaxCost(permanent, card.Owner.Enemy, false);
            }

            bool HasProperTrait(CardSource cardSource)
            {
                return cardSource.EqualsTraits("Mineral")
                        || cardSource.EqualsTraits("Rock");
            }

            bool HasProperAmountOfSources()
            {
                int totalSourceCount = 0;

                foreach (Permanent permanent in card.Owner.GetBattleAreaDigimons())
                {
                    totalSourceCount += permanent.DigivolutionCards.Count(HasProperTrait);
                }

                return totalSourceCount >= 3;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                int trashedCount = 0;

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                    permanentCondition: CanSelectPermanentCondition,
                    cardCondition: HasProperTrait,
                    maxCount: 3,
                    canNoTrash: false,
                    isFromOnly1Permanent: false,
                    activateClass: activateClass,
                    afterSelectionCoroutine: AfterTrashedCards
                ));

                IEnumerator AfterTrashedCards(Permanent permanent, List<CardSource> cards)
                {
                    trashedCount = cards.Count;

                    yield return null;
                }

                if (trashedCount == 3)
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition1,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
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
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, (hash) => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("On Play"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, (hash) => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("When Digivolving"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card)
                        && CardEffectCommons.IsExistOnBattleArea(card);
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), 1, true, SharedEffectDescription("When Attacking"));
                activateClass.SetHashString(SharedHashString);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleArea(card);
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnDigivolutionCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May place 3 [Mineral]/[Rock] trait cards from trash under this Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("EX10_044_AT");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When effects trash any of this Digimon's digivolution cards, you may place 3 [Mineral] or [Rock] trait cards from your trash as this Digimon's bottom digivolution cards.";
                }              

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnTrashDigivolutionCard(hashtable, perm => perm == card.PermanentOfThisCard(), cardEffect => cardEffect != null, source => source != null);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, HasProperTrait) >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    int maxCount = Math.Min(3, CardEffectCommons.MatchConditionOwnersCardCountInTrash(card, HasProperTrait));

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: HasProperTrait,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => false,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select [Mineral] or [Rock] to place on bottom of digivolution cards\n(cards will be placed so that cards with lower numbers are on top).",
                        maxCount: maxCount,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");
                    selectCardEffect.SetUpCustomMessage("Select [Mineral] or [Rock] to place on bottom of digivolution cards.", "The opponent is selecting [Mineral] or [Rock] to place on bottom of digivolution cards.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);

                        yield return null;
                    }

                    if (selectedCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
