using System.Collections;
using System.Collections.Generic;

// Dantemon
namespace DCGO.CardEffects.BT26
{
    public class BT26_086 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Rush
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RushSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Link +6
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfLinkMaxStaticEffect(changeValue: 6, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName() => "Link up to 7 [Appmon] cards with different names from sources, then may attack without suspending";

            string SharedEffectDescription(string tag)
                => $"[{tag}] You may link up to 7 [Appmon] trait cards with different names from this Digimon's digivolution cards to this Digimon without paying the costs. Then, this Digimon may attack without suspending.";

            bool SharedCanActivateCondition(Hashtable hashtable, ICardEffect activateClass)
                => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

            bool CanSelectSourceCondition(CardSource cardSource)
                => cardSource.ContainsTraits("Appmon");

            bool CanTargetCondition_ByPreSelecetedList(List<CardSource> selectedCards, CardSource cardSource)
                => !selectedCards.Exists(selected => cardSource.EqualsCardName(selected.CardNames[0]));

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                List<CardSource> availableSources = card.PermanentOfThisCard().DigivolutionCards.Filter(CanSelectSourceCondition);

                if (availableSources.Count >= 1)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectSourceCondition,
                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select up to 7 [Appmon] trait cards with different names to link to this Digimon.",
                        maxCount: 7,
                        canEndNotMax: true,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.DigivolutionCards,
                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    foreach (CardSource selectedCard in selectedCards)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddLinkCard(selectedCard, activateClass));
                    }
                }

                if (CardEffectCommons.IsExistOnBattleArea(card) && card.PermanentOfThisCard().CanAttack(activateClass, withoutTap: true))
                {
                    SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                    selectAttackEffect.SetUp(
                        attacker: card.PermanentOfThisCard(),
                        canAttackPlayerCondition: () => true,
                        defenderCondition: (permanent) => true,
                        cardEffect: activateClass);

                    selectAttackEffect.SetWithoutTap();

                    yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                }
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass((hash) => SharedCanActivateCondition(hash, activateClass), (hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }
            #endregion

            #region All Turns - When Linked
            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May delete 1 opponent Digimon, then if 7 link cards bounce opponent top security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT26_086_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When this Digimon gets linked, you may delete 1 of your opponent's Digimon. Then, if this Digimon has 7 link cards, return your opponent's top security card to the bottom of the deck.";

                bool ThisPermanentCondition(Permanent permanent) => permanent == card.PermanentOfThisCard();

                bool CanSelectDeleteTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenLinked(hashtable, ThisPermanentCondition, null);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteTargetCondition))
                    {
                        SelectPermanentEffect selectDeleteEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectDeleteEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDeleteTargetCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectDeleteEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectDeleteEffect.Activate());
                    }

                    if (CardEffectCommons.IsExistOnBattleArea(card) && card.PermanentOfThisCard().LinkedCards.Count >= 7 && card.Owner.Enemy.SecurityCards.Count >= 1)
                    {
                        List<CardSource> topSecurity = new List<CardSource>() { card.Owner.Enemy.SecurityCards[0] };
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(topSecurity));
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

                bool CanUseCondition(Hashtable hashtable) => true;

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition);

                        bool CanSelectCardCondition(CardSource cs)
                            => cs.IsDigimon && cs.ContainsTraits("Seven Code");

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                            selectMessage: "7 [Seven Code] trait Digimon cards with different names",
                            elementCount: 7,
                            reduceCost: 7);

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
