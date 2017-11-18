using UnityEngine;

namespace Team75.Shared {

    public interface IBarcodeProvider {
        Transform GetBarcodeLocation();
    }

}