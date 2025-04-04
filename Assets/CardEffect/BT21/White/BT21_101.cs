using System;
using System.Collections;
using System.Collections.Generic;

// Gaiamon
namespace DCGO.CardEffects.BT21
{
    public class BT21_101 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region App Fusion (Globemon & Charismon)

            if (timing == EffectTiming.None)
            {
                AddAppFusionConditionClass addAppFusionConditionClass = new AddAppFusionConditionClass();
                addAppFusionConditionClass.SetUpICardEffect($"App Fusion", (hashtable) => true, card);
                addAppFusionConditionClass.SetUpAddAppFusionConditionClass(getAppFusionCondition: GetAppFusion);
                addAppFusionConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAppFusionConditionClass);

                AppFusionCondition GetAppFusion(CardSource cardSource)
                {
                    string selectLinkMessage = "";
                    string selectDigimonMessage = "";
                    bool linkCondition(CardSource source)
                    {
                        if (source != null)
                        {
                            if (source.PermanentOfThisCard().TopCard.EqualsCardName("Globemon"))
                            {
                                if (source.PermanentOfThisCard().LinkedCards.Find(x => x.EqualsCardName("Charismon")))
                                {
                                    selectLinkMessage = "1 [Charismon]";
                                    return true;
                                }
                            }
                            if (source.PermanentOfThisCard().TopCard.EqualsCardName("Charismon"))
                            {
                                if (source.PermanentOfThisCard().LinkedCards.Find(x => x.EqualsCardName("Globemon")))
                                {
                                    selectLinkMessage = "1 [Globemon]";
                                    return true;
                                }
                            }
                            return false;
                        }

                        return false;
                    }
                    bool digimonCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        {
                            if (permanent.TopCard.EqualsCardName("Globemon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Charismon")))
                                {
                                    selectDigimonMessage = "1 [Charismon]";
                                    return true;
                                }
                            }
                            if (permanent.TopCard.EqualsCardName("Charismon"))
                            {
                                if (permanent.LinkedCards.Find(x => x.EqualsCardName("Globemon")))
                                {
                                    selectDigimonMessage = "1 [Globemon]";
                                    return true;
                                }
                            }
                            return false;
                        }

                        return false;
                    }

                    if (cardSource == card)
                    {
                        AppFusionCondition AppFusionCondition = new AppFusionCondition(
                            linkedCondition: linkCondition,
                            selectLinkMessage: selectLinkMessage,
                            digimonCondition: digimonCondition,
                            selectDigimonMessage: selectDigimonMessage,
                            cost: 0);

                        return AppFusionCondition;
                    }

                    return null;
                }
            }

            #endregion

            #region Blocker

            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));

            #endregion

            #region Link +1

            if (timing == EffectTiming.None) cardEffects.Add(CardEffectFactory.ChangeSelfLinkMaxStaticEffect(card.PermanentOfThisCard().LinkedMax + 1, false, card, null));

            #endregion

            #endregion

            #region YourTurn

            if (timing == EffectTiming.WhenLinked)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend, trash top sec", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT21_101_WhenLinked");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Your Turn] [Once Per Turn] When your Digimon get linked, by unsuspending this Digimon, trash your opponent's top security card.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && CardEffectCommons.IsOwnerTurn(card)
                    && CardEffectCommons.CanTriggerWhenLinked(hashtable, PermanentCondition, cardSource => true)
                    && card.PermanentOfThisCard().IsSuspended
                    && CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard());

                bool PermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool unsuspended = false;
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    selectPermanentEffect.SetUp
                        (
                        card.Owner,
                        permanent => CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent) && permanent == card.PermanentOfThisCard(),
                        null,
                        null,
                        1,
                        true,
                        false,
                        SelectPermanentCoroutine,
                        null,
                        SelectPermanentEffect.Mode.UnTap,
                        activateClass
                        );
                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        if (permanent.IsSuspended) unsuspended = true;
                        yield return null;
                    }
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    if (unsuspended)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner.Enemy,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                    }
                }
            }

            #endregion

            #region WD/WA Shared

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                {
                    return true;
                }
                return false;
            }

            bool CanSelectLinkCard(CardSource cardSource)
            {
                if (cardSource.HasAppmonTraits)
                {
                    return true;
                }
                return false;
            }

            bool CanSelectDigimon(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    return true;
                }
                return false;
            }
            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectDigimon))
                {
                    Permanent selectedPermanent = null;
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectDigimon));
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 digimon to add link", "The opponent is selecting 1 digimon to add link");
                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From This Digimon", value: true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From Hand", value: false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Choose where to get the digimon from";
                        string notSelectPlayerMessage = "The opponent is choosing effects.";

                        GManager.instance.userSelectionManager.SetBoolSelection(
                            selectionElements: selectionElements, selectPlayer: card.Owner,
                            selectPlayerMessage: selectPlayerMessage,
                            notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance
                            .userSelectionManager.WaitForEndSelect());

                        CardSource selectedCard = null;
                        if (GManager.instance.userSelectionManager.SelectedBoolValue)
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectLinkCard,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => true,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message: "Select 1 linked card.",
                                        maxCount: 1,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Custom,
                                        customRootCardList: card.PermanentOfThisCard().LinkedCards,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 linked card to remove", "The opponent is selecting 1 linked card to remove");

                            yield return StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCard = cardSource;
                                card.PermanentOfThisCard().LinkedCards.Remove(cardSource);
                                yield return null;
                            }
                        }
                        else
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                            selectHandEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectLinkCard,
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
                            selectHandEffect.SetUpCustomMessage("Select 1 linked card to remove", "The opponent is selecting 1 linked card to remove");
                            yield return StartCoroutine(selectHandEffect.Activate());
                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCard = cardSource;
                                yield return null;
                            }
                        }
                        if (selectedCard != null)
                        {
                            // Might need to talk to mike about this, things possibly to add
                            // if link condiiton for selectedCard is sucessful, ex: if it needs exact name or traits, does it meet them.
                            // Perphaps may lead to link requirements being added to the card itself, instead of an effect.
                            // if this method below (AddLinkCard), automatically handles if the link max is hit, and allows for owner to choose which link to discarded for new one.
                            selectedPermanent.AddLinkCard(selectedCard, activateClass);
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add new link to a digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, (hashtable) => SharedActivateCoroutine(hashtable, activateClass), -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[When Digivolving] You may link 1 Digimon card with the [Appmon] trait from your hand or this Digimon's digivolution cards to 1 of your Digimon without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add new link to a digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, (hashtable) => SharedActivateCoroutine(hashtable, activateClass), -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[When Attacking] You may link 1 Digimon card with the [Appmon] trait from your hand or this Digimon's digivolution cards to 1 of your Digimon without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnAttack(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}