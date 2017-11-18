using System.Collections;
using System.Collections.Generic;
using Team75.Client;
using UnityEngine;
using UnityEngine.UI;

public class Canvas_Test : MonoBehaviour
{
    private Text[] texts;

    void Start()
    {
        texts = GetComponentsInChildren<Text>();
        
    }

    void Update()
    {
        texts[0].text = "Item_Exist: " + Statics.instance.Item_Exist.ToString();
        texts[1].text = "Item_Scanned: " + Statics.instance.Item_Scanned.ToString();
        texts[2].text = "Customer_Exist: " + Statics.instance.Customer_Exist.ToString();
        texts[3].text = "Customer_Complete: " + Statics.instance.Customer_Completed.ToString();
        texts[4].text = "Time_Scanning: " + Statics.instance.Time_Scanning.ToString();

        linkarray<int>("Cust",Statics.instance.Distribution_Customer_Exist,5);
    }

    private void linkarray<T>(string title, T[] numbers,int index_begin)
    {
        for (int i = 0; i < numbers.Length; i++)
        {
            texts[i + index_begin].text = title + i.ToString() + ": " + numbers[i];
        }
    }
}
