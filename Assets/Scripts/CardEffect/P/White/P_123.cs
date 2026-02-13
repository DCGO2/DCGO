using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class P_123 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnMove)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Hatch and memory +1", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Your Turn] [Once Per Turn] When one of your Digimon moves from the breeding area to the battle area, you may hatch in your breeding area. Then, gain 1 memory.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerOnMove(hashtable, PermanentCondition))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.CanHatch)
                {
                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Hatch", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Not hatch", value : false, spriteIndex: 1),
                        };

                    string selectPlayerMessage = "Will you hatch in Breeding Area?";
                    string notSelectPlayerMessage = "The opponent is selecting whether to hatch in Breeding Area.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool doHatch = GManager.instance.userSelectionManager.SelectedBoolValue;

                    if (doHatch)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new HatchDigiEggClass(player: card.Owner, hashtable: CardEffectCommons.CardEffectHashtable(activateClass)).Hatch());
                    }
                }

                yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
            }
        }

        return cardEffects;
    }
}
