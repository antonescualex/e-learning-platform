using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enums;
using Lessons;
using UnityEngine;

namespace Services
{
    public sealed class LessonRequestFactory
    {
        private static readonly Regex AllowedShapeIdRegex =
            new Regex("^[A-Za-z0-9_-]{1,40}$", RegexOptions.Compiled);

        public CommonLessonRequest CreateCommonRequest(LessonId lessonId, int questionCount)
        {
            return new CommonLessonRequest
            {
                LessonId = lessonId.ToString(),
                QuestionCount = NormalizeQuestionCount(questionCount)
            };
        }

        public bool TryCreateShapesRequest(
            LessonId lessonId,
            int questionCount,
            IReadOnlyList<string> allowedShapeIds,
            out ShapesLessonRequest request)
        {
            string[] normalizedShapeIds = NormalizeAllowedShapeIds(allowedShapeIds);
            if (normalizedShapeIds.Length == 0)
            {
                request = null;
                return false;
            }

            request = new ShapesLessonRequest
            {
                LessonId = lessonId.ToString(),
                QuestionCount = NormalizeQuestionCount(questionCount),
                AllowedShapeIds = normalizedShapeIds
            };

            return true;
        }

        private static int NormalizeQuestionCount(int questionCount)
        {
            return Mathf.Clamp(questionCount, 1, 10);
        }

        private static string[] NormalizeAllowedShapeIds(IReadOnlyList<string> allowedShapeIds)
        {
            List<string> result = new List<string>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            if (allowedShapeIds == null)
            {
                return result.ToArray();
            }

            for (int i = 0; i < allowedShapeIds.Count; i++)
            {
                string candidate = allowedShapeIds[i];
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                candidate = candidate.Trim();
                if (!AllowedShapeIdRegex.IsMatch(candidate))
                {
                    continue;
                }

                if (seen.Add(candidate))
                {
                    result.Add(candidate);
                }
            }

            return result.ToArray();
        }

        [Serializable]
        public sealed class CommonLessonRequest
        {
            public string LessonId;
            public int QuestionCount;
        }

        [Serializable]
        public sealed class ShapesLessonRequest
        {
            public string LessonId;
            public int QuestionCount;
            public string[] AllowedShapeIds;
        }
    }
}
