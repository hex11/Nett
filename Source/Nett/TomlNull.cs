using System;
using Nett.Extensions;

namespace Nett
{
    public sealed class TomlNull : TomlValue<object>
    {
        public TomlNull(ITomlRoot root)
            : base(root, null)
        {
        }

        public override string ReadableTypeName => throw new NotImplementedException();

        public override TomlObjectType TomlType => throw new NotImplementedException();

        public override void Visit(ITomlObjectVisitor visitor)
        {

        }

        internal override TomlObject CloneFor(ITomlRoot root)
        {
            return new TomlNull(root);
        }

        internal override TomlValue ValueWithRoot(ITomlRoot root)
        {
            return new TomlNull(root);
        }

        internal override TomlObject WithRoot(ITomlRoot root)
        {
            return new TomlNull(root);
        }

        public override object Get(Type t)
        {
            if (t.IsValueType)
                throw new InvalidOperationException($"Cannot convert null to '{t.Name}'.");
            return null;
        }
    }
}
