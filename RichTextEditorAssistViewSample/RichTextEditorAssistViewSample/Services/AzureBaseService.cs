using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace RichTextEditorAssistViewSample
{
    public abstract class AzureBaseService
    {
        #region Fields
        /// <summary>
        /// The EndPoint
        /// </summary>
        private const string endpoint = "END_POINT";

        /// <summary>
        /// The Deployment name
        /// </summary>
        internal const string deploymentName = "DEPLOYMENT_NAME";

        /// <summary>
        /// The Image Deployment name
        /// </summary>
        internal const string imageDeploymentName = "IMAGE_MODEL_NAME";

        /// <summary>
        /// The API key
        /// </summary>
        private const string key = "KEY";

        /// <summary>
        /// The kernal
        /// </summary>
        internal IChatClient? Client { get; set; }

        /// <summary>
        /// The chat histroy
        /// </summary>
        internal string? ChatHistory { get; set; }

        /// <summary>
        /// The credential valid field
        /// </summary>
        private static bool _isCredentialValid = false;

        /// <summary>
        /// The already credential validated field
        /// </summary>
        private static bool isAlreadyValidated = false;
        #endregion

        public AzureBaseService()
        {
            ValidateCredential();
        }

        #region Properties
        /// <summary>
        /// Gets or Set a value indicating whether an credentials are valid or not.
        /// </summary>
        internal static bool IsCredentialValid
        {
            get => _isCredentialValid;
            set
            {
                if (_isCredentialValid != value)
                {
                    _isCredentialValid = value;
                    OnCredentialChanged?.Invoke(value);
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Validate Azure Credentials
        /// </summary>
        private async void ValidateCredential()
        {
            this.GetAzureOpenAIKernal();

            if (isAlreadyValidated)
            {
                return;
            }

            try
            {
                if (Client != null)
                {
                    await Client!.CompleteAsync("Hello, Test Check");
                    ChatHistory = string.Empty;
                    IsCredentialValid = true;
                    isAlreadyValidated = true;
                }
                else
                {
                    ShowAlertAsync();
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// Show Alert Popup
        /// </summary>
        private async void ShowAlertAsync()
        {
            var page = Application.Current?.Windows[0].Page;
            if (page != null && !IsCredentialValid)
            {
                isAlreadyValidated = true;
#if NET10_0
                await page.DisplayAlertAsync("Alert", "The Azure API key or endpoint is missing or incorrect. Please verify your credentials. You can also continue with the offline data.", "OK");
#else
                await page.DisplayAlert("Alert", "The Azure API key or endpoint is missing or incorrect. Please verify your credentials. You can also continue with the offline data.", "OK");
#endif
            }
        }
        #endregion

        #region Azure OpenAI
        /// <summary>
        /// To get the Azure open ai kernal method
        /// </summary>
        private void GetAzureOpenAIKernal()
        {
            var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key))
                      .AsChatClient(modelId: deploymentName);
            this.Client = client;
        }
        #endregion

        #region Events
        /// <summary>
        /// Represents the method that will handle IsCredentialValid change event.
        /// </summary>
        internal delegate void CredentialValidated(bool isCredentialValid);

        /// <summary>
        /// Represents the event when IsCredentialValid value is changed.
        /// </summary>
        internal static event CredentialValidated? OnCredentialChanged;
        #endregion

    }

    /// <summary>
    /// Provides functionality for interacting with an Azure-based AI service, enabling the execution of AI-powered
    /// operations such as generating responses to user prompts.
    /// </summary>
    public class AzureAIService : AzureBaseService
    {
        public AzureAIService()
        {

        }

        /// <summary>
        /// Method to get the results from AI service.
        /// </summary>
        /// <param name="userPrompt"></param>
        /// <param name="userAIPrompt"></param>
        /// <returns></returns>
        internal async Task<string> GetResultsFromAI(string userAIPrompt)
        {
            if (IsCredentialValid && Client != null)
            {
                ChatHistory = string.Empty;
                // Add the system message and user message to the options
                ChatHistory = ChatHistory + userAIPrompt;
                try
                {
                    var response = await Client.CompleteAsync(ChatHistory);
                    return response.ToString();
                }
                catch
                {
                    return string.Empty;
                }
            }

            return string.Empty;
        }
    }
}
