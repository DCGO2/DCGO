using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Tamer's effect to set Memory to 3

    public static ICardEffect SetMemoryTo3TamerEffect(CardSource card)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Set Memory to 3", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());

        string EffectDiscription()
        {
            return "[Start of Your Turn] If you have 2 or less memory, set your memory to 3.";
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass))
            {
                if (CardEffectCommons.IsOwnerTurn(card))
                {
                    return true;
                }
            }

            return false;
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass))
            {
                if (card.Owner.MemoryForPlayer <= 2)
                {
                    if (card.Owner.CanAddMemory(activateClass))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            yield return ContinuousController.instance.StartCoroutine(card.Owner.SetFixedMemory(3, activateClass));
        }

        return activateClass;
    }

    #endregion

    #region Tamer's effect to Gain 1 Memory if opponent has a digimon

    public static ICardEffect Gain1MemoryTamerOpponentDigimonEffect(CardSource card)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());

        string EffectDescription()
        {
            return "[Start of Your Main Phase] If your opponent has a Digimon, gain 1 memory.";
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                && CardEffectCommons.IsOwnerTurn(card);
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                && card.Owner.Enemy.GetBattleAreaDigimons().Count >= 1;
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            if (card.Owner.CanAddMemory(activateClass))
                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
        }

        return activateClass;
    }

    #endregion

    #region Tamer's effect to Gain 1 Memory if owner has condition digimon

    public static ICardEffect Gain1MemoryTamerOwnerDigimonConditionalEffect(string effectDescription, Func<Permanent, bool> permamentCondition, Func<bool> condition, CardSource card)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());

        string EffectDiscription() => effectDescription;

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass);
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                && card.Owner.CanAddMemory(activateClass)
                && condition()
                && card.Owner.GetBattleAreaDigimons().Any(permamentCondition);
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
        }

        return activateClass;
    }

    #endregion

    #region Tamer's Security effect to play oneself

    public static ICardEffect PlaySelfTamerSecurityEffect(CardSource card)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Play this card", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
        activateClass.SetIsSecurityEffect(true);

        string EffectDiscription()
        {
            return "[Security] Play this card without paying its memory cost.";
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            if (card.Owner.ExecutingCards.Contains(card))
            {
                if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: card, payCost: false, cardEffect: activateClass))
                {
                    return true;
                }
            }

            return false;
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            yield return ContinuousController.instance.StartCoroutine(
                CardEffectCommons.PlayPermanentCards(
                    cardSources: new List<CardSource>() { card },
                    activateClass: activateClass,
                    payCost: false,
                    isTapped: false,
                    root: SelectCardEffect.Root.Execution,
                    activateETB: true));
        }

        return activateClass;
    }

    #endregion

    #region Mind Link Tamer's effect to play themself from digivolution cards
    public static ICardEffect PlayMindLinkTamerFromDigivolutionCards(CardSource card, string cardName, string effectDescription)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect($"Play 1 [{cardName}] from this Digimon's digivolution cards", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, effectDescription);
        activateClass.SetIsInheritedEffect(true);

        bool CanSelectCardCondition(CardSource cardSource)
        {
            return cardSource.EqualsCardName(cardName)
                && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsExistOnBattleArea(card);
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsExistOnBattleArea(card)
                && card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1;
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            if (CardEffectCommons.IsExistOnBattleArea(card))
            {
                Permanent selectedPermanent = card.PermanentOfThisCard();

                if (selectedPermanent != null)
                {
                    if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                    {
                        int maxCount = 1;

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 digivolution card to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: selectedPermanent.DigivolutionCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to play.",
                            "The opponent is selecting 1 digivolution card to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.DigivolutionCards,
                            activateETB: true));
                    }
                }
            }
        }

        return activateClass;
    }   
    #endregion

    #region Digimon's Security effect to play oneself after battle

    public static ICardEffect PlaySelfDigimonAfterBattleSecurityEffect(CardSource card, EffectDuration deleteDigimon = EffectDuration.UntilEndBattle)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Play this card at the end of the battle", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
        activateClass.SetIsSecurityEffect(true);

        string EffectDiscription()
        {
            return "[Security] At the end of the battle, play this card without paying its memory cost.";
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            return CardEffectCommons.IsExistOnExecutingArea(card);
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            yield return null;

            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

            #region Play Card
            ActivateClass activateClass1 = new ActivateClass();
            activateClass1.SetUpICardEffect("Play this card", CanUseCondition1, card);
            activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
            card.Owner.UntilEndBattleEffects.Add(GetCardEffect1);

            string EffectDiscription1()
            {
                return "Play this card without paying its memory cost.";
            }

            bool CanUseCondition1(Hashtable hashtable)
            {
                return true;
            }

            bool CanActivateCondition1(Hashtable hashtable)
            {
                if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: card, payCost: false, cardEffect: activateClass1, root: SelectCardEffect.Root.Security))
                {
                    if (!card.Owner.LibraryCards.Contains(card) && !card.Owner.SecurityCards.Contains(card))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
            {
                if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: card, payCost: false, cardEffect: activateClass1, root: SelectCardEffect.Root.Security))
                {
                    if (!card.Owner.LibraryCards.Contains(card) && !card.Owner.SecurityCards.Contains(card))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource>() { card },
                            activateClass: activateClass1,
                            payCost: false,
                            isTapped: false,
                            root: SelectCardEffect.Root.Security,
                            activateETB: true));

                        if (deleteDigimon != EffectDuration.UntilEndBattle)
                        {
                            yield return new WaitForSeconds(0.2f);

                            #region Delete Digimon Played
                            Permanent playedDigimon = card.PermanentOfThisCard();

                            ActivateClass activateClass2 = new ActivateClass();
                            activateClass2.SetUpICardEffect("Delete this Digimon", CanUseCondition2, card);
                            activateClass2.SetUpActivateClass(CanActivateCondition2, ActivateCoroutine2, -1, false, EffectDiscription2());
                            activateClass2.SetEffectSourcePermanent(playedDigimon);
                            playedDigimon.UntilOpponentTurnEndEffects.Add(GetCardEffect);

                            string EffectDiscription2()
                            {
                                if (deleteDigimon == EffectDuration.UntilOwnerTurnEnd)
                                {
                                    return "[End of Your Turn] Delete this Digimon.";
                                }

                                if (deleteDigimon == EffectDuration.UntilOpponentTurnEnd)
                                {
                                    return "[End of Opponents Turn] Delete this Digimon.";
                                }

                                if (deleteDigimon == EffectDuration.UntilEachTurnEnd)
                                {
                                    return "[End of Turn] Delete this Digimon.";
                                }
                                return "";
                            }

                            bool CanUseCondition2(Hashtable hashtable1)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(playedDigimon, playedDigimon.TopCard))
                                {
                                    if (deleteDigimon == EffectDuration.UntilOwnerTurnEnd)
                                    {
                                        return CardEffectCommons.IsOwnerTurn(card);
                                    }

                                    if (deleteDigimon == EffectDuration.UntilOpponentTurnEnd)
                                    {
                                        return CardEffectCommons.IsOpponentTurn(card);
                                    }

                                    if (deleteDigimon == EffectDuration.UntilEachTurnEnd)
                                    {
                                        return CardEffectCommons.IsOwnerTurn(card)
                                            || CardEffectCommons.IsOpponentTurn(card);
                                    }
                                }

                                return false;
                            }

                            bool CanActivateCondition2(Hashtable hashtable1)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(playedDigimon))
                                {
                                    if (!playedDigimon.TopCard.CanNotBeAffected(activateClass2))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            IEnumerator ActivateCoroutine2(Hashtable _hashtable1)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(playedDigimon))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(
                                    new List<Permanent>() { playedDigimon },
                                    CardEffectCommons.CardEffectHashtable(activateClass2)).Destroy());
                                }
                            }

                            ICardEffect GetCardEffect(EffectTiming _timing)
                            {
                                if (_timing == EffectTiming.OnEndTurn)
                                {
                                    return activateClass2;
                                }

                                return null;
                            }
                            #endregion
                        }
                    }
                }
            }

            ICardEffect GetCardEffect1(EffectTiming _timing)
            {
                if (_timing == EffectTiming.OnEndBattle)
                {
                    return activateClass1;
                }

                return null;
            }
            #endregion

        }

        return activateClass;
    }

    #endregion

    #region Delay Option's Effect to gain 2 Memory

    public static ICardEffect Gain2MemoryOptionDelayEffect(CardSource card)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Memory +2", CanUseCondition, card);
        activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());

        string EffectDiscription()
        {
            return "[Main] <Delay> (Trash this card in your battle area to activate the effect below. You can't activate this effect the turn this card enters play.) - Gain 2 memory.";
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanDeclareOptionDelayEffect(card);
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            bool deleted = false;

            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

            IEnumerator SuccessProcess()
            {
                deleted = true;

                yield return null;
            }

            if (deleted)
            {
                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(2, activateClass));
            }
        }

        return activateClass;
    }

    #endregion

    #region Delay Option's Security effect to place oneself in battle area

    public static ICardEffect PlaceSelfDelayOptionSecurityEffect(CardSource card)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Place this card in battle area", CanUseCondition, card);
        activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
        activateClass.SetIsSecurityEffect(true);

        string EffectDiscription()
        {
            return "[Security] Place this card in its owner's battle area.";
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
        }

        bool CanActivateCondition(Hashtable hashtable)
        {
            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: card, payCost: false, cardEffect: activateClass, isPlayOption: true))
            {
                return true;
            }

            return false;
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
        }

        return activateClass;
    }

    #endregion

    #region Option's Security effect that "Activate this card's Main effect"

    public static ICardEffect ActivateMainOptionSecurityEffect(CardSource card, string effectName, string effectDiscription = "", Func<ICardEffect, IEnumerator> afterMainEffect = null)
    {
        ActivateClass mainActivateClass = CardEffectCommons.OptionMainEffect(card);

        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect(EffectName(), CanUseCondition, card);
        activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
        activateClass.SetIsSecurityEffect(true);

        string EffectName()
        {
            if (!string.IsNullOrEmpty(effectName)) return effectName;
            if (mainActivateClass != null) return mainActivateClass.EffectName;
            return "";
        }

        string EffectDiscription()
        {
            if (!string.IsNullOrEmpty(effectDiscription)) return effectDiscription;
            if (mainActivateClass != null) return mainActivateClass.EffectDiscription.Replace("[Main]", "[Security]");
            return "";
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanTriggerSecurityEffect(CardEffectCommons.OptionMainCheckHashtable(card), card);
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            if (mainActivateClass != null)
            {
                yield return ContinuousController.instance.StartCoroutine(mainActivateClass.Activate(CardEffectCommons.OptionMainCheckHashtable(card)));
            }

            if (afterMainEffect != null)
            {
                yield return ContinuousController.instance.StartCoroutine(afterMainEffect(activateClass));
            }
        }

        return activateClass;
    }

    #endregion

    #region Option's Main Effect to replace bottom security card with this card face up

    public static ICardEffect ReplaceBottomSecurityWithFaceUpOptionMainEffect(CardSource card)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Replace your bottom security card with this face-up card", CanUseCondition, card);
        activateClass.SetUpActivateClass(null, _ => ReplaceBottomSecurityWithFaceUpOptionEffect(card, activateClass), -1, false, EffectDescription());

        string EffectDescription()
        {
            return "[Main] Add your bottom security card to the hand. Then, place this card face up as the bottom security card.";
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
        }

        return activateClass;
    }

    #endregion

    #region Option's Effect to replace top security card with this card face up

    public static ICardEffect ReplaceTopSecurityWithFaceUpOptionMainEffect(CardSource card)
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect("Replace your top security card with this face-up card", CanUseCondition, card);
        activateClass.SetUpActivateClass(null, _ => ReplaceTopSecurityWithFaceUpOptionEffect(card, activateClass), -1, false, EffectDescription());

        string EffectDescription()
        {
            return "[Main] Add your top security card to the hand. Then, place this card face up as the top security card.";
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
        }

        return activateClass;
    }

    #endregion

    #region Option's Effect to replace bottom security card with this card face up

    public static IEnumerator ReplaceBottomSecurityWithFaceUpOptionEffect(CardSource card, ActivateClass activateClass)
    {
        if (card.Owner.SecurityCards.Count >= 1)
        {
            #region Add Bottom Security Card to Hand

            CardSource bottomCard = card.Owner.SecurityCards.Last();

            yield return ContinuousController.instance.StartCoroutine(
                CardObjectController.AddHandCards(new List<CardSource>() { bottomCard }, false, activateClass));

            yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                player: card.Owner,
                refSkillInfos: ref ContinuousController.instance.nullSkillInfos,
                activateClass).ReduceSecurity());

            #endregion
        }

        #region Place Face up as Bottom Security Card

        if (card.Owner.CanAddSecurity(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(
                card, toTop: false, faceUp: true));

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                .CreateRecoveryEffect(card.Owner));

            yield return ContinuousController.instance.StartCoroutine(new IAddSecurity(card).AddSecurity());
        }

        #endregion
    }

    #endregion

    #region Option's Effect to replace top security card with this card face up

    public static IEnumerator ReplaceTopSecurityWithFaceUpOptionEffect(CardSource card, ActivateClass activateClass)
    {
        if (card.Owner.SecurityCards.Count >= 1)
        {
            #region Add Top Security Card to Hand

            CardSource topCard = card.Owner.SecurityCards.First();

            yield return ContinuousController.instance.StartCoroutine(
                CardObjectController.AddHandCards(new List<CardSource>() { topCard }, false, activateClass));

            yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                player: card.Owner,
                refSkillInfos: ref ContinuousController.instance.nullSkillInfos,
                activateClass).ReduceSecurity());

            #endregion
        }

        #region Place Face up as Top Security Card

        if (card.Owner.CanAddSecurity(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(
                card, toTop: true, faceUp: true));

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>()
                .CreateRecoveryEffect(card.Owner));

            yield return ContinuousController.instance.StartCoroutine(new IAddSecurity(card).AddSecurity());
        }

        #endregion
    }

    #endregion

    #region Use Requirements
    public static IgnoreColorConditionClass UseRequirements(CardSource card, Func<CardSource, bool> cardCondition)
    {
        IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
        ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
        ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
        return ignoreColorConditionClass;

        bool PermanentCondition(Permanent permanent)
        {
            if (cardCondition == null)
                return false;

            return (permanent.IsDigimon || permanent.IsTamer)
                && cardCondition(permanent.TopCard); 
        }

        bool CanUseCondition(Hashtable hashtable)
        {
            return CardEffectCommons.HasMatchConditionOwnersPermanent(card, PermanentCondition)
                || CardEffectCommons.HasMatchConditionOwnersBreedingPermanent(card, PermanentCondition);
        }

        bool CardCondition(CardSource cardSource)
        {
            return cardSource == card;
        }
    }
    #endregion

    #region Jogress Condition Class
    public static AddJogressConditionClass GetJogressConditionClass(Func<Permanent, bool> permanentCondition1, string description1, Func<Permanent, bool> permanentCondition2, string description2, CardSource card, int cost = 0, Func<Hashtable, bool> canUseCondition = null)
    {
        AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
        addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
        addJogressConditionClass.SetUpAddJogressConditionClass(getJogressCondition: GetJogress);
        addJogressConditionClass.SetNotShowUI(true);
        return addJogressConditionClass;

        bool CanUseCondition(Hashtable hashtable)
        {
            return canUseCondition == null || canUseCondition(hashtable);
        }

        JogressCondition GetJogress(CardSource cardSource)
        {
            if (cardSource == card)
            {
                return CardEffectCommons.GetJogressConditions(
                    permanentCondition1, 
                    description1, 
                    permanentCondition2, 
                    description2, 
                    card
                    );
            }

            return null;
        }
    }
    #endregion

    #region DigiXrosEffect by Name
    public static AddDigiXrosConditionClass DigiXrosEffectFromNames(CardSource card, int CostReduction, Func<List<CardSource>, CardSource, bool> CanTargetCondition_ByPreSelecetedList, params string[] names)
    {
        AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
        addDigiXrosConditionClass.SetUpICardEffect($"DigiXros", CanUseCondition, card);
        addDigiXrosConditionClass.SetUpAddDigiXrosConditionClass(getDigiXrosCondition: GetDigiXros);
        addDigiXrosConditionClass.SetNotShowUI(true);
        return addDigiXrosConditionClass;

        bool CanUseCondition(Hashtable hashtable) => true;

        DigiXrosCondition GetDigiXros(CardSource cardSource)
        {
            if (cardSource == card)
            {
                return CardEffectCommons.GetDigiXrosConditionsFromNames(card, 2, null, names);
            }

            return null;
        }
    }
    #endregion
    
    #region Shared Effect Condition Creators

    /// Method that can be used to quickly set up effects with multiple trigger conditions
    /// Example: [When Digivolving] [When Attacking] (Do Something) can be set up with whenDigivolving = true and whenAttacking = true
    /// Does not support sharing with Link or Inherited effect, purposefully to ensure they don't accidentally share a hashValue on Once Per Turn effects
    /// 
    /// Always pass in the cardEffects, timing and card variables from the calling card script, this ensures the below methods are added to that card's effects for the correct timing
    /// 
    /// The minimal conditions for triggering and activating the timings are included automatically,
    /// further checks can be added to all created CanUseConditions and CanActivateConditions by passing those additonal checks with additionalUseCondition and additionalActivateCondition
    /// For example, if a effect read "[On Play] [When Attacking] By trashing 1 card in hand..." you might create 
    /// bool AdditionalActivateCondition(Hashtable hashtable) => card.Owner.HandCards.Count >= 1;
    /// and pass such a method into this function.
    /// 
    /// By default, the effects are no optional and have no max per turn count. You can omit those parameters when this is the case and only change them when needed.
    /// To ensure correct behaviour of X Per Turn, the hashValue is shared as the effect is once per turn regardless of how it is triggered.
    /// 
    /// Finally, for whichever combination of effects you want, you will set the corresponding booleans to true. Order does not matter when refering to them by name
    /// Examples: 
    /// "[when Moving] [When Digivolving] [When Attacking] ..." => (... whenMoving: true, whenDigivolving: true, whenAttacking: true)
    /// "[When Digivolving] [On Deletion] ..." => (... whenDigivolving: true, onDeletion: true)
    /// "[End of Attack] [End of Opponent's Turn] ..." => (... endOfAttack: true, endOfOpponentTurn: true)
    public static List<ICardEffect> ActivateClassesForSharedEffects(ref List<ICardEffect> cardEffects,
                                                                    EffectTiming timing, 
                                                                    CardSource card,
                                                                    string effectName, 
                                                                    Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine, 
                                                                    Func<string, string> effectDescription,
                                                                    bool optional,
                                                                    Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                                    Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null, 
                                                                    Func<Hashtable, bool> isSkippableFunction = null,
                                                                    bool isSkippable = false,
                                                                    int maxCountPerTurn = -1,
                                                                    string hashValue = null,
                                                                    bool whenMoving = false,
                                                                    bool onPlay = false,
                                                                    bool whenDigivolving = false,
                                                                    bool whenAttacking = false,
                                                                    bool onDeletion = false,
                                                                    bool whenLinking = false,
                                                                    bool endOfAttack = false,
                                                                    bool endOfYourTurn = false,
                                                                    bool endOfOpponentTurn = false,
                                                                    bool endOfAllTurns = false,
                                                                    bool startOfYourMainPhase = false,
                                                                    bool startOfOpponentsMainPhase = false,
                                                                    bool counter = false)
    {
        if (whenMoving && timing == EffectTiming.OnMove)
        {
            cardEffects.Add(WhenMovingClass(card, effectName, activateCoroutine, effectDescription("When Moving"), optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isSkippable: isSkippable));
        }
        if (onPlay && timing == EffectTiming.OnEnterFieldAnyone)
        {
            cardEffects.Add(OnPlayClass(card, effectName, activateCoroutine, effectDescription("On Play"), optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isSkippable: isSkippable));
        }
        if (whenDigivolving && timing == EffectTiming.OnEnterFieldAnyone)
        {
            cardEffects.Add(WhenDigivolvingClass(card, effectName, activateCoroutine, effectDescription("When Digivolving"), optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isSkippable: isSkippable));
        }
        if (whenAttacking && timing == EffectTiming.OnAllyAttack)
        {
            cardEffects.Add(WhenAttackingClass(card, effectName, activateCoroutine, effectDescription("When Attacking"), optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isSkippable: isSkippable));
        }
        if (onDeletion && timing == EffectTiming.OnDestroyedAnyone)
        {
            cardEffects.Add(OnDeletionClass(card, effectName, activateCoroutine, effectDescription("On Deletion"), optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isSkippable: isSkippable));
        }
        if (whenLinking && timing == EffectTiming.WhenLinked)
        {
            cardEffects.Add(WhenLinkingClass(card, effectName, activateCoroutine, effectDescription("When Linking"), optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isSkippable: isSkippable));
        }
        if (endOfAttack && timing == EffectTiming.OnEndAttack)
        {
            cardEffects.Add(EndOfAttackClass(card, effectName, activateCoroutine, effectDescription("End of Attack"), optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isSkippable: isSkippable));
        }
        if (endOfYourTurn && timing == EffectTiming.OnEndTurn)
        {
            cardEffects.Add(EndOfYourTurnClass(card, effectName, activateCoroutine, effectDescription("End of Your Turn"), optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isSkippable: isSkippable));
        }
        if (endOfOpponentTurn && timing == EffectTiming.OnEndTurn)
        {
            cardEffects.Add(EndOfYourOpponentsTurnClass(card, effectName, activateCoroutine, effectDescription("End of Opponent's Turn"), optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isSkippable: isSkippable));
        }
        if (endOfAllTurns && timing == EffectTiming.OnEndTurn)
        {
            cardEffects.Add(EndOfAllTurnsClass(card, effectName, activateCoroutine, effectDescription("End of All Turns"), optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isSkippable: isSkippable));
        }
        if(startOfYourMainPhase && timing == EffectTiming.OnStartMainPhase)
        {
            cardEffects.Add(StartOfYourMainPhaseClass(card, effectName, activateCoroutine, effectDescription("Start of Your Main Phase"), optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isSkippable: isSkippable));
        }
        if (startOfOpponentsMainPhase && timing == EffectTiming.OnStartMainPhase)
        {
            cardEffects.Add(StartOfYourOpponentsMainPhaseClass(card, effectName, activateCoroutine, effectDescription("Start of Opponent's Main Phase"), optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isSkippable: isSkippable));
        }
        if (counter && timing == EffectTiming.OnCounterTiming)
        {
            cardEffects.Add(CounterClass(card, effectName, activateCoroutine, effectDescription("Counter"), optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isSkippable: isSkippable));
        }

        return cardEffects;
    }

    #endregion

    #region Boilerplate ActivateClass

    public static ActivateClass ActivateClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, bool> canUseCondition,
                                                Func<Hashtable, ActivateClass, bool> canActivateCondition,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInheritedEffect = false,
                                                bool isLinkedEffect = false,
                                                bool isSecurityEffect = false,
                                                bool isSkippable = false
                                                )
    {
        ActivateClass activateClass = new ActivateClass();
        activateClass.SetUpICardEffect(effectName, hashtable => canUseCondition(hashtable, activateClass), card);
        activateClass.SetUpActivateClass(hashtable => canActivateCondition(hashtable, activateClass), hashtable => activateCoroutine(hashtable, activateClass), maxCountPerTurn, optional, effectDescription);
        activateClass.SetHashString(hashValue);
        activateClass.SetIsSecurityEffect(isSecurityEffect);
        activateClass.SetIsSkippableFunction(isSkippableFunction);
        activateClass.SetIsSkippable(isSkippable);
        return activateClass;
    }

    #endregion

    #region When Moving

    public static ActivateClass WhenMovingClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInherited = false,
                                                bool isSkippable = false)
    {
        return ActivateClass(card, effectName, CanUseCondition, CanActivateCondition, activateCoroutine, effectDescription, optional, isSkippableFunction, maxCountPerTurn, hashValue, isInherited, false, isSkippable: isSkippable);

        bool PermanentCondition(Permanent permanent)
        {
            return permanent == card.PermanentOfThisCard();
        }

        bool CanUseCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass) &&
                    CardEffectCommons.CanTriggerOnMove(hashtable, PermanentCondition) && 
                    (additionalUseCondition == null || additionalUseCondition(hashtable, activateClass));
        }

        bool CanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass) && 
                (additionalActivateCondition == null || additionalActivateCondition(hashtable, activateClass));
        }
    }

    #endregion

    #region On Play

    public static ActivateClass OnPlayClass(CardSource card,
                                            string effectName,
                                            Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                            string effectDescription,
                                            bool optional,
                                            Func<Hashtable, bool> isSkippableFunction = null,
                                            Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                            Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                            int maxCountPerTurn = -1,
                                            string hashValue = null,
                                            bool isInherited = false,
                                            bool isSkippable = false)
    {
        return ActivateClass(card, effectName, CanUseCondition, CanActivateCondition, activateCoroutine, effectDescription, optional, isSkippableFunction, maxCountPerTurn, hashValue, isInherited, false, isSkippable: isSkippable);

        bool CanUseCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass) &&
                CardEffectCommons.CanTriggerOnPlay(hashtable, card) && 
                (additionalUseCondition == null || additionalUseCondition(hashtable, activateClass));
        }

        bool CanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass) && 
                (additionalActivateCondition == null || additionalActivateCondition(hashtable, activateClass));
        }
    }

    #endregion

    #region When Digivolving

    public static ActivateClass WhenDigivolvingClass(CardSource card,
                                                        string effectName,
                                                        Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                        string effectDescription,
                                                        bool optional,
                                                        Func<Hashtable, bool> isSkippableFunction = null,
                                                        Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                        Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                        int maxCountPerTurn = -1,
                                                        string hashValue = null,
                                                        bool isInherited = false,
                                                        bool isLinked = false,
                                                        bool isSkippable = false)
    {
        return ActivateClass(card, effectName, CanUseCondition, CanActivateCondition, activateCoroutine, effectDescription, optional, isSkippableFunction, maxCountPerTurn, hashValue, isInherited, isLinked, isSkippable: isSkippable);

        bool CanUseCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass) &&
                CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card) && 
                (additionalUseCondition == null || additionalUseCondition(hashtable, activateClass));
        }

        bool CanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass) && 
                (additionalActivateCondition == null || additionalActivateCondition(hashtable, activateClass));
        }
    }

    #endregion

    #region When Attacking
    public static ActivateClass WhenAttackingClass(CardSource card,
                                                    string effectName,
                                                    Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                    string effectDescription,
                                                    bool optional,
                                                    Func<Hashtable, bool> isSkippableFunction = null,
                                                    Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                    Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                    int maxCountPerTurn = -1,
                                                    string hashValue = null,
                                                    bool isInherited = false,
                                                    bool isLinked = false,
                                                    bool isSkippable = false)
    {
        return ActivateClass(card, effectName, CanUseCondition, CanActivateCondition, activateCoroutine, effectDescription, optional, isSkippableFunction, maxCountPerTurn, hashValue, isInherited, isLinked, isSkippable: isSkippable);

        bool CanUseCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass) &&
                CardEffectCommons.CanTriggerOnAttack(hashtable, card) && 
                (additionalUseCondition == null || additionalUseCondition(hashtable, activateClass));
        }

        bool CanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass) && 
                (additionalActivateCondition == null || additionalActivateCondition(hashtable, activateClass));
        }
    }

    #endregion

    #region On Deletion

    public static ActivateClass OnDeletionClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInherited = false,
                                                bool isLinked = false,
                                                bool isSkippable = false)
    {
        return ActivateClass(card, effectName, CanUseCondition, CanActivateCondition, activateCoroutine, effectDescription, optional, isSkippableFunction, maxCountPerTurn, hashValue, isInherited, isLinked, isSkippable: isSkippable);

        bool CanUseCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.CanTriggerOnDeletion(hashtable, card, activateClass) && 
                (additionalUseCondition == null || additionalUseCondition(hashtable, activateClass));
        }

        bool CanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.CanActivateOnDeletion(card, activateClass)
                && (additionalActivateCondition == null || additionalActivateCondition(hashtable, activateClass));
        }
    }

    #endregion

    #region When Linking

    public static ActivateClass WhenLinkingClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInherited = false,
                                                bool isLinked = false,
                                                bool isSkippable = false
                                                )
    {
        return ActivateClass(card, effectName, CanUseCondition, CanActivateCondition, activateCoroutine, effectDescription, optional, isSkippableFunction, maxCountPerTurn, hashValue, isInherited, isLinked, isSkippable: isSkippable);

        bool CanUseCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass) &&
                CardEffectCommons.CanTriggerWhenLinking(hashtable, null, card) &&
                (additionalUseCondition == null || additionalUseCondition(hashtable, activateClass));
        }

        bool CanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass) &&
                (additionalActivateCondition == null || additionalActivateCondition(hashtable, activateClass));
        }
    }

    #endregion

    #region Security Effect

    public static ActivateClass SecurityClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                bool isSkippable = false)
    {
        ActivateClass activateClass = ActivateClass(card, effectName, CanUseCondition, additionalActivateCondition, activateCoroutine, effectDescription, optional, isSkippableFunction, -1, null, false, false, true, isSkippable);
        return activateClass;

        bool CanUseCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card) &&
                (additionalUseCondition == null || additionalUseCondition(hashtable, activateClass));
        }
    }

    #endregion

    #region End of Attack

    public static ActivateClass EndOfAttackClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInherited = false,
                                                bool isLinked = false,
                                                bool isSkippable = false)
    {
        return ActivateClass(card, effectName, CanUseCondition, CanActivateCondition, activateCoroutine, effectDescription, optional, isSkippableFunction, maxCountPerTurn, hashValue, isInherited, isLinked, isSkippable: isSkippable);

        bool CanUseCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                && CardEffectCommons.CanTriggerOnAttack(hashtable, card)
                && (additionalUseCondition == null 
                    || additionalUseCondition(hashtable, activateClass));
        }

        bool CanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass) &&
                (additionalActivateCondition == null || additionalActivateCondition(hashtable, activateClass));
        }

    }

    #endregion

    #region Counter

    public static ActivateClass CounterClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInherited = false,
                                                bool isLinked = false,
                                                bool isSkippable = false)
    {
        ActivateClass activateClass = ActivateClass(card, effectName, CanUseCondition, CanActivateCondition, activateCoroutine, effectDescription, optional, isSkippableFunction, maxCountPerTurn, hashValue, isInherited, isLinked, isSkippable: isSkippable);
        activateClass.SetIsCounterEffect(true);
        return activateClass;

        bool CanUseCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, permanent => CardEffectCommons.IsOpponentPermanent(permanent, card))
                && (additionalUseCondition == null 
                    || additionalUseCondition(hashtable, activateClass));
        }

        bool CanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass) &&
                (additionalActivateCondition == null || additionalActivateCondition(hashtable, activateClass));
        }

    }

    #endregion

    #region Start/End of Turn/Phase, Your/Opponent's/All Turns

/**
 *  Can be used for Your Turn, Opponent's Turn, All Turns, End of [Your|Opponent's|All] Turn, Start of Turn/Phase effects 
 *  based on the timing it is used at and additional use conditions passed
 */
    public static ActivateClass TurnTimingClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInherited = false,
                                                bool isLinked = false,
                                                bool yourTurn = false,
                                                bool opponentTurn = false,
                                                bool isSkippable = false)
    {
        return ActivateClass(card, effectName, CanUseCondition, CanActivateCondition, activateCoroutine, effectDescription, optional, isSkippableFunction, maxCountPerTurn, hashValue, isInherited, isLinked, isSkippable: isSkippable);

        bool CanUseCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                && (!yourTurn || CardEffectCommons.IsOwnerTurn(card))
                && (!opponentTurn || CardEffectCommons.IsOpponentTurn(card))
                && (additionalUseCondition == null || additionalUseCondition(hashtable, activateClass));
        }

        bool CanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
        {
            return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass) &&
                (additionalActivateCondition == null || additionalActivateCondition(hashtable, activateClass));
        }

    }

    #endregion

    #region Your Turn Classes

    public static ActivateClass StartOfYourTurnClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInherited = false,
                                                bool isLinked = false,
                                                bool isSkippable = false)
    {
        return TurnTimingClass(card, effectName, activateCoroutine, effectDescription, optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isInherited, isLinked, true, false, isSkippable);
    }

    public static ActivateClass StartOfYourMainPhaseClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInherited = false,
                                                bool isLinked = false,
                                                bool isSkippable = false)
    {
        return TurnTimingClass(card, effectName, activateCoroutine, effectDescription, optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isInherited, isLinked, true, false, isSkippable);
    }

    public static ActivateClass EndOfYourTurnClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInherited = false,
                                                bool isLinked = false,
                                                bool isSkippable = false)
    {
        return TurnTimingClass(card, effectName, activateCoroutine, effectDescription, optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isInherited, isLinked, true, false, isSkippable);
    }

    public static ActivateClass YourTurnClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInherited = false,
                                                bool isLinked = false,
                                                bool isSkippable = false)
    {
        return TurnTimingClass(card, effectName, activateCoroutine, effectDescription, optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isInherited, isLinked, true, false, isSkippable);
    }

    #endregion

    #region Opponent's Turn Classes

    public static ActivateClass StartOfOpponentsTurnClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInherited = false,
                                                bool isLinked = false,
                                                bool isSkippable = false)
    {
        return TurnTimingClass(card, effectName, activateCoroutine, effectDescription, optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isInherited, isLinked, false, true, isSkippable);
    }

    public static ActivateClass StartOfYourOpponentsMainPhaseClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInherited = false,
                                                bool isLinked = false,
                                                bool isSkippable = false)
    {
        return TurnTimingClass(card, effectName, activateCoroutine, effectDescription, optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isInherited, isLinked, false, true, isSkippable);
    }

    public static ActivateClass EndOfYourOpponentsTurnClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInherited = false,
                                                bool isLinked = false,
                                                bool isSkippable = false)
    {
        return TurnTimingClass(card, effectName, activateCoroutine, effectDescription, optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isInherited, isLinked, false, true, isSkippable);
    }

    public static ActivateClass OpponentsTurnClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInherited = false,
                                                bool isLinked = false,
                                                bool isSkippable = false)
    {
        return TurnTimingClass(card, effectName, activateCoroutine, effectDescription, optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isInherited, isLinked, false, true, isSkippable);
    }

    #endregion

    #region All Turn Classes

    public static ActivateClass EndOfAllTurnsClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInherited = false,
                                                bool isLinked = false,
                                                bool isSkippable = false)
    {
        return TurnTimingClass(card, effectName, activateCoroutine, effectDescription, optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isInherited, isLinked, true, true, isSkippable);
    }

    public static ActivateClass AllTurnsClass(CardSource card,
                                                string effectName,
                                                Func<Hashtable, ActivateClass, IEnumerator> activateCoroutine,
                                                string effectDescription,
                                                bool optional,
                                                Func<Hashtable, bool> isSkippableFunction = null,
                                                Func<Hashtable, ActivateClass, bool> additionalUseCondition = null,
                                                Func<Hashtable, ActivateClass, bool> additionalActivateCondition = null,
                                                int maxCountPerTurn = -1,
                                                string hashValue = null,
                                                bool isInherited = false,
                                                bool isLinked = false,
                                                bool isSkippable = false)
    {
        return TurnTimingClass(card, effectName, activateCoroutine, effectDescription, optional, isSkippableFunction, additionalUseCondition, additionalActivateCondition, maxCountPerTurn, hashValue, isInherited, isLinked, true, true, isSkippable);
    }

    #endregion

    #region At end of turn lose 3 memory
    public static ActivateClass EoTLose3Memory(CardSource card)
    {
        ActivateClass activateClass1 = new ActivateClass();
        activateClass1.SetUpICardEffect("Memory -3", CanUseCondition1, card);
        activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
        return activateClass1;

        string EffectDiscription1()
        {
            return "Lose 3 memory.";
        }

        bool CanUseCondition1(Hashtable hashtable)
        {
            return true;
        }

        bool CanActivateCondition1(Hashtable hashtable)
        {
            return true;
        }

        IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
        {
            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(-3, activateClass1));
        }
    }
    #endregion

    #region Place used Options in Security instead of trash effect
    public static OptionResolutionClass PlaceToSecurityEffect(CardSource card, bool toTop, bool isFaceUp = false, Func<bool> Condition = null)
    {
        if (card == null) return null;
        string location = toTop ? "top" : "bottom";
        string facing = isFaceUp ? "face up" : "face down";
        string effectName = $"Place the used Option card on {location} of security {facing}.";
        OptionResolutionClass placeToSecurityEffect = new();
        placeToSecurityEffect.SetUpICardEffect(effectName, CanUseCondition, card);
        placeToSecurityEffect.SetUpOptionResolutionClass(ResolutionCoroutine, CanResolveCondition);
        return placeToSecurityEffect;

        bool CanUseCondition(Hashtable hashtable)
        {
            return Condition == null || Condition();
        }

        bool CanResolveCondition(CardSource optionCard) => optionCard.Owner.CanAddSecurity(placeToSecurityEffect);
        
        IEnumerator ResolutionCoroutine(CardSource optionCard)
        {
            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(optionCard, toTop, isFaceUp));
        }
    }
    #endregion

    #region Add detail for Display
    public static AddDetailClass AddDetailClass(Func<Hashtable, bool> canUseCondition, Func<Permanent, bool> permanentCondition, string detail, bool triggerEffect, CardSource card)
    {
        AddDetailClass addDetailClass = new();
        addDetailClass.SetUpICardEffect("Added Detail", canUseCondition, card);
        addDetailClass.SetUpAddDetailClass(permanentCondition, detail, triggerEffect);
        return addDetailClass;
    }
    #endregion
}