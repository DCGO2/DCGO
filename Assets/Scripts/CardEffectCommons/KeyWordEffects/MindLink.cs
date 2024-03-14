using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class MindLinkClass
{
    public MindLinkClass(Permanent tamer, Func<Permanent, bool> digimonCondition, ICardEffect activateClass)
    {
        _tamer = tamer;
        _digimonCondition = digimonCondition;
        _activateClass = activateClass;
    }
    Permanent _tamer = null;
    Func<Permanent, bool> _digimonCondition = null;
    ICardEffect _activateClass = null;
    bool CanSelectPermanentCondition(Permanent permanent)
    {
        if (CardEffectCommons.IsPermanentExistsOnBattleArea(_tamer))
        {
            if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, _tamer.TopCard))
            {
                if (!permanent.IsToken)
                {
                    if (permanent.DigivolutionCards.Count(cardSource => cardSource.IsTamer) == 0)
                    {
                        if (_digimonCondition == null || _digimonCondition(permanent))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
    public IEnumerator MindLink()
    {
        if (_tamer == null) yield break;
        if (_tamer.TopCard == null) yield break;
        if (_digimonCondition == null) yield break;
        if (_activateClass == null) yield break;
        if (_activateClass.EffectSourceCard == null) yield break;

        CardSource card = _activateClass.EffectSourceCard;

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
                canNoSelect: true,
                canEndNotMax: false,
                selectPermanentCoroutine: SelectPermanentCoroutine,
                afterSelectPermanentCoroutine: null,
                mode: SelectPermanentEffect.Mode.Custom,
                cardEffect: _activateClass);

            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get a digivolution card.", "The opponent is selecting 1 Digimon that will get a digivolution card.");

            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

            IEnumerator SelectPermanentCoroutine(Permanent permanent)
            {
                Permanent selectedPermanent = permanent;

                if (selectedPermanent != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IPlacePermanentToDigivolutionCards(new List<Permanent[]>() { new Permanent[] { _tamer, selectedPermanent } }, false, _activateClass).PlacePermanentToDigivolutionCards());
                }
            }
        }
    }
}