using System.Collections.Generic;
using Team75.Shared;
using PGT.Core.DataStructures;
using UnityEngine;

namespace Team75.Client {

    public class FrenzyItemPlacer : ItemPlacer {

        public const int MAX_FRENZY_ITEMS = 300;

        int playerId;

        public void SetAvatar(Shared.Avatar _avatar, int _playerId) {
            var cust = new Customer();
            cust.CustomerId = _avatar.GetID();
            cust.Items = Sequence.Tabulate(MAX_FRENZY_ITEMS, (int i) => ItemDictionary.instance.GenerateRandomItemFrenzy()).ToArray();
            playerId = _playerId;

            SetAvatar(_avatar, cust);
        }

        protected override Transform GetItemPlacementTransform() {
            return VisibleCustomerQueue.instance.GetFrenzySpawnPosition(playerId);
        }

    }

}