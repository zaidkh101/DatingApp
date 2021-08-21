using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var ResultContext = await next();
            if (!ResultContext.HttpContext.User.Identity.IsAuthenticated) return;

            var UserId = ResultContext.HttpContext.User.GetUserId();

            var Repo = ResultContext.HttpContext.RequestServices.GetService<IUserRepository>();

            var User = await Repo.GetUserByIdAsync(UserId);
            User.LastActive = DateTime.Now;
            await Repo.SaveAllAsync();

        }
    }
}
