using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QControl.Application.Shared.Dto
{
    public sealed record UserTokenDto
    {
        public string Token { get; init; } = string.Empty;

        public string UserType { get; init; } = string.Empty;

        public bool RequiresBranchSelection { get; init; }

        public int? ActiveBranchId { get; init; }

        public IReadOnlyCollection<LoginBranchSelectionItemResponse> Branches { get; init; } =
            Array.Empty<LoginBranchSelectionItemResponse>();

        public bool FirstLoginFlag { get; init; }

        public bool PasswordExpiredFlag { get; init; }

        public DateTime? PasswordChangedOnUtc { get; init; }

        public DateTime? PasswordExpiresOnUtc { get; init; }
    }

    public sealed record LoginBranchSelectionItemResponse
    {
        public int BranchId { get; init; }

        public string EnglishName { get; init; } = string.Empty;

        public string ArabicName { get; init; } = string.Empty;

        public bool IsActive { get; init; }
    }
}
