using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QControl.Application.Options
{
    public sealed class PasswordPolicyOptions
    {
        public const string SectionName = "PasswordPolicy";

        public int ExpiryDays { get; init; } = 30;

        public int RequiredLength { get; init; } = 8;

        public int MaxLength { get; init; } = 200;

        public bool RequireDigit { get; init; } = true;

        public bool RequireLowercase { get; init; } = true;

        public bool RequireUppercase { get; init; } = true;

        public bool RequireNonAlphanumeric { get; init; } = true;
    }
}
