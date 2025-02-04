using EPiServer.ServiceLocation;
using EPiServer.Shell.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace AlloyDemo.Features.RegisterPersonas
{
    public static class RegisterPersonas
    {
        private static Func<bool> _isLocalRequest = () => false;
        private static Lazy<bool> _arePersonasRegistered = new Lazy<bool>(() => false);
        private static bool? _isEnabled = null;

        public static bool IsEnabled
        {
            get
            {
                if (_isEnabled.HasValue)
                {
                    return _isEnabled.Value;
                }

                var showPersonasRegistration = _isLocalRequest() && !_arePersonasRegistered.Value;
                if (!showPersonasRegistration)
                {
                    _isEnabled = false;
                }

                return showPersonasRegistration;
            }
            set
            {
                _isEnabled = value;
            }
        }

        public static void UseRegisterPersonas(this IApplicationBuilder app, Func<bool> isLocalRequest)
        {
            _isLocalRequest = isLocalRequest;
            _arePersonasRegistered = new Lazy<bool>(ArePersonasRegistered);
            
            if (isLocalRequest())
            {
                app.UseMiddleware<RegisterPersonasMiddleware>();
            }
        }

        private static bool ArePersonasRegistered()
        {
            var provider = ServiceLocator.Current.GetInstance<UIUserProvider>();
            foreach (var user in RegisterPersonasController.Users)
            {
                var task = provider.GetUserAsync(user.UserName);
                task.Wait();
                var u = task.Result;
                if (u == null) return false;
            }
            return true;
        }

        public class RegisterPersonasMiddleware
        {
            private readonly RequestDelegate _next;

            public RegisterPersonasMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                var registerUrl = "/RegisterPersonas";
                if (IsEnabled && !context.Request.Path.StartsWithSegments(registerUrl))
                {
                    context.Response.Redirect(registerUrl);
                    return;
                }

                await _next(context);
            }
        }
    }
}
