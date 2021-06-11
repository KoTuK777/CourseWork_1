using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Mapper
{
    public struct TypePair : IEquatable<TypePair>
    {
        public Type SourceType { get; private set; }
        public Type DestinationType { get; private set; }

        public TypePair(Type source, Type destination)
        {
            SourceType = source;
            DestinationType = destination;
        }

        public bool Equals([AllowNull] TypePair other)
        {
            return other.DestinationType == DestinationType &&
                other.SourceType == SourceType;
        }
    }
}
