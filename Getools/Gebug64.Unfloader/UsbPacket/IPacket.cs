namespace Gebug64.Unfloader.UsbPacket
{
    public interface IPacket
    {
        /// <summary>
        /// UNFLoader header packet `type` parameter.
        /// </summary>
        PacketType DataType { get; set; }

        /// <summary>
        /// UNFLoader header `size` parameter.
        /// </summary>
        int Size { get; set; }

        /// <summary>
        /// Gets the byte array without flashcart specific protocol wrapper.
        /// </summary>
        /// <returns></returns>
        byte[]? GetInnerData();

        /// <summary>
        /// Gets the byte array including flashcart specific protocol wrapper (if any).
        /// </summary>
        /// <returns></returns>
        byte[]? GetOuterData();
    }
}