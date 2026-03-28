package licenta.facultate.alex.backendlicenta.service;

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
                """.formatted(
                context.clientLessonId(),
                context.lessonId().promptHint(),
                typeSpecificRules(context.lessonType())
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
