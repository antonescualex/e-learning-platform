package licenta.facultate.alex.backendlicenta.validation;

import licenta.facultate.alex.backendlicenta.dto.api.LessonContentDtos;
import org.springframework.stereotype.Component;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Locale;
import java.util.Objects;
import java.util.Set;
import java.util.regex.Pattern;

@Component
public class LessonValidator {
    private static final Pattern HYPHENATED = Pattern.compile("^[A-Za-z]+(?:-[A-Za-z]+)+$");
    private static final Pattern READ_TOGETHER_ALLOWED = Pattern.compile("^[A-Za-z ]+(?:\\.[ ]*[A-Za-z ]+)?\\.?$");

    public List<String> validateTextChoice(LessonContentDtos.TextChoiceLessonResponse response, int expectedCount) {
        List<String> errors = new ArrayList<>();
        if (response == null || response.questions() == null) {
            errors.add("Questions is required");
            return errors;
        }
        if (response.questions().size() != expectedCount) {
            errors.add("Questions must contain exactly " + expectedCount + " items");
        }
        for (int i = 0; i < response.questions().size(); i++) {
            var q = response.questions().get(i);
            if (isBlank(q.questionText())) errors.add("Question " + i + ": QuestionText is required");
            validateAnswers(q.answers(), q.correctAnswerIndex(), errors, i);
        }
        return errors;
    }

    public List<String> validateShapes(LessonContentDtos.ShapesLessonResponse response, int expectedCount, Set<String> allowedShapeIds) {
        List<String> errors = new ArrayList<>();
        if (response == null || response.questions() == null) {
            errors.add("Questions is required");
            return errors;
        }
        if (response.questions().size() != expectedCount) {
            errors.add("Questions must contain exactly " + expectedCount + " items");
        }
        for (int i = 0; i < response.questions().size(); i++) {
            var q = response.questions().get(i);
            if (isBlank(q.shapeId()) || !allowedShapeIds.contains(q.shapeId())) {
                errors.add("Question " + i + ": ShapeId must be one of AllowedShapeIds");
            }
            validateAnswers(q.answers(), q.correctAnswerIndex(), errors, i);
        }
        return errors;
    }

    public List<String> validateClock(LessonContentDtos.ClockLessonResponse response, int expectedCount) {
        List<String> errors = new ArrayList<>();
        if (response == null || response.questions() == null) {
            errors.add("Questions is required");
            return errors;
        }
        if (response.questions().size() != expectedCount) {
            errors.add("Questions must contain exactly " + expectedCount + " items");
        }
        for (int i = 0; i < response.questions().size(); i++) {
            var q = response.questions().get(i);
            validateAnswers(q.answers(), q.correctAnswerIndex(), errors, i);
            if (q.hour() < 0 || q.hour() > 23) errors.add("Question " + i + ": Hour must be 0..23");
            if (q.minute() < 0 || q.minute() > 55 || q.minute() % 5 != 0) errors.add("Question " + i + ": Minute must be 0..55 and multiple of 5");
            if (q.answers() != null && q.answers().size() == 4 && q.correctAnswerIndex() >= 0 && q.correctAnswerIndex() <= 3) {
                String expected = "%02d:%02d".formatted(q.hour(), q.minute());
                String actual = q.answers().get(q.correctAnswerIndex());
                if (!expected.equals(actual)) {
                    errors.add("Question " + i + ": correct answer must exactly equal " + expected);
                }
            }
        }
        return errors;
    }

    public List<String> validateSyllableDivision(LessonContentDtos.SyllableDivisionLessonResponse response, int expectedCount) {
        List<String> errors = new ArrayList<>();
        if (response == null || response.questions() == null) {
            errors.add("Questions is required");
            return errors;
        }
        if (response.questions().size() != expectedCount) {
            errors.add("Questions must contain exactly " + expectedCount + " items");
        }
        for (int i = 0; i < response.questions().size(); i++) {
            var q = response.questions().get(i);
            if (isBlank(q.promptText())) errors.add("Question " + i + ": PromptText is required");
            if (isBlank(q.expectedAnswer()) || !HYPHENATED.matcher(q.expectedAnswer()).matches()) {
                errors.add("Question " + i + ": ExpectedAnswer must be hyphenated");
            }
        }
        return errors;
    }

    public List<String> validateWriteCorrectly(LessonContentDtos.WriteCorrectlyLessonResponse response, int expectedCount) {
        List<String> errors = new ArrayList<>();
        if (response == null || response.questions() == null) {
            errors.add("Questions is required");
            return errors;
        }
        if (response.questions().size() != expectedCount) {
            errors.add("Questions must contain exactly " + expectedCount + " items");
        }
        for (int i = 0; i < response.questions().size(); i++) {
            var q = response.questions().get(i);
            if (isBlank(q.sentenceText())) errors.add("Question " + i + ": SentenceText is required");
            if (isBlank(q.expectedAnswer())) errors.add("Question " + i + ": ExpectedAnswer is required");
            if (!isBlank(q.sentenceText()) && q.sentenceText().length() > 80) errors.add("Question " + i + ": SentenceText must be short");
            if (!isBlank(q.expectedAnswer()) && q.expectedAnswer().length() > 80) errors.add("Question " + i + ": ExpectedAnswer must be short");
        }
        return errors;
    }

    public List<String> validateReadTogether(LessonContentDtos.ReadTogetherLessonResponse response, int expectedCount) {
        List<String> errors = new ArrayList<>();
        if (response == null || response.questions() == null) {
            errors.add("Questions is required");
            return errors;
        }
        if (response.questions().size() != expectedCount) {
            errors.add("Questions must contain exactly " + expectedCount + " items");
        }

        Set<String> normalizedPassages = new HashSet<>();
        for (int i = 0; i < response.questions().size(); i++) {
            var q = response.questions().get(i);
            if (isBlank(q.passageText())) {
                errors.add("Question " + i + ": PassageText is required");
                continue;
            }

            String passage = q.passageText().trim();
            if (passage.length() > 80) errors.add("Question " + i + ": PassageText must be short");
            if (!READ_TOGETHER_ALLOWED.matcher(passage).matches()) {
                errors.add("Question " + i + ": PassageText must use only letters, spaces, and periods");
            }

            int sentences = sentenceCount(passage);
            if (sentences < 1 || sentences > 2) {
                errors.add("Question " + i + ": PassageText must be one short sentence or two very short sentences");
            }

            int words = wordCount(passage);
            if (words < 4 || words > 10) {
                errors.add("Question " + i + ": PassageText must contain 4 to 10 words");
            }

            String normalizedPassage = normalizeReadTogether(passage);
            if (!normalizedPassages.add(normalizedPassage)) {
                errors.add("Question " + i + ": PassageText must be unique");
            }
        }
        return errors;
    }

    private void validateAnswers(List<String> answers, int correctAnswerIndex, List<String> errors, int index) {
        if (answers == null || answers.size() != 4) {
            errors.add("Question " + index + ": Answers must contain exactly 4 items");
            return;
        }
        if (correctAnswerIndex < 0 || correctAnswerIndex > 3) {
            errors.add("Question " + index + ": CorrectAnswerIndex must be 0..3");
        }
        if (answers.stream().anyMatch(this::isBlank)) {
            errors.add("Question " + index + ": Answers cannot contain blanks");
        }
        if (!distinct(answers)) {
            errors.add("Question " + index + ": Answers must be distinct");
        }
    }

    private boolean distinct(List<String> values) {
        Set<String> set = new HashSet<>(values.stream().filter(Objects::nonNull).toList());
        return set.size() == values.size();
    }

    private boolean isBlank(String value) {
        return value == null || value.isBlank();
    }

    private int sentenceCount(String value) {
        int count = 0;
        for (String sentence : value.split("\\.")) {
            if (!sentence.isBlank()) {
                count++;
            }
        }
        return count;
    }

    private int wordCount(String value) {
        int count = 0;
        for (String token : value.split("\\s+")) {
            String word = token.replace(".", "").trim();
            if (!word.isBlank()) {
                count++;
            }
        }
        return count;
    }

    private String normalizeReadTogether(String value) {
        return value.toLowerCase(Locale.ROOT)
                .replace('.', ' ')
                .replaceAll("\\s+", " ")
                .trim();
    }
}
