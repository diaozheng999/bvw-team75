using UnityEngine;

namespace Team75.Shared {

    public static class NameRandomiser {

        static string[] dictionaryName = {
            "hello",
            "宦文山",
            "璩涵畅",
            "濮妙菱",
            "Українська",
            "Sally",
            "José",
            "உதயமூர்த்தி",
            "Marylouise Mcsweeney",
            "Temple Sereno",
            "Ron"
        };

        public static string GetName() {
            switch(Random.Range(0, 3)) {
                case 0:
                    return dictionaryName[Random.Range(0, dictionaryName.Length)];
                case 1:
                    return dictionaryName[Random.Range(0, dictionaryName.Length)] + "_" + (ushort)Random.value.GetHashCode();
                default:
                    return "C_"+Random.value.GetHashCode().ToString("X");
            }
        }

    }

}