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

            var val = ItemDictionary.instance.GetItemValue(ScannedValue);
            // Update score
            ScoreManager.instance.AddLine((uint)val);
            NetworkManager.instance.SendScoreUpdateMessage((uint)val);
            Scanner.instance.RemoveItem(this);
        }

        public override int GetId()
        {
            return ScannedValue;
        }

        void OnDestroy() {
            if(!scanned) Scanner.instance.RemoveItem(this);
        }
    }

}