using System.Collections.Generic;
using UnityEngine;
using PGT.Core;
using Team75.Shared;

namespace Team75.Client {

    public class ItemPlacer : MonoBehaviour {

        Shared.Avatar avatar;
        Customer customer;

        float itemPlacementDelay;
        
        Transform[] items;
        bool[] placed;

        Coroutine_ placeCoroutine = null;

        bool cleanedUp = false;


        public void SetAvatar(Shared.Avatar _avatar, Customer _customer) {
            avatar = _avatar;
            customer = _customer;

            itemPlacementDelay = avatar.GetItemPlacementDelay();

            items = new Transform[customer.Items.Length];
            placed = new bool[customer.Items.Length];
            
            placeCoroutine = this.StartCoroutine1(PlaceObjects());
        }

        public void Cleanup() {
            cleanedUp = true;
            ScoreManager.instance.ResetLines();
            if(placeCoroutine != null && !placeCoroutine.Completed) placeCoroutine.Interrupt();

            var len = customer.Items.Length;

            for (int i=0; i<len; ++i) {
                if(items[i] != null && !placed[i]) {
                    var _item = items[i].GetComponent<Item>();
                    _item.SetScanned(true, false);
                }
            }
        }

        IEnumerator<object> PlaceObjects(){
            yield return avatar.GetAwaiter();
            if(cleanedUp) yield break;
            var wfs = new WaitForSeconds(itemPlacementDelay);
            var len = customer.Items.Length;

            var itemPlacement = avatar.GetItemPlacementTransform();

            for(int i=0; i<len; ++i) {
                var _go = Instantiate(
                    ItemDictionary.instance.GetItem(customer.Items[i]), itemPlacement.position, itemPlacement.rotation
                );
                var _it = _go.AddComponent<Item>();
                _it.RequestTrackingId(customer.Items[i], this, i);
                items[i] = _go.transform;
                Scanner.instance.AddItem(_it);
                yield return wfs;
            }

            GameStateManager.instance.SetCallable();
        }

        public void PlaceItem(int i) {
            placed[i] = true;
            // set score
            var lineValue = ItemDictionary.instance.GetItemValue(customer.Items[i]);
            ScoreManager.instance.AddLine((uint)lineValue);
            NetworkManager.instance.SendScoreUpdateMessage((uint)lineValue);
            
            foreach(var p in placed) {
                if(!p) return;
            }

            var pid = GameStateManager.instance.GetPlayerId();
            NetworkManager.instance.SendCustomerLeave(pid);
            VisibleCustomerQueue.instance.CustomerLeave(pid);
            ScoreManager.instance.ResetLines();
        }

    }

}