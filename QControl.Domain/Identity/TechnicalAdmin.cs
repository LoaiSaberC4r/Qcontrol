using BuildingBlock.Domain.EntitiesHelper;
using Qcontrol.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QControl.Domain.Identity
{
    public sealed class TechnicalAdmin : AggregateRoot<Guid>
    {
        public Guid ApplicationUserId { get; private set; }

        public ApplicationUser ApplicationUser { get; private set; } = null!;

        public Guid CreatedByApplicationUserId { get; private set; }

        private TechnicalAdmin()
        {
        }

        private TechnicalAdmin(Guid id)
            : base(id)
        {
        }

        public static TechnicalAdmin CreateSeeded(
            Guid id,
            Guid applicationUserId)
        {
            return new TechnicalAdmin(id)
            {
                ApplicationUserId = applicationUserId,
                CreatedByApplicationUserId = applicationUserId
            };
        }

        public static TechnicalAdmin Create(
            Guid applicationUserId,
            Guid createdByApplicationUserId)
        {
            return new TechnicalAdmin(Guid.NewGuid())
            {
                ApplicationUserId = applicationUserId,
                CreatedByApplicationUserId = createdByApplicationUserId
            };
        }
    }
}