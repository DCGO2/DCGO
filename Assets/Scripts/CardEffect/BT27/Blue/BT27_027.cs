using System.Collections;
using System.Collections.Generic;

// Tenkomon
namespace DCGO.CardEffects.BT27
{
    public class BT27_027 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Trait Rule
            if (timing == EffectTiming.None)
            {
                ChangeTraitsClass changeTraitsClass = new ChangeTraitsClass();
                changeTraitsClass.SetUpICardEffect("[Rule] Trait: Has [Ice-Snow] Type.", _ => true, card);
                changeTraitsClass.SetUpChangeTraitsClass(changeeTraits: ChangeTraits);
                cardEffects.Add(changeTraitsClass);

                List<string> ChangeTraits(CardSource cardSource, List<string> cardTraits)
                {
                    if (cardSource == card && !cardTraits.Contains("Ice-Snow"))
                    {
                        cardTraits.Add("Ice-Snow");
                    }

                    return cardTraits;
                }
            }
            #endregion

            #region Iceclad
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.IcecladSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName = "Trash the bottom 2 sources of 1 opponent's Digimon, then may place 1 Lv.4 or lower [Ice-Snow] Digimon from hand as a bottom source";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Trash the bottom 2 digivolution cards of 1 of your opponent's Digimon. Then, you may place 1 level 4 or lower [Ice-Snow] trait Digimon card from your hand as any of your Digimon's bottom digivolution card.";

            bool OpponentDigimonWithSourcesCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.DigivolutionCards.Count >= 1;

            bool OwnDigimonCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

            bool IceSnowCardCondition(CardSource cardSource)
                => cardSource.IsDigimon
                    && cardSource.HasLevel
                    && cardSource.Level <= 4
                    && cardSource.EqualsTraits("Ice-Snow");

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                #region Trash the bottom 2 digivolution cards of 1 of the opponent's Digimon
                if (CardEffectCommons.HasMatchConditionPermanent(OpponentDigimonWithSourcesCondition))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: OpponentDigimonWithSourcesCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to trash its bottom 2 digivolution cards.", "The opponent is selecting 1 Digimon to trash its bottom 2 digivolution cards.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                            targetPermanent: permanent, trashCount: 2, isFromTop: false, activateClass: activateClass));
                    }
                }
                #endregion

                #region May place 1 Lv.4 or lower [Ice-Snow] Digimon from hand as a bottom source
                if (!CardEffectCommons.HasMatchConditionOwnersHand(card, IceSnowCardCondition)
                    || !CardEffectCommons.HasMatchConditionPermanent(OwnDigimonCondition)) yield break;

                CardSource selectedCard = null;

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: IceSnowCardCondition,
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

                selectHandEffect.SetUpCustomMessage("Select 1 level 4 or lower [Ice-Snow] trait Digimon card to place as a bottom digivolution card.", "The opponent is selecting 1 card to place as a bottom digivolution card.");
                selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");

                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                IEnumerator SelectCardCoroutine(CardSource cardSource)
                {
                    selectedCard = cardSource;
                    yield return null;
                }

                if (selectedCard == null) yield break;

                Permanent selectedPermanent = null;

                SelectPermanentEffect selectOwnPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectOwnPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: OwnDigimonCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectOwnPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectOwnPermanentEffect.SetUpCustomMessage("Select 1 of your Digimon to place the card under.", "The opponent is selecting 1 Digimon to place the card under.");

                yield return ContinuousController.instance.StartCoroutine(selectOwnPermanentEffect.Activate());

                IEnumerator SelectOwnPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;
                    yield return null;
                }

                if (selectedPermanent != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(
                        new List<CardSource>() { selectedCard }, activateClass));
                }
                #endregion
            }

            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                onPlay: true,
                whenDigivolving: true);
            #endregion

            #region End of Attack - OPT - Inherited
            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("BT27_027_Inherited");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[End of Attack] [Once Per Turn] Gain 1 memory.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnEndAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && card.Owner.CanAddMemory(activateClass);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
