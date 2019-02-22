using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking.Match;

namespace Assets.Scripts.Networking
{
    public static class AvailableMatchesList
    {
        public static event Action<List<MatchInfoSnapshot>> OnAvailableMatchesChange = delegate { };
        private static List<MatchInfoSnapshot> _matches = new List<MatchInfoSnapshot>();
        
        public static void HandleNewMatchList(List<MatchInfoSnapshot> matchList)
        {
            _matches = matchList;
            OnAvailableMatchesChange(_matches); 
        }
    }
}
