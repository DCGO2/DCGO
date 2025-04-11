using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
public partial class CardEffectCommons
{
    #region Can activate [Armor Purge]
    public static bool CanActivateArmorPurge(CardSource card)
    {
        if (IsExistOnBattleArea(card))
        {
            if (card.PermanentOfThisCard().DigivolutionCards.Count >= 1)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Armor Purge]
    public static IEnumerator ArmorPurgeProcess(CardSource card)
    {
        yield return ContinuousController.instance.StartCoroutine(new ArmorPurgeClass(card.PermanentOfThisCard()).ArmorPurge());
    }
    #endregion

    #region Armor Purge class
    public class ArmorPurgeClass
    {
        public ArmorPurgeClass(Permanent permanent)
        {
            _permanent = permanent;
        }

        Permanent _permanent { get; set; }

        public IEnumerator ArmorPurge()
        {
            if (_permanent != null)
            {
                if (_permanent.TopCard != null)
                {
                    if (_permanent.DigivolutionCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(_permanent));

                        CardSource topCard = _permanent.TopCard;

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveFromAllArea(topCard));

                        if (!topCard.IsToken)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(topCard));
                        }

                        _permanent.ShowDeleteEffect();

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().RemoveDigivolveRootEffect(topCard, _permanent));

                        _permanent.willBeRemoveField = false;

                        _permanent.HideDeleteEffect();

                        #region "When Top Card is Trashed" effect
                        #region Hashtable Setting
                        System.Collections.Hashtable hashtable = new System.Collections.Hashtable()
                        {
                            {"Permanent", _permanent},
                            {"CardSources", new List<CardSource> { topCard } }
                        };
                        #endregion

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.autoProcessing
                            .StackSkillInfos(hashtable,EffectTiming.WhenTopCardTrashed));

                        #endregion

                        #region Log
                        string log = "";

                        log += $"\nArmor Purge :";

                        log += $"\n{topCard.BaseENGCardNameFromEntity}({topCard.CardID})";

                        log += "\n";

                        PlayLog.OnAddLog?.Invoke(log);
                        #endregion
                    }
                }
            }

        }
    }
    #endregion
}