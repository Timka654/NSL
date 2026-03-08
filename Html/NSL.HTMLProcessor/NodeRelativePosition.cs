namespace NSL.HTMLProcessor
{
    public struct NodeSize
    {
        public readonly int OuterStartOffset => 0;

        public readonly int OuterEndOffset => TotalLength;

        public readonly int TotalLength => InnerContentLength + OpenNodeLength + CloseNodeLength;

        public readonly int InnerStartOffset => OuterStartOffset + OpenNodeLength;

        public readonly int InnerEndOffset => InnerStartOffset + InnerContentLength;

        public int OpenNodeLength { get; internal set; }

        public int CloseNodeLength { get; internal set; }

        public int InnerContentLength { get; internal set; }

        public override string ToString()
            => $"o: {OuterStartOffset}..{OuterEndOffset}, i: {InnerStartOffset}..{InnerEndOffset}({InnerContentLength})";

        public bool Equals(NodeSize other)
            => other.OpenNodeLength == OpenNodeLength
            && other.CloseNodeLength == CloseNodeLength
            && other.InnerContentLength == InnerContentLength;
    }

    //public struct NodeRelativePosition
    //{
    //    public int OuterStartOffset { get; internal set; }

    //    //public int OpenNodeLength { get; internal set; }

    //    //public int CloseNodeLength { get; internal set; }

    //    //public int InnerContentLength { get; internal set; }

    //    public readonly int TotalLength => OpenNodeLength + CloseNodeLength + InnerContentLength;

    //    public readonly int InnerStartOffset => OuterStartOffset + OpenNodeLength;

    //    public readonly int CloseNodeStartOffset => InnerStartOffset + (InnerContentLength - 1);

    //    public readonly int InnerEndOffset => CloseNodeStartOffset;

    //    public readonly int OuterEndOffset => CloseNodeStartOffset + CloseNodeLength;

    //    public override string ToString()
    //        => $"o: {OuterStartOffset}..{OuterEndOffset}, i: {InnerStartOffset}..{InnerEndOffset}({InnerContentLength})";
    //}
}
