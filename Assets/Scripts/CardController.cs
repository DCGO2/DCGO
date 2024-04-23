using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#region Trash cards from hand
public class IDiscardHands
{
    public IDiscardHands(List<IDiscardHand> discardHands, ICardEffect cardEffect)
    {
        foreach (IDiscardHand discardHand in discardHands)
        {
            this.discardHands.Add(discardHand);
        }

        this.cardEffect = cardEffect;
    }

    List<IDiscardHand> discardHands { get; set; } = new List<IDiscardHand>();
    ICardEffect cardEffect { get; set; }

    public IEnumerator DiscardHands()
    {
        foreach (IDiscardHand discardHand in discardHands)
        {
            yield return ContinuousController.instance.StartCoroutine(discardHand.Discard());
        }

        List<CardSource> discardedCards = new List<CardSource>();

        foreach (IDiscardHand discardHand in discardHands)
        {
            if (discardHand.discarded)
            {
                discardedCards.Add(discardHand.cardSource);
            }
        }

        if (discardedCards.Count >= 1)
        {
            #region "When cards are trashed from hand" effect

            #region Hashtable setting
            Hashtable hashtable = new Hashtable()
            {
                {"DiscardedCards", discardedCards},
                {"CardEffect", cardEffect},
            };
            #endregion

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable, EffectTiming.OnDiscardHand));

            #endregion
        }

        #region add log
        if (discardedCards.Count >= 1)
        {
            string log = "";

            log += $"\nDiscard hand:";

            foreach (CardSource cardSource in discardedCards)
            {
                log += $"\n{cardSource.BaseENGCardNameFromEntity}({cardSource.CardID})";
            }

            log += "\n";

            GManager.instance.playLog.AddLogString(log);
        }
        #endregion
    }
}

public class IDiscardHand
{
    public IDiscardHand(CardSource cardSource, Hashtable hashtable)
    {
        this.cardSource = cardSource;
        this.hashtable = hashtable;
    }
    public CardSource cardSource { get; private set; }

    public Hashtable hashtable { get; private set; }
    public bool discarded { get; private set; } = false;

    public IEnumerator Discard()
    {
        bool oldisHand = cardSource.Owner.HandCards.Contains(cardSource);

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().DeleteHandCardEffectCoroutine(cardSource));

        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource));

        bool isTrash = CardEffectCommons.IsExistOnTrash(cardSource);

        if (oldisHand && isTrash)
        {
            discarded = true;
        }
    }
}

#endregion

#region Play cards
public class PlayCardClass
{
    public PlayCardClass(List<CardSource> cardSources, Hashtable hashtable, bool payCost, Permanent targetPermanent, bool isTapped, SelectCardEffect.Root root,
    bool activateETB)
    {
        if (cardSources != null)
        {
            CardSources = cardSources.Filter(cardSource => cardSource != null).Clone();
        }

        _hashtable = hashtable;
        PayCost = payCost;
        _targetPermanent = targetPermanent;
        _isTapped = isTapped;
        Root = root;
        _activateETB = activateETB;
    }

    public void SetJogress(int[] jogressEvoRootsFrameIDs)
    {
        if (jogressEvoRootsFrameIDs != null)
        {
            _jogressEvoRootsFrameIDs = jogressEvoRootsFrameIDs.CloneArray();
        }
    }

    public void SetBurst(int BurstTamerFrameID, CardSource card)
    {
        if (0 <= BurstTamerFrameID && BurstTamerFrameID <= card.Owner.fieldCardFrames.Count - 1)
        {
            _burstTamerFrameID = BurstTamerFrameID;
        }
    }

    public void SetShowEffect()
    {
        _showEffect = true;
    }

    public void SetIgnoreLevel()
    {
        _ignoreLevel = true;
    }

    public void SetFixedCost(int FixedCost)
    {
        _fixedCost = FixedCost;
    }

    public void SetAddSecurityEndOption()
    {
        _addSecurityEndOption = true;
    }

    public void SetIsBreedingArea()
    {
        _isBreedingArea = true;
    }

    public List<CardSource> CardSources { get; private set; } = new List<CardSource>();
    Hashtable _hashtable = null;
    public bool PayCost { get; private set; }
    Permanent _targetPermanent = null;
    bool _isTapped = false;
    public SelectCardEffect.Root Root { get; private set; } = SelectCardEffect.Root.None;
    bool _activateETB = true;
    bool _showEffect = false;
    bool _ignoreLevel = false;
    int _fixedCost = -1;
    int[] _jogressEvoRootsFrameIDs = null;
    int _burstTamerFrameID = -1;
    bool _addSecurityEndOption = false;
    bool _isBreedingArea = false;

    public bool isJogress => _jogressEvoRootsFrameIDs != null && _jogressEvoRootsFrameIDs.Length == 2;

    bool IsBurst(CardSource card)
    {
        Permanent burstTamer = BurstTamer(card);

        if (burstTamer != null)
        {
            if (burstTamer.TopCard != null)
            {
                if (card.burstDigivolutionCondition != null)
                {
                    if (card.burstDigivolutionCondition.tamerCondition != null)
                    {
                        if (card.burstDigivolutionCondition.tamerCondition(burstTamer))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    Permanent BurstTamer(CardSource card)
    {
        if (0 <= _burstTamerFrameID && _burstTamerFrameID <= card.Owner.fieldCardFrames.Count - 1)
        {
            Permanent tamer = card.Owner.fieldCardFrames[_burstTamerFrameID].GetFramePermanent();

            return tamer;
        }

        return null;
    }

    public IEnumerator PlayCard()
    {
        bool burstDigivolved = false;

        List<CardSource> playedCards_fixed = new List<CardSource>();

        foreach (CardSource card in CardSources)
        {
            GManager.instance.GetComponent<SelectDigiXrosClass>().ResetSelectDigiXrosClass();

            if (card == null)
            {
                continue;
            }

            ICardEffect CardEffect = null;

            CardEffect = CardEffectCommons.GetCardEffectFromHashtable(this._hashtable);

            if (CardEffectCommons.IsExistOnTrash(card))
            {
                Root = SelectCardEffect.Root.Trash;
            }

            else if (card.Owner.HandCards.Contains(card))
            {
                Root = SelectCardEffect.Root.Hand;
            }

            else if (card.Owner.LibraryCards.Contains(card))
            {
                Root = SelectCardEffect.Root.Library;
            }

            else if (card.Owner.GetFieldPermanents().Count((permanent) => permanent.DigivolutionCards.Contains(card)) >= 1)
            {
                Root = SelectCardEffect.Root.DigivolutionCards;
            }

            else if (card.Owner.SecurityCards.Contains(card))
            {
                Root = SelectCardEffect.Root.Security;
            }

            List<Permanent> targetPermanents = new List<Permanent>();

            if (card.IsPermanent)
            {
                if (!isJogress)
                {
                    if (CardEffectCommons.IsOwnerPermanent(_targetPermanent, card))
                    {
                        targetPermanents.Add(_targetPermanent);
                    }
                }

                else
                {
                    for (int i = 0; i < _jogressEvoRootsFrameIDs.Length; i++)
                    {
                        int JogressFrameID = _jogressEvoRootsFrameIDs[i];

                        if (0 <= JogressFrameID && JogressFrameID <= card.Owner.fieldCardFrames.Count - 1)
                        {
                            Permanent targetPermanent = card.Owner.fieldCardFrames[JogressFrameID].GetFramePermanent();
                            targetPermanents.Add(targetPermanent);
                        }
                    }

                    foreach (Permanent permanent in targetPermanents)
                    {
                        permanent.ShowingPermanentCard.SetPermanentIndexText(targetPermanents);
                    }
                }
            }

            bool isEvolution = false;

            if (targetPermanents.Count >= 1)
            {
                if (!isJogress)
                {
                    if (IsBurst(card))
                    {
                        if (card.CanBurstDigivolutionFromTargetPermanent(targetPermanents[0], PayCost))
                        {
                            isEvolution = true;
                        }
                    }

                    else
                    {
                        if (card.CanEvolve(targetPermanents[0], true) || _ignoreLevel)
                        {
                            isEvolution = true;
                        }
                    }
                }

                else
                {
                    if (targetPermanents.Count == 2)
                    {
                        isEvolution = true;
                    }
                }
            }

            List<CardSource> oldTrashCards = new List<CardSource>();

            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForNonTurnPlayer)
            {
                foreach (Permanent permanent in player.GetFieldPermanents())
                {
                    permanent.oldIsTapped_playCard = permanent.IsSuspended;
                }
            }

            foreach (CardSource cardSource in card.Owner.TrashCards)
            {
                oldTrashCards.Add(cardSource);
            }

            // effect of removing digivolution cards
            if (card.IsPermanent && !isEvolution && card.PermanentOfThisCard() != null && Root == SelectCardEffect.Root.DigivolutionCards)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(card, card.PermanentOfThisCard()));
            }

            #region select digivolution cost
            int baseCost = -1;

            bool costSelected = false;

            yield return ContinuousController.instance.StartCoroutine(SelectCost());

            IEnumerator SelectCost()
            {
                if (!isJogress)
                {
                    if (PayCost)
                    {
                        if (_fixedCost < 0)
                        {
                            Permanent targetPermanent = null;

                            if (targetPermanents.Count >= 1)
                            {
                                targetPermanent = targetPermanents[0];
                            }

                            if (targetPermanent != null)
                            {
                                List<int> CostList = new List<int>();

                                if (!IsBurst(card))
                                {
                                    foreach (int cost in card.CostList(targetPermanent, ignoreLevel: _ignoreLevel, checkAvailability: false))
                                    {
                                        if (cost <= card.Owner.MaxMemoryCost)
                                        {
                                            CostList.Add(cost);
                                        }
                                    }
                                }

                                else
                                {
                                    CostList.Add(card.burstDigivolutionCondition.cost);
                                }

                                CostList = CostList.Distinct().ToList();

                                if (CostList.Count >= 1)
                                {
                                    if (CostList.Count == 1)
                                    {
                                        baseCost = CostList[0];
                                    }

                                    else
                                    {
                                        costSelected = true;

                                        bool MoveToExecuteCardEffect = true;

                                        if (card.Owner.HandCards.Contains(card) && card.ShowingHandCard != null)
                                        {
                                            if (card.ShowingHandCard.gameObject.activeSelf)
                                            {
                                                MoveToExecuteCardEffect = false;
                                            }
                                        }

                                        if (!card.Owner.isYou && GManager.instance.IsAI)
                                        {
                                            MoveToExecuteCardEffect = false;

                                            costSelected = false;
                                        }

                                        if (card.Owner.isYou && ContinuousController.instance.autoMinDigivolutionCost)
                                        {
                                            MoveToExecuteCardEffect = false;

                                            costSelected = false;
                                        }

                                        if (MoveToExecuteCardEffect)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().MoveToExecuteCardEffect(card));
                                        }

                                        SelectCountEffect selectCountEffect = GManager.instance.GetComponent<SelectCountEffect>();

                                        if (selectCountEffect != null)
                                        {
                                            selectCountEffect.SetUp(
                                                SelectPlayer: card.Owner,
                                                targetPermanent: null,
                                                MaxCount: 1,
                                                CanNoSelect: false,
                                                Message: "Which digivolution cost do you pay?",
                                                Message_Enemy: "The opponent is choosing which digivolution cost to pay.",
                                                SelectCountCoroutine: SelectCountCoroutine);

                                            selectCountEffect.SetCandidates(CostList);
                                            selectCountEffect.SetPreferMin(true);
                                            selectCountEffect.SetNotDoSync(true);
                                            selectCountEffect.SetIsDigivolutionCost(true);

                                            yield return ContinuousController.instance.StartCoroutine(selectCountEffect.Activate());

                                            IEnumerator SelectCountCoroutine(int count)
                                            {
                                                baseCost = count;
                                                yield return null;
                                            }
                                        }
                                    }
                                }
                            }

                            else
                            {
                                baseCost = card.BasePlayCostFromEntity;
                            }
                        }
                    }
                }
            }

            #endregion

            #region HashTable Setting

            Hashtable hashtable = CardEffectCommons.WouldEnterFieldHashtable(
                payCost: PayCost,
                card: card,
                root: Root,
                isEvolution: isEvolution,
                playCardClass: this,
                cardEffect: CardEffect,
                isJogress: isJogress,
                targetPermanents: targetPermanents
            );
            #endregion

            List<SkillInfo> skillInfos_BeforePayCost = AutoProcessing.GetSkillInfos(hashtable, EffectTiming.BeforePayCost)
            .Concat(AutoProcessing.GetSkillInfosOfCards(hashtable, EffectTiming.BeforePayCost, new List<CardSource>() { card }
                .Filter(cardSource => !CardEffectCommons.IsExistOnHand(cardSource) && !CardEffectCommons.IsExistOnTrash(cardSource) && cardSource.PermanentOfThisCard() == null)))
            .ToList();

            yield return ContinuousController.instance.StartCoroutine(AutoProcessing.ActivateBackgroundEffects(hashtable, EffectTiming.BeforePayCost));

            #region IsShowEffect()
            bool IsShowEffect()
            {
                if (skillInfos_BeforePayCost.Count >= 2)
                {
                    return true;
                }

                else if (skillInfos_BeforePayCost.Count == 1)
                {
                    if (skillInfos_BeforePayCost[0].CardEffect.CanActivate(skillInfos_BeforePayCost[0].Hashtable))
                    {
                        return true;
                    }
                }

                else if (card.HasDigiXros && !isEvolution)
                {
                    return true;
                }

                else if (IsBurst(card))
                {
                    return true;
                }

                return false;
            }
            #endregion

            #region effect
            if (PayCost || IsShowEffect())
            {
                if (!costSelected)
                {
                    if (card.IsOption || IsShowEffect())
                    {
                        bool noHandCard = true;

                        if (card.Owner.HandCards.Contains(card))
                        {
                            if (card.ShowingHandCard != null)
                            {
                                if (card.ShowingHandCard.gameObject.activeSelf)
                                {
                                    if (card.ShowingHandCard.gameObject.transform.GetChild(0).gameObject.activeSelf)
                                    {
                                        noHandCard = false;
                                    }
                                }
                            }
                        }

                        if (noHandCard)
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().MoveToExecuteCardEffect(card));
                        }
                    }

                    else
                    {
                        if (CardEffect == null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShrinkUpUseHandCard(GManager.instance.GetComponent<Effects>().ShowUseHandCard));
                        }
                    }
                }
            }
            #endregion

            #region show expected cost
            if (PayCost)
            {
                if (!isJogress)
                {
                    int cost = card.GetPayingCostWithBaseCost(baseCost, Root, targetPermanents, checkAvailability: false, FixedCost: _fixedCost);

                    GManager.instance.memoryObject.ShowMemoryPredictionLine(card.Owner.ExpectedMemory(cost));
                }

                else
                {
                    if (card.jogressCondition != null)
                    {
                        int cost = card.GetPayingCostWithBaseCost(card.jogressCondition.cost, Root, targetPermanents, checkAvailability: false, FixedCost: _fixedCost);

                        GManager.instance.memoryObject.ShowMemoryPredictionLine(card.Owner.ExpectedMemory(cost));
                    }
                }
            }
            #endregion

            #region process cut in effects before paying cost
            if (skillInfos_BeforePayCost.Count >= 1)
            {
                foreach (SkillInfo skillInfo in skillInfos_BeforePayCost)
                {
                    GManager.instance.autoProcessing_CutIn.PutStackedSkill(skillInfo);
                }

                if (IsShowEffect())
                {
                    foreach (Permanent targetPermanent in targetPermanents)
                    {
                        if (targetPermanent != null)
                        {
                            targetPermanent.ShowWillEvolutionEffect();
                        }
                    }
                }

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.TriggeredSkillProcess(false, AutoProcessing.HasExecutedSameEffect));

                foreach (Permanent targetPermanent in targetPermanents)
                {
                    if (targetPermanent != null)
                    {
                        targetPermanent.HideWillEvolutionEffect();
                    }
                }
            }
            #endregion

            #region select DigiXros
            if (card.HasDigiXros && !isEvolution)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<SelectDigiXrosClass>().Select(card));
            }
            #endregion

            #region Bounce Tamer of Burst digivolution

            if (IsBurst(card))
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.selectBurstDigivolutionEffect.BounceTamer(BurstTamer(card)));

                if (!GManager.instance.selectBurstDigivolutionEffect.TamerBounced)
                {
                    _burstTamerFrameID = -1;

                    yield return ContinuousController.instance.StartCoroutine(SelectCost());
                }

                else
                {
                    burstDigivolved = true;
                }
            }
            #endregion

            #region fix cost to pay
            int Cost = 0;

            if (PayCost)
            {
                if (!isJogress)
                {
                    Cost = card.GetPayingCostWithBaseCost(baseCost, Root, targetPermanents, checkAvailability: false, FixedCost: _fixedCost);
                }

                else
                {
                    if (card.jogressCondition != null)
                    {
                        Cost = card.GetPayingCostWithBaseCost(card.jogressCondition.cost, Root, targetPermanents, checkAvailability: false, FixedCost: _fixedCost);
                    }
                }

                GManager.instance.memoryObject.ShowMemoryPredictionLine(card.Owner.ExpectedMemory(Cost));
            }

            #endregion

            #region end play cards 
            bool endPlayCard = false;
            bool playFailed = false;

            if (PayCost)
            {
                if (Cost > card.Owner.MaxMemoryCost)
                {
                    endPlayCard = true;
                    playFailed = true;
                }
            }

            if (isEvolution)
            {
                if (targetPermanents != null)
                {
                    if (targetPermanents.Count >= 1)
                    {
                        foreach (Permanent permanent in targetPermanents)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard == null)
                                {
                                    endPlayCard = true;
                                }
                            }
                        }

                        if (!endPlayCard)
                        {
                            if (!isJogress && !IsBurst(card))
                            {
                                if (!_ignoreLevel && !card.CanPlayCardTargetFrame(targetPermanents[0].PermanentFrame, PayCost, CardEffect, root: Root, fixedCost: -1))
                                {
                                    endPlayCard = true;
                                    playFailed = true;
                                }
                            }

                            else if (isJogress)
                            {
                                if (!card.CanJogressFromTargetPermanents(targetPermanents, PayCost))
                                {
                                    endPlayCard = true;
                                    playFailed = true;
                                }
                            }

                            else if (IsBurst(card))
                            {
                                if (!card.CanBurstDigivolutionFromTargetPermanent(targetPermanents[0], PayCost))
                                {
                                    endPlayCard = true;
                                    playFailed = true;
                                }
                            }
                        }
                    }
                }
            }

            if (endPlayCard)
            {
                GManager.instance.playLog.AddLogString($"\nFailed to play:\n{card.BaseENGCardNameFromEntity}({card.CardID})\n");

                GManager.instance.GetComponent<SelectDigiXrosClass>().ResetSelectDigiXrosClass();

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().FailedPlayCardEffect(card));

                if (card.Owner.HandCards.Contains(card))
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(card));

                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { card }, false, null));
                }

                ContinuousController.instance.StartCoroutine(OffMemoryPredictionLine());

                foreach (HandCard handCard in card.Owner.brainStormObject.BrainStormHandCards)
                {
                    if (handCard.gameObject.activeSelf && handCard.cardSource == card)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.CloseBrainstrorm(card));
                    }
                }

                foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                {
                    foreach (FieldPermanentCard fieldPermanentCard in player.FieldPermanentObjects)
                    {
                        if (fieldPermanentCard != null)
                        {
                            fieldPermanentCard.OffPermanentIndexText();
                        }
                    }
                }

                if (playFailed)
                {
                    foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                    {
                        foreach (Permanent permanent in player.GetFieldPermanents())
                        {
                            permanent.IsSuspended = permanent.oldIsTapped_playCard;
                        }
                    }

                    foreach (CardSource cardSource in oldTrashCards)
                    {
                        if (!CardEffectCommons.IsExistOnTrash(cardSource))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource));
                        }
                    }
                }
            }
            #endregion

            card.Owner.UntilCalculateFixedCostEffect = new List<Func<EffectTiming, ICardEffect>>();

            if (endPlayCard)
            {
                continue;
            }

            #region pay cost
            if (PayCost)
            {
                // memory lose
                if (Cost <= card.Owner.MaxMemoryCost)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(-1 * Cost, null));
                }

                ContinuousController.instance.StartCoroutine(OffMemoryPredictionLine());
            }
            #endregion

            #region cut in effect after paying cost
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.StackSkillInfos(hashtable, EffectTiming.AfterPayCost));

            // cur in effect process
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.TriggeredSkillProcess(
                false,
                AutoProcessing.HasExecutedSameEffect));
            #endregion

            // add to played cards
            playedCards_fixed.Add(card);
        }

        #region filter cards
        List<CardSource> permanentCards = playedCards_fixed.Filter(cardSource => cardSource.IsPermanent);
        List<CardSource> optionCards = playedCards_fixed.Filter(cardSource => !cardSource.IsPermanent);

        #region play permanent
        PlayPermanentClass playPermanent = new PlayPermanentClass(permanentCards, _hashtable, _targetPermanent, _isTapped, Root, _activateETB);

        if (isJogress)
        {
            playPermanent.SetJogress(_jogressEvoRootsFrameIDs);
        }

        if (burstDigivolved)
        {
            playPermanent.SetBurstDigivolved();
        }

        if (_isBreedingArea)
        {
            playPermanent.SetIsBreedingArea();
        }

        yield return ContinuousController.instance.StartCoroutine(playPermanent.PlayPermanent());
        #endregion

        #region use option
        UseOptionClass useOption = new UseOptionClass(optionCards, _hashtable, Root)
        {
            _showEffect = _showEffect,
            _addSecurityEndOption = _addSecurityEndOption
        };

        yield return ContinuousController.instance.StartCoroutine(useOption.UseOption());
        #endregion

        #endregion

    }

    IEnumerator OffMemoryPredictionLine()
    {
        yield return new WaitForSeconds(0.5f);

        GManager.instance.memoryObject.OffMemoryPredictionLine();
    }
}
#endregion

#region Hatch DigiEgg
public class HatchDigiEggClass
{
    public HatchDigiEggClass(Player player, Hashtable hashtable)
    {
        _player = player;
        _hashtable = hashtable;
    }
    Player _player { get; set; }
    Hashtable _hashtable { get; set; }
    public IEnumerator Hatch()
    {
        if (_player == null) yield break;
        if (!_player.CanHatch) yield break;

        CardSource card = _player.DigitamaLibraryCards[0];

        PlayPermanentClass playPermanentClass = new PlayPermanentClass(
        cardSources: new List<CardSource>() { card },
        hashtable: _hashtable,
        targetPermanent: null,
        isTapped: false,
        root: SelectCardEffect.Root.Library,
        ActivateETB: true);

        playPermanentClass.SetIsBreedingArea();

        yield return ContinuousController.instance.StartCoroutine(playPermanentClass.PlayPermanent());

        if (card.PermanentOfThisCard() != null)
        {
            card.PermanentOfThisCard().EnterFieldTurnCount = -1;
        }
    }
}
#endregion

#region Play permanents

#region Hashtable setting class
public class OnEnterFieldHashtableParams
{
    public OnEnterFieldHashtableParams(
        Permanent permanent,
        List<CardSource> evoRoots,
        List<CardSource> evoRootTops,
        SelectCardEffect.Root root,
        List<int> oldLevels,
        bool isFromDigimonDigivolutionCards)
    {
        Permanent = permanent;

        if (evoRoots != null)
        {
            EvoRoots = evoRoots.Clone();
        }

        if (evoRootTops != null)
        {
            EvoRootTops = evoRootTops.Clone();
        }

        Root = root;

        if (oldLevels != null)
        {
            OldLevels = oldLevels.Clone();
        }

        IsFromDigimonDigivolutionCards = isFromDigimonDigivolutionCards;
    }
    public Permanent Permanent { get; private set; } = null;
    public List<CardSource> EvoRoots = new List<CardSource>();
    public List<CardSource> EvoRootTops { get; private set; } = new List<CardSource>();
    public SelectCardEffect.Root Root { get; private set; } = SelectCardEffect.Root.None;
    public List<int> OldLevels { get; private set; } = new List<int>();
    public bool IsFromDigimonDigivolutionCards { get; private set; } = false;
}
#endregion

public class PlayPermanentClass
{
    public PlayPermanentClass(List<CardSource> cardSources, Hashtable hashtable, Permanent targetPermanent, bool isTapped, SelectCardEffect.Root root, bool ActivateETB)
    {
        if (cardSources != null)
        {
            _cardSources = cardSources.Filter(cardSource => cardSource != null).Clone();
        }

        _hashtable = hashtable;
        _targetPermanent = targetPermanent;
        _isTapped = isTapped;
        _root = root;
        _activateETB = ActivateETB;
    }
    public void SetJogress(int[] jogressEvoRootsFrameIDs)
    {
        if (jogressEvoRootsFrameIDs != null)
        {
            _jogressEvoRootsFrameIDs = jogressEvoRootsFrameIDs.CloneArray();
        }
    }

    public void SetBurstDigivolved()
    {
        _burstDigivolved = true;
    }

    public void SetIsBreedingArea()
    {
        _isBreedingArea = true;
    }

    public void SetISPlayOption()
    {
        _isPlayOption = true;
    }
    List<CardSource> _cardSources = new List<CardSource>();
    Hashtable _hashtable = null;
    Permanent _targetPermanent = null;
    bool _isTapped = false;
    SelectCardEffect.Root _root = SelectCardEffect.Root.None;
    bool _activateETB = true;
    int[] _jogressEvoRootsFrameIDs = null;
    int _digiXrosCount = 0;
    bool _burstDigivolved = false;
    bool _isBreedingArea = false;
    bool _isPlayOption = false;

    public bool isJogress => _jogressEvoRootsFrameIDs != null && _jogressEvoRootsFrameIDs.Length == 2;

    public IEnumerator PlayPermanent()
    {
        List<OnEnterFieldHashtableParams> hashtableParams = new List<OnEnterFieldHashtableParams>();

        ICardEffect CardEffect = CardEffectCommons.GetCardEffectFromHashtable(_hashtable);

        bool isEvolution = false;

        foreach (CardSource card in _cardSources)
        {
            yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.CloseBrainstrorm(card));

            if (GManager.instance.GetComponent<SelectDigiXrosClass>().playCard == card)
            {
                _digiXrosCount = GManager.instance.GetComponent<SelectDigiXrosClass>().selectedDigicrossCards.Count;
            }

            bool isFromDigimonDigivolutionCards = card.Owner.GetBattleAreaDigimons().Some((permanent) => permanent.DigivolutionCards.Contains(card));

            bool isFromSecurity = card.Owner.SecurityCards.Contains(card);

            yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.CloseBrainstrorm(card));

            GManager.instance.turnStateMachine.isSync = true;

            Permanent permanent = null;
            isEvolution = false;
            List<CardSource> evoRoots = new List<CardSource>();
            List<CardSource> evoRootTops = new List<CardSource>();
            bool played = false;
            List<int> oldLevels = new List<int>();

            int frameId = -1;

            if (_targetPermanent == null)
            {
                if (_isBreedingArea)
                {
                    if (card.Owner.GetBreedingAreaPermanents().Count == 0)
                    {
                        FieldCardFrame digieggFrame = card.Owner.fieldCardFrames.Find(fieldCardFrame =>
                        fieldCardFrame.IsEmptyFrame()
                        && !fieldCardFrame.IsBattleAreaFrame()
                        && card.CanPlayCardTargetFrame(fieldCardFrame, false, CardEffect, isBreedingArea: _isBreedingArea));

                        if (digieggFrame != null)
                        {
                            frameId = digieggFrame.FrameID;
                        }
                    }
                }

                else
                {
                    FieldCardFrame preferredFrame = card.PreferredFrame();

                    if (preferredFrame != null)
                    {
                        frameId = preferredFrame.FrameID;
                    }
                }
            }

            else
            {
                frameId = _targetPermanent.PermanentFrame.FrameID;
            }

            if (!isJogress)
            {
                if (!(0 <= frameId && frameId < card.Owner.fieldCardFrames.Count))
                {
                    foreach (FieldCardFrame fieldCardFrame in card.Owner.fieldCardFrames)
                    {
                        if (card.CanPlayCardTargetFrame(fieldCardFrame, false, CardEffect, isBreedingArea: _isBreedingArea))
                        {
                            if (fieldCardFrame.IsEmptyFrame())
                            {
                                frameId = fieldCardFrame.FrameID;
                                break;
                            }
                        }
                    }
                }

                if (0 <= frameId && frameId < card.Owner.fieldCardFrames.Count)
                {
                    played = true;

                    if (_targetPermanent != null)
                    {
                        isEvolution = true;
                    }

                    else
                    {
                        if (!CardEffectCommons.CanPlayAsNewPermanent(card, false, CardEffect, isBreedingArea: _isBreedingArea, isPlayOption: _isPlayOption))
                        {
                            played = false;
                        }
                    }

                    if (played)
                    {
                        card.Init();

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(card));

                        if (isEvolution)
                        {
                            oldLevels.Add(_targetPermanent.Level);

                            GManager.instance.playLog.AddLogString($"\nEvolution:\n{card.BaseENGCardNameFromEntity}({card.CardID})\n");

                            permanent = _targetPermanent;
                            evoRoots.Add(_targetPermanent.TopCard);
                            evoRootTops.Add(_targetPermanent.TopCard);
                            permanent.AddCardSource(card);
                        }

                        else
                        {
                            GManager.instance.playLog.AddLogString($"\nPlay on field:\n{card.BaseENGCardNameFromEntity}({card.CardID})\n");

                            permanent = new Permanent(new List<CardSource>() { card }) { IsSuspended = _isTapped };

                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.CreateNewPermanent(permanent, frameId));
                            permanent.EnterFieldTurnCount = GManager.instance.turnStateMachine.TurnCount;
                        }

                        GManager.instance.turnStateMachine.isSync = true;

                        if (GManager.instance.turnStateMachine.DoneStartGame)
                        {
                            if (!isEvolution)
                            {
                                #region wheter to have ETB effects 
                                Hashtable hashtable = CardEffectCommons.OnEnterFieldHashtable(
                                hashtableParams: new List<OnEnterFieldHashtableParams>()
                                {
                                    new OnEnterFieldHashtableParams(
                                        permanent: permanent,
                                        evoRoots: evoRoots,
                                        evoRootTops: evoRootTops,
                                        root: _root,
                                        oldLevels: oldLevels,
                                        isFromDigimonDigivolutionCards:isFromDigimonDigivolutionCards
                                    )
                                },
                                isEvolution: isEvolution,
                                isJogress: isJogress,
                                digiXrosCount: _digiXrosCount,
                                cardEffect: CardEffect);

                                bool HasETB = permanent.EffectList(EffectTiming.OnEnterFieldAnyone).Some(cardEffect =>
                                cardEffect is ActivateICardEffect && cardEffect.CanUse(hashtable) && _activateETB && cardEffect.IsOnPlay);
                                #endregion

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateFieldPermanentCardEffect(permanent.ShowingPermanentCard, HasETB: HasETB, isDigiXros: _digiXrosCount >= 1));
                            }

                            else
                            {
                                bool isBlast = CardEffect != null && CardEffect.EffectName != null && CardEffect.EffectName.Contains("Blast");
                                //effect
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().DigivolveFieldPermanentCardEffect(permanent.ShowingPermanentCard, _burstDigivolved, isBlast));
                            }
                        }
                    }
                }
            }

            else
            {
                if (_jogressEvoRootsFrameIDs.Length == 2)
                {
                    played = true;

                    card.Init();

                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(card));

                    isEvolution = true;

                    List<Permanent> evoRootPermanents = new List<Permanent>();

                    for (int i = 0; i < _jogressEvoRootsFrameIDs.Length; i++)
                    {
                        int frameID = _jogressEvoRootsFrameIDs[i];
                        Permanent evoRoot = card.Owner.fieldCardFrames[frameID].GetFramePermanent();

                        if (evoRoot != null)
                        {
                            if (evoRoot.TopCard != null)
                            {
                                evoRootPermanents.Add(evoRoot);
                            }
                        }
                    }

                    int targetFrameID = evoRootPermanents[0].PermanentFrame.FrameID;

                    foreach (Permanent evoRootPermanent in evoRootPermanents)
                    {
                        evoRoots.Add(evoRootPermanent.TopCard);
                    }

                    List<CardSource> newDigivolutionCards = evoRootPermanents
                        .Map(evoRootPermanent => evoRootPermanent.cardSources)
                        .Flat();

                    foreach (Permanent evoRootPermanent in evoRootPermanents)
                    {
                        oldLevels.Add(evoRootPermanent.Level);

                        evoRootTops.Add(evoRootPermanent.TopCard);
                    }

                    foreach (Permanent evoRootPermanent in evoRootPermanents)
                    {
                        yield return ContinuousController.instance.StartCoroutine(evoRootPermanent.DiscardEvoRoots(ignoreOverflow: true, putToTrash: false));

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveField(evoRootPermanent, ignoreOverflow: true));
                    }

                    GManager.instance.playLog.AddLogString($"\nJogress:\n{card.BaseENGCardNameFromEntity}({card.CardID})\n");

                    permanent = new Permanent(new List<CardSource>() { card }) { IsSuspended = false };

                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.CreateNewPermanent(permanent, targetFrameID));
                    permanent.EnterFieldTurnCount = -1;

                    newDigivolutionCards.Reverse();

                    yield return ContinuousController.instance.StartCoroutine(permanent.AddDigivolutionCardsTop(newDigivolutionCards, null));

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateFieldPermanentCardEffect(permanent.ShowingPermanentCard, isDigiXros: false, jogressEvoRoots: evoRootTops.ToArray()));

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(evoRootTops, "DNA Digivolution Cards", true, true));
                }
            }

            if (played)
            {
                if (isFromSecurity)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                        player: card.Owner,
                        refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
                }

                if (isEvolution)
                {
                    card.Owner.DigivolveCount_ThisTurn++;
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, null).Draw());

                    if (_burstDigivolved)
                    {
                        if (permanent.TopCard != null)
                        {
                            permanent.IsBurstDigivolved = true;

                            GManager.instance.selectBurstDigivolutionEffect.AddTrashTopCardAtTurnEnd(permanent);
                        }
                    }

                    permanent.CardNamesJustAfterDigivolved = permanent.TopCard.CardNames.Clone();

                    permanent.DigivolvingEffect = CardEffect;
                }

                else
                {
                    permanent.PlayingEffect = CardEffect;

                    if (permanent.TopCard.HasLevel)
                    {
                        permanent.LevelJustAfterPlayed = permanent.Level;
                    }

                    if (permanent.TopCard.HasPlayCost)
                    {
                        permanent.PlayCostJustAfterPlayed = permanent.TopCard.GetCostItself;
                    }

                    permanent.CardNamesJustAfterPlayed = permanent.TopCard.CardNames.Clone();

                    permanent.TraitsJustAfterPlayed = permanent.TopCard.CardTraits.Clone();
                }

                #region move permanents (hybrid)
                if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                {
                    if (permanent.TopCard.PreferredFrame() != null)
                    {
                        if (permanent.PermanentFrame != permanent.TopCard.PreferredFrame())
                        {
                            if (permanent.TopCard.PreferredFrame().IsEmptyFrame())
                            {
                                bool move = false;

                                if (permanent.IsDigimon)
                                {
                                    if (permanent.TopCard.Owner.isYou)
                                    {
                                        if (permanent.TopCard.PreferredFrame().Frame.transform.parent.localPosition.y > permanent.PermanentFrame.Frame.transform.parent.localPosition.y + 5)
                                        {
                                            move = true;
                                        }
                                    }

                                    else
                                    {
                                        if (permanent.TopCard.PreferredFrame().Frame.transform.parent.localPosition.y < permanent.PermanentFrame.Frame.transform.parent.localPosition.y - 5)
                                        {
                                            move = true;
                                        }
                                    }
                                }

                                else if (permanent.IsTamer || permanent.TopCard.IsOption)
                                {
                                    if (permanent.TopCard.Owner.isYou)
                                    {
                                        if (permanent.TopCard.PreferredFrame().Frame.transform.parent.localPosition.y < permanent.PermanentFrame.Frame.transform.parent.localPosition.y - 5)
                                        {
                                            move = true;
                                        }
                                    }

                                    else
                                    {
                                        if (permanent.TopCard.PreferredFrame().Frame.transform.parent.localPosition.y > permanent.PermanentFrame.Frame.transform.parent.localPosition.y + 5)
                                        {
                                            move = true;
                                        }
                                    }
                                }

                                if (move)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.MovePermanent(permanent.PermanentFrame));
                                }
                            }
                        }
                    }
                }
                #endregion

                yield return GManager.instance.GetComponent<SelectDigiXrosClass>().AddDigivolutiuonCardsByEffect(card);
                yield return GManager.instance.GetComponent<SelectDigiXrosClass>().AddDigivolutiuonCards(card);

                if (GManager.instance.turnStateMachine.DoneStartGame)
                {
                    hashtableParams.Add(
                        new OnEnterFieldHashtableParams(
                            permanent: permanent,
                            evoRoots: evoRoots,
                            evoRootTops: evoRootTops,
                            root: _root,
                            oldLevels: oldLevels,
                            isFromDigimonDigivolutionCards: isFromDigimonDigivolutionCards
                            )
                    );
                }
            }

            GManager.instance.GetComponent<SelectDigiXrosClass>().ResetSelectDigiXrosClass();

            yield return GManager.instance.photonWaitController.StartWait("EndPlayPermanent");
        }

        // except [On Play] effect
        bool CardEffectCondition(ICardEffect cardEffect)
        {
            if (cardEffect == null)
            {
                return false;
            }

            if (!_activateETB)
            {
                if (cardEffect is ActivateICardEffect)
                {
                    if (cardEffect.IsOnPlay)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        Hashtable effectHashtable = CardEffectCommons.OnEnterFieldHashtable(
            hashtableParams: hashtableParams,
            isEvolution: isEvolution,
            isJogress: isJogress,
            digiXrosCount: _digiXrosCount,
            cardEffect: CardEffect);

        #region "When permanents are played" effect
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(
            hashtable: effectHashtable,
            timing: EffectTiming.OnEnterFieldAnyone,
            cardEffectCondition: CardEffectCondition));
        #endregion
    }
}
#endregion

#region Use option
public class UseOptionClass
{
    public UseOptionClass(List<CardSource> cardSources, Hashtable hashtable, SelectCardEffect.Root root)
    {
        if (cardSources != null)
        {
            _cardSources = cardSources.Filter(cardSource => cardSource != null).Clone();
        }

        _hashtable = hashtable;
        _root = root;
    }
    List<CardSource> _cardSources = new List<CardSource>();
    SelectCardEffect.Root _root = SelectCardEffect.Root.None;
    Hashtable _hashtable = null;
    public bool _showEffect = false;
    public bool _addSecurityEndOption = false;

    public IEnumerator UseOption()
    {
        ICardEffect CardEffect = CardEffectCommons.GetCardEffectFromHashtable(_hashtable);

        foreach (CardSource card in _cardSources)
        {
            if (_showEffect)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().DeleteHandCardEffectCoroutine(card));

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowUseHandCardEffect_PlayCard(card));

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().MoveToExecuteCardEffect(card));
            }

            GManager.instance.playLog.AddLogString($"\nPlay Option:\n{card.BaseENGCardNameFromEntity}({card.CardID})\n");

            card.Init();

            GManager.instance.turnStateMachine.isSync = true;

            card.SetFace();

            int cost = card.GetCostItself;

            //Remove from all areas
            yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(card));

            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddExecutingCard(card));

            yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.BrainStormCoroutine(card));

            #region Set HashTable
            Hashtable hashtable = new Hashtable()
        {
            {"Card", card},
            {"Root", _root},
            {"Cost", cost},
        };
            #endregion

            #region "When option is used" effect

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable, EffectTiming.OnUseOption));

            yield return ContinuousController.instance.StartCoroutine(AutoProcessing.ActivateBackgroundEffects(hashtable, EffectTiming.OnUseOption));
            #endregion

            #region effect process of option card
            foreach (ICardEffect cardEffect in card.EffectList(EffectTiming.OptionSkill))
            {
                if (cardEffect is ActivateICardEffect)
                {
                    if (cardEffect.CanUse(hashtable))
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.ActivateEffectProcess(
                                                    cardEffect,
                                                    hashtable));
                    }
                }
            }
            #endregion

            if (card.Owner.ExecutingCards.Contains(card))
            {
                if (_addSecurityEndOption && card.Owner.CanAddSecurity(CardEffect))
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(card));

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateRecoveryEffect(card.Owner));

                    yield return ContinuousController.instance.StartCoroutine(new IAddSecurity(card.Owner).AddSecurity());
                }

                else
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(card));
                }
            }

            yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.CloseBrainstrorm(card));

            yield return GManager.instance.photonWaitController.StartWait("EndPlayOption");
        }
    }
}
#endregion

#region Draw cards
public class DrawClass
{
    public DrawClass(Player player, int drawCount, ICardEffect cardEffect)
    {
        _player = player;
        _drawCount = drawCount;
        _cardEffect = cardEffect;
    }

    Player _player = null;
    int _drawCount = 0;
    ICardEffect _cardEffect = null;

    public IEnumerator Draw()
    {
        if (_drawCount <= 0) yield break;
        if (_player.LibraryCards.Count <= 0) yield break;

        List<CardSource> DrawCards = new List<CardSource>();

        for (int i = 0; i < _drawCount; i++)
        {
            if (_player.LibraryCards.Count > 0)
            {
                CardSource DrawCard = _player.LibraryCards[0];

                yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(DrawCard));

                DrawCards.Add(DrawCard);
            }
        }

        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(DrawCards, true, _cardEffect));

        #region add log
        if (DrawCards.Count >= 1)
        {
            GManager.instance.playLog.AddLogString($"\nDraw {DrawCards.Count} card{Utils.PluralFormSuffix(DrawCards.Count)}\n{_player.PlayerName}\n");
        }
        #endregion

        #region "When draw cards" effect
        if (DrawCards.Count >= 1)
        {
            #region Hashtable setting
            Hashtable _hashtable = new Hashtable()
            {
                {"Player", _player},
                {"CardEffect", _cardEffect}
            };
            #endregion

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(_hashtable, EffectTiming.OnDraw));
        }
        #endregion
    }
}
#endregion

#region Trash cards from deck too
public class IAddTrashCardsFromLibraryTop
{
    public IAddTrashCardsFromLibraryTop(int addTrashCount, Player player, ICardEffect cardEffect)
    {
        _addTrashCount = addTrashCount;
        _player = player;
        _cardEffect = cardEffect;
    }

    public void SetNotShowCards()
    {
        _notShowCards = true;
    }

    int _addTrashCount;
    Player _player;
    public List<CardSource> discardedCards = new List<CardSource>();
    ICardEffect _cardEffect { get; set; } = null;
    bool _notShowCards = false;

    public IEnumerator AddTrashCardsFromLibraryTop()
    {
        if (_addTrashCount <= 0) yield break;
        if (_player.LibraryCards.Count == 0) yield break;

        string log = "";

        log += $"\nTrash cards from deck :";

        for (int i = 0; i < _addTrashCount; i++)
        {
            if (_player.LibraryCards.Count > i)
            {
                CardSource DiscardCard = _player.LibraryCards[i];

                discardedCards.Add(DiscardCard);

                log += $"\n{DiscardCard.BaseENGCardNameFromEntity}({DiscardCard.CardID})";
            }
        }

        if (discardedCards.Count >= 1)
        {
            if (!_notShowCards)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(discardedCards, "Discarded Cards", true, true));
            }
        }

        yield return ContinuousController.instance.StartCoroutine(new ITrashDeckCards(discardedCards, _cardEffect).TrashDeckCards());

        if (discardedCards.Count >= 1)
        {
            for (int i = 0; i < discardedCards.Count; i++)
            {
                ContinuousController.instance.PlaySE(GManager.instance.DrawSE);
                yield return new WaitForSeconds(0.06f);
            }

            log += "\n";

            GManager.instance.playLog.AddLogString(log);
        }
    }
}
#endregion

#region Add security from deck top
public class IAddSecurityFromLibrary
{
    public IAddSecurityFromLibrary(Player player, int addLifeCount)
    {
        _player = player;
        _addSecurityCount = addLifeCount;
    }

    Player _player = null;
    int _addSecurityCount = 0;

    public IEnumerator AddSecurity()
    {
        int count = 0;

        for (int i = 0; i < _addSecurityCount; i++)
        {
            if (_player.LibraryCards.Count > 0)
            {
                CardSource StockCard = _player.LibraryCards[0];

                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(StockCard));

                count++;
            }
        }

        if (count > 0)
        {
            string log = "";

            log += $"\nAdd {count} cards to security:{_player.PlayerName}";

            log += "\n";

            GManager.instance.playLog.AddLogString(log);

            if (GManager.instance.turnStateMachine.DoneStartGame)
            {
                yield return ContinuousController.instance.StartCoroutine(new IAddSecurity(_player).AddSecurity());
            }
        }
    }
}
#endregion

#region Recovery
public class IRecovery
{
    public IRecovery(Player player, int AddLifeCount, ICardEffect cardEffect)
    {
        _player = player;
        _addLifeCount = AddLifeCount;
        _cardEffect = cardEffect;
    }
    Player _player = null;
    int _addLifeCount = 0;
    ICardEffect _cardEffect = null;

    public IEnumerator Recovery()
    {
        if (_player.LibraryCards.Count == 0) yield break;
        if (_addLifeCount <= 0) yield break;
        if (!_player.CanAddSecurity(_cardEffect)) yield break;

        yield return ContinuousController.instance.StartCoroutine(new IAddSecurityFromLibrary(_player, _addLifeCount).AddSecurity());

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateRecoveryEffect(_player));
    }
}
#endregion

#region Digi-Burst
public class IDigiBurst
{
    public IDigiBurst(Permanent permanent, int DigiBurstCount, ICardEffect cardEffect)
    {
        _permanent = permanent;
        _digiBurstCount = DigiBurstCount;
        _cardEffect = cardEffect;
    }
    Permanent _permanent = null;
    int _digiBurstCount = 0;
    ICardEffect _cardEffect = null;
    bool _upToMaxCount = false;

    public List<CardSource> discardedCards = new List<CardSource>();

    public void SetUpToMaxCount()
    {
        _upToMaxCount = true;
    }

    public bool CanDigiBurst()
    {
        if (_permanent != null)
        {
            if (_permanent.TopCard != null)
            {
                if (_upToMaxCount)
                {
                    if (_permanent.DigivolutionCards.Some((cardSource) => !cardSource.CanNotTrashFromDigivolutionCards(_cardEffect)))
                    {
                        return true;
                    }
                }

                else
                {
                    if (_permanent.DigivolutionCards.Count((cardSource) => !cardSource.CanNotTrashFromDigivolutionCards(_cardEffect)) >= _digiBurstCount)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public IEnumerator DigiBurst()
    {
        if (CanDigiBurst())
        {
            discardedCards = new List<CardSource>();

            List<CardSource> selectedCards = new List<CardSource>();

            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

            selectCardEffect.SetUp(
                        canTargetCondition: (cardSource) => !cardSource.CanNotTrashFromDigivolutionCards(_cardEffect),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: CanEndSelectCondition,
                        canNoSelect: () => false,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select digivolution cards to discard.",
                        maxCount: _digiBurstCount,
                        canEndNotMax: _upToMaxCount,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Custom,
                        customRootCardList: _permanent.DigivolutionCards,
                        canLookReverseCard: true,
                        selectPlayer: _permanent.TopCard.Owner,
                        cardEffect: null);

            selectCardEffect.SetUpCustomMessage("Select digivolution cards to discard.", "The opponent is selecting digivolution cards to discard.");

            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

            bool CanEndSelectCondition(List<CardSource> cardSources)
            {
                if (CardEffectCommons.HasNoElement(cardSources))
                {
                    return false;
                }

                return true;
            }

            IEnumerator SelectCardCoroutine(CardSource cardSource)
            {
                selectedCards.Add(cardSource);

                yield return null;
            }

            if (selectedCards.Count >= 1)
            {
                #region "When use Digi-Burst" effect

                #region Hashtable setting
                Hashtable hashtable = new Hashtable()
                {
                    {"Permanent", _permanent},
                    {"CardEffect", _cardEffect},
                };
                #endregion

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable, EffectTiming.OnUseDigiburst));
                #endregion

                // trash digivolution cards
                yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(_permanent, selectedCards, _cardEffect).TrashDigivolutionCards());

                foreach (CardSource cardSource in selectedCards)
                {
                    discardedCards.Add(cardSource);
                }

                #region add log
                if (selectedCards.Count >= 1)
                {
                    string log = "";

                    log += $"\nDigiburst :";

                    foreach (CardSource cardSource in selectedCards)
                    {
                        if (cardSource != null)
                        {
                            log += $"\n{cardSource.BaseENGCardNameFromEntity}({cardSource.CardID})";
                        }
                    }

                    log += "\n";

                    GManager.instance.playLog.AddLogString(log);
                }
                #endregion
            }
        }
    }
}
#endregion

#region Place permanents to deck bottom

public class DeckBottomBounceClass
{
    public DeckBottomBounceClass(List<Permanent> deckBounceTargetPermanents, Hashtable hashtable)
    {
        _deckBounceTargetPermanents = deckBounceTargetPermanents.Clone();

        _hashtable = hashtable;
    }

    public void SetNotShowCards()
    {
        _notShowCards = true;
    }
    public bool IsDeckBounced(Permanent permanent)
    {
        return DeckBouncedPermanents.Contains(permanent);
    }
    List<Permanent> _deckBounceTargetPermanents = new List<Permanent>();
    public List<Permanent> DeckBouncedPermanents { get; private set; } = new List<Permanent>();
    Hashtable _hashtable = new Hashtable();
    bool _notShowCards = false;
    public IEnumerator DeckBounce()
    {
        if (_deckBounceTargetPermanents == null) yield break;

        ICardEffect cardEffect = CardEffectCommons.GetCardEffectFromHashtable(_hashtable);

        _deckBounceTargetPermanents = _deckBounceTargetPermanents.Filter(permanent =>
        permanent != null
        && permanent.TopCard != null
        && (cardEffect == null ||
        (!permanent.TopCard.CanNotBeAffected(cardEffect) && !permanent.CannotReturnToLibrary(cardEffect))));

        if (_deckBounceTargetPermanents.Count == 0) yield break;

        _deckBounceTargetPermanents.ForEach(permanent => permanent.willBeRemoveField = true);

        #region cut in effect

        // "When permanents would return to deck" effect

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.StackSkillInfos(
            CardEffectCommons.WhenPermanentWouldRemoveFieldCheckHashtable(
                _deckBounceTargetPermanents,
                cardEffect,
                null
            ),
            EffectTiming.WhenReturntoLibraryAnyone));

        // "When permanents would remove field" effect

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.StackSkillInfos(
            CardEffectCommons.WhenPermanentWouldRemoveFieldCheckHashtable(
                _deckBounceTargetPermanents,
                cardEffect,
                null
            ),
            EffectTiming.WhenRemoveField));

        if (GManager.instance.autoProcessing_CutIn.HasAwaitingActivateEffects())
        {
            foreach (Permanent permanent in _deckBounceTargetPermanents)
            {
                permanent.ShowDeckBounceEffect();
            }

            // cut in effect process
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.TriggeredSkillProcess(false, AutoProcessing.HasExecutedSameEffect));

            foreach (Permanent permanent in _deckBounceTargetPermanents)
            {
                permanent.HideDeckBounceEffect();
            }
        }

        #endregion

        // fix deck bounce target permanents
        List<Permanent> deckBounceTargetPermanents_Fixed = _deckBounceTargetPermanents.Filter(permanent =>
        permanent != null
        && permanent.TopCard != null
        && permanent.willBeRemoveField);

        #region show cards
        List<CardSource> returnedCards = deckBounceTargetPermanents_Fixed.Map(permanent => permanent.TopCard);

        if (!_notShowCards)
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(returnedCards, "Deck bottom cards", true, true));
        }
        #endregion

        #region 
        List<CardSource> deckBottomCards = new List<CardSource>();

        foreach (Permanent permanent in deckBounceTargetPermanents_Fixed)
        {
            #region recoed used effect
            if (permanent.TopCard != null)
            {
                permanent.LibraryBounceEffect = cardEffect;
            }
            #endregion

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().DeckBounceEffect(permanent));

            yield return ContinuousController.instance.StartCoroutine(permanent.DiscardEvoRoots());

            CardSource topCard = permanent.TopCard;

            yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveField(permanent));

            deckBottomCards.Add(topCard);

            DeckBouncedPermanents.Add(permanent);
        }

        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(deckBottomCards));
        #endregion

        #region off icon
        foreach (Permanent permanent in _deckBounceTargetPermanents)
        {
            if (permanent != null)
            {
                if (permanent.TopCard != null)
                {
                    permanent.willBeRemoveField = false;
                }
            }
        }
        #endregion
    }
}
#endregion

#region Return permanent to hand
public class HandBounceClaass
{
    public HandBounceClaass(List<Permanent> bounceTargetPermanents, Hashtable hashtable)
    {
        _bounceTargetPermanents = bounceTargetPermanents.Clone().Filter(CardEffectCommons.IsPermanentExistsOnBattleArea);

        _hashtable = hashtable;
    }
    public bool IsBounced(Permanent permanent)
    {
        return BouncedPermanents.Contains(permanent);
    }
    List<Permanent> _bounceTargetPermanents = new List<Permanent>();
    public List<Permanent> BouncedPermanents { get; private set; } = new List<Permanent>();
    Hashtable _hashtable = new Hashtable();
    bool _notShowCards = false;
    public IEnumerator Bounce()
    {
        if (_bounceTargetPermanents == null) yield break;

        ICardEffect cardEffect = CardEffectCommons.GetCardEffectFromHashtable(_hashtable);

        _bounceTargetPermanents = _bounceTargetPermanents.Filter(permanent =>
        permanent != null
        && permanent.TopCard != null
        && (cardEffect == null ||
        (!permanent.TopCard.CanNotBeAffected(cardEffect) && !permanent.CannotReturnToHand(cardEffect))));

        if (_bounceTargetPermanents.Count == 0) yield break;

        _bounceTargetPermanents.ForEach(permanent => permanent.willBeRemoveField = true);

        #region cut in effect

        // "When permanents would return to hand" effect

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.StackSkillInfos(
            CardEffectCommons.WhenPermanentWouldRemoveFieldCheckHashtable(
                _bounceTargetPermanents,
                cardEffect,
                null
            ),
            EffectTiming.WhenReturntoHandAnyone));

        // "When permanents would remove field" effect

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.StackSkillInfos(
            CardEffectCommons.WhenPermanentWouldRemoveFieldCheckHashtable(
                _bounceTargetPermanents,
                cardEffect,
                null
            ),
            EffectTiming.WhenRemoveField));

        if (GManager.instance.autoProcessing_CutIn.HasAwaitingActivateEffects())
        {
            foreach (Permanent permanent in _bounceTargetPermanents)
            {
                permanent.ShowHandBounceEffect();
            }

            // shrink security Digimon display
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.ShrinkSecurityDigimonDisplay());

            // cut in effect process
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.TriggeredSkillProcess(false, AutoProcessing.HasExecutedSameEffect));

            foreach (Permanent permanent in _bounceTargetPermanents)
            {
                permanent.HideHandBounceEffect();
            }
        }
        #endregion

        // fix bounce target permanents
        List<Permanent> bounceTargetPermanents_Fixed = _bounceTargetPermanents.Filter(permanent =>
        permanent != null
        && permanent.TopCard != null
        && permanent.willBeRemoveField);

        #region "When permanents returned to hand" effect
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing
            .StackSkillInfos(CardEffectCommons.OnDeletionHashtable(
                bounceTargetPermanents_Fixed,
                cardEffect,
                null,
                false
                ),
                EffectTiming.OnPermamemtReturnedToHand));
        #endregion

        #region record parameter just before bounce
        foreach (Permanent permanent in bounceTargetPermanents_Fixed)
        {
            permanent.DPJustBeforeRemoveField = permanent.DP;

            if (permanent.TopCard.HasLevel)
            {
                permanent.LevelJustBeforeRemoveField = permanent.Level;
            }

            if (permanent.TopCard.HasPlayCost)
            {
                permanent.CostJustBeforeRemoveField = permanent.TopCard.GetCostItself;
            }

            permanent.CardNamesJustBeforeRemoveField = new List<string>(permanent.TopCard.CardNames);

            foreach (CardSource cardSource in permanent.cardSources)
            {
                cardSource.PermanentJustBeforeRemoveField = permanent;
            }
        }
        #endregion

        #region add log
        if (bounceTargetPermanents_Fixed.Count >= 1)
        {
            string log = "";

            log += $"\nReturn to hand:";

            foreach (Permanent permanent in bounceTargetPermanents_Fixed)
            {
                log += $"\n{permanent.TopCard.BaseENGCardNameFromEntity}({permanent.TopCard.CardID})";
            }

            log += "\n";

            GManager.instance.playLog.AddLogString(log);
        }
        #endregion

        #region show cards
        List<CardSource> returnedCards = bounceTargetPermanents_Fixed.Map(permanent => permanent.TopCard);

        if (!_notShowCards)
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(returnedCards, "Cards returned to hand", true, true));
        }
        #endregion

        #region return permanent cards to hand
        List<CardSource> handCards = new List<CardSource>();

        foreach (Permanent permanent in bounceTargetPermanents_Fixed)
        {
            #region record used effect
            if (permanent.TopCard != null)
            {
                permanent.HandBounceEffect = cardEffect;
            }
            #endregion

            // record whether to return to hand by Burst Digivolution
            permanent.IsReturnedToHandByBurstDigivolution = CardEffectCommons.IsBurst(_hashtable);

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().BounceEffect(permanent, !permanent.IsReturnedToHandByBurstDigivolution));

            yield return ContinuousController.instance.StartCoroutine(permanent.DiscardEvoRoots());

            CardSource topCard = permanent.TopCard;

            yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveField(permanent));

            yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(topCard));

            if (!topCard.IsDigiEgg)
            {
                handCards.Add(topCard);

                BouncedPermanents.Add(permanent);
            }
            else
            {
                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(new List<CardSource>() { topCard }));
            }
            
        }

        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(handCards, false, cardEffect));
        #endregion

        #region hide icon
        foreach (Permanent permanent in _bounceTargetPermanents)
        {
            if (permanent != null)
            {
                if (permanent.TopCard != null)
                {
                    permanent.willBeRemoveField = false;
                }
            }
        }
        #endregion
    }
}
#endregion

#region Place permanent to another permanent's digivolution cards
public class IPlacePermanentToDigivolutionCards
{
    public IPlacePermanentToDigivolutionCards(
        List<Permanent[]> permanentArrays,
        bool toTop,
        ICardEffect cardEffect,
        bool skipEffectAndActivateSkill = false)
    {
        permanentArrays.Clone().ForEach(permanentArray => _permanentArrays.Add(permanentArray.CloneArray()));
        _toTop = toTop;
        _cardEffect = cardEffect;
        _skipEffectAndActivateSkill = skipEffectAndActivateSkill;
    }

    public void SetNotShowCards()
    {
        _notShowCards = true;
    }

    List<Permanent[]> _permanentArrays = new List<Permanent[]>();
    ICardEffect _cardEffect = null;
    bool _toTop = false;
    bool _notShowCards = false;
    bool _skipEffectAndActivateSkill = false;
    public bool Placed { get; private set; } = false;
    public IEnumerator PlacePermanentToDigivolutionCards()
    {
        if (_permanentArrays.Count == 0)
        {
            yield break;
        }

        List<CardSource> addedDigivolutionCards = new List<CardSource>();

        List<Permanent> removeFieldPermanents = new List<Permanent>();

        foreach (Permanent[] permanentArray in _permanentArrays)
        {
            if (permanentArray.Length == 2)
            {
                Permanent DigivolutionPermanent = permanentArray[0];
                Permanent getDigivolutionPermanent = permanentArray[1];

                if (DigivolutionPermanent != null && getDigivolutionPermanent != null)
                {
                    if (DigivolutionPermanent.TopCard != null && getDigivolutionPermanent.TopCard != null && !getDigivolutionPermanent.IsToken)
                    {
                        if (_cardEffect != null)
                        {
                            if (DigivolutionPermanent.TopCard.CanNotBeAffected(_cardEffect) || getDigivolutionPermanent.TopCard.CanNotBeAffected(_cardEffect))
                            {
                                continue;
                            }
                        }

                        removeFieldPermanents.Add(DigivolutionPermanent);
                    }
                }
            }
        }

        foreach (Permanent permanent in removeFieldPermanents)
        {
            permanent.willBeRemoveField = true;
        }

        if (removeFieldPermanents.Count == 0)
        {
            yield break;
        }

        #region 
        List<SkillInfo> skillInfos = new List<SkillInfo>();

        Hashtable hashtable1 =
        CardEffectCommons.WhenPermanentWouldRemoveFieldCheckHashtable(
                removeFieldPermanents,
                _cardEffect,
                null);

        #region  "When permanents would remove field" effect

        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            #region permanents' effects
            foreach (Permanent permanent1 in player.GetFieldPermanents())
            {
                foreach (ICardEffect cardEffect in permanent1.EffectList(EffectTiming.WhenRemoveField))
                {
                    if (cardEffect is ActivateICardEffect)
                    {
                        if (cardEffect.CanTrigger(hashtable1))
                        {
                            skillInfos.Add(new SkillInfo(cardEffect, hashtable1, EffectTiming.WhenRemoveField));
                        }
                    }
                }
            }
            #endregion

            #region players' effects
            foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.WhenRemoveField))
            {
                if (cardEffect is ActivateICardEffect)
                {
                    if (cardEffect.CanTrigger(hashtable1))
                    {
                        skillInfos.Add(new SkillInfo(cardEffect, hashtable1, EffectTiming.WhenRemoveField));
                    }
                }
            }
            #endregion
        }
        #endregion

        if (skillInfos.Count >= 1)
        {
            foreach (SkillInfo skillInfo in skillInfos)
            {
                GManager.instance.autoProcessing_CutIn.PutStackedSkill(skillInfo);
            }

            bool showEffect()
            {
                if (skillInfos.Count >= 2)
                {
                    return true;
                }

                else if (skillInfos.Count == 1)
                {
                    if (skillInfos[0].CardEffect.CanActivate(skillInfos[0].Hashtable))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (showEffect())
            {
                foreach (Permanent Permanent in removeFieldPermanents)
                {
                    if (Permanent != null)
                    {
                        if (Permanent.TopCard != null)
                        {
                            if (Permanent.willBeRemoveField)
                            {
                                if (Permanent.ShowingPermanentCard != null)
                                {
                                    if (Permanent.ShowingPermanentCard.WillRemoveFieldObject != null)
                                    {
                                        Permanent.ShowingPermanentCard.WillRemoveFieldObject.transform.parent.gameObject.SetActive(true);
                                        Permanent.ShowingPermanentCard.WillRemoveFieldObject.SetActive(true);
                                    }
                                }
                            }
                        }
                    }
                }

                #region shrink security Digimon display
                if (GManager.instance.attackProcess.IsAttacking)
                {
                    if (GManager.instance.attackProcess.SecurityDigimon != null)
                    {
                        if (GManager.instance.GetComponent<Effects>().ShowUseHandCard.gameObject.activeSelf && GManager.instance.GetComponent<Effects>().ShowUseHandCard.cardSource == GManager.instance.attackProcess.SecurityDigimon)
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().MoveToExecuteCardEffect(GManager.instance.attackProcess.SecurityDigimon));
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SecurityDigimon.Owner.brainStormObject.BrainStormCoroutine(GManager.instance.attackProcess.SecurityDigimon));
                        }
                    }
                }
                #endregion
            }

            // cut in effect process
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.TriggeredSkillProcess(false, AutoProcessing.HasExecutedSameEffect));

            foreach (Permanent Permanent in removeFieldPermanents)
            {
                if (Permanent != null)
                {
                    if (Permanent.TopCard != null)
                    {
                        if (Permanent.ShowingPermanentCard != null)
                        {
                            if (Permanent.ShowingPermanentCard.WillRemoveFieldObject != null)
                            {
                                Permanent.ShowingPermanentCard.WillRemoveFieldObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        foreach (Permanent[] permanentArray in _permanentArrays)
        {
            if (permanentArray.Length == 2)
            {
                Permanent DigivolutionPermanent = permanentArray[0];
                Permanent getDigivolutionPermanent = permanentArray[1];

                if (DigivolutionPermanent != null && getDigivolutionPermanent != null)
                {
                    if (DigivolutionPermanent.TopCard != null && getDigivolutionPermanent.TopCard != null && !getDigivolutionPermanent.IsToken && DigivolutionPermanent.willBeRemoveField)
                    {
                        yield return ContinuousController.instance.StartCoroutine(DigivolutionPermanent.DiscardEvoRoots());

                        CardSource cardSource = DigivolutionPermanent.TopCard;

                        #region record used effect
                        if (_cardEffect != null)
                        {
                            if (DigivolutionPermanent.TopCard != null)
                            {
                                DigivolutionPermanent.PlaceOtherPermanentEffect = _cardEffect;
                            }
                        }
                        #endregion

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveField(
                            DigivolutionPermanent,
                            ignoreOverflow: true));

                        List<CardSource> digivolutionCards = new List<CardSource>() { cardSource };

                        if (_toTop)
                        {
                            yield return ContinuousController.instance.StartCoroutine(getDigivolutionPermanent.AddDigivolutionCardsTop(digivolutionCards, _cardEffect));
                        }

                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(getDigivolutionPermanent.AddDigivolutionCardsBottom(digivolutionCards, _cardEffect, _skipEffectAndActivateSkill));
                        }

                        addedDigivolutionCards.Add(cardSource);

                        Placed = true;
                    }
                }
            }
        }

        #region add log
        if (addedDigivolutionCards.Count >= 1)
        {
            string log = "";

            log += $"\nPlace in digivolution cards:";

            foreach (CardSource cardSource in addedDigivolutionCards)
            {
                log += $"\n{cardSource.BaseENGCardNameFromEntity}({cardSource.CardID})";
            }

            log += "\n";

            GManager.instance.playLog.AddLogString(log);
        }
        #endregion

        #region hide icon
        if (addedDigivolutionCards.Count >= 1)
        {
            if (!_notShowCards)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(addedDigivolutionCards, "Digivolution Cards", true, true));
            }
        }

        #endregion
    }
}
#endregion

#region Place permanents to security

public class IPutSecurityPermanent
{
    public IPutSecurityPermanent(Permanent permanent, Hashtable hashtable, bool toTop)
    {
        _permanent = permanent;
        _hashtable = hashtable;
        _toTop = toTop;
    }
    Permanent _permanent = null;
    Hashtable _hashtable = new Hashtable();
    bool _toTop = false;
    public IEnumerator PutSecurity()
    {
        if (_permanent == null) yield break;
        if (_permanent.TopCard == null) yield break;

        ICardEffect cardEffect = CardEffectCommons.GetCardEffectFromHashtable(_hashtable);

        if (_permanent.TopCard.CanNotBeAffected(cardEffect) || !_permanent.TopCard.Owner.CanAddSecurity(cardEffect)) yield break;

        #region "When permanents would remove field" effect

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.StackSkillInfos(
            CardEffectCommons.WhenPermanentWouldRemoveFieldCheckHashtable(
                new List<Permanent>() { _permanent },
                cardEffect,
                null
            ),
            EffectTiming.WhenRemoveField));

        if (GManager.instance.autoProcessing_CutIn.HasAwaitingActivateEffects())
        {
            _permanent.ShowWillRemoveFieldEffect();

            //effect
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.ShrinkSecurityDigimonDisplay());

            // cut in effect process
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.TriggeredSkillProcess(false, AutoProcessing.HasExecutedSameEffect));

            _permanent.HideWillRemoveFieldEffect();
        }
        #endregion

        CardSource topCard = _permanent.TopCard;
        if (topCard == null) yield break;

        #region add log
        string log = "";

        log += $"\nPut cards on top of security:";

        log += $"\n{_permanent.TopCard.BaseENGCardNameFromEntity}({_permanent.TopCard.CardID})";

        log += "\n";

        GManager.instance.playLog.AddLogString(log);
        #endregion

        #region show cards
        if (!topCard.IsToken)
        {
            if (_toTop)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { topCard }, "Security Top Card", true, true));
            }

            else
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { topCard }, "Security Bottom Card", true, true));
            }
        }
        #endregion

        #region place permanent to security
        yield return ContinuousController.instance.StartCoroutine(_permanent.DiscardEvoRoots());

        yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveField(_permanent));

        if (!topCard.IsToken)
        {
            if (!topCard.IsDigiEgg)
            {
                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(topCard));

                if (!_toTop)
                {
                    topCard.Owner.SecurityCards.Remove(topCard);
                    topCard.Owner.SecurityCards.Add(topCard);
                }

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateRecoveryEffect(topCard.Owner));

                yield return ContinuousController.instance.StartCoroutine(new IAddSecurity(topCard.Owner).AddSecurity());
            }
            else
            {
                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(new List<CardSource>() { topCard }));
            }
            
        }

        #endregion
    }
}
#endregion

#region Delete permanents
public class DestroyPermanentsClass
{
    public DestroyPermanentsClass(List<Permanent> destroyTargetPermanents, Hashtable hashtable, bool notShowCards = false)
    {
        _destroytargetPermanents = destroyTargetPermanents.Clone();
        _hashtable = hashtable;
        _notShowCards = notShowCards;
    }
    public bool IsDestroyed(Permanent permanent)
    {
        return DestroyedPermanents.Contains(permanent);
    }
    List<Permanent> _destroytargetPermanents = new List<Permanent>();
    public List<Permanent> DestroyedPermanents { get; private set; } = new List<Permanent>();
    Hashtable _hashtable = null;
    bool _notShowCards = false;

    public IEnumerator Destroy()
    {
        if (_destroytargetPermanents == null) yield break;

        ICardEffect cardEffect = CardEffectCommons.GetCardEffectFromHashtable(_hashtable);
        IBattle battle = CardEffectCommons.GetBattleFromHashtable(_hashtable);
        bool isDPZero = CardEffectCommons.IsDPZeroDelete(_hashtable);

        _destroytargetPermanents = _destroytargetPermanents.Filter(permanent =>
        permanent != null
        && permanent.TopCard != null
        && (cardEffect == null ||
        (!permanent.TopCard.CanNotBeAffected(cardEffect) && permanent.CanBeDestroyedBySkill(cardEffect))));

        if (_destroytargetPermanents.Count == 0) yield break;

        _destroytargetPermanents.ForEach(permanent => permanent.willBeRemoveField = true);

        #region cut in effect

        // "When permanents would be deleted" effect

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.StackSkillInfos(
            CardEffectCommons.WhenPermanentWouldRemoveFieldCheckHashtable(
                _destroytargetPermanents,
                cardEffect,
                battle
            ),
            EffectTiming.WhenPermanentWouldBeDeleted));

        // "When permanents would remove field" effect

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.StackSkillInfos(
            CardEffectCommons.WhenPermanentWouldRemoveFieldCheckHashtable(
                _destroytargetPermanents,
                cardEffect,
                battle
            ),
            EffectTiming.WhenRemoveField));

        if (GManager.instance.autoProcessing_CutIn.HasAwaitingActivateEffects())
        {
            foreach (Permanent permanent in _destroytargetPermanents)
            {
                permanent.ShowDeleteEffect();
            }

            // effect
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.ShrinkSecurityDigimonDisplay());

            // cut in effect process
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.TriggeredSkillProcess(false, AutoProcessing.HasExecutedSameEffect));

            foreach (Permanent permanent in _destroytargetPermanents)
            {
                permanent.HideDeleteEffect();
            }
        }
        #endregion

        // fix delete target permanents
        List<Permanent> destroyTargetPermanents_Fixed = _destroytargetPermanents.Filter(permanent =>
            permanent != null
            && permanent.TopCard != null
            && permanent.willBeRemoveField);

        #region "When permanents are deleted" effect
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing
            .StackSkillInfos(CardEffectCommons.OnDeletionHashtable(
                destroyTargetPermanents_Fixed,
                cardEffect,
                battle,
                isDPZero
                ),
                EffectTiming.OnDestroyedAnyone));

        #endregion

        #region record parameters just before deletion
        foreach (Permanent permanent in destroyTargetPermanents_Fixed)
        {
            permanent.DPJustBeforeRemoveField = permanent.DP;

            if (permanent.TopCard.HasLevel)
            {
                permanent.LevelJustBeforeRemoveField = permanent.Level;
            }

            if (permanent.TopCard.HasPlayCost)
            {
                permanent.CostJustBeforeRemoveField = permanent.TopCard.GetCostItself;
            }

            permanent.CardNamesJustBeforeRemoveField = new List<string>(permanent.TopCard.CardNames);

            foreach (CardSource cardSource in permanent.cardSources)
            {
                cardSource.PermanentJustBeforeRemoveField = permanent;
            }
        }
        #endregion

        #region add log
        if (destroyTargetPermanents_Fixed.Count >= 1)
        {
            string log = "";

            log += $"\nDelete:";

            foreach (Permanent permanent in destroyTargetPermanents_Fixed)
            {
                log += $"\n{permanent.TopCard.BaseENGCardNameFromEntity}({permanent.TopCard.CardID})";
            }

            log += "\n";

            GManager.instance.playLog.AddLogString(log);
        }
        #endregion

        #region show cards
        List<CardSource> destroyedCards = destroyTargetPermanents_Fixed.Map(permanent => permanent.TopCard);

        if (!_notShowCards)
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(destroyedCards, "Deleted cards", true, true));
        }
        #endregion

        #region trash permanent cards

        foreach (Permanent permanent in destroyTargetPermanents_Fixed)
        {
            Debug.Log($"DESTORYED PERMANENT: {permanent.TopCard.BaseENGCardNameFromEntity}");
            #region record wheter to be deleted by battle
            if (battle != null)
            {
                if (permanent.TopCard != null)
                {
                    permanent.IsDestroyedByBattle = true;
                }
            }
            #endregion

            #region record used effect
            if (permanent.TopCard != null)
            {
                permanent.DestroyingEffect = cardEffect;
            }
            #endregion

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().DestroyPermanentEffect(permanent));

            yield return ContinuousController.instance.StartCoroutine(permanent.DiscardEvoRoots());

            CardSource topCard = permanent.TopCard;

            yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveField(permanent));

            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(topCard));

            DestroyedPermanents.Add(permanent);
        }
        #endregion

        #region hide icon
        foreach (Permanent permanent in _destroytargetPermanents)
        {
            if (permanent != null)
            {
                if (permanent.TopCard != null)
                {
                    permanent.willBeRemoveField = false;
                }
            }
        }
        #endregion
    }
}
#endregion

#region Security check
public class ISecurityCheck
{
    public ISecurityCheck(Permanent AttackingPermanent, Player player)
    {
        this.AttackingPermanent = AttackingPermanent;
        this.player = player;
    }
    Permanent AttackingPermanent { get; set; }
    Player player { get; set; }

    public IEnumerator SecurityCheck()
    {
        bool StopSecurityCheck()
        {
            if (AttackingPermanent == null)
            {
                return true;
            }

            if (AttackingPermanent.TopCard == null)
            {
                return true;
            }

            return false;
        }

        if (!StopSecurityCheck())
        {
            if (AttackingPermanent.Strike == 0)
            {
                yield break;
            }

            GManager.instance.turnStateMachine.IsSelecting = true;

            GManager.instance.turnStateMachine.isSecurityCehck = true;

            #region loop while there are 1 or more security cards
            if (player.SecurityCards.Count >= 1)
            {
                int checkedCount = 0;

                while (true)
                {
                    if (StopSecurityCheck())
                    {
                        break;
                    }

                    if (checkedCount >= AttackingPermanent.Strike)
                    {
                        break;
                    }

                    if (player.SecurityCards.Count >= 1)
                    {
                        List<SkillInfo> triggeredSkillInfos = new List<SkillInfo>();

                        CardSource brokenSecurityCard = player.SecurityCards[0];

                        Hashtable hashtable = new Hashtable()
                            {
                                {"AttackingPermanent", AttackingPermanent},
                                {"Card", brokenSecurityCard}
                            };

                        foreach (SkillInfo skillInfo in AutoProcessing.GetSkillInfos(hashtable, EffectTiming.OnSecurityCheck))
                        {
                            triggeredSkillInfos.Add(skillInfo);
                        }

                        checkedCount++;

                        GManager.instance.playLog.AddLogString($"\nSecurity Check:\n{brokenSecurityCard.BaseENGCardNameFromEntity}({brokenSecurityCard.CardID})\n");

                        if (brokenSecurityCard.IsDigimon)
                        {
                            GManager.instance.attackProcess.SecurityDigimon = brokenSecurityCard;
                        }

                        #region effect
                        player.securityObject.securityBreakGlass.ShowBlueMatarial();

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().BreakSecurityEffect(player));

                        yield return new WaitForSeconds(0.1f);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().EnterSecurityCardEffect(brokenSecurityCard));
                        #endregion

                        yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                            player: brokenSecurityCard.Owner,
                            refSkillInfos: ref triggeredSkillInfos).ReduceSecurity());

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddExecutingCard(brokenSecurityCard));

                        #region security effect
                        List<SkillInfo> secuityEffectSkillInfos = new List<SkillInfo>();

                        foreach (ICardEffect cardEffect in brokenSecurityCard.EffectList(EffectTiming.SecuritySkill))
                        {
                            if (cardEffect is ActivateICardEffect)
                            {
                                Hashtable hashtable1 = new Hashtable();
                                hashtable1.Add("Card", brokenSecurityCard);

                                if (cardEffect.CanUse(hashtable1))
                                {
                                    secuityEffectSkillInfos.Add(new SkillInfo(cardEffect, hashtable1, EffectTiming.SecuritySkill));
                                }
                            }
                        }

                        List<SkillInfo> beingStackedSkillInfos = GManager.instance.autoProcessing.StackedSkillInfos.Filter(skillInfo => skillInfo.Timing == EffectTiming.OnLoseSecurity || skillInfo.Timing == EffectTiming.OnSecurityCheck);

                        bool isSkillStacked = (beingStackedSkillInfos.Count == 1 && beingStackedSkillInfos[0].CardEffect.CanActivate(beingStackedSkillInfos[0].Hashtable)) || (beingStackedSkillInfos.Count >= 2);

                        if (secuityEffectSkillInfos.Count == 0 && !isSkillStacked)
                        {
                            if (!brokenSecurityCard.IsDigimon)
                            {
                                yield return new WaitForSeconds(0.3f);
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShrinkUpUseHandCard(GManager.instance.GetComponent<Effects>().ShowUseHandCard));
                            }
                        }

                        else
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().MoveToExecuteCardEffect(brokenSecurityCard));
                            yield return ContinuousController.instance.StartCoroutine(player.brainStormObject.BrainStormCoroutine(brokenSecurityCard));

                            List<SkillInfo> skillInfos = new List<SkillInfo>();

                            foreach (SkillInfo skillInfo in secuityEffectSkillInfos)
                            {
                                skillInfos.Add(skillInfo);
                            }

                            List<CardSource> cardSources = new List<CardSource>();

                            for (int j = 0; j < skillInfos.Count; j++)
                            {
                                cardSources.Add(brokenSecurityCard);
                            }

                            while (skillInfos.Count >= 1)
                            {
                                SkillInfo selectedSkillInfo = null;

                                if (skillInfos.Count == 1)
                                {
                                    selectedSkillInfo = skillInfos[0];
                                }

                                else
                                {
                                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                        canTargetCondition: (cardSource) => true,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        canNoSelect: () => false,
                                        selectCardCoroutine: null,
                                        afterSelectCardCoroutine: null,
                                        message: "select security effect",
                                        maxCount: 1,
                                        canEndNotMax: false,
                                        isShowOpponent: false,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Custom,
                                        customRootCardList: cardSources,
                                        canLookReverseCard: true,
                                        selectPlayer: player,
                                        cardEffect: null);

                                    selectCardEffect.SetUpSkillInfos(skillInfos);

                                    selectCardEffect.SetUpCustomMessage("select security effect", "the opponent is selecting security effect");

                                    selectCardEffect.SetUpAfterSelectIndexCoroutine(AfterSelectIndexCoroutine);

                                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                    IEnumerator AfterSelectIndexCoroutine(List<int> selectedIndexes)
                                    {
                                        if (selectedIndexes.Count == 1)
                                        {
                                            selectedSkillInfo = skillInfos[selectedIndexes[0]];
                                            yield return null;
                                        }
                                    }
                                }

                                if (selectedSkillInfo != null)
                                {
                                    ICardEffect cardEffect = selectedSkillInfo.CardEffect;
                                    Hashtable hashtable2 = selectedSkillInfo.Hashtable;

                                    if (cardEffect.EffectSourceCard == brokenSecurityCard)
                                    {
                                        if (cardEffect is ActivateICardEffect)
                                        {
                                            skillInfos.Remove(selectedSkillInfo);

                                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.ActivateEffectProcess(
                                                cardEffect,
                                                hashtable2));
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        // auto process check
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());

                        // stack effect of "When cards are removed from security" and "checks secuirty"
                        foreach (SkillInfo skillInfo in triggeredSkillInfos)
                        {
                            GManager.instance.autoProcessing.PutStackedSkill(skillInfo);
                        }

                        // auto process check
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());

                        if (!StopSecurityCheck())
                        {
                            #region battle with security Digimon
                            if (brokenSecurityCard.IsDigimon)
                            {
                                bool doBattle = true;

                                #region "ignore battle" effect
                                Hashtable hashtable3 = new Hashtable()
                                {
                                    {"Card", brokenSecurityCard}
                                };

                                #region the security Digimon's effect
                                foreach (ICardEffect cardEffect in brokenSecurityCard.EffectList(EffectTiming.None))
                                {
                                    if (cardEffect is IDontBattleSecurityDigimonEffect)
                                    {
                                        if (cardEffect.CanUse(hashtable3))
                                        {
                                            if (((IDontBattleSecurityDigimonEffect)cardEffect).DontBattleSecurityDigimon(brokenSecurityCard))
                                            {
                                                doBattle = false;
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region player's effect
                                foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                                {
                                    foreach (ICardEffect cardEffect in player.EffectList(EffectTiming.None))
                                    {
                                        if (cardEffect is IDontBattleSecurityDigimonEffect)
                                        {
                                            if (cardEffect.CanUse(hashtable3))
                                            {
                                                if (((IDontBattleSecurityDigimonEffect)cardEffect).DontBattleSecurityDigimon(brokenSecurityCard))
                                                {
                                                    doBattle = false;
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #endregion

                                if (doBattle)
                                {
                                    yield return new WaitForSeconds(0.3f);

                                    yield return ContinuousController.instance.StartCoroutine(new IBattle(AttackingPermanent: AttackingPermanent, DefendingPermanent: null, DefendingCard: brokenSecurityCard).Battle());
                                }
                            }
                            #endregion
                        }

                        #region effect
                        yield return ContinuousController.instance.StartCoroutine(brokenSecurityCard.Owner.brainStormObject.CloseBrainstrorm(brokenSecurityCard));

                        if (brokenSecurityCard.Owner.ExecutingCards.Contains(brokenSecurityCard))
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(brokenSecurityCard));

                            if (GManager.instance.GetComponent<Effects>().ShowUseHandCard.gameObject.activeSelf)
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShrinkUpUseHandCard(GManager.instance.GetComponent<Effects>().ShowUseHandCard));
                            }
                        }

                        GManager.instance.GetComponent<Effects>().ShowUseHandCard.OffDP();

                        GManager.instance.attackProcess.SecurityDigimon = null;
                        #endregion

                        // reset effect until end of security check
                        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                        {
                            player.UntilSecurityCheckEndEffects = new List<Func<EffectTiming, ICardEffect>>();
                        }

                        // auto process check
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());

                        GManager.instance.turnStateMachine.IsSelecting = true;
                    }

                    else
                    {
                        break;
                    }
                }
            }
            #endregion

            GManager.instance.turnStateMachine.isSecurityCehck = false;
        }
    }
}

#endregion

#region trash cards from security
public class IDestroySecurity
{
    public IDestroySecurity(Player player, int destroySecurityCount, ICardEffect cardEffect, bool fromTop)
    {
        _player = player;
        _destroySecurityCount = destroySecurityCount;
        _cardEffect = cardEffect;
        _fromTop = fromTop;
    }
    Player _player = null;
    int _destroySecurityCount = 0;
    ICardEffect _cardEffect = null;
    bool _fromTop = false;

    public IEnumerator DestroySecurity()
    {
        bool StopDestroySecurity()
        {
            if (_player.SecurityCards.Count == 0)
            {
                return true;
            }

            if (!_player.CanReduceSecurity())
            {
                return true;
            }

            return false;
        }

        if (_player.SecurityCards.Count >= 1 && _destroySecurityCount >= 1)
        {
            int count = 0;

            List<CardSource> discardedCards = new List<CardSource>();

            while (true)
            {
                if (StopDestroySecurity())
                {
                    break;
                }

                if (count >= _destroySecurityCount)
                {
                    break;
                }

                if (_player.SecurityCards.Count >= 1)
                {
                    count++;

                    CardSource destroyedSecurityCard = _player.SecurityCards[0];

                    if (!_fromTop)
                    {
                        destroyedSecurityCard = _player.SecurityCards[_player.SecurityCards.Count - 1];
                    }

                    discardedCards.Add(destroyedSecurityCard);

                    #region effect
                    _player.securityObject.securityBreakGlass.ShowBlueMatarial();

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().BreakSecurityEffect(_player));

                    yield return new WaitForSeconds(0.1f);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().EnterSecurityCardEffect(destroyedSecurityCard));

                    yield return new WaitForSeconds(0.5f);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().DestroySecurityEffect(destroyedSecurityCard));
                    #endregion

                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(destroyedSecurityCard));
                }
            }

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(discardedCards, "Discarded Cards", true, true));

            if (discardedCards.Count >= 1)
            {
                yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                    player: _player,
                    refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
            }

            if (discardedCards.Count >= 1)
            {
                #region "When security cards are trashed" effect

                #region Hashtable setting
                Hashtable hashtable = new Hashtable()
                {
                    {"DiscardedCards", discardedCards},
                    {"CardEffect", _cardEffect},
                };
                #endregion

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable, EffectTiming.OnDiscardSecurity));

                #endregion
            }

            #region add log
            if (discardedCards.Count >= 1)
            {
                string log = "";

                log += $"\nDiscarded Security Cards:";

                foreach (CardSource cardSource in discardedCards)
                {
                    if (cardSource != null)
                    {
                        log += $"\n{cardSource.BaseENGCardNameFromEntity}({cardSource.CardID})";
                    }
                }

                log += "\n";

                GManager.instance.playLog.AddLogString(log);
            }
            #endregion
        }
    }
}
#endregion

#region Battle
public class IBattle
{
    public IBattle(Permanent AttackingPermanent, Permanent DefendingPermanent, CardSource DefendingCard)
    {
        this.AttackingPermanent = AttackingPermanent;
        this.DefendingPermanent = DefendingPermanent;
        this.DefendingCard = DefendingCard;
    }
    public Permanent AttackingPermanent { get; private set; } = null;
    public Permanent DefendingPermanent { get; private set; } = null;
    CardSource DefendingCard { get; set; } = null;
    public Hashtable hashtable { get; set; } = new Hashtable();

    public Permanent enemyPermanent(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent == AttackingPermanent)
            {
                return DefendingPermanent;
            }

            else if (permanent == DefendingPermanent)
            {
                return AttackingPermanent;
            }
        }

        return null;
    }

    public IEnumerator Battle()
    {
        hashtable = new Hashtable();

        if (AttackingPermanent != null)
        {
            bool IsExistingDefender()
            {
                // whether defender exists
                if ((DefendingPermanent == null) != (DefendingCard == null))
                {
                    if (DefendingPermanent != null)
                    {
                        if (DefendingPermanent.TopCard != null)
                        {
                            return true;
                        }
                    }

                    else if (DefendingCard != null)
                    {
                        return true;
                    }
                }

                return false;
            }

            if (IsExistingDefender())
            {
                if (AttackingPermanent != null)
                {
                    AttackingPermanent.battle = this;
                }

                if (DefendingPermanent != null)
                {
                    DefendingPermanent.battle = this;
                }

                List<Permanent> WinnerPermanents = new List<Permanent>();
                List<Permanent> LoserPermanents = new List<Permanent>();
                CardSource LoserCard = null;

                //add log
                string log = $"\nBattle:\n{AttackingPermanent.TopCard.BaseENGCardNameFromEntity}({AttackingPermanent.TopCard.CardID})";

                if (DefendingPermanent != null)
                {
                    log += $"\n??\n{DefendingPermanent.TopCard.BaseENGCardNameFromEntity}({DefendingPermanent.TopCard.CardID})\n";
                }

                else if (DefendingCard != null)
                {
                    log += $"\n??\n{DefendingCard.BaseENGCardNameFromEntity}({DefendingCard.CardID})\n";
                }

                GManager.instance.playLog.AddLogString(log);

                #region "At the start of battle" effect

                Permanent _AttackingPermanent = null;
                Permanent _DefendingPermanent = null;

                if (AttackingPermanent != null)
                {
                    if (AttackingPermanent.TopCard != null)
                    {
                        _AttackingPermanent = new Permanent(AttackingPermanent.cardSources);
                    }
                }

                if (DefendingPermanent != null)
                {
                    if (DefendingPermanent.TopCard != null)
                    {
                        _DefendingPermanent = new Permanent(DefendingPermanent.cardSources);
                    }
                }

                hashtable.Add("AttackingPermanent", _AttackingPermanent);
                hashtable.Add("DefendingPermanent", _DefendingPermanent);
                hashtable.Add("DefendingCard", DefendingCard);

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable, EffectTiming.OnStartBattle));

                #endregion

                #region Show effect
                bool ShowEffect()
                {
                    if (GManager.instance.autoProcessing.StackedSkillInfos.Count >= 2)
                    {
                        return true;
                    }

                    else if (GManager.instance.autoProcessing.StackedSkillInfos.Count == 1)
                    {
                        if (GManager.instance.autoProcessing.StackedSkillInfos[0].CardEffect.CanActivate(GManager.instance.autoProcessing.StackedSkillInfos[0].Hashtable))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                if (ShowEffect())
                {
                    if (GManager.instance.attackProcess.IsAttacking)
                    {
                        if (GManager.instance.attackProcess.SecurityDigimon != null)
                        {
                            if (GManager.instance.GetComponent<Effects>().ShowUseHandCard.gameObject.activeSelf && GManager.instance.GetComponent<Effects>().ShowUseHandCard.cardSource == GManager.instance.attackProcess.SecurityDigimon)
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().MoveToExecuteCardEffect(GManager.instance.attackProcess.SecurityDigimon));
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SecurityDigimon.Owner.brainStormObject.BrainStormCoroutine(GManager.instance.attackProcess.SecurityDigimon));
                            }
                        }
                    }
                }
                #endregion

                // auto process check
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());

                GManager.instance.turnStateMachine.IsSelecting = true;

                #region battle with permanent
                if (DefendingPermanent != null)
                {
                    if (AttackingPermanent.DP > DefendingPermanent.DP)
                    {
                        WinnerPermanents.Add(AttackingPermanent);

                        if (DefendingPermanent.CanBeDestroyedByBattle(AttackingPermanent, DefendingPermanent, DefendingCard))
                        {
                            LoserPermanents.Add(DefendingPermanent);
                        }
                    }

                    else if (AttackingPermanent.DP == DefendingPermanent.DP)
                    {
                        WinnerPermanents.Add(AttackingPermanent);
                        WinnerPermanents.Add(DefendingPermanent);

                        if (AttackingPermanent.CanBeDestroyedByBattle(AttackingPermanent, DefendingPermanent, DefendingCard))
                        {
                            LoserPermanents.Add(AttackingPermanent);
                        }

                        if (DefendingPermanent.CanBeDestroyedByBattle(AttackingPermanent, DefendingPermanent, DefendingCard))
                        {
                            LoserPermanents.Add(DefendingPermanent);
                        }
                    }

                    else if (AttackingPermanent.DP < DefendingPermanent.DP)
                    {
                        WinnerPermanents.Add(DefendingPermanent);

                        if (AttackingPermanent.CanBeDestroyedByBattle(AttackingPermanent, DefendingPermanent, DefendingCard))
                        {
                            LoserPermanents.Add(AttackingPermanent);
                        }
                    }
                }
                #endregion

                #region battle with card
                else if (DefendingCard != null)
                {
                    if (AttackingPermanent.DP > DefendingCard.CardDP)
                    {
                        WinnerPermanents.Add(AttackingPermanent);
                        LoserCard = DefendingCard;
                    }

                    else if (AttackingPermanent.DP == DefendingCard.CardDP)
                    {
                        if (AttackingPermanent.CanBeDestroyedByBattle(AttackingPermanent, DefendingPermanent, DefendingCard))
                        {
                            LoserPermanents.Add(AttackingPermanent);
                        }

                        LoserCard = DefendingCard;
                    }

                    else if (AttackingPermanent.DP < DefendingCard.CardDP)
                    {
                        if (AttackingPermanent.CanBeDestroyedByBattle(AttackingPermanent, DefendingPermanent, DefendingCard))
                        {
                            LoserPermanents.Add(AttackingPermanent);
                        }
                    }
                }
                #endregion

                
                List<Permanent> _WinnerPermanents = new List<Permanent>();
                List<Permanent> _LoserPermanents = new List<Permanent>();

                foreach (Permanent permanent in WinnerPermanents)
                {
                    if (permanent.TopCard != null)
                    {
                        _WinnerPermanents.Add(new Permanent(permanent.cardSources));
                    }
                }

                foreach (Permanent permanent in LoserPermanents)
                {
                    if (permanent.TopCard != null)
                    {
                        _LoserPermanents.Add(new Permanent(permanent.cardSources));
                    }
                }

                hashtable.Add("WinnerPermanents", _WinnerPermanents);
                hashtable.Add("WinnerPermanents_real", WinnerPermanents);
                hashtable.Add("LoserPermanents", _LoserPermanents);
                hashtable.Add("LoserPermanents_real", LoserPermanents);
                hashtable.Add("LoserCard", LoserCard);
                hashtable.Add("battle", this);


                // battle effect
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().BattleEffect(WinnerPermanents, LoserPermanents, LoserCard));

                yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(LoserPermanents, hashtable).Destroy());

                //Fix winner/loser permanents
                int index = -1;
                foreach (Permanent permanent in LoserPermanents)
                {
                    if(permanent.TopCard != null)
                    {
                        Debug.Log($"LoserPermanents: {LoserPermanents[0]}, {LoserPermanents[0].TopCard.name}");
                        index = LoserPermanents.IndexOf(permanent);
                    }
                }

                if(index >= 0)
                {
                    LoserPermanents.RemoveAt(index);
                    _LoserPermanents.RemoveAt(index);

                    Debug.Log($"LoserPermanents: {LoserPermanents.Count}, {LoserPermanents.Count}");
                    hashtable["LoserPermanents"] = _LoserPermanents;
                    hashtable["LoserPermanents_real"] = LoserPermanents;
                }

                

                // "At the end of battle" effect
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable, EffectTiming.OnEndBattle));

                #region move up effect
                if (ShowEffect())
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.ShrinkSecurityDigimonDisplay());
                }
                #endregion
            }
        }

        // auto process check
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.AutoProcessCheck());

        #region reset effect until the end of battle
        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
        {
            foreach (Permanent permanent in player.GetFieldPermanents())
            {
                permanent.UntilEndBattleEffects = new List<Func<EffectTiming, ICardEffect>>();
            }

            player.UntilEndBattleEffects = new List<Func<EffectTiming, ICardEffect>>();
        }
        #endregion

        if (AttackingPermanent != null)
        {
            AttackingPermanent.battle = null;
        }

        if (DefendingPermanent != null)
        {
            DefendingPermanent.battle = null;
        }
    }
}
#endregion

#region De-digivolve
public class IDegeneration
{
    public IDegeneration(Permanent permanent, int DegenerationCount, ICardEffect cardEffect)
    {
        _permanent = permanent;
        _degenerationCount = DegenerationCount;
        _cardEffect = cardEffect;
    }

    Permanent _permanent = null;
    int _degenerationCount = 0;
    ICardEffect _cardEffect = null;

    public IEnumerator Degeneration()
    {
        if (_cardEffect == null) yield break;
        if (_cardEffect.EffectSourceCard == null) yield break;
        if (_permanent == null) yield break;
        if (_permanent.TopCard == null) yield break;
        if (_permanent.ImmuneFromDeDigivolve()) yield break;
        if (_permanent.TopCard.CanNotBeAffected(_cardEffect)) yield break;

        int maxCount = Math.Min(_permanent.DigivolutionCards.Count, _degenerationCount);

        SelectCountEffect selectCountEffect = GManager.instance.GetComponent<SelectCountEffect>();

        if (selectCountEffect != null)
        {
            Player selectPlayer = _cardEffect.EffectSourceCard.Owner;

            selectCountEffect.SetUp(
                SelectPlayer: selectPlayer,
                targetPermanent: _permanent,
                MaxCount: maxCount,
                CanNoSelect: false,
                Message: "How many cards do you trash?",
                Message_Enemy: "The opponent is choosing how many cards to trash.",
                SelectCountCoroutine: SelectCountCoroutine);

            yield return ContinuousController.instance.StartCoroutine(selectCountEffect.Activate());

            IEnumerator SelectCountCoroutine(int count)
            {
                _degenerationCount = count;
                yield return null;
            }
        }

        if (_degenerationCount >= 1)
        {
            int count = 0;

            List<CardSource> selectedCards = new List<CardSource>();

            while (true)
            {
                bool StopCondition()
                {
                    if (_permanent.HasNoDigivolutionCards)
                    {
                        return true;
                    }

                    if (_permanent.Level == 3)
                    {
                        if (_permanent.TopCard != null)
                        {
                            if (_permanent.TopCard.HasLevel)
                            {
                                return true;
                            }
                        }
                    }

                    if (count >= _degenerationCount)
                    {
                        return true;
                    }

                    return false;
                }

                if (StopCondition())
                {
                    break;
                }

                if (count == 0)
                {
                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(_permanent));
                }

                CardSource cardSource = _permanent.TopCard;

                selectedCards.Add(cardSource);

                yield return ContinuousController.instance.StartCoroutine(new AceOverflowClass(new List<CardSource>() { cardSource }).Overflow());

                yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(cardSource));

                if (!cardSource.IsToken)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource));
                }

                _permanent.ShowingPermanentCard.ShowPermanentData(true);
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(cardSource, _permanent));

                count++;
            }

            #region add log
            if (selectedCards.Count >= 1)
            {
                string log = "";

                log += $"\nDe-Digivolve :";

                foreach (CardSource cardSource in selectedCards)
                {
                    if (cardSource != null)
                    {
                        log += $"\n{cardSource.BaseENGCardNameFromEntity}({cardSource.CardID})";
                    }
                }

                log += "\n";

                GManager.instance.playLog.AddLogString(log);
            }
            #endregion
        }
    }
}
#endregion

#region Trash digivolution cards
public class ITrashDigivolutionCards
{
    public ITrashDigivolutionCards(Permanent permanent, List<CardSource> trashTargetCards, ICardEffect cardEffect)
    {
        _permanent = permanent;

        _trashTargetCards = trashTargetCards.Clone();

        _cardEffect = cardEffect;
    }

    Permanent _permanent = null;
    List<CardSource> _trashTargetCards = new List<CardSource>();
    ICardEffect _cardEffect = null;

    public IEnumerator TrashDigivolutionCards()
    {
        if (_trashTargetCards == null) yield break;
        if (_cardEffect == null) yield break;
        if (_permanent == null) yield break;
        if (_permanent.TopCard == null) yield break;
        if (_permanent.HasNoDigivolutionCards) yield break;
        if (_permanent.TopCard.CanNotBeAffected(_cardEffect)) yield break;
        _trashTargetCards = _trashTargetCards.Filter((cardSource) => _permanent.DigivolutionCards.Contains(cardSource) && !cardSource.CanNotTrashFromDigivolutionCards(_cardEffect));
        if (_trashTargetCards.Count == 0) yield break;

        string message = "Discarded card" + Utils.PluralFormSuffix(_trashTargetCards.Count);
        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(_trashTargetCards, message, true, true));

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(_permanent));

        #region "When digivolution cards are trashed" effect

        #region Hashtable Setting
        Hashtable hashtable = new Hashtable()
            {
                {"CardEffect", _cardEffect},
                {"Permanent", _permanent},
                {"DiscardedCards", _trashTargetCards},
            };
        #endregion

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable, EffectTiming.OnDigivolutionCardDiscarded));
        #endregion

        yield return ContinuousController.instance.StartCoroutine(new AceOverflowClass(_trashTargetCards).Overflow());

        foreach (CardSource cardSource in _trashTargetCards)
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(cardSource, _permanent));

            yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(cardSource));

            if (!cardSource.IsToken)
            {
                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource));
            }
        }
    }
}
#endregion

#region Return digivolution cards to deck bottom
public class ReturnToLibraryBottomDigivolutionCardsClass
{
    public ReturnToLibraryBottomDigivolutionCardsClass(Permanent permanent, List<CardSource> cardSources, Hashtable hashtable)
    {
        _permanent = permanent;

        _cardSources = cardSources.Clone();

        _hashtable = hashtable;
    }

    Permanent _permanent = null;
    List<CardSource> _cardSources = new List<CardSource>();
    Hashtable _hashtable = null;

    public IEnumerator ReturnToLibraryBottomDigivolutionCards()
    {
        if (_permanent == null) yield break;
        if (_permanent.TopCard == null) yield break;
        if (_cardSources == null) yield break;

        _cardSources = _cardSources.Filter(cardSource => _permanent.DigivolutionCards.Contains(cardSource));

        if (_cardSources.Count == 0) yield break;

        ICardEffect CardEffect = CardEffectCommons.GetCardEffectFromHashtable(_hashtable);

        if (CardEffect != null && _permanent.TopCard.CanNotBeAffected(CardEffect)) yield break;

        string message = "Deck Bottom card" + Utils.PluralFormSuffix(_cardSources.Count);

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(_cardSources, message, true, true));

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(_permanent));

        #region "When digivolution cards are returned to deck" effect

        #region Hashtable Setting
        Hashtable hashtable1 = new Hashtable()
        {
            {"Permanent", _permanent},
            {"DeckBottomCards", _cardSources},
            {"CardEffect", CardEffect},
        };
        #endregion

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable1, EffectTiming.OnDigivolutionCardReturnToDeckBottom));

        #endregion

        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(_cardSources));
    }
}
#endregion

#region Reduce security
public class IReduceSecurity
{
    public IReduceSecurity(Player player, ref List<SkillInfo> refSkillInfos)
    {
        _player = player;
        _refSkillInfos = refSkillInfos;
    }
    Player _player = null;
    List<SkillInfo> _refSkillInfos = null;

    public IEnumerator ReduceSecurity()
    {
        #region "When cards are removed from security" effect

        #region Hashtable Setting
        Hashtable hashtable = new Hashtable()
        {
            {"Player", _player}
        };
        #endregion

        if (_refSkillInfos == null)
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable, EffectTiming.OnLoseSecurity));
        }

        else
        {
            foreach (SkillInfo skillInfo in AutoProcessing.GetSkillInfos(hashtable, EffectTiming.OnLoseSecurity))
            {
                _refSkillInfos.Add(skillInfo);
            }
        }
        #endregion
    }
}
#endregion

#region Add Security
public class IAddSecurity
{
    public IAddSecurity(Player player)
    {
        _player = player;
    }
    Player _player { get; set; }

    public IEnumerator AddSecurity()
    {
        #region "When security cards are added" effect

        #region Hashtable setting
        Hashtable hashtable = new Hashtable()
        {
            {"Player", _player}
        };
        #endregion

        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable, EffectTiming.OnAddSecurity));
        #endregion
    }
}
#endregion

#region Suspend permanents

public class SuspendPermanentsClass
{
    public SuspendPermanentsClass(List<Permanent> permanents, Hashtable hashtable)
    {
        _permanents = permanents;
        _hashtable = hashtable;
    }

    List<Permanent> _permanents { get; set; }
    Hashtable _hashtable { get; set; }
    public IEnumerator Tap()
    {
        if (_permanents.Count == 0) yield break;

        ICardEffect CardEffect = CardEffectCommons.GetCardEffectFromHashtable(_hashtable);

        bool IsBlock = CardEffectCommons.IsBlock(_hashtable);

        bool IsAttack = CardEffectCommons.IsAttack(_hashtable);

        List<Permanent> suspendTargetPermanents = _permanents.Filter(PermanentCondition);

        bool PermanentCondition(Permanent permanent)
        {
            if (permanent != null)
            {
                if (permanent.TopCard != null)
                {
                    if (permanent.IsSuspended)
                    {
                        return false;
                    }

                    if (!permanent.CanSuspend)
                    {
                        return false;
                    }

                    if (CardEffect != null)
                    {
                        if (permanent.TopCard.CanNotBeAffected(CardEffect))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        foreach (Permanent permanent in suspendTargetPermanents)
        {
            permanent.IsSuspended = true;

            permanent.DPWhenSuspended = permanent.DP;

            if (permanent.ShowingPermanentCard != null)
            {
                permanent.ShowingPermanentCard.ShowPermanentData(true);
            }
        }

        if (suspendTargetPermanents.Count >= 1)
        {
            #region "Effects when permanents suspend

            #region Hashtable Setting
            Hashtable _hashtable = new Hashtable(){
                {"Permanents", suspendTargetPermanents},
                {"IsBlock", IsBlock}
            };

            if (CardEffect != null)
            {
                _hashtable.Add("CardEffect", CardEffect);
            }
            #endregion

            if (IsAttack) yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.StackSkillInfos(_hashtable, EffectTiming.OnTappedAnyone));
            else yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(_hashtable, EffectTiming.OnTappedAnyone));

            #endregion

            yield return new WaitForSeconds(0.3f);
        }
    }
}
#endregion

#region Unsuspend permanents

public class IUnsuspendPermanents
{
    public IUnsuspendPermanents(List<Permanent> permanents, ICardEffect cardEffect)
    {
        _permanents = permanents.Clone().Filter(CardEffectCommons.IsPermanentExistsOnBattleArea);
        _cardEffect = cardEffect;
    }

    List<Permanent> _permanents { get; set; }
    ICardEffect _cardEffect { get; set; }
    public IEnumerator Unsuspend()
    {
        //Permanent list that will unsuspend
        List<Permanent> untappedPermanets = _permanents.Filter((permanent) =>
            permanent != null
            && permanent.TopCard != null
            && permanent.IsSuspended
            && permanent.CanUnsuspend
            && (_cardEffect == null || !permanent.TopCard.CanNotBeAffected(_cardEffect)));

        #region "When permanents would unsuspend" effect

        #region Hashtable Setting
        Hashtable hashtable1 = new Hashtable()
        {
            {"CardEffect", _cardEffect},
            {"Permanents", untappedPermanets},
        };
        #endregion

        List<SkillInfo> skillInfos = AutoProcessing.GetSkillInfos(hashtable1, EffectTiming.WhenUntapAnyone);

        if (skillInfos.Count >= 1)
        {
            foreach (SkillInfo skillInfo in skillInfos)
            {
                GManager.instance.autoProcessing_CutIn.PutStackedSkill(skillInfo);
            }

            foreach (Permanent permanent in untappedPermanets)
            {
                permanent.ShowUnsuspendEffect();
            }

            //cut in effects
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing_CutIn.TriggeredSkillProcess(false, AutoProcessing.HasExecutedSameEffect));

            untappedPermanets.ForEach(permanent =>
            {
                if (permanent.ShowingPermanentCard != null)
                {
                    permanent.ShowingPermanentCard.WillUntapObject.SetActive(false);
                }
            });
        }

        #endregion

        // fix unsuspend target permanents
        List<Permanent> untappedPermanets_Fixed = untappedPermanets.Filter((permanent) =>
            permanent != null
            && permanent.TopCard != null
            && permanent.IsSuspended
            && permanent.CanUnsuspend
            && (_cardEffect == null || !permanent.TopCard.CanNotBeAffected(_cardEffect)));

        if (untappedPermanets_Fixed.Count >= 1)
        {
            foreach (Permanent permanent in untappedPermanets_Fixed)
            {
                permanent.IsSuspended = false;

                if (permanent.ShowingPermanentCard != null)
                {
                    permanent.ShowingPermanentCard.ShowPermanentData(true);
                }
            }

            #region "When permanents are unsuspended" effect

            #region Hashtable Setting
            Hashtable hashtable2 = new Hashtable()
            {
                {"CardEffect", _cardEffect},
                {"Permanents", untappedPermanets_Fixed},
            };
            #endregion

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(hashtable2, EffectTiming.OnUnTappedAnyone));
            #endregion

            yield return new WaitForSeconds(0.3f);
        }

        foreach (Permanent permanent in untappedPermanets)
        {
            permanent.UntilNextUntapEffects = new List<Func<EffectTiming, ICardEffect>>();
        }
    }
}
#endregion

#region Trash cards from deck top
public class ITrashDeckCards
{
    public ITrashDeckCards(List<CardSource> cardSources, ICardEffect cardEffect)
    {
        this.cardSources = new List<CardSource>();

        foreach (CardSource cardSource in cardSources)
        {
            this.cardSources.Add(cardSource);
        }

        this.cardEffect = cardEffect;
    }
    List<CardSource> cardSources { get; set; } = new List<CardSource>();
    ICardEffect cardEffect { get; set; } = null;

    public IEnumerator TrashDeckCards()
    {
        List<CardSource> trashCards = new List<CardSource>();

        foreach (CardSource cardSource in cardSources)
        {
            if (cardSource.Owner.LibraryCards.Contains(cardSource))
            {
                trashCards.Add(cardSource);
            }
        }

        if (trashCards.Count >= 1)
        {
            foreach (CardSource cardSource in trashCards)
            {
                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource));
            }

            #region "When cards are trashed from security" effect

            #region Hashtable setting
            Hashtable hashtable = new Hashtable()
            {
                {"DiscardedCards", trashCards},
                {"CardEffect", cardEffect},
            };
            #endregion

            yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing.StackSkillInfos(
                hashtable, EffectTiming.OnDiscardLibrary));
            #endregion
        }
    }
}
#endregion

#region Overflow of ace
public class AceOverflowClass
{
    public AceOverflowClass(List<CardSource> cardSources)
    {
        _cardSources = cardSources.Clone();
    }
    List<CardSource> _cardSources = new List<CardSource>();

    public IEnumerator Overflow()
    {
        _cardSources = _cardSources
        .Filter(cardSource => cardSource.IsACE && CardEffectCommons.IsExistOnBattleArea(cardSource))
        .OrderBy(cardSource => cardSource.Owner == GManager.instance.turnStateMachine.gameContext.TurnPlayer ? -1 : 1)
        .ToList();

        foreach (CardSource cardSource in _cardSources)
        {
            yield return ContinuousController.instance.StartCoroutine(cardSource.Owner.AddMemory(-cardSource.OverflowMemory, null));

            string log = $"\nOverflow -{cardSource.OverflowMemory}:\n{cardSource.BaseENGCardNameFromEntity}({cardSource.CardID})\n";

            GManager.instance.playLog.AddLogString(log);
        }
    }
}
#endregion

