using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class P_024 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] If you have [Tai Kamiya] in play, you may place 1 of your [Agumon] cards at the bottom of its owner's deck to trigger <Draw 3>. (Draw 3 cards from your deck.) Trash that Digimon's digivolution cards.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                {
                    if (permanent.TopCard.CardNames.Contains("Tai Kamiya"))
                    {
                        return true;
                    }

                    if (permanent.TopCard.CardNames.Contains("TaiKamiya"))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanSelectPermanentCondition1(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                {
                    if (permanent.TopCard.CardNames.Contains("Agumon"))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                {
                    if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition1) >= 1)
                    {
                        Permanent bouncePermanent = null;

                        int maxCount = Math.Min(1, card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition1));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition1,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 [Agumon] to return to the bottom of deck.", "The opponent is selecting 1 [Agumon] to return to the bottom of deck.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            bouncePermanent = permanent;

                            yield return null;
                        }

                        if (bouncePermanent != null)
                        {
                            if (bouncePermanent.TopCard != null)
                            {
                                if (!bouncePermanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    if (!bouncePermanent.CannotReturnToLibrary(activateClass))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(new List<Permanent>() { bouncePermanent }, CardEffectCommons.CardEffectHashtable(activateClass)).DeckBounce());

                                        if (bouncePermanent.TopCard == null && bouncePermanent.LibraryBounceEffect == activateClass)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 3, activateClass).Draw());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect($"Add this card to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] Add this card to its owner's hand.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
            }
        }

        return cardEffects;
    }
}
