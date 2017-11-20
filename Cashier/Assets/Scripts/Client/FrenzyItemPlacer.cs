using System.Collections.Generic;
using Team75.Shared;
using PGT.Core.DataStructures;
using UnityEngine;

namespace Team75.Client {

    public class FrenzyItemPlacer : ItemPlacer {

        public const int MAX_FRENZY_ITEMS = 300;

        public const float RADIUS = 1.5f;


        int playerId;

        public void SetAvatar(Shared.Avatar _avatar, int _playerId) {
            var cust = new Customer();
            cust.CustomerId = _avatar.GetID();
            cust.Items = Sequence.Tabulate(MAX_FRENZY_ITEMS, (int i) => ItemDictionary.instance.GenerateRandomItemFrenzy()).ToArray();
            playerId = _playerId;

            SetAvatar(_avatar, cust);
        }

        protected override Vector3 Jitter() {
            var r = Random.Range(0, RADIUS);
            var theta = Random.Range(-Mathf.PI, Mathf.PI); 

            return new Vector3(r * Mathf.Cos(theta), 0, r * Mathf.Sin(theta));
        }

        protected override Transform GetItemPlacementTransform() {
            return VisibleCustomerQueue.instance.GetFrenzySpawnPosition(playerId);
        }

    }

}