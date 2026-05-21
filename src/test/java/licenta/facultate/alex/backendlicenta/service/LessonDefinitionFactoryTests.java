package licenta.facultate.alex.backendlicenta.service;

import licenta.facultate.alex.backendlicenta.model.LessonId;
import licenta.facultate.alex.backendlicenta.model.LessonRequestContext;
import org.junit.jupiter.api.Test;
import tools.jackson.databind.JsonNode;
import tools.jackson.databind.ObjectMapper;

import java.util.List;

import static org.assertj.core.api.Assertions.assertThat;

class LessonDefinitionFactoryTests {

    private final LessonDefinitionFactory factory = new LessonDefinitionFactory(new ObjectMapper());

    @Test
    void readTogetherIncludesSpeechFriendlyInstructionsAndSchema() {
        LessonRequestContext context = new LessonRequestContext(LessonId.ENGLISH_READ_TOGETHER, 3, List.of());

        String instructions = factory.buildInstructions(context);
        String input = factory.buildInput(context, null);
        JsonNode schema = factory.buildSchema(context);

        assertThat(instructions).contains("PassageText must be a very short child-friendly English read-aloud passage.");
        assertThat(input).contains("Keep passages easy to read aloud for speech recognition.");
        assertThat(input).contains("All passages must be different.");
        assertThat(schema.at("/properties/Questions/items/properties/PassageText/maxLength").asInt()).isEqualTo(80);
        assertThat(schema.at("/properties/Questions/items/required/0").asText()).isEqualTo("PassageText");
        assertThat(schema.at("/properties/Questions/items/properties/PassageText/pattern").asText())
                .isEqualTo("^[A-Za-z ]+(?:\\.[ ]*[A-Za-z ]+)?\\.?$");
    }
}
