using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCGO.CardEffects.EX6
{
    public class EX6_029 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();
            
            #region Ace - Blast DNA Digivolve
            
            if (timing == EffectTiming.OnCounterTiming)
            {
                List<BlastDNACondition> blastDNAConditions = new List<BlastDNACondition>
                {
                    new("Angewomon"),
                    new("LadyDevimon")
                };
                cardEffects.Add(CardEffectFactory.BlastDNADigivolveEffect(card: card,
                    blastDNAConditions: blastDNAConditions, condition: null));
            }
            
            #endregion
            
            #region DNA Digivolution
            
            if (timing == EffectTiming.None)
            {
                AddJogressConditionClass addJogressConditionClass = new AddJogressConditionClass();
                addJogressConditionClass.SetUpICardEffect($"DNA Digivolution", CanUseCondition, card);
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
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent,
                                                    card))
                                            {
                                                if (permanent.TopCard.CardColors.Contains(CardColor.Yellow))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(5))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            
                            return false;
                        }
                        
                        bool PermanentCondition2(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (permanent.TopCard.Owner == card.Owner)
                                    {
                                        if (permanent.IsDigimon)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent,
                                                    card))
                                            {
                                                if (permanent.TopCard.CardColors.Contains(CardColor.Purple))
                                                {
                                                    if (permanent.Levels_ForJogress(card).Contains(5))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            
                            return false;
                        }
                        
                        JogressConditionElement[] elements =
                        {
                            new(PermanentCondition1, "a level 5 Yellow Digimon"),
                            new(PermanentCondition2, "a level 5 Purple Digimon")
                        };
                        
                        JogressCondition jogressCondition = new JogressCondition(elements, 0);
                        
                        return jogressCondition;
                    }
                    
                    return null;
                }
            }
            
            #endregion
            
            #region On Play/ When Digivolving Shared

            bool CanSelectPermanentSharedCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                {
                    if (permanent != card.PermanentOfThisCard())
                    {
                        if (permanent.IsDigimon)
                        {
                            if (!permanent.TopCard.Equals(card))
                                return true;
                        }
                    }
                }
                
                return false;
            }
            
            #endregion
            
            #region On Play
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Play 1 level 5 or lower Digimon card with the [Angel]/[Archangel]/[Fallen Angel] trait from your hand or trash, then, if DNA digivolving, place 1 other Digimon at the bottom of its owner's security stack, and trash cards from the top of your opponent's security stack until it has 4 left.",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] You may play 1 level 5 or lower Digimon card with the [Angel]/[Archangel]/[Fallen Angel] trait from your hand or trash without paying the cost. Then, if DNA digivolving, place 1 other Digimon at the bottom of its owner's security stack, and trash cards from the top of your opponent's security stack until it has 4 left.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
                
                bool IsCardAngelCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false,
                            cardEffect: activateClass))
                    {
                        if (cardSource.IsDigimon)
                        {
                            if(cardSource.HasLevel && cardSource.Level <= 5)
                            {
                                if (cardSource.HasAngelTraits)
                                {
                                    return true;
                                }
                            }                            
                        }
                    }
                    
                    return false;
                }
                
                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count(IsCardAngelCondition) >= 1)
                        {
                            return true;
                        }
                        
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsCardAngelCondition))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = card.Owner.HandCards.Count(IsCardAngelCondition) >= 1;
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsCardAngelCondition);
                    
                    if (canSelectHand || canSelectTrash)
                    {
                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                            {
                                new(message: "From hand", value: true, spriteIndex: 0),
                                new(message: "From trash", value: false, spriteIndex: 1),
                            };
                            
                            string selectPlayerMessage = "From which area do you play a card?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";
                            
                            GManager.instance.userSelectionManager.SetBoolSelection(
                                selectionElements: selectionElements, selectPlayer: card.Owner,
                                selectPlayerMessage: selectPlayerMessage,
                                notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectHand);
                        }
                        
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());
                        
                        bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;
                        
                        List<CardSource> selectedCards = new List<CardSource>();
                        
                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }
                        
                        if (fromHand)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                            
                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsCardAngelCondition,
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
                            
                            selectHandEffect.SetUpCustomMessage("Select 1 card to play.",
                                "The opponent is selecting 1 card to play.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");
                            
                            yield return StartCoroutine(selectHandEffect.Activate());
                        }
                        
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                            
                            selectCardEffect.SetUp(
                                canTargetCondition: IsCardAngelCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to play.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);
                            
                            selectCardEffect.SetUpCustomMessage("Select 1 card to play.",
                                "The opponent is selecting 1 card to play.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");
                            
                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }
                        
                        SelectCardEffect.Root root =
                            (fromHand) ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                        
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: root,
                            activateETB: true));
                    }
                    
                    if (CardEffectCommons.IsJogress(hashtable))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentSharedCondition))
                        {
                            int maxCount = Math.Min(1,
                                CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentSharedCondition));
                            
                            SelectPermanentEffect selectPermanentEffect =
                                GManager.instance.GetComponent<SelectPermanentEffect>();
                            
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentSharedCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);
                            
                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place on bottom of security.",
                                "The opponent is selecting 1 Digimon to place on bottom of security.");
                            
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            
                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                if (permanent.TopCard != null)
                                {
                                    if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(
                                            new IPutSecurityPermanent(
                                                permanent,
                                                CardEffectCommons.CardEffectHashtable(activateClass),
                                                toTop: false).PutSecurity());
                                    }
                                }
                            }
                        }
                        
                        if (card.Owner.Enemy.SecurityCards.Count > 4)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                player: card.Owner.Enemy,
                                destroySecurityCount: card.Owner.Enemy.SecurityCards.Count - 4,
                                cardEffect: activateClass,
                                fromTop: true).DestroySecurity());
                        }
                    }
                }
            }
            
            #endregion
            
            #region When Digivolving
            
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(
                    "Play 1 level 5 or lower Digimon card with the [Angel]/[Archangel]/[Fallen Angel] trait from your hand or trash, then, if DNA digivolving, place 1 other Digimon at the bottom of its owner's security stack, and trash cards from the top of your opponent's security stack until it has 4 left.",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] You may play 1 level 5 or lower Digimon card with the [Angel]/[Archangel]/[Fallen Angel] trait from your hand or trash without paying the cost. Then, if DNA digivolving, place 1 other Digimon at the bottom of its owner's security stack, and trash cards from the top of your opponent's security stack until it has 4 left.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool IsCardAngelCondition(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false,
                            cardEffect: activateClass))
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.HasLevel && cardSource.Level <= 5)
                            {
                                if (cardSource.HasAngelTraits)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {                    
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }
                
                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = card.Owner.HandCards.Count(IsCardAngelCondition) >= 1;
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, IsCardAngelCondition);
                    
                    if (canSelectHand || canSelectTrash)
                    {
                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                            {
                                new(message: "From hand", value: true, spriteIndex: 0),
                                new(message: "From trash", value: false, spriteIndex: 1),
                            };
                            
                            string selectPlayerMessage = "From which area do you play a card?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";
                            
                            GManager.instance.userSelectionManager.SetBoolSelection(
                                selectionElements: selectionElements, selectPlayer: card.Owner,
                                selectPlayerMessage: selectPlayerMessage,
                                notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectHand);
                        }
                        
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());
                        
                        bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;
                        
                        List<CardSource> selectedCards = new List<CardSource>();
                        
                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }
                        
                        if (fromHand)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();
                            
                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: IsCardAngelCondition,
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
                            
                            selectHandEffect.SetUpCustomMessage("Select 1 card to play.",
                                "The opponent is selecting 1 card to play.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");
                            
                            yield return StartCoroutine(selectHandEffect.Activate());
                        }
                        
                        else
                        {
                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                            
                            selectCardEffect.SetUp(
                                canTargetCondition: IsCardAngelCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to play.",
                                maxCount: 1,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);
                            
                            selectCardEffect.SetUpCustomMessage("Select 1 card to play.",
                                "The opponent is selecting 1 card to play.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");
                            
                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }
                        
                        SelectCardEffect.Root root =
                            (fromHand) ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
                        
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: root,
                            activateETB: true));
                    }
                    
                    if (CardEffectCommons.IsJogress(hashtable))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentSharedCondition))
                        {
                            int maxCount = Math.Min(1,
                                CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentSharedCondition));
                            
                            SelectPermanentEffect selectPermanentEffect =
                                GManager.instance.GetComponent<SelectPermanentEffect>();
                            
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentSharedCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);
                            
                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place on bottom of security.",
                                "The opponent is selecting 1 Digimon to place on bottom of security.");
                            
                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                            
                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                if (permanent != null)
                                {
                                    if (permanent.TopCard != null)
                                    {
                                        if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(
                                                new IPutSecurityPermanent(
                                                    permanent,
                                                    CardEffectCommons.CardEffectHashtable(activateClass),
                                                    toTop: false).PutSecurity());
                                        }
                                    }
                                }
                            }
                        }
                        
                        if (card.Owner.Enemy.SecurityCards.Count > 4)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                player: card.Owner.Enemy,
                                destroySecurityCount: card.Owner.Enemy.SecurityCards.Count - 4,
                                cardEffect: activateClass,
                                fromTop: true).DestroySecurity());
                        }
                    }
                }
            }
            
            #endregion
            
            return cardEffects;
        }
    }
}