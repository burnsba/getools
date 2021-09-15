namespace Getools.Lib.BinPack
{
    public interface IPointerVariable : IBinData, IGetoolsLibObject
    {
        string AddressOfVariableName { get; set; }
        bool IsNull { get; }
        int PointedToOffset { get; set; }
        int PointedToSize { get; set; }

        void AssignPointer(IGetoolsLibObject pointsTo);
        IGetoolsLibObject Dereference();
    }
}