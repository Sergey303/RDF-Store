namespace RDFStore.CascadingIndex
{
    public struct IndexCascadingCellRow
    {
        public long Offset;
        public int Key1;
        public int HKey2;

        public IndexCascadingCellRow(object row)
        {
            object[] cells = (object[]) row;
            Offset = (long) cells[0];
            Key1 = (int) cells[1];
            HKey2 = (int) cells[2];
        }

        public object ToObject()
        {
            return new object[] {Offset, Key1, HKey2};
        }
    }
}