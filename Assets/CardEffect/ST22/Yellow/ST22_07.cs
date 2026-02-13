using System.Collections;
using System.Collections.Generic;
using System.Linq;

//ST22_07 Rika Nonaka
namespace DCGO.CardEffects.ST22
{
    public class ST22_07 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region SoYMP/OP Shared

            string EffectDiscriptionShared(string tag)
            {
                return $"[{tag}] By placing 1 Option card with the [Onmyōjutsu] or [Plug-In] trait from your hand under this Tamer, ＜Draw 1＞ and gain 1 memory.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.IsOption &&
                       (cardSource.EqualsTraits("Onmyōjutsu") || cardSource.EqualsTraits("Plug-In"));
            }

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.HandCards.Some(CanSelectCardCondition))
                    {
                        return true;
                    }
                }
                return false;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool placed = false;

                int maxCount = 1;

                List<CardSource> selectedCards = new List<CardSource>();

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectCardCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: CanEndSelectCondition,
                    maxCount: maxCount,
                    canNoSelect: true,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: null,
                    mode: SelectHandEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectHandEffect.SetUpCustomMessage("Select 1 card to place under Tamer.", "The opponent is selecting 1 card to place under Tamer.");
                selectHandEffect.SetNotShowCard();

                yield return StartCoroutine(selectHandEffect.Activate());

                bool CanEndSelectCondition(List<CardSource> cardSources)
                {
                    if (CardEffectCommons.HasNoElement(cardSources))
                    {
                        return false;
                    }

                    return true;
                }

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCards.Add(cardSource);

                    yield return null;
                }

                if (selectedCards.Count >= 1)
                {
                    if (!card.PermanentOfThisCard().IsToken)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));

                        placed = true;
                    }
                }

                if (placed)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            #endregion

            #region Start of main

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 Option under this Tamer from hand to Draw 1 and gain 1 mem", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, hash => SharedActivateCoroutine(hash, activateClass), -1, true, EffectDiscriptionShared("Start of Your Main Phase"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }
                    return false;
                }                
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 Option under this Tamer from hand to Draw 1 and gain 1 mem", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, hash => SharedActivateCoroutine(hash, activateClass), -1, true, EffectDiscriptionShared("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPlay(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Use 1 [Onmyōjutsu] or [Plug-In] trait Option card.", CanUseCondition,
                    card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Your Turn] When one of your Digimon with [Renamon], [Kyubimon], [Taomon] or [Sakuyamon] in its name attacks, by suspending this Tamer, you may use 1 [Onmyōjutsu] or [Plug-In] trait Option card with as high or lower a use cost as that Digimon's level from under this Tamer without paying the cost.";
                }

                bool AttackingPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                           (permanent.TopCard.ContainsCardName("Renamon") ||
                           permanent.TopCard.ContainsCardName("Kyubimon") ||
                           permanent.TopCard.ContainsCardName("Taomon") ||
                           permanent.TopCard.ContainsCardName("Sakuyamon"));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.IsOwnerTurn(card) &&
                           CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, AttackingPermanent);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                           CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool SelectSourceCard(CardSource source)
                {
                    return source.IsOption
                        && GManager.instance.attackProcess.AttackingPermanent.Level >= source.GetCostItself
                        && source.HasOnmyoOrPluginTraits
                        && !source.CanNotPlayThisOption;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() },
                            CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    List<CardSource> selectedCards = new List<CardSource>();
                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCards.Add(cardSource);
                        yield return null;
                    }

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: SelectSourceCard,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 option card to use",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.DigivolutionCards,
                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 option card to use.", "The opponent is selecting 1 option card to use.");
                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    if (selectedCards.Count > 0) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                        cardSources: selectedCards,
                        activateClass: activateClass,
                        payCost: false,
                        root: SelectCardEffect.Root.DigivolutionCards));
                }
            }

            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}
