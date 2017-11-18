using Team75.Shared;

namespace Team75.Client {

    public class ScannableAvatar : Scannable {
        bool scanned = false;

        int ScannedValue;

        void Start() {
            Scanner.instance.AddItem(this);
            var avatar = GetComponent<Avatar>();
            ScannedValue = avatar.GetScannedValue();
        }

        void SetScannedValue(int _value) {
            ScannedValue = _value;
        }

        public override void SetScanned(bool _scanned) {
            scanned = _scanned;
            // Update score
            ScoreManager.instance.AddLine((uint)ScannedValue);
            NetworkManager.instance.SendScoreUpdateMessage((uint)ScannedValue);
            Scanner.instance.RemoveItem(this);
        }

        void OnDestroy() {
            if(!scanned) Scanner.instance.RemoveItem(this);
        }
    }

}