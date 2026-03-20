using System;
using System.Collections;
using System.Collections.Generic;

// Gaogamon
namespace DCGO.CardEffects.ST24
{
    public class ST24_03 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("DATA SQUAD");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 3, permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "May bounce enemy lvl 3, may place top deck face down under [DATA SQUAD] tamer";

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: false,
                    onPlay: true,
                    whenDigivolving: true);

            string SharedEffectDescription(string tag) => $"[{tag}] You may return 1 of your opponent's level 3 Digimon to the hand. Then, you may place the top card of your deck face down under any of your [DATA SQUAD] trait Tamers.";

            bool CanSelectOpponentLevel3Digimon(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.HasLevel
                    && permanent.TopCard.Level == 3;
            }

            bool CanSelectOwnDataSquadTamer(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.TopCard.EqualsTraits("DATA SQUAD");
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentLevel3Digimon));
                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectOpponentLevel3Digimon,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Bounce,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to bounce.", "The opponent is selecting a digimon to bounce.");
                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                if (card.Owner.LibraryCards.Count > 1 && CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnDataSquadTamer))
                {
                    SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect1.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOwnDataSquadTamer,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect1.SetUpCustomMessage("Select 1 [DATA SQUAD] trait Tamer to place the top card of your deck face down under.", "The opponent is selecting 1 [DATA SQUAD] trait Tamer to place the top card of your deck face down under.");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanent)
                    {
                        if (permanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(permanent[0].AddDigivolutionCardsBottom(
                                                        new List<CardSource> { card.Owner.LibraryCards[0] }, activateClass, isFacedown: true));
                        }
                    }
                }
            }
            #endregion

            #region Inherited
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("If you have 7 or fewer cards in hand, <Draw 1>.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("ST24_03_Inherited");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                 => "[When Attacking] [Once Per Turn] If your hand has 7 or fewer cards, <Draw 1>. (Draw 1 card from your deck.)";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                        CardEffectCommons.IsOwnerTurn(card) &&
                        CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                        card.Owner.HandCards.Count <= 7;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
