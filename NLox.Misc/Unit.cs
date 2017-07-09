namespace NLox.Misc
{
    /// <summary>
    /// Represents a type with only one value, like void but an actual type
    /// 
    /// Useable in generics unlike void
    /// </summary>
    public struct Unit
    {
        public static Unit value { get; } = new Unit();

        public override bool Equals(object obj)
            => obj is Unit;

        public override int GetHashCode() => 0;
    }
}
