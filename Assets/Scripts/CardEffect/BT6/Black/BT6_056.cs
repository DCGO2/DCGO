using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT6_056 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("De-Digivolve 1 to 1 Digimon at the end of the battle", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] At the end of the battle, trigger <De-Digivolve 1> on 1 of your opponent's Digimon. (Trash 1 card from the top of one of your opponent's Digimon. If it has no digivolution cards, or becomes a level 3 Digimon, you can't trash any more cards.)";
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
                activateClass1.SetUpICardEffect("De-Digivolve 1 to 1 Digimon", CanUseCondition1, card);
                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                card.Owner.UntilEndBattleEffects.Add(GetCardEffect1);

                string EffectDiscription1()
                {
                    return "Trigger <De-Digivolve 1> on up to 1 of your opponent's Digimon. (Trash a card from the top of one of your opponent's Digimon. If it has no digivolution cards, or becomes a level 3 Digimon, you can't trash any more cards.)";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition1(Hashtable hashtable)
                {
                    return true;
                }

                bool CanActivateCondition1(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
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
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to De-Digivolve.", "The opponent is selecting 1 Digimon to De-Digivolve.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            Permanent selectedPermanent = permanent;

                            yield return ContinuousController.instance.StartCoroutine(new IDegeneration(selectedPermanent, 1, activateClass1).Degeneration());
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
        }

        return cardEffects;
    }
}
