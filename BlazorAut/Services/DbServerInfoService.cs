using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using BlazorAut.Data;

namespace BlazorAut.Services
{
    public class DbServerInfoService
    {
        private readonly ApplicationDbContext _context;

        public DbServerInfoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DbServerInfo> GetDbServerInfoAsync()
        {
            var result = await _context.DbServerInfo
                .FromSqlRaw(@"
            SELECT
                pg_stat_activity.usename AS ""CurrentUser"",
                CASE
                    WHEN pg_user.usesysid = 10 THEN 'superuser'
                    ELSE 'regular user'
                END AS ""AuthType"",
                CASE
                    WHEN pg_shadow.passwd IS NOT NULL THEN 'encrypted'
                    ELSE 'not encrypted'
                END AS ""EncryptionType"",
                host(client_addr) AS ""ClientIp"",
                backend_start AS ""LoginDate"",
                version() AS ""ServerVersion"",
                pg_postmaster_start_time() AS ""ServerStartTime"",
                inet_server_addr()::text AS ""ServerName"",
                current_database() AS ""CurrentDatabase""
            FROM
                pg_stat_activity
            JOIN
                pg_user ON pg_stat_activity.usename = pg_user.usename
            LEFT JOIN
                pg_shadow ON pg_user.usesysid = pg_shadow.usesysid
            WHERE
                pid = pg_backend_pid()")
        .FirstOrDefaultAsync();

            return result;
        }
    }

    
}
