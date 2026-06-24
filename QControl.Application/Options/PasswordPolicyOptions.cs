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
    }
}