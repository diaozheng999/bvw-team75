using PGT.Core;
using UnityEngine;
using System;

namespace Team75.Shared {

    [RequireComponent(typeof(Avatar))]
    public class SpecialEffect : MonoBehaviour {

        [SerializeField] TextMesh scannable;

        protected virtual void BeforeEnqueue(Action cont) {}
        protected virtual void AfterEnqueue() {}

        protected virtual void BeforeQueueMove(Action cont) {}
        protected virtual void AfterQueueMove() {}

        protected virtual void BeforeDequeue(Action cont) {}
        protected virtual void AfterDequeue() {}

        protected virtual void BeforeLeave(Action cont) {}
        protected virtual void AfterLeave() {}

        protected void Start() {
            var _avatar = GetComponent<Avatar>();
        }

    }
}