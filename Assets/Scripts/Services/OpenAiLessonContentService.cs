using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data.StaticData.Lesson;
using Lessons;
using Services.Interfaces;
using UnityEngine;
using UnityEngine.Networking;

namespace Services
{
    public class OpenAiLessonContentService : ILessonContentService
    {
        private const string ApiUrl = "https://api.openai.com/v1/responses";
        private const string ApiKey = "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";
        private const string Model = "gpt-5.4-mini";
        private const int TimeoutSeconds = 45;

        public IEnumerator GenerateTextLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<TextMathsQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            if (string.IsNullOrWhiteSpace(ApiKey) || ApiKey.Contains("PASTE_OPENAI"))
            {
                onError?.Invoke("OpenAI API key is missing in OpenAiLessonContentService.cs");
                yield break;
            }

            OpenAiResponsesRequest requestDto = new OpenAiResponsesRequest
            {
                model = Model,
                instructions = BuildInstructions(),
                input = BuildPrompt(lessonId, Mathf.Max(1, questionCount)),
                max_output_tokens = 1200
            };

            string requestJson = JsonUtility.ToJson(requestDto);

            using UnityWebRequest request = new UnityWebRequest(ApiUrl, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(requestJson));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = TimeoutSeconds;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + ApiKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke("OpenAI request failed: " + request.error + "\n" + request.downloadHandler.text);
                yield break;
            }

            OpenAiResponsesResponse response =
                JsonUtility.FromJson<OpenAiResponsesResponse>(request.downloadHandler.text);

            string rawText = ExtractOutputText(response);

            if (string.IsNullOrWhiteSpace(rawText))
            {
                onError?.Invoke("OpenAI response does not contain output_text.");
                yield break;
            }

            List<TextMathsQuestionDefinition> questions;
            try
            {
                questions = ParseQuestions(rawText, questionCount);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Could not parse AI lesson.\n" + ex.Message + "\n\nRaw output:\n" + rawText);
                yield break;
            }

            onSuccess?.Invoke(questions);
        }

        private string BuildInstructions()
        {
            return
                "You generate educational multiple-choice exercises for children. " +
                "Return only plain text. " +
                "Do not use markdown. " +
                "Do not add explanations. " +
                "All generated question text and all answer text must be in English. " +
                "Each exercise must have exactly one unambiguously correct answer. " +
                "The other three answers must be clearly incorrect and must not also fit the question. " +
                "Across the full set of generated exercises, vary the CorrectIndex values as evenly as possible. " +
                "Avoid repeating the same CorrectIndex too often. " +
                "Each exercise must have exactly 3 lines. " +
                "Line 1 starts with 'Question:'. " +
                "Line 2 starts with 'Answers:' and contains exactly 4 answers separated only by ' | '. " +
                "Line 3 starts with 'CorrectIndex:' and contains a number from 0 to 3. " +
                "Separate exercises with a line containing only ###. " +
                "Do not add any extra text before the first exercise or after the last exercise.";
        }

        private string BuildPrompt(LessonId lessonId, int questionCount)
        {
            string outputLanguage = "English";
            string lessonInstructions = GetLessonInstructions(lessonId);

            string correctIndexDistributionRule = questionCount == 5
                ? "- in a 5-exercise set, do not use the same CorrectIndex more than twice\n"
                : "- distribute CorrectIndex values as evenly as possible across the full set\n";

            return
                "Generate exactly " + questionCount + " exercises for this lesson.\n" +
                "Lesson: " + lessonId.ToDisplayName() + "\n" +
                "Language of question and answers: " + outputLanguage + "\n" +
                "Lesson-specific rules: " + lessonInstructions + "\n\n" +
                "Output format for every exercise must be exactly:\n" +
                "Question: <question text>\n" +
                "Answers: <answer 1> | <answer 2> | <answer 3> | <answer 4>\n" +
                "CorrectIndex: <0..3>\n" +
                "###\n\n" +
                "Rules:\n" +
                "- exactly " + questionCount + " exercises\n" +
                "- no numbering\n" +
                "- no blank lines inside an exercise\n" +
                "- keep questions short\n" +
                "- keep answers short\n" +
                "- exactly one clearly correct answer in each exercise\n" +
                "- the other 3 answers must be clearly wrong and must not also make sense\n" +
                "- avoid ambiguous distractors\n" +
                "- avoid placing the correct answer at the same index in consecutive exercises when possible\n" +
                correctIndexDistributionRule +
                "- for mathematics lessons, never generate story problems or verbal questions when a compact numeric format is possible\n" +
                "- tags must remain exactly in English: Question, Answers, CorrectIndex";
        }

        private string GetLessonInstructions(LessonId lessonId)
        {
            switch (lessonId)
            {
                case LessonId.MathematicsNaturalNumbers:
                    return "Use only short numeric exercises about natural numbers. Prefer formats like 'Which number is greater: 45 or 54?' or 'What comes after 29?'. Do not generate story problems.";

                case LessonId.MathematicsAddAndSubtract:
                    return "Every question must be only an arithmetic expression like '5 + 3 = ?' or '99 - 21 = ?'. Do not use words in the question. All 4 answers must be numeric strings only.";

                case LessonId.MathematicsMeasurements:
                    return "Use only short measurement conversion questions like '3 m = ? cm' or '500 g = ? kg'. Do not generate story problems.";

                case LessonId.MathematicsMultiplyAndDivide:
                    return "Every question must be only an arithmetic expression like '6 x 4 = ?' or '36 / 6 = ?'. Use only exact integer divisions. Do not use words in the question. All 4 answers must be numeric strings only.";

                case LessonId.EnglishReadTogether:
                    return "Create very short reading comprehension style questions, but keep the entire prompt on one question line.";

                case LessonId.EnglishWriteCorrectly:
                    return "Create spelling or correct writing exercises.";

                case LessonId.EnglishCompleteTheSentence:
                    return "Create complete-the-sentence exercises where exactly one answer completes the sentence naturally and grammatically. The other three answers must be clearly incorrect because of grammar, tense, word form, meaning, or collocation. Avoid any sentence where more than one answer could reasonably fit.";

                case LessonId.EnglishSynonyms:
                    return "Create easy synonym exercises with exactly one correct synonym. The other answers must not also be valid synonyms.";

                case LessonId.EnglishOpposites:
                    return "Create easy opposite word exercises with exactly one correct antonym. The other answers must not also be acceptable opposites.";

                case LessonId.EnglishSyllableDivision:
                    return "Create easy syllable division exercises.";

                default:
                    return "Create child-friendly text multiple choice exercises.";
            }
        }

        private string ExtractOutputText(OpenAiResponsesResponse response)
        {
            if (response == null || response.output == null) return string.Empty;

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < response.output.Length; i++)
            {
                OpenAiOutputItem item = response.output[i];
                if (item == null || item.content == null) continue;

                for (int j = 0; j < item.content.Length; j++)
                {
                    OpenAiOutputContent content = item.content[j];
                    if (content != null && content.type == "output_text" && !string.IsNullOrEmpty(content.text))
                    {
                        builder.Append(content.text);
                    }
                }
            }

            string text = builder.ToString().Trim();

            if (text.StartsWith("```"))
            {
                text = text.Trim('`').Trim();
                if (text.StartsWith("text")) text = text.Substring(4).Trim();
                if (text.StartsWith("plaintext")) text = text.Substring(9).Trim();
            }

            return text;
        }

        private List<TextMathsQuestionDefinition> ParseQuestions(string rawText, int expectedCount)
        {
            List<string> blocks = SplitBlocks(rawText);
            if (blocks.Count != expectedCount)
            {
                throw new Exception("Expected " + expectedCount + " exercise blocks, but got " + blocks.Count + ".");
            }

            List<TextMathsQuestionDefinition> result = new List<TextMathsQuestionDefinition>();

            for (int i = 0; i < blocks.Count; i++)
            {
                string[] lines = blocks[i]
                    .Replace("\r\n", "\n")
                    .Split('\n')
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToArray();

                if (lines.Length != 3)
                {
                    throw new Exception("Block " + (i + 1) + " must contain exactly 3 lines.");
                }

                string question = ReadValue(lines[0], "Question:");
                string answersLine = ReadValue(lines[1], "Answers:");
                string correctIndexLine = ReadValue(lines[2], "CorrectIndex:");

                string[] answers = answersLine
                    .Split('|')
                    .Select(answer => answer.Trim())
                    .ToArray();

                if (answers.Length != 4)
                {
                    throw new Exception("Block " + (i + 1) + " must contain exactly 4 answers.");
                }

                if (!int.TryParse(correctIndexLine, out int correctIndex) || correctIndex < 0 || correctIndex > 3)
                {
                    throw new Exception("Block " + (i + 1) + " has invalid CorrectIndex.");
                }

                result.Add(new TextMathsQuestionDefinition
                {
                    QuestionText = question,
                    Answers = answers,
                    CorrectAnswerIndex = correctIndex
                });
            }

            return result;
        }

        private List<string> SplitBlocks(string rawText)
        {
            string[] lines = rawText.Replace("\r\n", "\n").Split('\n');
            List<string> blocks = new List<string>();
            StringBuilder current = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line == "###")
                {
                    AddCurrentBlock(blocks, current);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line)) continue;

                current.AppendLine(line);
            }

            AddCurrentBlock(blocks, current);
            return blocks;
        }

        private void AddCurrentBlock(List<string> blocks, StringBuilder current)
        {
            string block = current.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(block))
            {
                blocks.Add(block);
            }

            current.Clear();
        }

        private string ReadValue(string line, string prefix)
        {
            if (!line.StartsWith(prefix, StringComparison.Ordinal))
            {
                throw new Exception("Expected line to start with '" + prefix + "' but got: " + line);
            }

            string value = line.Substring(prefix.Length).Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception("Value is empty for prefix '" + prefix + "'.");
            }

            return value;
        }

        private IEnumerator SendPlainTextPrompt(
    string instructions,
    string input,
    int maxOutputTokens,
    Action<string> onSuccess,
    Action<string> onError)
        {
            if (string.IsNullOrWhiteSpace(ApiKey) || ApiKey.Contains("PASTE_OPENAI"))
            {
                onError?.Invoke("OpenAI API key is missing in OpenAiLessonContentService.cs");
                yield break;
            }

            OpenAiResponsesRequest requestDto = new OpenAiResponsesRequest
            {
                model = Model,
                instructions = instructions,
                input = input,
                max_output_tokens = maxOutputTokens
            };

            string requestJson = JsonUtility.ToJson(requestDto);

            using UnityWebRequest request = new UnityWebRequest(ApiUrl, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(requestJson));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = TimeoutSeconds;
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + ApiKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke("OpenAI request failed: " + request.error + "\n" + request.downloadHandler.text);
                yield break;
            }

            OpenAiResponsesResponse response =
                JsonUtility.FromJson<OpenAiResponsesResponse>(request.downloadHandler.text);

            string rawText = ExtractOutputText(response);
            if (string.IsNullOrWhiteSpace(rawText))
            {
                onError?.Invoke("OpenAI response does not contain output_text.");
                yield break;
            }

            onSuccess?.Invoke(rawText);
        }

        public IEnumerator GenerateClockLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<ClockQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            int normalizedCount = Mathf.Max(1, questionCount);

            string rawText = null;
            string errorMessage = null;

            yield return SendPlainTextPrompt(
                BuildClockInstructions(),
                BuildClockPrompt(lessonId, normalizedCount),
                1000,
                value => rawText = value,
                value => errorMessage = value);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                onError?.Invoke(errorMessage);
                yield break;
            }

            List<ClockQuestionDefinition> questions;
            try
            {
                questions = ParseClockQuestions(rawText, normalizedCount);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Could not parse AI clock lesson.\n" + ex.Message + "\n\nRaw output:\n" + rawText);
                yield break;
            }

            onSuccess?.Invoke(questions);
        }

        public IEnumerator GenerateSyllableDivisionLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<SyllableDivisionQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            int normalizedCount = Mathf.Max(1, questionCount);

            string rawText = null;
            string errorMessage = null;

            yield return SendPlainTextPrompt(
                BuildSyllableDivisionInstructions(),
                BuildSyllableDivisionPrompt(lessonId, normalizedCount),
                800,
                value => rawText = value,
                value => errorMessage = value);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                onError?.Invoke(errorMessage);
                yield break;
            }

            List<SyllableDivisionQuestionDefinition> questions;
            try
            {
                questions = ParseSyllableDivisionQuestions(rawText, normalizedCount);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Could not parse AI syllable-division lesson.\n" + ex.Message + "\n\nRaw output:\n" + rawText);
                yield break;
            }

            onSuccess?.Invoke(questions);
        }

        private string BuildClockInstructions()
        {
            return
                "You generate clock-reading multiple-choice exercises for children. " +
                "Return only plain text. " +
                "Do not use markdown. " +
                "Do not add explanations. " +
                "All visible text must be in English. " +
                "Each exercise must have exactly 4 lines. " +
                "Line 1 starts with 'Hour:' and contains an integer from 0 to 23. " +
                "Line 2 starts with 'Minute:' and contains an integer from 0 to 59. " +
                "Line 3 starts with 'Answers:' and contains exactly 4 answers separated only by ' | '. " +
                "Line 4 starts with 'CorrectIndex:' and contains a number from 0 to 3. " +
                "Separate exercises with a line containing only ###. " +
                "Do not add any extra text.";
        }

        private string BuildClockPrompt(LessonId lessonId, int questionCount)
        {
            return
                "Generate exactly " + questionCount + " clock-reading exercises for the lesson " + lessonId.ToDisplayName() + ".\n" +
                "Use 24-hour answer format HH:mm.\n" +
                "Use only minutes that are multiples of 5.\n" +
                "All 4 answers must be different.\n" +
                "The correct answer must exactly match Hour and Minute.\n" +
                "Every block must look exactly like this:\n" +
                "Hour: 17\n" +
                "Minute: 30\n" +
                "Answers: 17:30 | 17:00 | 15:30 | 05:30\n" +
                "CorrectIndex: 0\n" +
                "###";
        }

        private List<ClockQuestionDefinition> ParseClockQuestions(string rawText, int expectedCount)
        {
            List<string> blocks = SplitBlocks(rawText);
            if (blocks.Count != expectedCount)
            {
                throw new Exception("Expected " + expectedCount + " clock blocks, but got " + blocks.Count + ".");
            }

            List<ClockQuestionDefinition> result = new List<ClockQuestionDefinition>();

            for (int i = 0; i < blocks.Count; i++)
            {
                string[] lines = blocks[i]
                    .Replace("\r\n", "\n")
                    .Split('\n')
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToArray();

                if (lines.Length != 4)
                {
                    throw new Exception("Clock block " + (i + 1) + " must contain exactly 4 lines.");
                }

                if (!int.TryParse(ReadValue(lines[0], "Hour:"), out int hour) || hour < 0 || hour > 23)
                {
                    throw new Exception("Clock block " + (i + 1) + " has invalid Hour.");
                }

                if (!int.TryParse(ReadValue(lines[1], "Minute:"), out int minute) || minute < 0 || minute > 59)
                {
                    throw new Exception("Clock block " + (i + 1) + " has invalid Minute.");
                }

                string[] answers = ReadValue(lines[2], "Answers:")
                    .Split('|')
                    .Select(answer => answer.Trim())
                    .ToArray();

                if (answers.Length != 4)
                {
                    throw new Exception("Clock block " + (i + 1) + " must contain exactly 4 answers.");
                }

                if (!int.TryParse(ReadValue(lines[3], "CorrectIndex:"), out int correctIndex) || correctIndex < 0 || correctIndex > 3)
                {
                    throw new Exception("Clock block " + (i + 1) + " has invalid CorrectIndex.");
                }

                string expectedAnswer = hour.ToString("00") + ":" + minute.ToString("00");
                if (answers[correctIndex] != expectedAnswer)
                {
                    throw new Exception("Clock block " + (i + 1) + " has mismatched correct answer.");
                }

                result.Add(new ClockQuestionDefinition
                {
                    Hour = hour,
                    Minute = minute,
                    Answers = answers,
                    CorrectAnswerIndex = correctIndex
                });
            }

            return result;
        }

        private string BuildSyllableDivisionInstructions()
        {
            return
                "You generate syllable-division exercises for children. " +
                "Return only plain text. " +
                "Do not use markdown. " +
                "Do not add explanations. " +
                "All visible text must be in English. " +
                "Each exercise must have exactly 2 lines. " +
                "Line 1 starts with 'Prompt:'. " +
                "Line 2 starts with 'ExpectedAnswer:'. " +
                "Separate exercises with a line containing only ###. " +
                "ExpectedAnswer must use hyphens between syllables.";
        }

        private string BuildSyllableDivisionPrompt(LessonId lessonId, int questionCount)
        {
            return
                "Generate exactly " + questionCount + " syllable-division exercises for the lesson " + lessonId.ToDisplayName() + ".\n" +
                "Use common English words appropriate for children.\n" +
                "Each prompt must ask the user to divide one word into syllables.\n" +
                "ExpectedAnswer must contain the correct hyphenated syllable split.\n" +
                "Every block must look exactly like this:\n" +
                "Prompt: Divide into syllables: banana\n" +
                "ExpectedAnswer: ba-na-na\n" +
                "###";
        }

        private List<SyllableDivisionQuestionDefinition> ParseSyllableDivisionQuestions(string rawText, int expectedCount)
        {
            List<string> blocks = SplitBlocks(rawText);
            if (blocks.Count != expectedCount)
            {
                throw new Exception("Expected " + expectedCount + " syllable blocks, but got " + blocks.Count + ".");
            }

            List<SyllableDivisionQuestionDefinition> result = new List<SyllableDivisionQuestionDefinition>();

            for (int i = 0; i < blocks.Count; i++)
            {
                string[] lines = blocks[i]
                    .Replace("\r\n", "\n")
                    .Split('\n')
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToArray();

                if (lines.Length != 2)
                {
                    throw new Exception("Syllable block " + (i + 1) + " must contain exactly 2 lines.");
                }

                string prompt = ReadValue(lines[0], "Prompt:");
                string expectedAnswer = ReadValue(lines[1], "ExpectedAnswer:");

                result.Add(new SyllableDivisionQuestionDefinition
                {
                    PromptText = prompt,
                    ExpectedAnswer = expectedAnswer
                });
            }

            return result;
        }

        public IEnumerator GenerateReadTogetherLesson(
            LessonId lessonId,
            int questionCount,
            Action<List<ReadTogetherQuestionDefinition>> onSuccess,
            Action<string> onError)
        {
            int normalizedCount = Mathf.Max(1, questionCount);

            string rawText = null;
            string errorMessage = null;

            yield return SendPlainTextPrompt(
                BuildReadTogetherInstructions(),
                BuildReadTogetherPrompt(lessonId, normalizedCount),
                700,
                value => rawText = value,
                value => errorMessage = value);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                onError?.Invoke(errorMessage);
                yield break;
            }

            List<ReadTogetherQuestionDefinition> questions;
            try
            {
                questions = ParseReadTogetherQuestions(rawText, normalizedCount);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Could not parse AI read-together lesson.\n" + ex.Message + "\n\nRaw output:\n" + rawText);
                yield break;
            }

            onSuccess?.Invoke(questions);
        }

        private string BuildReadTogetherInstructions()
        {
            return
                "You generate read-aloud exercises for children. " +
                "Return only plain text. " +
                "Do not use markdown. " +
                "Do not add explanations. " +
                "All visible text must be in English. " +
                "Each exercise must have exactly 1 line. " +
                "That line must start with 'Passage:'. " +
                "Separate exercises with a line containing only ###. " +
                "Do not add any extra text before the first exercise or after the last exercise.";
        }

        private string BuildReadTogetherPrompt(LessonId lessonId, int questionCount)
        {
            return
                "Generate exactly " + questionCount + " read-aloud exercises for the lesson " + lessonId.ToDisplayName() + ".\n" +
                "Use very short child-friendly English passages that are easy to read aloud.\n" +
                "Each passage must be either one short sentence or two very short sentences.\n" +
                "Each passage should contain about 4 to 10 words total.\n" +
                "Avoid numbers, abbreviations, names, quotes, semicolons, and uncommon punctuation.\n" +
                "Prefer simple words and present-tense sentences.\n" +
                "All passages must be different.\n" +
                "Every block must look exactly like this:\n" +
                "Passage: The little bird can fly.\n" +
                "###";
        }

        private List<ReadTogetherQuestionDefinition> ParseReadTogetherQuestions(string rawText, int expectedCount)
        {
            List<string> blocks = SplitBlocks(rawText);
            if (blocks.Count != expectedCount)
            {
                throw new Exception("Expected " + expectedCount + " read-together blocks, but got " + blocks.Count + ".");
            }

            List<ReadTogetherQuestionDefinition> result = new List<ReadTogetherQuestionDefinition>();

            for (int i = 0; i < blocks.Count; i++)
            {
                string[] lines = blocks[i]
                    .Replace("\r\n", "\n")
                    .Split('\n')
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToArray();

                if (lines.Length != 1)
                {
                    throw new Exception("Read-together block " + (i + 1) + " must contain exactly 1 line.");
                }

                result.Add(new ReadTogetherQuestionDefinition
                {
                    PassageText = ReadValue(lines[0], "Passage:")
                });
            }

            return result;
        }

        public IEnumerator GenerateWriteCorrectlyLesson(
    LessonId lessonId,
    int questionCount,
    Action<List<WriteCorrectlyQuestionDefinition>> onSuccess,
    Action<string> onError)
        {
            int normalizedCount = Mathf.Max(1, questionCount);

            string rawText = null;
            string errorMessage = null;

            yield return SendPlainTextPrompt(
                BuildWriteCorrectlyInstructions(),
                BuildWriteCorrectlyPrompt(lessonId, normalizedCount),
                900,
                value => rawText = value,
                value => errorMessage = value);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                onError?.Invoke(errorMessage);
                yield break;
            }

            List<WriteCorrectlyQuestionDefinition> questions;
            try
            {
                questions = ParseWriteCorrectlyQuestions(rawText, normalizedCount);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Could not parse AI write-correctly lesson.\n" + ex.Message + "\n\nRaw output:\n" + rawText);
                yield break;
            }

            onSuccess?.Invoke(questions);
        }

        private string BuildWriteCorrectlyInstructions()
        {
            return
                "You generate write-correctly dictation exercises for children. " +
                "Return only plain text. " +
                "Do not use markdown. " +
                "Do not add explanations. " +
                "All visible text must be in English. " +
                "Each exercise must have exactly 2 lines. " +
                "Line 1 starts with 'Sentence:'. " +
                "Line 2 starts with 'ExpectedAnswer:'. " +
                "Separate exercises with a line containing only ###. " +
                "ExpectedAnswer must be the exact correct sentence the learner should type.";
        }

        private string BuildWriteCorrectlyPrompt(LessonId lessonId, int questionCount)
        {
            return
                "Generate exactly " + questionCount + " write-correctly dictation exercises for the lesson " + lessonId.ToDisplayName() + ".\n" +
                "Use short child-friendly English sentences.\n" +
                "Each sentence must contain between 3 and 8 words.\n" +
                "Avoid numbers, abbreviations, quotes, semicolons, and unusual punctuation.\n" +
                "Use only simple punctuation like period, question mark, or exclamation mark when needed.\n" +
                "All sentences must be different.\n" +
                "Every block must look exactly like this:\n" +
                "Sentence: The cat is sleeping.\n" +
                "ExpectedAnswer: The cat is sleeping.\n" +
                "###";
        }

        private List<WriteCorrectlyQuestionDefinition> ParseWriteCorrectlyQuestions(string rawText, int expectedCount)
        {
            List<string> blocks = SplitBlocks(rawText);
            if (blocks.Count != expectedCount)
            {
                throw new Exception("Expected " + expectedCount + " write-correctly blocks, but got " + blocks.Count + ".");
            }

            List<WriteCorrectlyQuestionDefinition> result = new List<WriteCorrectlyQuestionDefinition>();

            for (int i = 0; i < blocks.Count; i++)
            {
                string[] lines = blocks[i]
                    .Replace("\r\n", "\n")
                    .Split('\n')
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToArray();

                if (lines.Length != 2)
                {
                    throw new Exception("Write-correctly block " + (i + 1) + " must contain exactly 2 lines.");
                }

                result.Add(new WriteCorrectlyQuestionDefinition
                {
                    SentenceText = ReadValue(lines[0], "Sentence:"),
                    ExpectedAnswer = ReadValue(lines[1], "ExpectedAnswer:")
                });
            }

            return result;
        }

        public IEnumerator GenerateTextChoiceLesson(
    LessonId lessonId,
    int questionCount,
    Action<List<TextMathsQuestionDefinition>> onSuccess,
    Action<string> onError)
        {
            int normalizedCount = Mathf.Max(1, questionCount);

            string rawText = null;
            string errorMessage = null;

            yield return SendPlainTextPrompt(
                BuildInstructions(),
                BuildPrompt(lessonId, normalizedCount),
                1200,
                value => rawText = value,
                value => errorMessage = value);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                onError?.Invoke(errorMessage);
                yield break;
            }

            List<TextMathsQuestionDefinition> questions;
            try
            {
                questions = ParseQuestions(rawText, normalizedCount);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Could not parse AI lesson.\n" + ex.Message + "\n\nRaw output:\n" + rawText);
                yield break;
            }

            onSuccess?.Invoke(questions);
        }

        public IEnumerator GenerateShapesLesson(
    LessonId lessonId,
    int questionCount,
    IReadOnlyList<string> allowedShapeIds,
    Action<List<ShapeQuestionDefinition>> onSuccess,
    Action<string> onError)
        {
            int normalizedCount = Mathf.Max(1, questionCount);

            if (allowedShapeIds == null || allowedShapeIds.Count == 0)
            {
                onError?.Invoke("Shapes lesson requires at least one allowed shape id.");
                yield break;
            }

            string rawText = null;
            string errorMessage = null;

            yield return SendPlainTextPrompt(
                BuildShapesInstructions(),
                BuildShapesPrompt(lessonId, normalizedCount, allowedShapeIds),
                1000,
                value => rawText = value,
                value => errorMessage = value);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                onError?.Invoke(errorMessage);
                yield break;
            }

            List<ShapeQuestionDefinition> questions;
            try
            {
                questions = ParseShapeQuestions(rawText, normalizedCount, allowedShapeIds);
            }
            catch (Exception ex)
            {
                onError?.Invoke("Could not parse AI shapes lesson.\n" + ex.Message + "\n\nRaw output:\n" + rawText);
                yield break;
            }

            onSuccess?.Invoke(questions);
        }

        private string BuildShapesInstructions()
        {
            return
                "You generate geometrical-shape multiple-choice exercises for children. " +
                "Return only plain text. " +
                "Do not use markdown. " +
                "Do not add explanations. " +
                "All visible answer text must be in English. " +
                "Each exercise must have exactly 3 lines. " +
                "Line 1 starts with 'ShapeId:' and contains one allowed shape id exactly as provided. " +
                "Line 2 starts with 'Answers:' and contains exactly 4 answers separated only by ' | '. " +
                "Line 3 starts with 'CorrectIndex:' and contains a number from 0 to 3. " +
                "Separate exercises with a line containing only ###. " +
                "Do not add any extra text.";
        }

        private string BuildShapesPrompt(LessonId lessonId, int questionCount, IReadOnlyList<string> allowedShapeIds)
        {
            string correctIndexDistributionRule = questionCount == 5
                ? "- in a 5-exercise set, do not use the same CorrectIndex more than twice\n"
                : "- distribute CorrectIndex values as evenly as possible across the full set\n";

            return
                "Generate exactly " + questionCount + " exercises for this lesson.\n" +
                "Lesson: " + lessonId.ToDisplayName() + "\n" +
                "Allowed shape ids and expected English labels:\n" +
                BuildShapeOptionsText(allowedShapeIds) + "\n\n" +
                "Output format for every exercise must be exactly:\n" +
                "ShapeId: <shape id>\n" +
                "Answers: <answer 1> | <answer 2> | <answer 3> | <answer 4>\n" +
                "CorrectIndex: <0..3>\n" +
                "###\n\n" +
                "Rules:\n" +
                "- exactly " + questionCount + " exercises\n" +
                "- use only ShapeId values from the allowed list\n" +
                "- the correct answer text must match the label of the chosen ShapeId\n" +
                "- the other 3 answers must be labels of other allowed shapes\n" +
                "- answers inside the same exercise must all be different\n" +
                "- no numbering\n" +
                "- no blank lines inside an exercise\n" +
                correctIndexDistributionRule +
                "- tags must remain exactly in English: ShapeId, Answers, CorrectIndex";
        }

        private string BuildShapeOptionsText(IReadOnlyList<string> allowedShapeIds)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < allowedShapeIds.Count; i++)
            {
                string id = allowedShapeIds[i];
                if (string.IsNullOrWhiteSpace(id)) continue;

                if (builder.Length > 0) builder.Append('\n');
                builder.Append("- ").Append(id.Trim()).Append(" => ").Append(HumanizeShapeId(id));
            }

            return builder.ToString();
        }

        private string HumanizeShapeId(string shapeId)
        {
            if (string.IsNullOrWhiteSpace(shapeId)) return string.Empty;

            string[] parts = shapeId.Trim().Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].ToLowerInvariant();
                parts[i] = char.ToUpperInvariant(part[0]) + part.Substring(1);
            }

            return string.Join(" ", parts);
        }

        private List<ShapeQuestionDefinition> ParseShapeQuestions(string rawText, int expectedCount, IReadOnlyList<string> allowedShapeIds)
        {
            List<string> blocks = SplitBlocks(rawText);
            if (blocks.Count != expectedCount)
            {
                throw new Exception("Expected " + expectedCount + " shape blocks, but got " + blocks.Count + ".");
            }

            HashSet<string> allowedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < allowedShapeIds.Count; i++)
            {
                string id = allowedShapeIds[i];
                if (!string.IsNullOrWhiteSpace(id)) allowedIds.Add(id.Trim());
            }

            List<ShapeQuestionDefinition> result = new List<ShapeQuestionDefinition>();

            for (int i = 0; i < blocks.Count; i++)
            {
                string[] lines = blocks[i]
                    .Replace("\r\n", "\n")
                    .Split('\n')
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToArray();

                if (lines.Length != 3)
                {
                    throw new Exception("Shape block " + (i + 1) + " must contain exactly 3 lines.");
                }

                string shapeId = ReadValue(lines[0], "ShapeId:");
                string[] answers = ReadValue(lines[1], "Answers:")
                    .Split('|')
                    .Select(answer => answer.Trim())
                    .ToArray();

                if (!allowedIds.Contains(shapeId))
                {
                    throw new Exception("Shape block " + (i + 1) + " contains an unknown ShapeId: " + shapeId);
                }

                if (answers.Length != 4)
                {
                    throw new Exception("Shape block " + (i + 1) + " must contain exactly 4 answers.");
                }

                if (answers.Distinct(StringComparer.OrdinalIgnoreCase).Count() != 4)
                {
                    throw new Exception("Shape block " + (i + 1) + " must contain 4 distinct answers.");
                }

                if (!int.TryParse(ReadValue(lines[2], "CorrectIndex:"), out int correctIndex) || correctIndex < 0 || correctIndex > 3)
                {
                    throw new Exception("Shape block " + (i + 1) + " has invalid CorrectIndex.");
                }

                result.Add(new ShapeQuestionDefinition
                {
                    ShapeId = shapeId,
                    Answers = answers,
                    CorrectAnswerIndex = correctIndex
                });
            }
            return result;
        }

        [Serializable]
        private class OpenAiResponsesRequest
        {
            public string model;
            public string instructions;
            public string input;
            public int max_output_tokens;
        }

        [Serializable]
        private class OpenAiResponsesResponse
        {
            public OpenAiOutputItem[] output;
        }

        [Serializable]
        private class OpenAiOutputItem
        {
            public string type;
            public OpenAiOutputContent[] content;
        }

        [Serializable]
        private class OpenAiOutputContent
        {
            public string type;
            public string text;
        }
    }
}
