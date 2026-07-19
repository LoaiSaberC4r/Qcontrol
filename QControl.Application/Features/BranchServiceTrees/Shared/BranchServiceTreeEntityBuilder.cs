using Qcontrol.Application.Features.BranchServiceTrees.Command.CreateBranchServiceTree;
using QControl.Domain.Entities;

namespace Qcontrol.Application.Features.BranchServiceTrees.Shared;

internal static class BranchServiceTreeEntityBuilder
{
    public static CreatedBranchServiceTreeNode Build(
        CreateBranchServiceTreeNodeCommand requestNode,
        int? rootParentServiceId,
        int branchId,
        Guid createdByApplicationUserId,
        ICollection<Service> services,
        ICollection<BranchService> assignments)
    {
        var service = Service.CreateBranchScoped(
            rootParentServiceId,
            branchId,
            requestNode.ArabicName,
            requestNode.EnglishName,
            requestNode.ServiceCode,
            requestNode.IsServiceCodeRequired,
            requestNode.ArabicUserMessage,
            requestNode.EnglishUserMessage,
            requestNode.IsTicketIssuable.GetValueOrDefault(),
            requestNode.IsClientInputRequired,
            requestNode.HasReservation,
            requestNode.OrderNo,
            requestNode.Priority,
            requestNode.RangePrefix,
            requestNode.RangeStartNumber,
            requestNode.RangeEndNumber,
            requestNode.WaitingDuration,
            requestNode.NoOfTicketCopies,
            createdByApplicationUserId);

        return BuildCore(
            requestNode,
            service,
            branchId,
            createdByApplicationUserId,
            services,
            assignments);
    }

    public static CreatedBranchServiceTreeNodeResponse ToResponse(
        CreatedBranchServiceTreeNode node)
    {
        return new CreatedBranchServiceTreeNodeResponse
        {
            Id = node.Service.Id,
            ParentServiceId = node.Service.ParentServiceId,
            ArabicName = node.Service.ArabicName,
            EnglishName = node.Service.EnglishName,
            ServiceCode = node.Service.ServiceCode,
            IsServiceCodeRequired = node.Service.IsServiceCodeRequired,
            Scope = node.Service.Scope,
            OwnerBranchId = node.Service.OwnerBranchId.GetValueOrDefault(),
            IsTicketIssuable = node.Service.IsTicketIssuable,
            Children = node.Children
                .Select(ToResponse)
                .ToList()
        };
    }

    private static CreatedBranchServiceTreeNode Build(
        CreateBranchServiceTreeNodeCommand requestNode,
        Service parentService,
        int branchId,
        Guid createdByApplicationUserId,
        ICollection<Service> services,
        ICollection<BranchService> assignments)
    {
        var service = Service.CreateBranchScoped(
            parentService,
            branchId,
            requestNode.ArabicName,
            requestNode.EnglishName,
            requestNode.ServiceCode,
            requestNode.IsServiceCodeRequired,
            requestNode.ArabicUserMessage,
            requestNode.EnglishUserMessage,
            requestNode.IsTicketIssuable.GetValueOrDefault(),
            requestNode.IsClientInputRequired,
            requestNode.HasReservation,
            requestNode.OrderNo,
            requestNode.Priority,
            requestNode.RangePrefix,
            requestNode.RangeStartNumber,
            requestNode.RangeEndNumber,
            requestNode.WaitingDuration,
            requestNode.NoOfTicketCopies,
            createdByApplicationUserId);

        return BuildCore(
            requestNode,
            service,
            branchId,
            createdByApplicationUserId,
            services,
            assignments);
    }

    private static CreatedBranchServiceTreeNode BuildCore(
        CreateBranchServiceTreeNodeCommand requestNode,
        Service service,
        int branchId,
        Guid createdByApplicationUserId,
        ICollection<Service> services,
        ICollection<BranchService> assignments)
    {
        services.Add(service);
        assignments.Add(BranchService.Create(
            branchId,
            service,
            createdByApplicationUserId));

        var createdNode = new CreatedBranchServiceTreeNode(service);

        foreach (var child in requestNode.Children ??
                 Array.Empty<CreateBranchServiceTreeNodeCommand>())
        {
            createdNode.Children.Add(Build(
                child,
                service,
                branchId,
                createdByApplicationUserId,
                services,
                assignments));
        }

        return createdNode;
    }
}

internal sealed class CreatedBranchServiceTreeNode
{
    public CreatedBranchServiceTreeNode(Service service)
    {
        Service = service;
    }

    public Service Service { get; }

    public List<CreatedBranchServiceTreeNode> Children { get; } = new();
}
