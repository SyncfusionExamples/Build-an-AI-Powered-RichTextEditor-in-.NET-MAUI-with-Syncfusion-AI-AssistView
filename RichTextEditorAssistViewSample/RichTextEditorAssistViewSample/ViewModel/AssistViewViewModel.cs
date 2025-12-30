using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.AIAssistView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace RichTextEditorAssistViewSample
{
    public class AssistViewViewModel : INotifyPropertyChanged
    {
        #region Field
        /// <summary>
        /// Field to hold the collection of assist items.
        /// </summary>
        private ObservableCollection<IAssistItem> _assistItems;

        /// <summary>
        /// Field to hold the collection of suggestions.
        /// </summary>
        private ObservableCollection<ISuggestion> _suggestions;

        /// <summary>
        /// Field to hold the Azure AI service instance.
        /// </summary>
        private AzureAIService azureAIService;

        /// <summary>
        /// Field to hold the suggestion item selected command.
        /// </summary>
        private ICommand suggestionItemSelectedCommand;

        /// <summary>
        /// Field to hold the HTML markup for the editor content.
        /// </summary>
        private string editorHtml;
        #endregion

        #region Constructor
        public AssistViewViewModel()
        {
            azureAIService = new AzureAIService();
            this._assistItems = new ObservableCollection<IAssistItem>();
            _suggestions = new ObservableCollection<ISuggestion>
            {
                new AssistSuggestion { Text = "Paraphraser" },
                new AssistSuggestion { Text = "Grammer Checker" }
            };
            SuggestionItemSelectedCommand = new Command(OnSuggestionTapCommand);
        }
        #endregion

        /// <summary>
        /// Gets or sets the collection of assist items associated with this instance.
        /// </summary>
        public ObservableCollection<IAssistItem> AssistItems
        {
            get
            {
                return this._assistItems;
            }

            set
            {
                this._assistItems = value;
                RaisePropertyChanged("AssistItems");
            }
        }

        /// <summary>
        /// Gets or sets the HTML markup used for the editor content.
        /// </summary>
        public string EditorHtml
        {
            get => editorHtml;
            set
            {
                if (editorHtml != value)
                {
                    editorHtml = value;
                    RaisePropertyChanged(nameof(EditorHtml));
                }
            }
        }

        public ObservableCollection<ISuggestion> Suggestions => _suggestions;

        /// <summary>
        /// Gets or sets the suggestion item selected command.
        /// </summary>
        public ICommand SuggestionItemSelectedCommand
        {
            get
            {
                return this.suggestionItemSelectedCommand;
            }
            set
            {
                this.suggestionItemSelectedCommand = value;
            }
        }

        /// <summary>
        /// Handles the tap event for a suggestion item, executing the appropriate action based on the selected
        /// suggestion.
        /// </summary>
        private async void OnSuggestionTapCommand(object obj)
        {
            var args = obj as SuggestionItemSelectedEventArgs;
            if (args == null || args.SelectedItem is not ISuggestion s) return;

            // We handle the flow ourselves (don’t show the built-in loader)
            args.CancelRequest = true;

            switch (s.Text)
            {
                case "Paraphraser":
                    {
                        var request = new AssistItem { Text = s.Text, IsRequested = true };
                        AssistItems.Add(request);
                        await GetResponseWithSuggestion(request).ConfigureAwait(true);
                        break;
                    }

                case "Humanize":
                case "Professional":
                case "Simple":
                case "Academic":
                    {
                        var request = new AssistItem { Text = s.Text, IsRequested = true };
                        AssistItems.Add(request);
                        await GetResult(request).ConfigureAwait(true);
                        break;
                    }
                case "Grammer Checker":
                    {
                        var request = new AssistItem { Text = s.Text, IsRequested = true };
                        AssistItems.Add(request);
                        await GetResult(request).ConfigureAwait(true);
                        break;
                    }

                case "Accept":
                    {
                        if (args.RequestItem is AssistItem responseItem)
                        {
                            EditorHtml = EnsureHtml(responseItem.Text);
                        }
                        break;
                    }
            }

        }

        /// <summary>
        /// Generates a response item with a suggestion for improving the provided input and adds it to the collection
        /// of assist items.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task GetResponseWithSuggestion(object inputQuery)
        {
            await Task.Delay(1000).ConfigureAwait(true);
            if (inputQuery != null)
            {
                var suggestion = this.GetParaphraserSuggestion();
                await Task.Delay(1000).ConfigureAwait(true);
                AssistItem responseItem = new AssistItem() { Text = "Do you want to improve the text?", Suggestion = suggestion };
                responseItem.RequestItem = inputQuery;
                this.AssistItems.Add(responseItem);
            }
        }

        /// <summary>
        /// Creates and returns an assist item suggestion containing a predefined set of paraphrasing options.
        /// </summary>
        /// <returns>An <see cref="AssistItemSuggestion"/> that includes paraphrasing suggestions such as "Humanize",
        /// "Professional", "Simple", and "Academic".</returns>
        private AssistItemSuggestion GetParaphraserSuggestion()
        {
            var promptSuggestions = new AssistItemSuggestion();
            _suggestions = new ObservableCollection<ISuggestion>
            {
                new AssistSuggestion { Text = "Humanize" },
                new AssistSuggestion { Text = "Professional" },
                new AssistSuggestion { Text = "Simple" },
                new AssistSuggestion { Text = "Academic" }
            };
            promptSuggestions.Items = _suggestions;
            return promptSuggestions;
        }

        /// <summary>
        /// Processes the specified input query, retrieves a response from the AI service, and adds the result to the
        /// collection of assist items.
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task GetResult(object inputQuery)
        {
            await Task.Delay(1000).ConfigureAwait(true);
            AssistItem request = (AssistItem)inputQuery;
            if (request != null)
            {
                var userAIPrompt = GetUserAIPrompt(request.Text, EditorHtml);
                var response = await azureAIService!.GetResultsFromAI(request.Text, userAIPrompt).ConfigureAwait(true);
                response = response.Replace("\n", "<br>");
                AssistItem responseItem = new AssistItem() { Text = response, Suggestion = GetAcceptSuggestion() };
                responseItem.RequestItem = inputQuery;
                this.AssistItems.Add(responseItem);
            }
        }

        /// <summary>
        /// Generates an AI prompt string based on the user's command and the provided editor content, tailoring the
        /// instructions for grammar checking or rephrasing as appropriate.
        /// </summary>
        /// <returns>A formatted string containing the AI prompt.</returns>
        private string GetUserAIPrompt(string userPrompt, string editorHtmlContent)
        {
            // If the AI expects plain text, strip tags; otherwise pass HTML as-is
            var editorPlainText = StripHtml(editorHtmlContent ?? string.Empty);

            if (string.Equals(userPrompt, "Grammer Checker", StringComparison.OrdinalIgnoreCase))
            {
                return $"Given user command: {userPrompt}.\n" +
                       $"Source text (from editor):\n{editorPlainText}\n\n" +
                       $@"Act as a STRICT GRAMMAR/COPY EDITOR.

                       Task:
                       - Correct ONLY grammar, spelling, subject–verb, punctuation, pronouns, capitalization, and spacing.
                       - Do NOT rephrase, simplify, or change wording, synonyms, tone, style, or sentence order.
                       - Keep all original meaning and structure.

                       Scope (apply to every sentence in the paragraph, then check cross‑sentence consistency):
                       - Agreement: subject–verb, pronouns, number, person.
                       - Tense/Aspect consistency across sentences.
                       - Articles/determiners (a/an/the), prepositions, conjunctions.
                       - Punctuation: commas (including serial/Oxford if clearly required by the structure), periods, colons/semicolons, em/en dashes, quotes.
                       - Run‑ons and fragments: fix only by minimal punctuation or capitalization changes (no rewriting).
                       - Capitalization (sentences, proper nouns), spelling (US/UK spelling may be preserved consistently).
                       - Spacing: single spaces between words; spaces around punctuation as appropriate.
                       - Numbers, dates, technical terms, names: do not alter wording—only fix capitalization/punctuation if incorrect.
                       - Hyphenation for compound modifiers when needed (e.g., “state‑of‑the‑art”).

                       Formatting and output rules:
                       - Preserve all original line breaks and paragraph breaks.
                       - Preserve whitespace except where a grammar fix requires change.
                       - If the input is already correct, return it EXACTLY unchanged.
                       - OUTPUT: Return ONLY the corrected text, with no explanations, notes, or extra characters.

                       Text:
                       {{plain}}";
            }
            else
            {
                return $"Given user command: {userPrompt}.\n" +
                       $"Source text (from editor):\n{editorPlainText}\n\n" +
                       "Please rephrase while preserving meaning.\n" +
                       "- Provide a humanized tone if requested.\n" +
                       "- Provide a professional tone if requested.\n" +
                       "- Provide a simple tone if requested.\n" +
                       "- Provide an academic tone if requested.\n" +
                       "- Fix grammatical errors without any alternative words if requested.\n" +
                       "- Keep length roughly similar.\n" +
                       "- Return only the modified text without additional commentary.";
            }
        }

        /// <summary>
        /// Creates a suggestion containing a single 'Accept' action item.
        /// </summary>
        /// <returns>An <see cref="AssistItemSuggestion"/> instance with one suggestion item labeled 'Accept'.</returns>
        private AssistItemSuggestion GetAcceptSuggestion()
        {
            return new AssistItemSuggestion
            {
                Items = new ObservableCollection<ISuggestion>
                {
                    new AssistSuggestion { Text = "Accept" },
                }
            };
        }

        /// <summary>
        /// Ensures that the specified content is formatted as HTML.
        /// </summary>
        /// <returns>A string containing the HTML-formatted content.</returns>
        private static string EnsureHtml(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return string.Empty;
            // If it already looks like HTML, return as is; else wrap in <p> and replace line breaks
            return content.Contains("<") ? content : $"<p>{content.Replace("\n", "<br>")}</p>";
        }

        private static string StripHtml(string html) =>
            string.IsNullOrEmpty(html) ? string.Empty : Regex.Replace(html, "<.*?>", string.Empty);

        #region PropertyChanged

        /// <summary>
        /// Property changed handler.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when property is changed.
        /// </summary>
        /// <param name="propName">changed property name</param>
        public void RaisePropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion
    }
}
