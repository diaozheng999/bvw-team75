using System.Collections.Generic;
using PGT.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Team75.Client {

    public class ScoreManager : Singleton<ScoreManager> {

        [SerializeField] Text revenueDisplay;
        [SerializeField] Text opponentRevenueDisplay;
        [SerializeField] Text subtotalDisplay;
        [SerializeField] Text lineItemsDisplay;
        [SerializeField] int maxLineItems;
        [SerializeField] Text timerDisplay;
        

        LinkedList<string> lineItems;

        uint revenue;
        private uint opponentrevenue;
        uint subtotal;

        float time;

        public void Reset() {
            revenueDisplay.text = "$0";
            opponentRevenueDisplay.text = "$0";
            subtotalDisplay.text = "$0";
            lineItemsDisplay.text = "";

            lineItems = new LinkedList<string>();
        }

        public void ResetLines() {
            subtotal = 0;
            lineItems.Clear();

            subtotalDisplay.text = "$0";
            lineItemsDisplay.text = "";
        }

        public void AddLine(uint amount) {
            lineItems.AddLast("$"+amount);
            subtotal += amount;
            revenue += amount;
            if(lineItems.Count > maxLineItems) {
                lineItems.RemoveFirst();
            }

            subtotalDisplay.text = "$"+subtotal;
            revenueDisplay.text = "$"+revenue;
            lineItemsDisplay.text = string.Join("\n", lineItems);
        }

        public void SetOpponentScore(uint value) {
            opponentRevenueDisplay.text = "$"+value;
            opponentrevenue = value;
        }

        public void SetTime(float _time) {
            time = _time;
        }

        void Update() {
            time = Mathf.Max(0, time - Time.deltaTime);
            timerDisplay.text = ParseTime();
            
        }

        public float GetTime()
        {
            return time;
        }

        
        public string ParseTime() {
            var mins = Mathf.FloorToInt(time / 60);
            var secs = Mathf.FloorToInt(time % 60);
            return string.Format("{0}:{1}", mins.ToString("D2"), secs.ToString("D2"));
        }

        public uint GetScore() {
            return revenue;
        }
        
        public uint GetOpponentScore()
        {
            return opponentrevenue;
        }

    }

}