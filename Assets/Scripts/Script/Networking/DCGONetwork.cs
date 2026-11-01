using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCGO.Networking
{
    public class DCGONetwork : MonoBehaviour
    {
        public static INetworkProvider Provider { get; set; }

        private static INetworkProvider _onlineProvider = null;

        public static bool IsOffline => Provider is OfflineNetworkProvider;

        public static void UseOfflineProvider()
        {
            if (IsOffline)
            {
                return;
            }

            _onlineProvider = Provider;
            Provider = new OfflineNetworkProvider();
        }

        public static void UseOnlineProvider()
        {
            if (!IsOffline)
            {
                return;
            }

            Provider = _onlineProvider;
            _onlineProvider = null;
        }
    }
}
