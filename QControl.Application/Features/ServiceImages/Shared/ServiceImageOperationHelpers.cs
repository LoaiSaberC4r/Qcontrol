using BuildingBlock.Domain.Results;
using Qcontrol.Application.Features.Services.Shared;
using QControl.Application.Abstraction.Presistence;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.ServiceImages.Shared;

internal static class ServiceImageOperationHelpers
{
    public static async Task<Result<Service>> LoadServiceForMutationAsync(
        IWriteReadRepository<Service> serviceReadRepository,
        int serviceId,
        string operation,
        CancellationToken cancellationToken)
    {
        var service = await serviceReadRepository.FirstOrDefaultAsync(
            new GetServiceForMutationSpec(serviceId),
            cancellationToken);

        if (service is null)
        {
            return Result<Service>.Fail(new Error(
                $"Services.Images.{operation}.ServiceNotFound",
                ServiceFeatureMessages.NotFound,
                ErrorType.NotFound));
        }

        if (service.IsDeleted)
        {
            return Result<Service>.Fail(new Error(
                $"Services.Images.{operation}.ServiceDeleted",
                ServiceFeatureMessages.Deleted,
                ErrorType.Conflict));
        }

        return Result<Service>.Ok(service);
    }

    public static Error AuthenticationRequired(string operation) =>
        new(
            $"Services.Images.{operation}.AuthenticationRequired",
            ServiceFeatureMessages.AuthenticationRequired,
            ErrorType.Unauthorized);

    public static Error InvalidRowVersion(string operation) =>
        new(
            $"Services.Images.{operation}.InvalidRowVersion",
            Qcontrol.Domain.Resources.ErrorMessage.RowVersion_Invalid,
            ErrorType.Validation);

    public static Error ConcurrencyConflict(string operation) =>
        new(
            $"Services.Images.{operation}.ConcurrencyConflict",
            Qcontrol.Domain.Resources.ErrorMessage.Concurrency_Conflict,
            ErrorType.Conflict);
}
