using UnityEngine;
using UnityEngine.UI;
using PGT.Core;
using Team75.Shared;


namespace Team75.Server {

    public class Tally : MonoBehaviour {
        [SerializeField] Text[] revenue;
        [SerializeField] Text[] custSrv;
        [SerializeField] Text[] itemScn;
        [SerializeField] Text[] custSat;
        [SerializeField] Text[] topItems;

        void Start() {
            var package = Messenger.GetMessage<GameStat[]>("stats");

            for(int i=0; i<2; ++i) {
                revenue[i].text = package[i].revenue.ToString();
                custSrv[i].text = package[i].customerServed.ToString();
                itemScn[i].text = package[i].itemScanned.ToString();
                custSat[i].text = (package[i].fracCustomerServed * 100).ToString() + "%";
                for(int j=0; j<package[i].itemIds.Length; ++j) {
                    topItems[i].text += string.Format("{0}:{1}\n", package[i].itemIds[j], package[i].itemCounts[j]);
                }
            }
        }
    }

}