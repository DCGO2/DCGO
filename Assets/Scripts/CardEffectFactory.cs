using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor.Rendering;
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
            if (CardEffectCommons.IsExistOnBattleArea(card))
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
            if (CardEffectCommons.IsExistOnBattleArea(card))
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

    #region Digimon's Security effect to play oneself after battle
    public static ICardEffect PlaySelfDigimonAfterBattleSecurityEffect(CardSource card)
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
            if (CardEffectCommons.IsExistOnExecutingArea(card))
            {
                return true;
            }

            return false;
        }

        IEnumerator ActivateCoroutine(Hashtable _hashtable)
        {
            yield return null;

            ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

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
}
