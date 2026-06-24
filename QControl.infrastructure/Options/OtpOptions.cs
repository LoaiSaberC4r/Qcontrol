using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QControl.infrastructure.Options
{
    public sealed class OtpOptions
    {
        public string Secret { get; init; } = default!;
    }
}