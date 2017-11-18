using PGT.Core;
using UnityEngine;
using System;

namespace Team75.Shared {

    [RequireComponent(typeof(Avatar))]
    public class SpecialEffect : MonoBehaviour {

        [Flags]
        public enum ExecutionFlag {
            NONE = 0,
            BEFORE_ENQUEUE = 1, AFTER_ENQUEUE = 2, 
            BEFORE_QUEUE_MOVE = 4, AFTER_QUEUE_MOVE = 8, 
            BEFORE_DEQUEUE = 16, AFTER_DEQUEUE = 32, 
            BEFORE_LEAVE = 64, AFTER_LEAVE = 128, 
            AFTER_ITEMS = 256
        }

        [SerializeField] TextMesh scannable;
        protected ExecutionFlag executionFlag = ExecutionFlag.NONE;

        protected virtual void BeforeEnqueue(Action cont) => cont();
        protected virtual void AfterEnqueue(Action cont) => cont();

        protected virtual void BeforeQueueMove(Action cont) => cont();
        protected virtual void AfterQueueMove(Action cont) => cont();

        protected virtual void BeforeDequeue(Action cont) => cont();
        protected virtual void AfterDequeue(Action cont) => cont();

        protected virtual void BeforeLeave(Action cont) => cont();
        protected virtual void AfterLeave(Action cont) => cont();

        protected virtual void AfterItems(Action cont) => cont();

        protected void Start() {
            var _avatar = GetComponent<Avatar>();
            _avatar.AddSpecialEffect(executionFlag, 
                BeforeEnqueue,
                AfterEnqueue,
                BeforeQueueMove,
                AfterQueueMove,
                BeforeDequeue,
                AfterDequeue,
                BeforeLeave,
                AfterLeave,
                AfterItems
            );
        }

    }
}