using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
namespace BlazorAut.Services
{
    public class ClientInfoMiddleware
    { //to use in home page
        private readonly RequestDelegate _next;

        public ClientInfoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Capture client and server data
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            var ipFromXForwarded = GetClientIps(context);
            var serverName = Environment.MachineName;
            var osVersion = Environment.OSVersion.ToString();
            var osDescription = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
            var osArchitecture = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString();
            var currentDirectory = Environment.CurrentDirectory;
            var processorCount = Environment.ProcessorCount.ToString();
            var systemDirectory = Environment.SystemDirectory;
            var logicalDrives = String.Join(", ", Environment.GetLogicalDrives());
            var systemPageSize = Environment.SystemPageSize.ToString();
            var userDomainName = Environment.UserDomainName;
            var userName = Environment.UserName;
            var is64BitOs = Environment.Is64BitOperatingSystem.ToString();
            var is64BitProcess = Environment.Is64BitProcess.ToString();
            var clrVersion = Environment.Version.ToString();

            // Assign values to context
            context.Items["ClientIp"] = clientIp;
            context.Items["UserAgent"] = userAgent;
            context.Items["ServerName"] = serverName;
            context.Items["OSVersion"] = osVersion;
            context.Items["OSDescription"] = osDescription;
            context.Items["OSArchitecture"] = osArchitecture;
            context.Items["CurrentDirectory"] = currentDirectory;
            context.Items["ProcessorCount"] = processorCount;
            context.Items["SystemDirectory"] = systemDirectory;
            context.Items["LogicalDrives"] = logicalDrives;
            context.Items["SystemPageSize"] = systemPageSize;
            context.Items["UserDomainName"] = userDomainName;
            context.Items["UserName"] = userName;
            context.Items["Is64BitOs"] = is64BitOs;
            context.Items["Is64BitProcess"] = is64BitProcess;
            context.Items["ClrVersion"] = clrVersion;
            context.Items["IpFromXForwarded"] = ipFromXForwarded;
            // Call the next middleware in the pipeline
            await _next(context);
        }

        public string GetClientIps(HttpContext context)
        {

            var forwardedHeader = context.Request.Headers["X-Forwarded-For"];
            if (!string.IsNullOrEmpty(forwardedHeader))
            {

                var ips = forwardedHeader.ToString().Split(',');

                // Обрезаем пробелы и объединяем IP-адреса с разделителем |
                var joinedIps = string.Join("|", ips.Select(ip => ip.Trim()));


                return joinedIps;
            }


            return "no X-Forwarded-For";
            //context.Connection.RemoteIpAddress?.ToString();
        }

    }
}
