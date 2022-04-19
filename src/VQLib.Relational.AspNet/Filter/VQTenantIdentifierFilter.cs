using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using VQLib.Jwt;
using VQLib.Relational.Entity;
using VQLib.Relational.Repository;
using VQLib.Session;

namespace VQLib.Relational.AspNet.Filter
{
    public class VQTenantIdentifierFilter<T> : IAsyncResourceFilter where T : VQBaseEntity
    {
        public const string CLAIM_TENANT_KEY = "TenantKey";

        private readonly IVQSessionService _vQSessionService;
        private readonly IVQRepositoryWithoutTenant<T> _tenantEntityRepository;

        public VQTenantIdentifierFilter(IVQSessionService vQSessionService, IVQRepositoryWithoutTenant<T> tenantEntityRepository)
        {
            _vQSessionService = vQSessionService;
            _tenantEntityRepository = tenantEntityRepository;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var claims = context.HttpContext.GetClaims();

            if (claims == null)
            {
                _ = await next();
                return;
            }

            await FillTenantId(_vQSessionService, claims, context);
            await FillOtherData(_vQSessionService, claims, context);

            _ = await next();
        }

        protected virtual async Task FillTenantId(IVQSessionService _vQSessionService, IEnumerable<Claim> claims, ResourceExecutingContext context)
        {
            var tenantKey = claims.FirstOrDefault(x => x.Type == CLAIM_TENANT_KEY)?.Value;
            if (string.IsNullOrWhiteSpace(tenantKey))
                throw new KeyNotFoundException();

            var id = await _tenantEntityRepository.GetIdByKey(tenantKey, context.HttpContext.RequestAborted);

            _vQSessionService.TenantId = id;
        }

        protected virtual Task FillOtherData(IVQSessionService _vQSessionService, IEnumerable<Claim> claims, ResourceExecutingContext context)
        {
            return Task.CompletedTask;
        }
    }
}