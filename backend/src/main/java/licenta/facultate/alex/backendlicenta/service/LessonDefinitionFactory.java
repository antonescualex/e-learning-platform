package licenta.facultate.alex.backendlicenta.service;

import licenta.facultate.alex.backendlicenta.model.LessonId;
import licenta.facultate.alex.backendlicenta.model.LessonRequestContext;
import tools.jackson.core.JacksonException;
import tools.jackson.databind.JsonNode;
import tools.jackson.databind.ObjectMapper;
import licenta.facultate.alex.backendlicenta.model.LessonType;
import org.springframework.stereotype.Component;

@Component
public class LessonDefinitionFactory {
    private final ObjectMapper objectMapper;

    public LessonDefinitionFactory(ObjectMapper objectMapper) {
        this.objectMapper = objectMapper;
    }

    public String buildInstructions(LessonRequestContext context) {
        return """
                You generate lesson content for a Unity educational app for children.
                Return only valid JSON matching the provided JSON schema.
                Never return markdown, explanations, notes, comments, or extra properties.
                Keep all content safe, age-appropriate, short, and classroom-friendly.
                LessonId: %s
                Lesson semantics: %s

                Type-specific rules:
                %s
                
                Lesson-specific rules:
                %s
                """.formatted(
                context.clientLessonId(),
                context.lessonId().promptHint(),
                typeSpecificRules(context.lessonType()),
                lessonSpecificRules(context.lessonId())
        );
    }

    public String buildInput(LessonRequestContext context, String repairNotes) {
        StringBuilder sb = new StringBuilder();
        sb.append("Generate exactly ").append(context.questionCount()).append(" questions.\n");
        sb.append("LessonId: ").append(context.clientLessonId()).append("\n");

        if (context.lessonType() == LessonType.SHAPES) {
            sb.append("AllowedShapeIds: ").append(String.join(", ", context.allowedShapeIds())).append("\n");
        }

        if (context.lessonType() == LessonType.WRITE_CORRECTLY) {
            sb.append("For each item, SentenceText must be the correctly written sentence.\n");
            sb.append("ExpectedAnswer must be the exact same sentence normalized to lowercase and without punctuation.\n");
            sb.append("Do not introduce grammar mistakes into SentenceText.\n");
        }

        if (context.lessonType() == LessonType.READ_TOGETHER) {
            sb.append("Each item must contain only PassageText.\n");
            sb.append("PassageText must contain 4 to 10 words total.\n");
            sb.append("Use one short sentence or two very short sentences in simple present tense.\n");
            sb.append("Keep passages easy to read aloud for speech recognition.\n");
            sb.append("Use only simple words and periods when punctuation is needed.\n");
            sb.append("Do not use numbers, abbreviations, names, quotes, answer options, blanks, or questions.\n");
            sb.append("All passages must be different.\n");
        }

        if (repairNotes != null && !repairNotes.isBlank()) {
            sb.append("Previous response was invalid. Regenerate the entire JSON and fix exactly these problems:\n");
            sb.append(repairNotes).append("\n");
        }

        return sb.toString();
    }

    public JsonNode buildSchema(LessonRequestContext context) {
        return switch (context.lessonType()) {
            case TEXT_CHOICE -> textChoiceSchema(context.questionCount());
            case SHAPES -> shapesSchema(context.questionCount(), context.allowedShapeIds());
            case CLOCK -> clockSchema(context.questionCount());
            case SYLLABLE_DIVISION -> syllableSchema(context.questionCount());
            case WRITE_CORRECTLY -> writeCorrectlySchema(context.questionCount());
            case READ_TOGETHER -> readTogetherSchema(context.questionCount());
            default -> throw new IllegalStateException("No schema for lesson type " + context.lessonType());
        };
    }

    private String typeSpecificRules(LessonType type) {
        return switch (type) {
            case TEXT_CHOICE -> """
                    Each question must contain QuestionText, Answers[4], and CorrectAnswerIndex.
                    Exactly one answer must be correct.
                    Answers must be distinct and plausible for children.
                    """;
            case SHAPES -> """
                    Use only ShapeId values from AllowedShapeIds.
                    Each question must contain Answers[4] and CorrectAnswerIndex.
                    Answers inside a question must be distinct.
                    """;
            case CLOCK -> """
                    Each question must contain Hour, Minute, Answers[4], and CorrectAnswerIndex.
                    Minute must be a multiple of 5.
                    The answer at CorrectAnswerIndex must exactly equal HH:mm derived from Hour and Minute.
                    """;
            case SYLLABLE_DIVISION -> """
                    Each item must contain PromptText and ExpectedAnswer.
                    ExpectedAnswer must be the same English word split into syllables with hyphens.
                    """;
            case WRITE_CORRECTLY -> """
                    Each item must contain SentenceText and ExpectedAnswer.
                    SentenceText must be a short child-friendly English sentence written correctly, with normal capitalization and punctuation.
                    ExpectedAnswer must contain exactly the same sentence content as SentenceText, but normalized for typing practice:
                    remove ending punctuation,
                    remove commas and apostrophes,
                    use lowercase only,
                    keep words in the same order,
                    do not change vocabulary,
                    do not add or remove words,
                    do not paraphrase.
                    Example:
                    SentenceText: "The cat is sleeping."
                    ExpectedAnswer: "the cat is sleeping"
                    """;
            case READ_TOGETHER -> """
                    Each item must contain only PassageText.
                    PassageText must be a very short child-friendly English read-aloud passage.
                    Use one short sentence or two very short sentences.
                    Keep each passage at about 4 to 10 words total.
                    Prefer simple present-tense wording with familiar words.
                    Avoid numbers, abbreviations, names, quotes, semicolons, answer options, blanks, and unusual punctuation.
                    All PassageText values must be different.
                    """;
            default -> "";
        };
    }

    private String lessonSpecificRules(LessonId lessonId) {
        return switch (lessonId) {
            case MATHEMATICS_ADD_AND_SUBTRACT -> """
                    Generate only arithmetic questions in the exact format "A + B = ?" or "A - B = ?".
                    Use small whole numbers suitable for children.
                    Across the full set, distribute addition and subtraction as evenly as possible; the difference between their counts must not exceed 1.
                    Keep all QuestionText values different.
                    Do not use equivalent duplicates such as "2 + 3 = ?" and "3 + 2 = ?".
                    For subtraction, keep all results non-negative.
                    The correct answer must be a whole number and must match the arithmetic expression exactly.
                    Do not repeat the same correct result in the same set.
                    All Answers values must be whole-number strings only.
                    Wrong answers must be distinct, plausible, and close to the correct answer.
                    Distribute CorrectAnswerIndex as evenly as possible across 0, 1, 2, and 3; the difference between the most used and least used index must not exceed 1.
                    """;
            case MATHEMATICS_MULTIPLY_AND_DIVIDE -> """
                    Generate only arithmetic questions in the exact format "A x B = ?" or "A / B = ?".
                    Use small whole numbers suitable for children.
                    Across the full set, distribute multiplication and division as evenly as possible; the difference between their counts must not exceed 1.
                    Keep all QuestionText values different.
                    Do not use equivalent duplicates such as "3 x 4 = ?" and "4 x 3 = ?".
                    Do not use inverse duplicates such as "3 x 4 = ?" and "12 / 3 = ?" in the same set.
                    For division, every answer must be exact with no remainder, fraction, or decimal.
                    The correct answer must be a whole number and must match the arithmetic expression exactly.
                    Do not repeat the same correct result in the same set.
                    All Answers values must be whole-number strings only.
                    Wrong answers must be distinct, plausible, and close to the correct answer.
                    Distribute CorrectAnswerIndex as evenly as possible across 0, 1, 2, and 3; the difference between the most used and least used index must not exceed 1.
                    """;
            case MATHEMATICS_NATURAL_NUMBERS -> """
                    Generate only exercises about natural numbers using decimal digits.
                    Use small natural numbers suitable for children.
                    Questions must be logically meaningful and must require the child to reason about natural numbers.
                    Do not generate trivial restatements such as "How many birds are there if you have three birds?".
                    Across the full set, diversify the question types as much as possible.
                    Use a mix of skills such as:
                    - finding the number before a given number,
                    - finding the number after a given number,
                    - choosing the greater number,
                    - choosing the smaller number,
                    - identifying the number between two numbers,
                    - completing a simple counting sequence,
                    - choosing the greatest or smallest number from the answer options.
                    Do not use the same question pattern more than twice in the same set.
                    Keep all QuestionText values different.
                    Do not use emoji, icons, pictograms, special math symbols, accented letters, superscripts, subscripts, or any non-ASCII characters.
                    Use only standard keyboard characters: letters A-Z/a-z, digits 0-9, spaces, and basic punctuation such as ?, ., ,, -, +, /, and =.
                    Use words like "greater than", "less than", "before", "after", and "between" instead of comparison symbols.
                    The correct answer must be a natural number and must match the question exactly.
                    Do not repeat the same correct result in the same set.
                    All Answers values must be natural-number strings only.
                    Wrong answers must be distinct, plausible, and close to the correct answer.
                    Distribute CorrectAnswerIndex as evenly as possible across 0, 1, 2, and 3; the difference between the most used and least used index must not exceed 1.
                    """;
            case ENGLISH_COMPLETE_THE_SENTENCE -> """
                    Generate only fill-in-the-blank English sentences.
                    QuestionText must contain exactly one blank written as "____".
                    The answer at CorrectAnswerIndex must be the only answer that creates a natural, meaningful, grammatically correct sentence.
                    The other three answers must not make sense in the sentence, even if they are valid English words.
                    Wrong answers should be clearly incompatible with the sentence context, but still child-friendly.
                    Answers should usually be single words or very short phrases.
                    Keep all QuestionText values different.
                    Distribute CorrectAnswerIndex as evenly as possible across 0, 1, 2, and 3; the difference between the most used and least used index must not exceed 1.
                    """;
            case ENGLISH_SYNONYMS, ENGLISH_OPPOSITES -> """
                    Keep all QuestionText values different.
                    Exactly one answer must be correct for each question.
                    Distribute CorrectAnswerIndex as evenly as possible across 0, 1, 2, and 3.
                    The difference between the most used and least used CorrectAnswerIndex must not exceed 1.
                    Avoid using the same CorrectAnswerIndex in consecutive questions unless unavoidable.
                    """;
            default -> "";
        };
    }

    private JsonNode textChoiceSchema(int count) {
        return read("""
                {
                  "type": "object",
                  "additionalProperties": false,
                  "properties": {
                    "Questions": {
                      "type": "array",
                      "minItems": %d,
                      "maxItems": %d,
                      "items": {
                        "type": "object",
                        "additionalProperties": false,
                        "properties": {
                          "QuestionText": { "type": "string", "minLength": 1 },
                          "Answers": {
                            "type": "array",
                            "minItems": 4,
                            "maxItems": 4,
                            "items": { "type": "string", "minLength": 1 }
                          },
                          "CorrectAnswerIndex": { "type": "integer", "minimum": 0, "maximum": 3 }
                        },
                        "required": ["QuestionText", "Answers", "CorrectAnswerIndex"]
                      }
                    }
                  },
                  "required": ["Questions"]
                }
                """.formatted(count, count));
    }

    private JsonNode shapesSchema(int count, java.util.List<String> allowedShapeIds) {
        return read("""
                {
                  "type": "object",
                  "additionalProperties": false,
                  "properties": {
                    "Questions": {
                      "type": "array",
                      "minItems": %d,
                      "maxItems": %d,
                      "items": {
                        "type": "object",
                        "additionalProperties": false,
                        "properties": {
                          "ShapeId": { "type": "string", "enum": %s },
                          "Answers": {
                            "type": "array",
                            "minItems": 4,
                            "maxItems": 4,
                            "items": { "type": "string", "minLength": 1 }
                          },
                          "CorrectAnswerIndex": { "type": "integer", "minimum": 0, "maximum": 3 }
                        },
                        "required": ["ShapeId", "Answers", "CorrectAnswerIndex"]
                      }
                    }
                  },
                  "required": ["Questions"]
                }
                """.formatted(count, count, toJson(allowedShapeIds)));
    }

    private JsonNode clockSchema(int count) {
        return read("""
                {
                  "type": "object",
                  "additionalProperties": false,
                  "properties": {
                    "Questions": {
                      "type": "array",
                      "minItems": %d,
                      "maxItems": %d,
                      "items": {
                        "type": "object",
                        "additionalProperties": false,
                        "properties": {
                          "Hour": { "type": "integer", "minimum": 0, "maximum": 23 },
                          "Minute": { "type": "integer", "minimum": 0, "maximum": 55, "multipleOf": 5 },
                          "Answers": {
                            "type": "array",
                            "minItems": 4,
                            "maxItems": 4,
                            "items": { "type": "string", "pattern": "^(?:[01]\\\\d|2[0-3]):[0-5]\\\\d$" }
                          },
                          "CorrectAnswerIndex": { "type": "integer", "minimum": 0, "maximum": 3 }
                        },
                        "required": ["Hour", "Minute", "Answers", "CorrectAnswerIndex"]
                      }
                    }
                  },
                  "required": ["Questions"]
                }
                """.formatted(count, count));
    }

    private JsonNode syllableSchema(int count) {
        return read("""
                {
                  "type": "object",
                  "additionalProperties": false,
                  "properties": {
                    "Questions": {
                      "type": "array",
                      "minItems": %d,
                      "maxItems": %d,
                      "items": {
                        "type": "object",
                        "additionalProperties": false,
                        "properties": {
                          "PromptText": { "type": "string", "minLength": 1 },
                          "ExpectedAnswer": { "type": "string", "pattern": "^[A-Za-z]+(?:-[A-Za-z]+)+$" }
                        },
                        "required": ["PromptText", "ExpectedAnswer"]
                      }
                    }
                  },
                  "required": ["Questions"]
                }
                """.formatted(count, count));
    }

    private JsonNode writeCorrectlySchema(int count) {
        return read("""
                {
                  "type": "object",
                  "additionalProperties": false,
                  "properties": {
                    "Questions": {
                      "type": "array",
                      "minItems": %d,
                      "maxItems": %d,
                      "items": {
                        "type": "object",
                        "additionalProperties": false,
                        "properties": {
                          "SentenceText": { "type": "string", "minLength": 1, "maxLength": 80 },
                          "ExpectedAnswer": { "type": "string", "minLength": 1, "maxLength": 80 }
                        },
                        "required": ["SentenceText", "ExpectedAnswer"]
                      }
                    }
                  },
                  "required": ["Questions"]
                }
                """.formatted(count, count));
    }

    private JsonNode readTogetherSchema(int count) {
        return read("""
                {
                  "type": "object",
                  "additionalProperties": false,
                  "properties": {
                    "Questions": {
                      "type": "array",
                      "minItems": %d,
                      "maxItems": %d,
                      "items": {
                        "type": "object",
                        "additionalProperties": false,
                        "properties": {
                          "PassageText": {
                            "type": "string",
                            "minLength": 1,
                            "maxLength": 80,
                            "pattern": "^[A-Za-z ]+(?:\\\\.[ ]*[A-Za-z ]+)?\\\\.?$"
                          }
                        },
                        "required": ["PassageText"]
                      }
                    }
                  },
                  "required": ["Questions"]
                }
                """.formatted(count, count));
    }

    private JsonNode read(String value) {
        try {
            return objectMapper.readTree(value);
        } catch (JacksonException ex) {
            throw new IllegalStateException("Invalid schema JSON", ex);
        }
    }

    private String toJson(Object value) {
        try {
            return objectMapper.writeValueAsString(value);
        } catch (JacksonException ex) {
            throw new IllegalStateException("Could not serialize schema helper", ex);
        }
    }
}
