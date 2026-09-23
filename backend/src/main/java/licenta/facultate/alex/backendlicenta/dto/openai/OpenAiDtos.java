package licenta.facultate.alex.backendlicenta.dto.openai;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.annotation.JsonProperty;
import tools.jackson.databind.JsonNode;

import java.util.List;

public final class OpenAiDtos {
    private OpenAiDtos() {
    }

    @JsonInclude(JsonInclude.Include.NON_NULL)
    public record ResponsesRequest(
            @JsonProperty("model") String model,
            @JsonProperty("store") boolean store,
            @JsonProperty("max_output_tokens") int maxOutputTokens,
            @JsonProperty("instructions") String instructions,
            @JsonProperty("input") String input,
            @JsonProperty("reasoning") Reasoning reasoning,
            @JsonProperty("text") Text text
    ) {
    }

    public record Reasoning(@JsonProperty("effort") String effort) {
    }

    public record Text(@JsonProperty("format") JsonSchemaFormat format) {
    }

    public record JsonSchemaFormat(
            @JsonProperty("type") String type,
            @JsonProperty("name") String name,
            @JsonProperty("strict") boolean strict,
            @JsonProperty("schema") JsonNode schema
    ) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record ResponsesResponse(
            @JsonProperty("id") String id,
            @JsonProperty("output_text") String outputText,
            @JsonProperty("output") List<OutputItem> output,
            @JsonProperty("error") OpenAiError error
    ) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record OutputItem(
            @JsonProperty("type") String type,
            @JsonProperty("content") List<ContentPart> content
    ) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record ContentPart(
            @JsonProperty("type") String type,
            @JsonProperty("text") String text
    ) {
    }

    @JsonIgnoreProperties(ignoreUnknown = true)
    public record OpenAiError(
            @JsonProperty("code") String code,
            @JsonProperty("message") String message
    ) {
    }
}
