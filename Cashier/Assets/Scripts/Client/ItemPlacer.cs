using System.Collections.Generic;
using UnityEngine;
using PGT.Core;
using Team75.Shared;

namespace Team75.Client {

    public class ItemPlacer : MonoBehaviour {

        Shared.Avatar avatar;
        Customer customer;

        protected float itemPlacementDelay;
        
        protected Transform[] items;
        protected bool[] placed;

        Coroutine_ placeCoroutine = null;

        bool cleanedUp = false;
        


        public void SetAvatar(Shared.Avatar _avatar, Customer _customer) {
            avatar = _avatar;
            customer = _customer;

            itemPlacementDelay = avatar.GetItemPlacementDelay();

            items = new Transform[customer.Items.Length];
            placed = new bool[customer.Items.Length];
            
            avatar.AddSpecialEffect(SpecialEffect.ExecutionFlag.AFTER_DEQUEUE, null, null, null, null, null, StartPlacing, null, null, null);
        }

        public void StartPlacing(System.Action cont) {
            cont();
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

        protected virtual Transform GetItemPlacementTransform () {
            return avatar.GetItemPlacementTransform();
        }

        IEnumerator<object> PlaceObjects(){
            //yield return avatar.GetAwaiter();
            if(cleanedUp) yield break;
            var wfs = new WaitForSeconds(itemPlacementDelay);
            var len = customer.Items.Length;

            var itemPlacement = GetItemPlacementTransform();

            for(int i=0; i<len; ++i)
            {
                yield return wfs;
                var _go = Instantiate(
                    ItemDictionary.instance.GetItem(customer.Items[i]), itemPlacement.position, itemPlacement.rotation
                );
                Statics.instance.GenerateItem(customer.Items[i]);
                var _it = _go.AddComponent<Item>();
                _it.RequestTrackingId(customer.Items[i], this, i);
                items[i] = _go.transform;
                Scanner.instance.AddItem(_it);
            }

            GameStateManager.instance.SetCallable();
            NetworkManager.instance.SendCustomerFinishItems();
            avatar.OnAfterItems();

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

            Statics.instance.CustomerComplete(VisibleCustomerQueue.instance.GetActiveCustomer(GameStateManager.instance.GetPlayerId()).GetID());
            var pid = GameStateManager.instance.GetPlayerId();
            NetworkManager.instance.SendCustomerLeave(pid);
            VisibleCustomerQueue.instance.CustomerLeave(pid, () => {
                ScoreManager.instance.ResetLines();
                GameStateManager.instance.SetCallable();
            });
        }

    }

}