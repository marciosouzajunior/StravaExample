using Newtonsoft.Json;
using StravaExample.Models;
using StravaExample.Services.Impl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(StravaService))]
namespace StravaExample.Services.Impl
{
    public class StravaService : IStravaService
    {
        private IDialog dialog;
        private IProperties properties;
        private IDatabase database;
        private IHttpHandler httpHandler;

        private string clientId = "ReplaceWithClientID";
        private string clientSecret = "ReplaceWithClientSecret";
        private string redirectUri = "myapp://myapp.com";
        private string requestScope = "activity:read_all,activity:write";

        public StravaService()
        {
            dialog = DependencyService.Get<IDialog>();
            properties = DependencyService.Get<IProperties>();
            database = DependencyService.Get<IDatabase>();
            httpHandler = DependencyService.Get<IHttpHandler>();
            httpHandler.BaseAddress = new Uri("https://www.strava.com");
        }

        public StravaService(IDialog dialog, IProperties properties, IDatabase database, IHttpHandler httpHandler)
        {
            this.dialog = dialog;
            this.properties = properties;
            this.database = database;
            this.httpHandler = httpHandler;
            httpHandler.BaseAddress = new Uri("https://www.strava.com");
        }

        public async Task Connect()
        {
            try
            {
                // Request access
                string authURL = "https://www.strava.com/api/v3/oauth/authorize" +
                    "?client_id=" + clientId +
                    "&redirect_uri=" + redirectUri +
                    "&response_type=code" +
                    "&approval_prompt=auto" +
                    "&scope=" + requestScope;

                WebAuthenticatorResult authResult = await WebAuthenticator.AuthenticateAsync(
                    new Uri(authURL),
                    new Uri(redirectUri));

                if (authResult == null
                    || !authResult.Properties.ContainsKey("code")
                    || !authResult.Properties.ContainsKey("scope")
                    || !CheckScope(authResult.Properties["scope"]))
                {
                    Console.WriteLine("Fail to connect.");
                    return;
                }

                // Token exchange               
                string responseCode = authResult.Properties["code"];
                StravaTokenRequest tokenRequest = new StravaTokenRequest()
                {
                    client_id = clientId,
                    client_secret = clientSecret,
                    code = responseCode,
                    grant_type = "authorization_code"
                };
                StringContent content = new StringContent(
                    JsonConvert.SerializeObject(tokenRequest),
                    Encoding.UTF8,
                    "application/json");
                HttpResponseMessage httpResponseMessage = await httpHandler.PostAsync("/api/v3/oauth/token", content);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    string tokenResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                    StravaTokenResponse stravaTokenResponse = JsonConvert.DeserializeObject<StravaTokenResponse>(tokenResponse);
                    properties.Save("stravaToken", JsonConvert.SerializeObject(stravaTokenResponse));
                    await dialog.DisplayAlert("Connect", "Connected to Strava.", "OK");
                }
                else
                {
                    await dialog.DisplayAlert("Connect", "Fail to connect.", "OK");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Fail to connect: " + e.Message);
            }
        }

        public bool CheckScope(string responseScope)
        {
            string[] requestedScopeArray = requestScope.Split(',');
            foreach (string requestedScopeItem in requestedScopeArray)
            {
                if (!responseScope.Contains(requestedScopeItem))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task Disconnect()
        {
            try
            {
                string accessToken = await GetAccessToken();
                HttpResponseMessage httpResponseMessage =
                    await httpHandler.PostAsync("/api/v3/oauth/deauthorize?access_token=" + accessToken, null);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    await dialog.DisplayAlert("Disconnect", "Disconnected from Strava.", "OK");
                }
                else
                {
                    await dialog.DisplayAlert("Disconnect", "Fail to disconnect from Strava.", "OK");
                }
                properties.Remove("stravaToken");
                properties.Remove("stravaSyncDate");
            }
            catch (Exception e)
            {
                Console.WriteLine("Fail to disconnect from Strava: " + e.Message);
            }
        }

        public bool IsConnected()
        {
            try
            {
                return properties.Contains("stravaToken");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error checking if is connected: " + e.Message);
                return false;
            }
        }

        public async Task<string> GetAccessToken()
        {
            try
            {
                bool containsStravaToken = properties.Contains("stravaToken");
                if (!containsStravaToken)
                {
                    return null;
                }

                // Get saved token
                string stravaTokenStr = (string)properties.Get("stravaToken");
                StravaTokenResponse stravaToken = JsonConvert.DeserializeObject<StravaTokenResponse>(stravaTokenStr);

                // Token is still valid
                DateTime tokenExpiresAt = DateTimeUtil.ConvertEpochToDateTime(stravaToken.expires_at);
                if (tokenExpiresAt > DateTime.Now)
                {
                    return stravaToken.access_token;
                }

                // Request new token
                StravaRefreshTokenRequest refreshTokenRequest = new StravaRefreshTokenRequest()
                {
                    client_id = clientId,
                    client_secret = clientSecret,
                    grant_type = "refresh_token",
                    refresh_token = stravaToken.refresh_token
                };
                StringContent content = new StringContent(
                    JsonConvert.SerializeObject(refreshTokenRequest),
                    Encoding.UTF8,
                    "application/json");
                HttpResponseMessage httpResponseMessage = await httpHandler.PostAsync("/api/v3/oauth/token", content);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    string tokenResult = await httpResponseMessage.Content.ReadAsStringAsync();
                    stravaToken = JsonConvert.DeserializeObject<StravaTokenResponse>(tokenResult);
                    properties.Save("stravaToken", JsonConvert.SerializeObject(stravaToken));
                    return stravaToken.access_token;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting access token: " + e.Message);
                return null;
            }
        }

        public async Task NewActivity(AppActivity appActivity)
        {

            if (!IsConnected())
            {
                return;
            }

            try
            {
                StravaSync stravaSync = new StravaSync()
                {
                    AppActivityId = appActivity.Id
                };
                await database.Insert(stravaSync);
                await dialog.DisplayAlert("NewActivity", "Activity inserted and ready to sync.", "OK");
            }
            catch (Exception e)
            {
                await dialog.DisplayAlert("NewActivity", "Error adding activity: " + e.Message, "OK");
            }
        }

        public async Task<bool> UploadActivities()
        {

            StreamContent gpxStreamContent = null;
            MultipartFormDataContent multipartFormDataContent = null;

            try
            {
                List<StravaSync> stravaSyncList = await database.GetAll<StravaSync>();
                foreach (StravaSync stravaSync in stravaSyncList)
                {

                    AppActivity appActivity = await database.Get<AppActivity>(stravaSync.AppActivityId);
                    List<AppActivityHR> appActivityHRList = await database.Query<AppActivityHR>(
                        "SELECT * FROM AppActivityHR WHERE AppActivityId = ? ORDER BY SecondsMeasure;", appActivity.Id);

                    // Create form data with gpx file
                    string gpxString = ConvertAppActivityToStravaGPX(appActivity, appActivityHRList);
                    gpxStreamContent = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(gpxString)));
                    multipartFormDataContent = new MultipartFormDataContent();
                    multipartFormDataContent.Add(gpxStreamContent, "file", "myapp_activity_" + appActivity.Id + "_.gpx");

                    // Create request
                    string accessToken = await GetAccessToken();
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v3/uploads"
                        + "?data_type=gpx"
                        + "&name=" + appActivity.Name
                        + "&external_id=myapp_" + appActivity.Id);
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    requestMessage.Content = multipartFormDataContent;
                    HttpResponseMessage httpResponseMessage = await httpHandler.SendAsync(requestMessage);

                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        await database.Delete(stravaSync);
                    }
                    else
                    {
                        return false;
                    }

                }

                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("Error uploading activities: " + e.Message);
                return false;
            }
            finally
            {
                gpxStreamContent?.Dispose();
                multipartFormDataContent?.Dispose();
            }
        }

        public async Task<bool> DownloadActivities()
        {
            try
            {
                // Get activity list
                long stravaSyncDate = GetSyncDate();
                string accessToken = await GetAccessToken();
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/v3/athlete/activities?after=" + stravaSyncDate);
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage httpResponseMessage = await httpHandler.SendAsync(requestMessage);

                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    return false;
                }

                // Get activity stream
                string streamResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                List<StravaActivity> activitiesList = JsonConvert.DeserializeObject<List<StravaActivity>>(streamResponse);

                // Ignore activites from sended by app itself
                activitiesList.RemoveAll(a => a.external_id.Contains("myapp"));

                foreach (StravaActivity activity in activitiesList)
                {
                    requestMessage = new HttpRequestMessage(HttpMethod.Get,
                        "/api/v3/activities/" + activity.id + "/streams?keys=time,heartrate&key_by_type=true");
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    httpResponseMessage = await httpHandler.SendAsync(requestMessage);

                    if (!httpResponseMessage.IsSuccessStatusCode)
                    {
                        return false;
                    }

                    streamResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                    StravaActivityStream activityStream = JsonConvert.DeserializeObject<StravaActivityStream>(streamResponse);
                    activity.activityStream = activityStream;
                }

                UpdateSyncDate();
                await SaveActivities(activitiesList);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error downloading activities: " + e.Message);
                return false;
            }
        }

        public async Task SaveActivities(List<StravaActivity> activitiesList)
        {
            try
            {
                AppActivity appActivity;
                List<AppActivityHR> appActivityHRList;
                foreach (StravaActivity activity in activitiesList)
                {
                    appActivity = ConvertStravaActivityToAppActivity(activity);
                    await database.Insert(appActivity);
                    appActivityHRList = ConvertStravaActivityStreamToAppActivityHR(activity.activityStream, appActivity.Id);
                    await database.InsertAll(appActivityHRList);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error saving activities: " + e.Message);
            }
        }

        public AppActivity ConvertStravaActivityToAppActivity(StravaActivity stravaActivity)
        {
            AppActivity appActivity = new AppActivity();
            appActivity.Name = stravaActivity.name;
            appActivity.StartDate = stravaActivity.start_date_local;
            appActivity.ElapsedTime = stravaActivity.elapsed_time;
            return appActivity;
        }

        private List<AppActivityHR> ConvertStravaActivityStreamToAppActivityHR(StravaActivityStream activityStream, int appActivityId)
        {
            List<AppActivityHR> appActivityHRList = new List<AppActivityHR>();
            AppActivityHR appActivityHR;

            if (activityStream.heartrate == null
                || activityStream.time == null
                || activityStream.heartrate.original_size != activityStream.time.original_size)
            {
                return appActivityHRList;
            }

            for (int index = 0; index < activityStream.time.original_size; index++)
            {
                appActivityHR = new AppActivityHR
                {
                    AppActivityId = appActivityId,
                    HR = activityStream.heartrate.data[index],
                    SecondsMeasure = activityStream.time.data[index]
                };
                appActivityHRList.Add(appActivityHR);
            }
            return appActivityHRList;
        }

        public string ConvertAppActivityToStravaGPX(AppActivity appActivity, List<AppActivityHR> appActivityHRList)
        {

            string trkpt = "";
            DateTime hrMeasureDate;
            foreach (AppActivityHR hrActivity in appActivityHRList)
            {
                hrMeasureDate = appActivity.StartDate.AddSeconds(hrActivity.SecondsMeasure);
                trkpt += ""
                    + "<trkpt>"
                    + "<time>" + DateTimeUtil.ConvertDateTimeToISO8601(hrMeasureDate) + "</time>"
                    + "<extensions>"
                    + "<gpxtpx:TrackPointExtension>"
                    + "<gpxtpx:hr>" + hrActivity.HR + "</gpxtpx:hr>"
                    + "</gpxtpx:TrackPointExtension>"
                    + "</extensions>"
                    + "</trkpt>";
            }

            string gpx = ""
                + "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                + "<gpx xmlns=\"http://www.topografix.com/GPX/1/1\" xmlns:gpxtpx=\"http://www.garmin.com/xmlschemas/TrackPointExtension/v1\" xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\" xmlns:ns1=\"http://www.cluetrust.com/XML/GPXDATA/1/0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" creator=\"MyApp\" version=\"1.3\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.garmin.com/xmlschemas/GpxExtensions/v3 http://www.garmin.com/xmlschemas/GpxExtensionsv3.xsd http://www.garmin.com/xmlschemas/TrackPointExtension/v1 http://www.garmin.com/xmlschemas/TrackPointExtensionv1.xsd\">"
                + "<metadata>"
                + "<time>" + DateTimeUtil.ConvertDateTimeToISO8601(appActivity.StartDate) + "</time>"
                + "</metadata>"
                + "<trk>"
                + "<name>" + appActivity.Name + "</name>"
                + "<trkseg>"
                + trkpt
                + "</trkseg>"
                + "</trk>"
                + "</gpx>";

            return gpx;

        }

        public long GetSyncDate()
        {
            bool containsStravaSyncDate = properties.Contains("stravaSyncDate");
            long stravaSyncDate;
            if (containsStravaSyncDate)
            {
                stravaSyncDate = (long)properties.Get("stravaSyncDate");
            }
            else
            {
                stravaSyncDate = DateTimeUtil.ConvertDateTimeToEpoch(DateTime.Now.ToUniversalTime());
                properties.Save("stravaSyncDate", stravaSyncDate);
            }
            return stravaSyncDate;
        }

        public long UpdateSyncDate()
        {
            long syncDate = DateTimeUtil.ConvertDateTimeToEpoch(DateTime.Now.ToUniversalTime());
            properties.Save("stravaSyncDate", syncDate);
            return syncDate;
        }

        public static Task SyncTask;
        public event EventHandler OnSyncCompleted;

        public void Sync()
        {
            if (IsConnected() && (SyncTask == null || SyncTask.IsCompleted))
            {
                SyncTask = Task.Run(async () =>
                {
                    bool downloadResult = await DownloadActivities();
                    bool uploadResult = await UploadActivities();
                    UpdateSyncDate();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        OnSyncCompleted?.Invoke(this, new SyncCompletedEventArgs(downloadResult, uploadResult));
                    });
                });
            }
        }

    }

    public class SyncCompletedEventArgs : EventArgs
    {
        public bool DownloadResult { get; set; }
        public bool UploadResult { get; set; }
        public SyncCompletedEventArgs(bool downloadResult, bool uploadResult)
        {
            DownloadResult = downloadResult;
            UploadResult = uploadResult;
        }
    }
}
