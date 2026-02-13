using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT6_098 : CEntity_Effect
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
                return "[Main] Return 1 of your opponent's level 5 or lower Digimon to its owner's hand. If your opponent has 3 or more Digimon in play, return 1 of your opponent's Digimon to the bottom of its owner's deck instead. Trash all of the digivolution cards of that Digimon.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (permanent != null)
                {
                    if (permanent.TopCard != null)
                    {
                        if (permanent.IsDigimon)
                        {
                            if (permanent.TopCard.Owner == card.Owner.Enemy)
                            {
                                if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                {
                                    if (card.Owner.Enemy.GetBattleAreaDigimons().Count >= 3)
                                    {
                                        return true;
                                    }

                                    else
                                    {
                                        if (permanent.Level <= 5)
                                        {
                                            if (permanent.TopCard.HasLevel)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
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
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    SelectPermanentEffect.Mode mode = SelectPermanentEffect.Mode.Bounce;

                    if (card.Owner.Enemy.GetBattleAreaDigimons().Count >= 3)
                    {
                        mode = SelectPermanentEffect.Mode.PutLibraryBottom;
                    }

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
                        mode: mode,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }
        }

        if (timing == EffectTiming.SecuritySkill)
        {
            CardEffectCommons.AddActivateMainOptionSecurityEffect(card: card, cardEffects: ref cardEffects, effectName: $"Return 1 level 5 or lower Digimon to hand or deck");
        }

        return cardEffects;
    }
}
