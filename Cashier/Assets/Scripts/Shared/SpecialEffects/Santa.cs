using System;
using System.Collections;
using Team75.Client;
using UnityEngine;


namespace Team75.Shared.SpecialEffects {

    public class Santa : SpecialEffect {
        
        Animator _anim = null;
        Animator anim {
            get {
                if(_anim == null) _anim = GetComponent<Animator>();
                return _anim;
            }
        }

        [SerializeField] float angularVelocity = 0.1f;
        [SerializeField] float angularAcceleration = 0.01f;

        [SerializeField] bool spin = false;
        Action stopSpin = null;

        public Santa() : base() {
            executionFlag = ExecutionFlag.AFTER_DEQUEUE | ExecutionFlag.BEFORE_LEAVE;
        }

        protected override void AfterDequeue(Action cont) {
            cont();
            spin = true;
        }

        void Update() {
            if (spin) {
                transform.Rotate(Vector3.up, angularVelocity * Time.deltaTime);
                angularVelocity += angularAcceleration * Time.deltaTime;

                if(stopSpin != null && Vector3.Angle(transform.forward, Vector3.back) < 5) {
                    stopSpin();
                    stopSpin = null;
                    spin = false;
                }
            }
        }

        protected override void BeforeLeave(Action cont) => stopSpin = cont;

    }

}