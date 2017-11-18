using System;
using System.Collections;
using UnityEngine;

namespace Team75.Shared {

    public class Zombie : SpecialEffect {
        


        protected override void BeforeEnqueue(System.Action cont) {
            StartCoroutine(DoSomething(cont));
        }

        IEnumerator DoSomething(Action cont) {
            yield return new WaitForSeconds(5);
            cont();
        }

        
    }

}