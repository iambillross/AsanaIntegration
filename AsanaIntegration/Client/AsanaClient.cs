using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AsanaIntegration.Client
{
    public class AsanaClient
    {
        public String Base64Key { get; set; }
        private readonly String _asanaApiUrl;

        public AsanaClient(String asanaApiUrl, String apiKey)
        {
            Base64Key = Base64Encode(apiKey + ":");
            _asanaApiUrl = asanaApiUrl;
        }

        /// <summary>
        /// Async method gets all assigned workspaces from Asana.
        /// </summary>
        /// <returns>JObject json object</returns>
        public async Task<JObject> GetWorkspaces()
        {
            String asanaUrl = _asanaApiUrl + "/workspaces";

            return JObject.Parse(await GetFromApi(asanaUrl));
        }

        /// <summary>
        /// Async method gets all assigned projects for a given workspace.
        /// </summary>
        /// <param name="workspaceId">The ID of the Asana workspace</param>
        /// <returns>JObject json object</returns>
        public async Task<JObject> GetWorkspaceProjects(long workspaceId)
        {
            String asanaUrl = _asanaApiUrl + "/workspaces/" + workspaceId + "/projects";

            return JObject.Parse(await GetFromApi(asanaUrl));
        }

        /// <summary>
        /// Async method gets all assigned tasks for a given project.
        /// </summary>
        /// <param name="projectId">The ID of the Asana project</param>
        /// <returns>JObject of project tasks</returns>
        public async Task<JObject> GetProjectTasks(long projectId)
        {
            String asanaUrl = _asanaApiUrl + "/projects/" + projectId + "/tasks";

            return JObject.Parse(await GetFromApi(asanaUrl));
        }

        /// <summary>
        /// Async method gets all assigned teams for a given organization.
        /// </summary>
        /// <param name="organizationId">An Organization is the same as a Workspace</param>
        /// <returns>JObject of the organization teams</returns>
        public async Task<JObject> GetTeams(long organizationId)
        {
            String asanaUrl = _asanaApiUrl + "/organizations/" + organizationId + "/teams";

            return JObject.Parse(await GetFromApi(asanaUrl));
        }

        /// <summary>
        /// Async method posts a new project to a given workspace and team.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="workspaceId"></param>
        /// <param name="teamId"></param>
        /// <returns>JObject of the creatd project</returns>
        public async Task<JObject> PostProject(String name, long workspaceId, long teamId)
        {
            String asanaUrl = _asanaApiUrl + "/projects";

            List<KeyValuePair<String, String>> queryParameters = new List<KeyValuePair<String, String>>()
            {
                new KeyValuePair<String, String>("name", name),
                new KeyValuePair<String, String>("workspace", workspaceId.ToString()),
                new KeyValuePair<String, String>("team", teamId.ToString())
            };

            return JObject.Parse(await PostToApi(asanaUrl, queryParameters));
        }

        #region Private memebers
        private String Base64Encode(string plainText)
        {
            Byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);

            return Convert.ToBase64String(plainTextBytes);
        }

        private async Task<String> GetFromApi(String asanaUrl)
        {
            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization =  new AuthenticationHeaderValue("Basic", Base64Key);

            HttpResponseMessage response = await httpClient.GetAsync(asanaUrl);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return await response.Content.ReadAsStringAsync();
            }

            throw new HttpRequestException("Asana API returned status code " + response.StatusCode);
        }

        private async Task<String> PostToApi(String asanaUrl, IEnumerable<KeyValuePair<String, String>> queryKeyValuePairs)
        {
            HttpClient httpClient = new HttpClient();
                
            httpClient.DefaultRequestHeaders.Authorization =  new AuthenticationHeaderValue("Basic", Base64Key);

            FormUrlEncodedContent content = new FormUrlEncodedContent(queryKeyValuePairs);
            HttpResponseMessage response = await httpClient.PostAsync(asanaUrl, content);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                return await response.Content.ReadAsStringAsync();
            }

            throw new HttpRequestException("Asana API returned status code " + response.StatusCode);
        }
        #endregion
    }
}
