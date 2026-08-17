using BuildingBlock.Application.MultiTenancy;
using Microsoft.EntityFrameworkCore;

using BuildingBlock.Infrastracture.Extensions;
using BuildingBlock.Infrastracture.MultiTenancy;
using BuildingBlock.Infrastracture.Presistence;
using Qcontrol.Domain.Identity;
using QControl.Application.Abstraction.Security;
using QControl.Domain.Entities;
using QControl.Domain.Identity;

namespace QControl.infrastructure.Persistence
{
    public class PlatformWriteDbContext : DbContext
    {
        private readonly ICurrentTenantContext _tenantContext;
        private readonly ICurrentBranchContext _branchContext;

        // ✅ دي اللي الفلتر هيعتمد عليها (per DbContext instance)
        public Guid? CurrentAccountId => _tenantContext.AccountId;

        public bool IsPlatformAdmin => _tenantContext.IsPlatformAdmin;

        public bool IsBranchScopeEnabled => _branchContext.IsBranchActor;

        public int? CurrentBranchId => _branchContext.ActiveBranchId;

        public DbSet<ServiceSchedule> ServiceSchedules =>
            Set<ServiceSchedule>();

        public DbSet<ServiceScheduleTimeSlot> ServiceScheduleTimeSlots =>
            Set<ServiceScheduleTimeSlot>();

        public DbSet<GeneralBrand> GeneralBrands => Set<GeneralBrand>();

        public PlatformWriteDbContext(
            DbContextOptions<PlatformWriteDbContext> options,
            ICurrentTenantContext tenantContext,
            ICurrentBranchContext branchContext)
            : base(options)
        {
            _tenantContext = tenantContext;
            _branchContext = branchContext;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyWriteConfigurations(typeof(PlatformWriteDbContext).Assembly);
            modelBuilder.ApplySoftDeleteQueryFilter();

            modelBuilder.ApplyTenantQueryFilters(this);
            ApplyBranchScopeFilters(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private void ApplyBranchScopeFilters(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Branch>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.Id == CurrentBranchId &&
                     x.IsActive));

            modelBuilder.Entity<Location>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.BranchId == CurrentBranchId &&
                     x.Branch.IsActive));

            modelBuilder.Entity<BranchBranding>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.BranchId == CurrentBranchId &&
                     x.Branch.IsActive));

            modelBuilder.Entity<BranchAdvertisement>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.BranchId == CurrentBranchId &&
                     x.Branch.IsActive));

            modelBuilder.Entity<BranchVideo>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.BranchId == CurrentBranchId &&
                     x.Branch.IsActive));

            modelBuilder.Entity<WaitingArea>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.BranchId == CurrentBranchId &&
                     x.Branch.IsActive));

            modelBuilder.Entity<Window>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.BranchId == CurrentBranchId &&
                     x.WaitingArea.Branch.IsActive));

            modelBuilder.Entity<Terminal>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.BranchId == CurrentBranchId &&
                     x.Window.WaitingArea.Branch.IsActive));

            modelBuilder.Entity<Display>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.BranchId == CurrentBranchId &&
                     x.Branch.IsActive));

            modelBuilder.Entity<DisplayWindow>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.Display.BranchId == CurrentBranchId &&
                     x.Window.BranchId == CurrentBranchId &&
                     x.Display.Branch.IsActive &&
                     x.Window.WaitingArea.Branch.IsActive));

            modelBuilder.Entity<BranchService>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.BranchId == CurrentBranchId &&
                     x.Branch.IsActive));

            modelBuilder.Entity<BranchServiceSegment>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.BranchService.BranchId == CurrentBranchId &&
                     x.BranchService.Branch.IsActive));

            modelBuilder.Entity<SegmentGlobalizationRequest>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.BranchId == CurrentBranchId &&
                     x.Branch.IsActive));

            modelBuilder.Entity<ServiceWorkflow>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.BranchId == CurrentBranchId &&
                     x.Branch.IsActive));

            modelBuilder.Entity<ServiceSchedule>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.BranchId == CurrentBranchId &&
                     x.Branch.IsActive));

            modelBuilder.Entity<ServiceScheduleTimeSlot>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.ServiceSchedule.BranchId == CurrentBranchId &&
                     x.ServiceSchedule.Branch.IsActive));

            modelBuilder.Entity<ApplicationUserBranch>()
                .HasQueryFilter(x =>
                    !IsBranchScopeEnabled ||
                    (CurrentBranchId.HasValue &&
                     x.BranchId == CurrentBranchId &&
                     x.Branch.IsActive));
        }
    }
}
