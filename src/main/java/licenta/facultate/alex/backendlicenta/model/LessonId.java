package licenta.facultate.alex.backendlicenta.model;

import licenta.facultate.alex.backendlicenta.exception.ApiException;
import org.springframework.http.HttpStatus;

import java.util.Arrays;

public enum LessonId {
    MATHEMATICS_NATURAL_NUMBERS("MathematicsNaturalNumbers", LessonType.TEXT_CHOICE, "Simple natural numbers for young children."),
    MATHEMATICS_ADD_AND_SUBTRACT("MathematicsAddAndSubtract", LessonType.TEXT_CHOICE, "Small addition and subtraction exercises."),
    MATHEMATICS_MEASUREMENTS("MathematicsMeasurements", LessonType.TEXT_CHOICE, "Basic units and simple measurement comparisons."),
    MATHEMATICS_MULTIPLY_AND_DIVIDE("MathematicsMultiplyAndDivide", LessonType.TEXT_CHOICE, "Simple multiplication and division."),
    ENGLISH_COMPLETE_THE_SENTENCE("EnglishCompleteTheSentence", LessonType.TEXT_CHOICE, "Fill-the-gap sentence completion."),
    ENGLISH_SYNONYMS("EnglishSynonyms", LessonType.TEXT_CHOICE, "Easy synonym choices."),
    ENGLISH_OPPOSITES("EnglishOpposites", LessonType.TEXT_CHOICE, "Easy opposite-word choices."),
    MATHEMATICS_GEOMETRICAL_SHAPES("MathematicsGeometricalShapes", LessonType.SHAPES, "Recognize common geometric shapes."),
    MATHEMATICS_WHAT_IS_THE_TIME("MathematicsWhatIsTheTime", LessonType.CLOCK, "Read clocks and select matching HH:mm times."),
    ENGLISH_SYLLABLE_DIVISION("EnglishSyllableDivision", LessonType.SYLLABLE_DIVISION, "Split simple English words into syllables."),
    ENGLISH_WRITE_CORRECTLY("EnglishWriteCorrectly", LessonType.WRITE_CORRECTLY, "Correct short English sentences for children."),
    ENGLISH_READ_TOGETHER("EnglishReadTogether", LessonType.READ_TOGETHER, "Very short English read-aloud passages for children.");

    private final String clientValue;
    private final LessonType type;
    private final String promptHint;

    LessonId(String clientValue, LessonType type, String promptHint) {
        this.clientValue = clientValue;
        this.type = type;
        this.promptHint = promptHint;
    }

    public String clientValue() {
        return clientValue;
    }

    public LessonType type() {
        return type;
    }

    public String promptHint() {
        return promptHint;
    }

    public static LessonId fromClientValue(String value) {
        return Arrays.stream(values())
                .filter(it -> it.clientValue.equals(value))
                .findFirst()
                .orElseThrow(() -> new ApiException(
                        HttpStatus.BAD_REQUEST,
                        "INVALID_LESSON_ID",
                        "Unsupported lessonId: " + value
                ));
    }
}
