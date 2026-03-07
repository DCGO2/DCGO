using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Tommy, Takuya, & Zoe
namespace DCGO.CardEffects.AD1
{
    public class AD1_020 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared OP/SOMP

            string SharedEffectName = "Place up to 2 [Hybrid] cards with different colors under, if so <Draw 1> and if 4 or more under gain 2 memory";

            string SharedEffectDescription(string tag) =>
                $"[{tag}] You may place up to 2 [Hybrid] trait cards with different colors from your hand or trash under this Tamer. If this effect placed, <Draw 1>. Then, if there are 4 or more [Hybrid] trait cards under this Tamer, gain 2 memory.";

            bool IsHybridCard(CardSource cardSource)
            {
                return cardSource.ContainsTraits("Hybrid")
                    && cardSource.Owner == card.Owner
                    && cardSource.CardColors.Any(c => card.CardColors.Contains(c));
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
                        return !cardSource.CardColors.Any(c => selectedColors.Contains(c));
                    }
                    return true;
                }

                while (selectedCards.Count < 2)
                {
                    List<CardSource> validHandCards = card.Owner.HandCards.Filter(CanSelectCardCondition).ToList();
                    List<CardSource> validTrashCards = card.Owner.TrashCards.Filter(CanSelectCardCondition).ToList();

                    if (validHandCards.Count == 0 && validTrashCards.Count == 0)
                        break;

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    if (validHandCards.Count > 0)
                    {
                        selectionElements.Add(new(message: "from Hand", value: 1, spriteIndex: 0));
                    }
                    if (validTrashCards.Count > 0)
                    {
                        selectionElements.Add(new(message: "from Trash", value: 2, spriteIndex: 0));
                    }

                    GManager.instance.userSelectionManager.SetIntSelection(
                        selectionElements: selectionElements,
                        selectPlayer: card.Owner,
                        selectPlayerMessage: "From which area will you select a [Hybrid] card to place under this Tamer?",
                        notSelectPlayerMessage: "The opponent is choosing from which area to select a [Hybrid] card.");

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    int prevCount = selectedCards.Count;

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
                    }

                    if (selectedCards.Count == prevCount)
                        break;
                }

                if (selectedCards.Count > 0)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        card.PermanentOfThisCard().AddDigivolutionCardsBottom(selectedCards, activateClass));

                    if (card.Owner.LibraryCards.Count() > 0)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(
                            card.Owner,
                            1,
                            activateClass).Draw());
                    }
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

            #region End of Your Turn

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Attack with this Digimon with Security A. +1",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true,
                    EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("AD1_020_EoYT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[End of Your Turn] [Once Per Turn] By attacking with this Digimon with the [Hybrid] or [Ten Warriors] trait, it gains <Security A. +1> (This Digimon checks 1 additional security card.) for the attack.";
                }

                bool HasHybridOrTenWarriors(Permanent permanent)
                {
                    return permanent.TopCard.ContainsTraits("Hybrid")
                        || permanent.TopCard.ContainsTraits("Ten Warriors")
                        || permanent.TopCard.ContainsTraits("TenWarriors");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        Permanent thisPermanent = card.PermanentOfThisCard();
                        if (!GManager.instance.attackProcess.IsAttacking)
                        {
                            return HasHybridOrTenWarriors(thisPermanent)
                                && thisPermanent.CanAttack(activateClass);
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(thisPermanent))
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.ChangeDigimonSAttack(
                                targetPermanent: thisPermanent,
                                changeValue: 1,
                                effectDuration: EffectDuration.UntilEndAttack,
                                activateClass: activateClass));

                        if (thisPermanent.CanAttack(activateClass))
                        {
                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: thisPermanent,
                                canAttackPlayerCondition: () => true,
                                defenderCondition: (permanent) => true,
                                cardEffect: activateClass);

                            selectAttackEffect.SetCanNotSelectNotAttack();

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }
                    }
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
