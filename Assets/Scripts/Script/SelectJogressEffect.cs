using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class SelectJogressEffect : MonoBehaviour
{
    public void SetUp_SelectWheterToJogress
        (CardSource card,
        CardSource evoRoot,
        bool canNoSelect,
        Func<IEnumerator> endSelectCoroutine_Digivolve,
        Func<IEnumerator> endSelectCoroutine_Jogress,
        Func<IEnumerator> noSelectCoroutine)
    {
        _card = card;
        _evoRoot = evoRoot;
        _canNoSelect = canNoSelect;
        _endSelectCoroutine_Digivolve = endSelectCoroutine_Digivolve;
        _endSelectCoroutine_Jogress = endSelectCoroutine_Jogress;
        _noSelectCoroutine = noSelectCoroutine;
    }

    public void SetUp_SelectDigivolutionRoots
        (CardSource card,
        bool isLocal,
        bool isPayCost,
        bool canNoSelect,
        Func<List<Permanent>, IEnumerator> endSelectCoroutine_SelectDigivolutionRoots,
        Func<IEnumerator> noSelectCoroutine)
    {
        _card = card;
        _isLocal = isLocal;
        _isPayCost = isPayCost;
        _canNoSelect = canNoSelect;
        _endSelectCoroutine_SelectDigivolutionRoots = endSelectCoroutine_SelectDigivolutionRoots;
        _noSelectCoroutine = noSelectCoroutine;

        _customPermanentConditions = null;
    }

    public void SetUpCustomPermanentConditions(Func<Permanent, bool>[] customPermanentConditions)
    {
        _customPermanentConditions = customPermanentConditions.CloneArray();
    }

    CardSource _card = null;
    CardSource _evoRoot = null;
    bool _isLocal = false;

    bool _isPayCost = false;
    bool _canNoSelect = false;
    Func<IEnumerator> _endSelectCoroutine_Digivolve = null;
    Func<IEnumerator> _endSelectCoroutine_Jogress = null;
    Func<List<Permanent>, IEnumerator> _endSelectCoroutine_SelectDigivolutionRoots = null;
    Func<IEnumerator> _noSelectCoroutine = null;
    Func<Permanent, bool>[] _customPermanentConditions = null;
    public IEnumerator SelectWheterToJogress()
    {
        if (_card != null)
        {
            if (_evoRoot != null)
            {
                CardSource anotherEvoRootCard = null;

                List<Permanent> anotherEvoRootPermanentCandidates = new List<Permanent>();

                foreach (Permanent permanent in _card.Owner.GetBattleAreaDigimons())
                {
                    if (permanent != _evoRoot.PermanentOfThisCard())
                    {
                        if (_card.CanJogressFromTargetPermanent(permanent, false))
                        {
                            if (_card.CanJogressFromTargetPermanents(new List<Permanent>() { _evoRoot.PermanentOfThisCard(), permanent }, false))
                            {
                                if (anotherEvoRootPermanentCandidates.Count((permanent1) => permanent1.TopCard.CardID == permanent.TopCard.CardID) == 0)
                                {
                                    anotherEvoRootPermanentCandidates.Add(permanent);
                                }
                            }
                        }
                    }
                }

                if (anotherEvoRootPermanentCandidates.Count == 1)
                {
                    anotherEvoRootCard = anotherEvoRootPermanentCandidates[0].TopCard;
                }

                yield return StartCoroutine(GManager.instance.selectCardPanel.OpenSelectCardPanel(
                            Message: "With which method would you like to Digivolve?",
                            RootCardSources: new List<CardSource>() { _card, _card },
                            _CanTargetCondition: (cardSource) => true,
                            _CanTargetCondition_ByPreSelecetedList: null,
                            _CanEndSelectCondition: null,
                            _MaxCount: 1,
                            _CanEndNotMax: false,
                            _CanNoSelect: () => _canNoSelect,
                            CanLookReverseCard: true,
                            skillInfos: null,
                            root: SelectCardEffect.Root.None,
                            isCenter: true,
                            evoRootsArray: new CardSource[][] { new CardSource[] { _evoRoot }, new CardSource[] { _evoRoot, anotherEvoRootCard } },
                            titleStrings: new List<string>() { "Normal Digivolution", "<color=#FF633E>DNA</color> Digivollution" }));

                if (GManager.instance.selectCardPanel.SelectedIndex.Count > 0)
                {
                    int index = GManager.instance.selectCardPanel.SelectedIndex[0];

                    switch (index)
                    {
                        case 0:
                            if (_endSelectCoroutine_Digivolve != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(_endSelectCoroutine_Digivolve());
                            }
                            break;

                        case 1:
                            if (_endSelectCoroutine_Jogress != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(_endSelectCoroutine_Jogress());
                            }
                            break;
                    }
                }

                else
                {
                    if (_noSelectCoroutine != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(_noSelectCoroutine());
                    }
                }
            }
        }
    }
    public IEnumerator SelectDigivolutionRoots()
    {
        bool active = false;
        SelectPermanentEffect selectPermanentEffect = null;

        if (_card != null)
        {
            if (_card.CanPlayJogress(_isPayCost))
            {
                if (_card.jogressCondition.Count > 0)
                {
                    foreach (JogressCondition dnaCondition in _card.jogressCondition)
                    {
                        if(dnaCondition != null)
                        {
                            if(dnaCondition.elements.Length == 2)
                            {
                                if (GManager.instance != null)
                                {
                                    if (GManager.instance.turnStateMachine != null)
                                    {
                                        if (GManager.instance.turnStateMachine.gameContext != null)
                                        {
                                            if (GManager.instance.turnStateMachine.gameContext.ActiveCardList.Count >= 1)
                                            {
                                                selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                                if (selectPermanentEffect != null)
                                                {
                                                    active = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (active)
        {
            List<Permanent> selectedEvoRoots = new List<Permanent>();
            JogressCondition selectedDNA = _card.jogressCondition[0];

            if (_card.jogressCondition.Count > 1)
            {
                #region select DNA condition
                SelectDNACondition selectDNACondition = GManager.instance.GetComponent<SelectDNACondition>();
                selectDNACondition.SetUp(_card.Owner, _card, SelectDNA);

                yield return ContinuousController.instance.StartCoroutine(selectDNACondition.Activate());

                IEnumerator SelectDNA(int dnaSelection)
                {
                    selectedDNA = _card.jogressCondition[dnaSelection];

                    yield return null;
                }
                #endregion
            }

            
            for (int i = 0; i < selectedDNA.elements.Length; i++)
            {
                JogressConditionElement element = selectedDNA.elements[i];

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, _card))
                    {
                        if(!_card.CanNotEvolve(permanent))
                        {
                            if (!selectedEvoRoots.Contains(permanent))
                            {
                                if (element.EvoRootCondition != null)
                                {
                                    if (element.EvoRootCondition(permanent))
                                    {
                                        if (_customPermanentConditions != null)
                                        {
                                            if (_customPermanentConditions.Length == 1)
                                            {
                                                if (_customPermanentConditions[0] != null)
                                                {
                                                    if (_card.Owner.GetBattleAreaDigimons().Count(_customPermanentConditions[0]) >= 1)
                                                    {
                                                        if (selectedEvoRoots.Count(_customPermanentConditions[0]) == 0)
                                                        {
                                                            if (!_customPermanentConditions[0](permanent))
                                                            {
                                                                if (i == 1)
                                                                {
                                                                    return false;
                                                                }

                                                                else
                                                                {
                                                                    JogressConditionElement element1 = selectedDNA.elements[1];

                                                                    if (_card.Owner.GetBattleAreaDigimons().Count((permanent1) => permanent1 != permanent && element1.EvoRootCondition(permanent1) && _customPermanentConditions[0](permanent1)) == 0)
                                                                    {
                                                                        return false;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }

                                                    else
                                                    {
                                                        return false;
                                                    }
                                                }
                                            }

                                            else if (_customPermanentConditions.Length == 2)
                                            {
                                                if (_customPermanentConditions[0] != null && _customPermanentConditions[1] != null)
                                                {
                                                    if (_card.Owner.GetBattleAreaDigimons().Count(_customPermanentConditions[0]) >= 1 && _card.Owner.GetBattleAreaDigimons().Count(_customPermanentConditions[1]) >= 1)
                                                    {
                                                        if (i == 1)
                                                        {
                                                            if (selectedEvoRoots.Count(_customPermanentConditions[0]) == 0 && selectedEvoRoots.Count(_customPermanentConditions[1]) == 0)
                                                            {
                                                                return false;
                                                            }

                                                            if (selectedEvoRoots.Count(_customPermanentConditions[0]) == 0 && selectedEvoRoots.Count(_customPermanentConditions[1]) == 1)
                                                            {
                                                                if (!_customPermanentConditions[0](permanent))
                                                                {
                                                                    return false;
                                                                }
                                                            }

                                                            if (selectedEvoRoots.Count(_customPermanentConditions[0]) == 1 && selectedEvoRoots.Count(_customPermanentConditions[1]) == 0)
                                                            {
                                                                if (!_customPermanentConditions[1](permanent))
                                                                {
                                                                    return false;
                                                                }
                                                            }
                                                        }

                                                        else
                                                        {
                                                            if (_customPermanentConditions[0](permanent) || _customPermanentConditions[1](permanent))
                                                            {
                                                                JogressConditionElement element1 = selectedDNA.elements[1];

                                                                if (_customPermanentConditions[0](permanent))
                                                                {
                                                                    if (_card.Owner.GetBattleAreaDigimons().Count((permanent1) => permanent1 != permanent && element1.EvoRootCondition(permanent1) && _customPermanentConditions[1](permanent1)) == 0)
                                                                    {
                                                                        return false;
                                                                    }
                                                                }

                                                                if (_customPermanentConditions[1](permanent))
                                                                {
                                                                    if (_card.Owner.GetBattleAreaDigimons().Count((permanent1) => permanent1 != permanent && element1.EvoRootCondition(permanent1) && _customPermanentConditions[0](permanent1)) == 0)
                                                                    {
                                                                        return false;
                                                                    }
                                                                }
                                                            }

                                                            else
                                                            {
                                                                return false;
                                                            }
                                                        }
                                                    }

                                                    else
                                                    {
                                                        return false;
                                                    }
                                                }
                                            }
                                        }

                                        if (i == 0)
                                        {
                                            List<Permanent> nextCandidates = _card.Owner.GetBattleAreaDigimons()
                                                .Filter(permanent1 => permanent1 != permanent);

                                            if (nextCandidates.Count(selectedDNA.elements[1].EvoRootCondition) == 0)
                                            {
                                                return false;
                                            }
                                        }

                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                Permanent selectedPermanent = null;

                if (maxCount >= 1)
                {
                    selectPermanentEffect.SetUp(
                    selectPlayer: _card.Owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: _canNoSelect,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: null);

                    selectPermanentEffect.SetUpCustomMessage($"Select {element.SelectMessage}.", $"The opponent is selecting {element.SelectMessage}.");

                    if (this._isLocal)
                    {
                        selectPermanentEffect.SetIsLocal();
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }
                }

                if (selectedPermanent == null)
                {
                    break;
                }

                else
                {
                    selectedEvoRoots.Add(selectedPermanent);

                    foreach (Permanent permanent in selectedEvoRoots)
                    {
                        permanent.ShowingPermanentCard.SetPermanentIndexText(selectedEvoRoots);
                    }
                }
            }

            //do not jogress
            if (selectedEvoRoots.Count != selectedDNA.elements.Length)
            {
                if (_noSelectCoroutine != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(_noSelectCoroutine());
                }
            }

            //do jogress
            else
            {
                if (_endSelectCoroutine_SelectDigivolutionRoots != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(_endSelectCoroutine_SelectDigivolutionRoots(selectedEvoRoots));
                }
            }

            foreach (Permanent permanent in _card.Owner.GetBattleAreaDigimons())
            {
                if (permanent != null)
                {
                    if (permanent.ShowingPermanentCard != null)
                    {
                        permanent.ShowingPermanentCard.OffPermanentIndexText();
                    }
                }
            }
        }
    }
}
