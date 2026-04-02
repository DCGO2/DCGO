using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectFactory
{
    #region Processing of [Arts Digivolve]
    public static OptionResolutionClass ArtsDigivolveEffect(CardSource card)
    {
        if (card == null) return null;
        OptionResolutionClass artsDigivolutionClass = new();
        artsDigivolutionClass.SetUpICardEffect("Arts Digivolve", CanUseCondition, card);
        artsDigivolutionClass.SetUpOptionResolutionClass(ResolutionCoroutine, CanResolveCondition);
        return artsDigivolutionClass;

        bool CanUseCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnExecutingArea(card);

        bool CanResolveCondition(CardSource optionCard) => CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectPermanentCondition);

        bool CanSelectPermanentCondition(Permanent permanent)
        {
            return (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                    || CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card))
                && card.CanPlayCardTargetFrame(permanent.PermanentFrame, false, artsDigivolutionClass, SelectCardEffect.Root.Execution);
        }
        
        IEnumerator ResolutionCoroutine(CardSource optionCard)
        {
            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
    
            selectPermanentEffect.SetUp(
                selectPlayer: card.Owner,
                canTargetCondition: CanSelectPermanentCondition,
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                maxCount: 1,
                canNoSelect: true,
                canEndNotMax: false,
                selectPermanentCoroutine: SelectPermanentCoroutine,
                afterSelectPermanentCoroutine: null,
                mode: SelectPermanentEffect.Mode.Custom,
                cardEffect: artsDigivolutionClass);

            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to Arts Digivolve.", "The opponent is selecting 1 Digimon to Arts Digivolve.");

            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

            IEnumerator SelectPermanentCoroutine(Permanent permanent)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoExcecutingAreaCard(
                    permanent,
                    null,
                    false,
                    null,
                    null,
                    -1,
                    artsDigivolutionClass,
                    null,
                    ignoreSelection: true
                ));
            }
        }
    }
    #endregion
}