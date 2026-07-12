using QControl.Application.Shared.Dto;
using QControl.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QControl.Application.Abstraction.Security
{
    public interface IJwtProvider
    {
        Task<UserTokenDto> Generate(
      Guid userId,
      string email,
      string phoneNumber,
      IReadOnlyCollection<string> roleNames,
      UserType userType,
      IReadOnlyCollection<string> permissions,
      int? activeBranchId = null,
      bool passwordChangeRequired = false,
      CancellationToken cancellationToken = default);
    }
}
