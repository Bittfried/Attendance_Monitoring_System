using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Attendance_Monitoring_System
{
    internal static class AttendanceApiClient
    {
        private const string SupabaseUrl = "https://klydsxazcmxavgqvxrjv.supabase.co";
        // Publishable keys are not secrets; Supabase RLS and RPC permissions must enforce access.
        private const string SupabasePublishableKey = "sb_publishable_By0K2pvbnVBRQ8tp_Ny-dg_qCExPABw";

        private static readonly HttpClient Client = CreateClient();

        public static async Task LogAttendanceAsync(string rawCode)
        {
            using (var content = new StringContent(
                JsonConvert.SerializeObject(new { raw_code = rawCode }),
                Encoding.UTF8,
                "application/json"))
            using (var response = await Client.PostAsync(
                SupabaseUrl + "/rest/v1/rpc/log_attendance",
                content))
            {
                response.EnsureSuccessStatusCode();
            }
        }

        public static async Task<List<Attendance>> GetTodayAsync()
        {
            using (var response = await Client.GetAsync(
                SupabaseUrl + "/rest/v1/attendance_today_view"
                + "?select=first_name,last_name,time_in,time_out&order=time_in.desc"))
            {
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Attendance>>(json)
                    ?? new List<Attendance>();
            }
        }

        private static HttpClient CreateClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

            client.DefaultRequestHeaders.Add("apikey", SupabasePublishableKey);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + SupabasePublishableKey);

            return client;
        }
    }

    public class Attendance
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public DateTime? time_in { get; set; }
        public DateTime? time_out { get; set; }
    }
}
