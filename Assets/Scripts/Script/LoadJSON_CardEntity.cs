using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCGO.CardEntities
{
    [CreateAssetMenu(fileName = "CardEntity_JSONLoader", menuName = "Create JSONLoader/Create JSONLoader_CardEntity")]
    public class LoadJSON_CardEntity : ScriptableObject
    {
        public int prevCardIndex;
        public int setCardIndex;
        [HideInInspector]
        public int promoCardIndex;
    }
}