using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
using System.Net.Security;

public class DesperadeBluster_BT3_100 : CEntity_Effect
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
                return "[Main] Trash up to 2 digivolution cards from the bottom of all of your opponent's Digimon. Then, if you have a green Digimon in play, suspend 1 of your opponent's Digimon with no digivolution cards.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                {
                    if (!permanent.TopCard.CanNotBeAffected(activateClass))
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
                List<Permanent> selectedPermanents = new List<Permanent>();

                foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons())
                {
                    if (!permanent.TopCard.CanNotBeAffected(activateClass))
                    {
                        if (permanent.DigivolutionCards.Count((cardSource) => !cardSource.CanNotTrashFromDigivolutionCards(activateClass)) >= 1)
                        {
                            selectedPermanents.Add(permanent);
                        }
                    }
                }

                if (selectedPermanents.Count >= 1)
                {
                    foreach (Permanent selectedPermanent in selectedPermanents)
                    {
                        int maxCount = Math.Min(2, selectedPermanent.DigivolutionCards.Count);

                        SelectCountEffect selectCountEffect = GManager.instance.GetComponent<SelectCountEffect>();

                        selectCountEffect.SetUp(
                            SelectPlayer: card.Owner,
                            targetPermanent: selectedPermanent,
                            MaxCount: maxCount,
                            CanNoSelect: false,
                            Message: $"How many digivolution cards will you trash from {selectedPermanent.TopCard.CardNames[0]}?",
                            Message_Enemy: $"The opponent is choosing how many digivolution cards to trash from {selectedPermanent.TopCard.CardNames[0]}.",
                            SelectCountCoroutine: SelectCountCoroutine);

                        yield return ContinuousController.instance.StartCoroutine(selectCountEffect.Activate());

                        IEnumerator SelectCountCoroutine(int count)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: count, isFromTop: false, activateClass: activateClass));
                        }
                    }
                }

                if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent.TopCard.CardColors.Contains(CardColor.Green)))
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Trash digivolution cards and suspend 1 Digimon");
        }

        return cardEffects;
    }
}
