namespace Team75.Shared {

    public struct GameStat {
        public uint revenue;
        public uint customerServed;
        public uint customerCompleted;
        public uint itemScanned;
        public uint itemGiven;
        public ushort[] itemIds;
        public uint[] itemCounts;

        public float fracItemScanned {
            get {
                return (float)itemScanned / (float)itemGiven;
            }
        }

        public float fracCustomerServed {
            get {
                return (float)customerCompleted / (float)customerServed;
            }
        }
    }

}